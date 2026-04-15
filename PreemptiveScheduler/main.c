#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include "thread.h"

int counter = 0;
mutex_t m, m1, m2;
rwlock_t rw;

void worker1(void *args) {
  for (int i = 0; i < 3; i++) {
    printf("Worker %d\n", thread_self());
    usleep(100000);
  }
}

void worker2(void *args) {
  for (int i = 0; i < 3; i++) {
    printf("Worker %d\n", thread_self());
    usleep(100000);
  }
}

void worker_ret(void *args) {
  int *x = malloc(sizeof(int));
  *x = 42;
  thread_exit(x);
}

void worker1_m(void *args) {
  for (int i = 0; i < 50; i++) {
    usleep(1000);
    mutex_lock(&m);
    counter++;
    mutex_unlock(&m);
    // usleep(100000);
  }
}

void t1_d(void *args) {
  mutex_lock(&m1);
  thread_yield();
  mutex_lock(&m2);
}

void t2_d(void *args) {
  mutex_lock(&m2);
  thread_yield();
  mutex_lock(&m1);
}

void deadlock_checker(void *args) {
  usleep(1000000);
  raise(SIGQUIT);
}

void reader(void *arg) {
  rwlock_rdlock(&rw);
  printf("Reader %d reading: %d\n", thread_self(), counter);
  usleep(100000);
  rwlock_unlock(&rw);
  printf("Reader %d done.\n", thread_self());
}

void writer(void *arg) {
  rwlock_wrlock(&rw);
  printf("Writer %d writing\n", thread_self());
  counter++;
  usleep(200000);
  rwlock_unlock(&rw);
  printf("Writer %d done.\n", thread_self());
}

void independent_workers() {
  printf("Independent workers\n");
  thread_t t1, t2;

  thread_create(&t1, worker1, NULL);
  thread_create(&t2, worker1, NULL);

  thread_join(t1, NULL);
  thread_join(t2, NULL);

  printf("DONE - independent workers\n");
  printf("\n");
}

void return_val_worker() {
  printf("Return val worker\n");

  thread_t t1;

  thread_create(&t1, worker_ret, NULL);

  void *ret;
  thread_join(t1, &ret);
  printf("Returned value %d\n", *(int *)ret);
  printf("\n");
}

void mutex_workers() {
  printf("Mutex workers\n");
  thread_t t1, t2;
  mutex_init(&m);

  thread_create(&t1, worker1_m, NULL);
  thread_create(&t2, worker1_m, NULL);

  thread_join(t1, NULL);
  thread_join(t2, NULL);

  printf("Counter = %d\n", counter);
  printf("\n");
}

void deadlock_workers() {
  printf("Deadlock workers\n");
  thread_t t1, t2, t3;

  mutex_init(&m1);
  mutex_init(&m2);

  thread_create(&t1, t1_d, NULL);
  thread_create(&t2, t2_d, NULL);
  thread_create(&t3, deadlock_checker, NULL);

  thread_join(t1, NULL);
  thread_join(t2, NULL);
  thread_join(t3, NULL);
}

void read_write_workers() {
  printf("Read-Write workers\n");
  rwlock_init(&rw);

  thread_t r1, r2, w1, w2;
  thread_create(&r1, writer, NULL);
  thread_create(&r2, reader, NULL);
  thread_create(&w1, writer, NULL);
  thread_create(&w2, reader, NULL);

  thread_join(r1, NULL);
  thread_join(r2, NULL);
  thread_join(w1, NULL);
  thread_join(w2, NULL);
  printf("Done read-write workers\n");
  printf("Counter = %d\n", counter);
  printf("\n");
}

int main(void) {
  thread_init();
  printf("Main thread: %d\n", thread_self());

  independent_workers();
  return_val_worker();
  mutex_workers();
  read_write_workers();
  deadlock_workers();

  printf("MAIN THREAD DONE\n");
  return 0;
}
