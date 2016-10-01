#include "thread.h"
#include "Defines.h"

Thread::Thread()
{
}

#ifdef windows

void Thread::YieldThread()
{
    SwitchToThread();
}


#else
void Thread::YieldThread() {

}

#endif
