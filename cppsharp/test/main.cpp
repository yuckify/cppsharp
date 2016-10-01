#include <cppsharp.h>

extern void initInternalCall();

int main(int argc, char** argv) {
	
	cppsharp::AssemblyManager::instance();
	initInternalCall();
	cppsharp::AssemblyManager::instance().GetRuntime().JitExec(argc, argv);
	
/*
	mono_config_parse (NULL);
	cppsharp::Domain::__domain = mono_jit_init ("Test.exe");

	MonoAssembly *assembly = mono_domain_assembly_open (cppsharp::Domain::__domain, "Test.exe");
	if (!assembly) exit (-2);
		initInternalCall();
	mono_jit_exec (cppsharp::Domain::__domain, assembly, argc, argv);
*/
	
	return 0;
}

