#!/bin/bash

# if windows add castxml to PATH
cast_path="$PWD/castxml/bin"
if [[ $OS = "Windows_NT" && -z $(echo $PATH | grep $cast_path) ]]; then
	export PATH="$PATH:$cast_path"
fi

# delete all old files
rm -rf bin
mkdir bin

source_file="Test.hpp"
base=${source_file%%.*}
dll="bin/$base.dll"
cs_file="bin/$base.cs"
cppsharp_files="bin/${base}_cppsharp.cpp main.cpp bin/cppsharp_init.cpp"

if [[ $OS = "Windows_NT" ]]; then
	target="bin/${base}.exe"
else
	target="bin/$base"
fi

lib="lib/cppsharp.cpp"

if [[ $OS = "Windows_NT" ]]; then
	mono_cflags="-I:\"C:/Program Files/Mono/include/mono-2.0\""
else
	mono_cflags="-I:/usr/include/mono-2.0/"
fi
# generate the interface
echo "Generating c# interface"
../bin/Debug/cppsharp.exe "$mono_cflags" -I:./lib -I:other_dir -o:bin $source_file

# compile the test program
echo "Compiling c# code"
mcs -platform:x64 Main.cs $cs_file lib/cppsharp.cs CallbackTest.cs -unsafe -out:$dll

echo "Compiling c++ code"
lflags="$(pkg-config --libs --cflags monosgen-2)"
cflags+="-Iother_dir -Ilib -I. -Ibin"
if [[ $OS = "Windows_NT" ]]; then
#	lflags="\"C:/Program Files/Mono/lib/monosgen-2.0.lib\""
#	cflags+=" -I\"C:/Program Files/Mono/include/mono-2.0\" /DEBUG"
#	echo "run the following command in the developer command prompt"
#	echo cl.exe /EHsc $lib $cppsharp_files $cflags $lflags /link /out:$target
	b2 toolset=msvc address-model=64
	cp `find -name Test.exe` bin/
else
	cflags+=" -g"
	g++ -std=c++14 -fpermissive $lib $cppsharp_files $cflags $lflags -o $target
fi



