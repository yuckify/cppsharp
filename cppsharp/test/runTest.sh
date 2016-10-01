#!/bin/bash

# delete all old files
rm -rf bin
mkdir bin

source_file="Test.hpp"
base=${source_file%%.*}
dll="bin/$base.dll"
cs_file="bin/$base.cs"
interop_files="bin/${base}_interop.cpp main.cpp bin/init.cpp"
target="bin/$base"

lib="lib/cppsharp.cpp lib/thread.cpp lib/atomic.cpp"

# generate the interface
../bin/Debug/cppsharp.exe -I:lib -I:other_dir -o:bin -lib:$(basename $dll) $source_file

# compile the test program
mcs Main.cs $cs_file lib/cppsharp.cs CallbackTest.cs -unsafe -out:$dll

lflags="$(pkg-config --libs --cflags monosgen-2)"
cflags="-Iother_dir -Ilib -I. -Ibin -g"
g++ -fpermissive $lib $interop_files $cflags $lflags -o $target



