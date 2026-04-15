#ifndef THREAD_H
#define THREAD_H
#include <ucontext.h>

#define STACK_SIZE (256 * 1024)

typedef int thread_t;

int thread_init(void);
thread_t thread_self(void);
void thread_yield(void);
int thread_create(thread_t *tid, void (*fn)(void *), void *args);
void thread_exit(void *ret_val);
int thread_join(thread_t tid, void **ret_val);
int setup_timer(void);

typedef struct mutex {
  int is_locked;
  struct tcb *owner;
  struct tcb *waiting_queue;
} mutex_t;

int mutex_init(mutex_t *m);
int mutex_lock(mutex_t *m);
int mutex_unlock(mutex_t *m);

typedef struct rwlock {
  int readers;
  int writer;
  struct tcb *writer_owner;
  struct tcb *reader_queue;
  struct tcb *writer_queue;
} rwlock_t;

int rwlock_init(rwlock_t *rw);
int rwlock_rdlock(rwlock_t *rw);
int rwlock_wrlock(rwlock_t *rw);
int rwlock_unlock(rwlock_t *rw);
int rwlock_destroy(rwlock_t *rw);
#endif
