#ifndef ATOMIC_H
#define ATOMIC_H

#include <stdint.h>

#if defined(_WIN32)
#include <Windows.h>
#endif

namespace Atomics
{

uint16_t CompareAndSwap(volatile uint16_t* dest, uint16_t cmp, uint16_t swap);
uint32_t CompareAndSwap(volatile uint32_t* dest, uint32_t cmp, uint32_t swap);
uint64_t CompareAndSwap(volatile uint64_t* dest, uint64_t cmp, uint64_t swap);
void* CompareAndSwap(volatile void** dest, void* cmp, void* swap);

uint32_t Increment(volatile uint32_t* dest);
uint64_t Increment(volatile uint64_t* dest);

uint32_t Decrement(volatile uint32_t* dest);
uint64_t Decrement(volatile uint64_t* dest);


uint32_t Add(volatile uint32_t* dest, uint32_t value);
uint64_t Add(volatile uint64_t* dest, uint64_t value);

}

template<class T>
class Atomic
{
public:
    Atomic() {}
    Atomic(T val) : _val(val) {}
    Atomic<T>& operator=(T val) { _val = val; return *this; }
    operator T() { return _val; }
    
    /** Compare this value with a new value and replace it with the new value
      * if they are equal.
      * @param cmp The value to compare with.
      * @param swap The new value.
      * @return The old value.
      */
    T CompareAndSwap(T cmp, T swap) { return Atomics::CompareAndSwap(&_val, cmp, swap); }
    
    T operator+=(T value) { return Atomics::Add(&_val, value); }
    T operator++() { return Atomics::Increment(&_val); }
    T operator--() { return Atomics::Decrement(&_val); }
    
private:
    volatile T _val;
};

#endif // ATOMIC_H
