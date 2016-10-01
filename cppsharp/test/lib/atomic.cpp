#include"atomic.h"

namespace Atomics
{

#if defined(_WIN32)

uint16_t CompareAndSwap(volatile uint16_t* dest, uint16_t cmp, uint16_t swap)
{
    return InterlockedCompareExchange16((volatile SHORT*)dest, swap, cmp);
}

uint32_t CompareAndSwap(volatile uint32_t* dest, uint32_t cmp, uint32_t swap)
{
    return InterlockedCompareExchange(dest, swap, cmp);
}

uint64_t CompareAndSwap(volatile uint64_t* dest, uint64_t cmp, uint64_t swap)
{
    return InterlockedCompareExchange64((volatile LONGLONG*)dest, swap, cmp);
}

void* CompareAndSwap(volatile void** dest, void* cmp, void* swap)
{
    return InterlockedCompareExchangePointer((volatile PVOID*)dest, swap, cmp);
}

uint32_t Increment(volatile uint32_t* dest)
{
    return InterlockedIncrement(dest);
}

uint64_t Increment(volatile uint64_t* dest)
{
    return InterlockedIncrement64((volatile LONGLONG*)dest);
}

uint32_t Decrement(volatile uint32_t* dest)
{
    return InterlockedDecrement(dest);
}

uint64_t Decrement(volatile uint64_t* dest)
{
    return InterlockedDecrement64((volatile LONGLONG*)dest);
}

uint32_t Add(volatile uint32_t* dest, uint32_t value)
{
    return InterlockedExchangeAdd(dest, value);
}

uint64_t Add(volatile uint64_t* dest, uint64_t value)
{
    return InterlockedExchangeAdd(dest, value);
}

#else

uint16_t CompareAndSwap(volatile uint16_t* dest, uint16_t cmp, uint16_t swap)
{
    return __sync_val_compare_and_swap(dest, swap, cmp);
}

uint32_t CompareAndSwap(volatile uint32_t* dest, uint32_t cmp, uint32_t swap)
{
    return __sync_val_compare_and_swap(dest, swap, cmp);
}

uint64_t CompareAndSwap(volatile uint64_t* dest, uint64_t cmp, uint64_t swap)
{
    return __sync_val_compare_and_swap(dest, swap, cmp);
}

void* CompareAndSwap(volatile void** dest, void* cmp, void* swap)
{
    return __sync_val_compare_and_swap((void **)dest, swap, cmp);
}

uint32_t Increment(volatile uint32_t* dest)
{
    return __sync_fetch_and_add(dest, 1);
}

uint64_t Increment(volatile uint64_t* dest)
{
    return __sync_fetch_and_add(dest, 1);
}

uint32_t Decrement(volatile uint32_t* dest)
{
    return __sync_fetch_and_sub(dest, 1);
}

uint64_t Decrement(volatile uint64_t* dest)
{
    return __sync_fetch_and_sub(dest, 1);
}

uint32_t Add(volatile uint32_t* dest, uint32_t value)
{
    return __sync_fetch_and_add(dest, value);
}

uint64_t Add(volatile uint64_t* dest, uint64_t value)
{
    return __sync_fetch_and_add(dest, value);
}

#endif

}
