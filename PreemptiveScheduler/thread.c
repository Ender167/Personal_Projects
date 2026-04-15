#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/time.h>

#include "thread.h"

typedef enum { RUNNING, READY, BLOCKED, TERMINATED } state_t;

typedef struct tcb {
  thread_t tid;
  ucontext_t ctx;
  state_t state;
  void *stack;
  void *ret_val;
  struct tcb *waiting;
  struct tcb *joiner;
  struct mutex *waiting_on;

  struct tcb *next;
  struct tcb *next_all;
} tcb_t;

static tcb_t *main_thread = NULL;

static tcb_t *current = NULL;
static tcb_t *ready_queue = NULL;
static int next_tid = 0;

static tcb_t *all_threads = NULL;

static tcb_t *waiting_for(tcb_t *t) {
  if (t->waiting && t->waiting->state != TERMINATED) {
    return t->waiting;
  }
  if (t->waiting_on && t->waiting_on->owner &&
      t->waiting_on->owner->state != TERMINATED) {
    return t->waiting_on->owner;
  }
  return NULL;
}

static int detect_cycle(tcb_t *start) {
  if (!start) return 0;

  tcb_t *slow = start;
  tcb_t *fast = start;

  while (1) {
    slow = waiting_for(slow);
    fast = waiting_for(fast);
    if (fast) {
      fast = waiting_for(fast);
    }

    if (!slow || !fast) return 0;

    if (slow == fast) break;
  }

  tcb_t *p = slow;

  do {
    if (p == start) return 1;
    p = waiting_for(p);
  } while (p && p != slow);

  return 0;
}

static void detect_deadlock(void) {
  tcb_t *p = all_threads;
  int found = 0;
  while (p != NULL) {
    if (waiting_for(p)) {
      if (detect_cycle(p)) {
        found = 1;
        printf("Deadlock detected: thread %d\n", p->tid);
      }
    }

    p = p->next_all;
  }
  if (found == 0) {
    printf("No deadlock detected.\n");
  }
}

static void signal_handler(int signum) {
  if (signum == SIGALRM) {
    thread_yield();
  }
  if (signum == SIGQUIT) {
    detect_deadlock();
  }
}
static void add_to_all_threads(tcb_t *t) {
  t->next_all = all_threads;
  all_threads = t;
}

static void remove_from_all_threads(tcb_t *t) {
  tcb_t **pp = &all_threads;
  while (*pp) {
    if (*pp == t) {
      *pp = t->next_all;
      return;
    }
    pp = &(*pp)->next_all;
  }
}

static tcb_t *find_thread(thread_t tid) {
  tcb_t *p = all_threads;
  while (p != NULL) {
    if (p->tid == tid) {
      return p;
    }
    p = p->next_all;
  }
  return NULL;
}

static int is_in_queue(tcb_t *q, tcb_t *t) {
  tcb_t *p = q;
  while (p) {
    if (p == t) return 1;
    p = p->next;
  }

  return 0;
}
static void enqueue(tcb_t **queue, tcb_t *t) {
  if (is_in_queue(*queue, t)) return;

  t->next = NULL;
  if (!*queue) {
    *queue = t;
  } else {
    tcb_t *p = *queue;
    while (p->next != NULL) {
      p = p->next;
    }
    p->next = t;
  }
}

static tcb_t *dequeue(tcb_t **queue) {
  if (!*queue) {
    return NULL;
  }
  tcb_t *t = *queue;
  *queue = t->next;
  t->next = NULL;
  return t;
}

int mutex_init(mutex_t *m) {
  m->is_locked = 0;
  m->owner = NULL;
  m->waiting_queue = NULL;
  return 0;
}

int mutex_lock(mutex_t *m) {
  while (m->is_locked) {
    current->waiting_on = m;
    current->state = BLOCKED;
    enqueue(&m->waiting_queue, current);
    thread_yield();
  }
  m->is_locked = 1;
  m->owner = current;

  return 0;
}

int mutex_unlock(mutex_t *m) {
  if (m->waiting_queue) {
    tcb_t *next = dequeue(&m->waiting_queue);
    next->state = READY;
    m->owner = next;
    next->waiting_on = NULL;
    enqueue(&ready_queue, next);
  } else {
    m->is_locked = 0;
    m->owner = NULL;
  }

  return 0;
}

int rwlock_init(rwlock_t *rw) {
  rw->readers = 0;
  rw->writer = 0;
  rw->writer_owner = NULL;
  rw->reader_queue = NULL;
  rw->writer_queue = NULL;

  return 0;
}

int rwlock_rdlock(rwlock_t *rw) {
  while (rw->writer || rw->writer_queue) {
    current->state = BLOCKED;
    enqueue(&rw->reader_queue, current);
    thread_yield();
  }

  rw->readers++;
  return 0;
}
int rwlock_wrlock(rwlock_t *rw) {
  while (rw->writer || rw->readers > 0) {
    current->state = BLOCKED;
    enqueue(&rw->writer_queue, current);
    thread_yield();
  }

  rw->writer = 1;
  rw->writer_owner = current;
  return 0;
}

int rwlock_unlock(rwlock_t *rw) {
  if (rw->writer && rw->writer_owner == current) {
    rw->writer = 0;
    rw->writer_owner = NULL;

    if (rw->writer_queue != NULL) {
      tcb_t *next = dequeue(&rw->writer_queue);
      next->state = READY;
      enqueue(&ready_queue, next);
    }

    else {
      while (rw->reader_queue) {
        tcb_t *r = dequeue(&rw->reader_queue);
        r->state = READY;
        enqueue(&ready_queue, r);
      }
    }

    return 0;
  }
  if (rw->readers > 0) {
    rw->readers--;

    if (rw->readers == 0 && rw->writer_queue) {
      tcb_t *next = dequeue(&rw->writer_queue);
      next->state = READY;
      enqueue(&ready_queue, next);
    }

    return 0;
  }
  return -1;
}

int rwlock_destroy(rwlock_t *rw) {
  if (rw->readers || rw->writer || rw->reader_queue || rw->writer_queue) {
    return -1;
  }

  return 0;
}

static void thread_stub(void (*fn)(void *), void *args) {
  fn(args);

  thread_exit(NULL);
}

int setup_timer() {
  struct itimerval timer;
  timer.it_value.tv_sec = 0;
  timer.it_value.tv_usec = 100000;
  timer.it_interval.tv_sec = 0;
  timer.it_interval.tv_usec = 100000;

  if (setitimer(ITIMER_REAL, &timer, NULL) == -1) {
    return -1;
  }
  if (signal(SIGALRM, signal_handler) == SIG_ERR) {
    return -1;
  }
}

int thread_init(void) {
  current = malloc(sizeof(tcb_t));

  current->tid = next_tid++;
  current->state = RUNNING;
  current->next = NULL;
  current->waiting = NULL;
  current->waiting_on = NULL;
  getcontext(&current->ctx);

  main_thread = current;
  add_to_all_threads(current);

  signal(SIGQUIT, signal_handler);
  setup_timer();
  return 0;
}

void thread_yield(void) {
  if (!ready_queue) {
    return;
  }

  tcb_t *prev = current;
  current->state = READY;
  enqueue(&ready_queue, prev);

  tcb_t *next = dequeue(&ready_queue);
  next->state = RUNNING;
  current = next;
  swapcontext(&prev->ctx, &next->ctx);
}

thread_t thread_self(void) { return current->tid; }

int thread_create(thread_t *tid, void (*fn)(void *), void *args) {
  tcb_t *t = malloc(sizeof(tcb_t));

  t->tid = next_tid++;
  t->state = READY;
  t->next = NULL;
  t->stack = malloc(STACK_SIZE);
  t->waiting = NULL;
  t->waiting_on = NULL;
  t->joiner = NULL;

  getcontext(&t->ctx);
  t->ctx.uc_stack.ss_sp = t->stack;
  t->ctx.uc_stack.ss_size = STACK_SIZE;
  t->ctx.uc_link = NULL;

  makecontext(&t->ctx, (void (*)())thread_stub, 2, fn, args);

  add_to_all_threads(t);

  enqueue(&ready_queue, t);
  *tid = t->tid;
  return 0;
}

void thread_exit(void *ret_val) {
  current->state = TERMINATED;
  current->ret_val = ret_val;

  if (current->joiner) {
    tcb_t *j = current->joiner;
    j->state = READY;
    enqueue(&ready_queue, j);
    current->joiner = NULL;
    j->waiting = NULL;
  }

  tcb_t *next = dequeue(&ready_queue);
  if (!next) {
    if (current == main_thread) exit(0);

    main_thread->state = RUNNING;
    current = main_thread;
    setcontext(&main_thread->ctx);
  }

  tcb_t *prev = current;
  current = next;
  next->state = RUNNING;

  setcontext(&next->ctx);
}

int thread_join(thread_t tid, void **ret_val) {
  if (tid == current->tid) {
    return -1;
  }
  tcb_t *p = find_thread(tid);
  if (!p) {
    return -1;
  }
  if (p->state == TERMINATED) {
    if (ret_val) {
      *ret_val = p->ret_val;
    }
    remove_from_all_threads(p);
    free(p->stack);
    free(p);
    return 0;
  }

  current->state = BLOCKED;
  p->joiner = current;
  current->waiting = p;

  tcb_t *next = dequeue(&ready_queue);
  if (!next) {
    return -1;
  }
  tcb_t *prev = current;
  current = next;
  next->state = RUNNING;

  swapcontext(&prev->ctx, &next->ctx);

  current->waiting = NULL;

  if (ret_val) {
    *ret_val = p->ret_val;
  }
  remove_from_all_threads(p);
  free(p->stack);
  free(p);

  return 0;
}
