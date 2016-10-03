using System;
using System.Threading;
using System.Diagnostics;

class MainTest
{
	static int count = 0;
	static int passed = 0;
	
	static int total = 0;
	static int total_passed = 0;

	public static void StartTest(string test)
	{
		Console.WriteLine ("Tests: " + test);
	}

	public static void EndTest(string test)
	{
		if (passed != count)
			Console.WriteLine ("\tFailed " + (count - passed) + " in unit");
		else
			Console.WriteLine ("\tPassed");
		passed = 0;
		count = 0;
	}

	public static void EXPECT_TRUE(bool check, string msg)
	{
		total++;
		count++;
		if(check) {
			passed++;
			total_passed++;
		}
		else Console.WriteLine ("Failed " + msg);
	}

	unsafe public static void testFloat(ref float f)
	{
		fixed(float* t = &f)
		{

		}
	}

	static void Main (string[] args)
	{

		Console.WriteLine("Start Tests");
		
		Work.Test tmp = new Work.Test();

		// test the size of the fundamental types
		StartTest ("type size");
		EXPECT_TRUE (sizeof(sbyte) == tmp.sizeOfchar(), "type size, sbyte");
		EXPECT_TRUE (sizeof(byte) == tmp.sizeOfunsignedchar(), "type size, byte");
		EXPECT_TRUE (sizeof(short) == tmp.sizeOfshort(), "type size, short");
		EXPECT_TRUE (sizeof(ushort) == tmp.sizeOfunsignedshort(), "type size, ushort");
		EXPECT_TRUE (sizeof(int) == tmp.sizeOfint(), "type size, int");
		EXPECT_TRUE (sizeof(uint) == tmp.sizeOfunsignedint(), "type size, uint");
		EXPECT_TRUE (sizeof(long) == tmp.sizeOflonglong(), "type size, long");
		EXPECT_TRUE (sizeof(ulong) == tmp.sizeOfunsignedlonglong(), "type size, ulong");
		EndTest ("Passed");

		// test the fundamental types as input arguements
		StartTest ("type arguement");
		sbyte _sbyte = 0;
		byte _byte = 0;
		short _short = 0;
		ushort _ushort = 0;
		int _int = 0;
		uint _uint = 0;
		long _long = 0;
		ulong _ulong = 0;
		EXPECT_TRUE (0 == tmp.inputTest(_sbyte), "type arg, sbyte");
		EXPECT_TRUE (1 == tmp.inputTest(_byte), "type arg, byte");
		EXPECT_TRUE (2 == tmp.inputTest(_short), "type arg, short");
		EXPECT_TRUE (3 == tmp.inputTest(_ushort), "type arg, ushort");
		EXPECT_TRUE (4 == tmp.inputTest(_int), "type arg, int");
		EXPECT_TRUE (5 == tmp.inputTest(_uint), "type arg, uint");
		EXPECT_TRUE (6 == tmp.inputTest(_long), "type arg, long");
		EXPECT_TRUE (7 == tmp.inputTest(_ulong), "type arg, ulong");
		EXPECT_TRUE (8 == tmp.inputTest8(tmp), "type arg, Test");
		EXPECT_TRUE (9 == tmp.inputTest9(tmp), "type arg, Test*");
		EXPECT_TRUE (10 == tmp.inputTest10(tmp), "type arg, Test&");
		EndTest ("Passed");

		// test the fundamental types as input arguements by reference
		StartTest ("type arguement by reference");
		sbyte _sbyteref = 0;
		byte _byteref = 0;
		short _shortref = 0;
		ushort _ushortref = 0;
		int _intref = 0;
		uint _uintref = 0;
		long _longref = 0;
		ulong _ulongref = 0;
		EXPECT_TRUE (11 == tmp.inputTestRef(ref _sbyteref) && _sbyteref == 11, "type arg, sbyte&");
		EXPECT_TRUE (12 == tmp.inputTestRef(ref _byteref) && _byteref == 12, "type arg, byte&");
		EXPECT_TRUE (13 == tmp.inputTestRef(ref _shortref) && _shortref == 13, "type arg, short&");
		EXPECT_TRUE (14 == tmp.inputTestRef(ref _ushortref) && _ushortref == 14, "type arg, ushort&");
		EXPECT_TRUE (15 == tmp.inputTestRef(ref _intref) && _intref == 15, "type arg, int&");
		EXPECT_TRUE (16 == tmp.inputTestRef(ref _uintref) && _uintref == 16, "type arg, uint&");
		EXPECT_TRUE (17 == tmp.inputTestRef(ref _longref) && _longref == 17, "type arg, long&");
		EXPECT_TRUE (18 == tmp.inputTestRef(ref _ulongref) && _ulongref == 18, "type arg, ulong&");
		EndTest ("Passed");

		// test the fundamental types as input arguements by pointer
		StartTest ("type arguement by pointer");
		sbyte _sbyteptr = 0;
		byte _byteptr = 0;
		short _shortptr = 0;
		ushort _ushortptr = 0;
		int _intptr = 0;
		uint _uintptr = 0;
		long _longptr = 0;
		ulong _ulongptr = 0;
		EXPECT_TRUE (19 == tmp.inputTestPtr(ref _sbyteptr) && _sbyteptr == 19, "type arg, sbyte*");
		EXPECT_TRUE (20 == tmp.inputTestPtr(ref _byteptr) && _byteptr == 20, "type arg, byte*");
		EXPECT_TRUE (21 == tmp.inputTestPtr(ref _shortptr) && _shortptr == 21, "type arg, short*");
		EXPECT_TRUE (22 == tmp.inputTestPtr(ref _ushortptr) && _ushortptr == 22, "type arg, ushort*");
		EXPECT_TRUE (23 == tmp.inputTestPtr(ref _intptr) && _intptr == 23, "type arg, int*");
		EXPECT_TRUE (24 == tmp.inputTestPtr(ref _uintptr) && _uintptr == 24, "type arg, uint*");
		EXPECT_TRUE (25 == tmp.inputTestPtr(ref _longptr) && _longptr == 25, "type arg, long*");
		EXPECT_TRUE (26 == tmp.inputTestPtr(ref _ulongptr) && _ulongptr == 26, "type arg, ulong*");
		EndTest ("Passed");

		// test the enums types as input arguements by value, ref, pointer
		StartTest ("type arguement by pointer");
		Work.Test.enum_test_public _enumval = Work.Test.enum_test_public.no;
		EXPECT_TRUE (Work.Test.enum_test_public.yes == (_enumval = tmp.enumValue(_enumval)),
			"type arg, enum");
		EndTest ("Passed");

		// test the get/set generation
		StartTest ("get/set");
		EXPECT_TRUE (tmp.Var == 1000, "get Var");
		tmp.Var = 123;
		EXPECT_TRUE (tmp.Var == 123, "set Var");
		EXPECT_TRUE (tmp.A == 1001, "get A");
		tmp.B = 123;
		EXPECT_TRUE (tmp.A == 123, "set B");
		
		EXPECT_TRUE (tmp.VarA == 1002, "get VarA");
		tmp.VarA = 123;
		EXPECT_TRUE (tmp.VarA == 123, "set VarA");
		
		EXPECT_TRUE (tmp.VarC == 1003, "get VarC");
		tmp.VarC = 123;
		EXPECT_TRUE (tmp.VarC == 123, "set VarC");
		
		EXPECT_TRUE (tmp.VarE == 1004, "get VarE");
		tmp.VarD = 123;
		EXPECT_TRUE (tmp.VarE == 123, "set VarD");
		
		EndTest ("Passed");
		
		// test the operators
		StartTest ("operators");
		// member operators
//		EXPECT_TRUE (tmp == 27, "==");
//		EXPECT_TRUE (tmp != 27, "!=");

		// global operators
		EXPECT_TRUE ((tmp + 1) == 30, "+");
		EXPECT_TRUE ((tmp - 1) == -29, "-");
		EndTest("Passed");

		StartTest ("string");
		EXPECT_TRUE (tmp.stdStringPassThrough("pass") == "pass", "string pass");
		tmp.stdStringInput("input");
		EXPECT_TRUE (tmp.stdStringRet() == "input", "string pass2");
		EndTest("Passed");

		StartTest ("Callback");
		tmp.testCallbacks();
		EndTest ("Passed");
		
		//*********************************************************************
		// print the results
		Console.WriteLine ("");
		Console.WriteLine ("Tests Run " + total);
		Console.WriteLine ("Tests Passed " + total_passed);
		Console.WriteLine ("");

		//*********************************************************************
		// performance tests, not counted in the test bin, these are used to optimize
		// measuring the performance of calling a function c#->c++ and c++->c++
		Console.WriteLine ("Performance function call");
		DateTime begA = DateTime.Now;
		for(int i=0; i<100000000; i++)
		{
			tmp.callMe();
		}
		DateTime endA = DateTime.Now;
		Console.WriteLine ("c#->c++ : " + (endA.Ticks - begA.Ticks)*100/1000 + " us");

		DateTime begB = DateTime.Now;
		tmp.cppFunctionPerformance();
		DateTime endB = DateTime.Now;
		Console.WriteLine ("c++->c++ : " + (endB.Ticks - begB.Ticks)*100/1000 + " us");

	}
}

