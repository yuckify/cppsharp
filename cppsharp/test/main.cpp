#include <cppsharp.h>
#include <iostream>

extern void cppsharp_init();

int main(int argc, char** argv) {
	cppsharp::AssemblyManager::instance();
	cppsharp_init();
	cppsharp::AssemblyManager::instance().GetRuntime().JitExec(argc, argv);
	return 0;
}

