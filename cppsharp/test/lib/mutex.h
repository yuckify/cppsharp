#ifndef MUTEX_H
#define MUTEX_H

#include <pthread.h>
#include <semaphore.h>
#include <stdint.h>

#include "atomic.h"
#include "thread.h"
#include "traits.h"

class SpinLock
{
public:
    SpinLock() : _var(0) {}
    
    void Lock() { while (_var.CompareAndSwap(0, 1) == 1); }
    void Unlock() { _var = 0; }
    bool Trylock() { return _var.CompareAndSwap(0, 1) == 0; }
    
private:
    Atomic<uint32_t> _var;
};

class Mutex
{
public:
    Mutex() { pthread_mutex_init(&_mutex, NULL); }
    ~Mutex() { pthread_mutex_destroy(&_mutex); }
    
    void Lock() { pthread_mutex_lock(&_mutex); }
    void Unlock() { pthread_mutex_unlock(&_mutex); }
    bool Trylock() { return pthread_mutex_trylock(&_mutex) == 0; }
    
private:
    pthread_mutex_t _mutex;
};

class Semaphore;

template<class T>
class ScopedLock
{
public:
    ScopedLock(T& lock) : _lock(lock) { _lock.Lock(); }
    ~ScopedLock() { _lock.Unlock(); }
private:
    T& _lock;
};

class Semaphore
{
public:
    Semaphore(Mutex& mutex) : _mutex(mutex), _count(0) {}
    
    void Lock()
    {
        ScopedLock<Mutex> locker(_mutex);
        ++_count;
    }
    
    void Unlock()
    {
        --_count;
    }
    
    /** This call blocks until the use count of the semaphore is 0 again.
      * \code
      * Mutex mut;
      * Semaphore sem(mut);
      * {
      *     ScopedLock<Mutex> locker(mut);
      *     sem.Wait();
      *     // do something
      * }
      * \endcode
      */
    void Wait()
    {
        while (_count != 0) Thread::YieldThread();
    }
    
private:
    Atomic<unsigned int> _count;
    Mutex& _mutex;
};


#endif // MUTEX_H
