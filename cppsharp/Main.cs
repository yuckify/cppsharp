using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace cppsharp
{

	public sealed class TempFile : IDisposable
	{
		public TempFile() : 
		this(Path.GetTempPath()) { }
		
		public TempFile(string directory) {
			Create(Path.Combine(directory, Path.GetRandomFileName()));
		}

		~TempFile() {
			Delete();
		}

		public void Dispose() {
//			Delete();
//			GC.SuppressFinalize(this);
		}

		
		public string FilePath { get; private set; }
		
		private void Create(string path) {
			FilePath = path;
			using (File.Create(FilePath)) { };
		}

		private void Delete() {
			/*
			if (FilePath == null) return;
			File.Delete(FilePath);
			FilePath = null;
			*/
		}
	}

	// TODO consider using project file for input
	class MainClass
	{
		static void usage()
		{
			Console.WriteLine ("cppsharp [options] source-file\n" +
							   "\t-help -h             : print this help menu\n" +
							   "\t-c                   : compile only, will not link the *.main files and the main.cpp file\n" +
							   "\t-lib:<lib name>      : the name of the library that will contain the c# object code\n" +
							   "\t-import:<lib name>   : name of the library that will contain the c++ object code\n" +
							   "\t\tsetting this will use DllImport instead of InternalCall\n");
		}

		// outputs the list of *.main files for use by the linker
		static List<string> compileSource(string file, string dllImport, List<string> srcFiles, List<string> includePaths, string outDir)
		{
			List<string> ret = new List<string>();

			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

			// create a temp file to store the xml in
			using(cppsharp.TempFile tmp = new TempFile())
			using(cppsharp.TempFile gccOutputFileName = new TempFile())
			{
				string includeOps = "";
				foreach(string str in includePaths)
					includeOps += "-I" + str + " ";

				// run the command to
				switch (Environment.OSVersion.Platform) {
				case System.PlatformID.Unix:
					startInfo.FileName = "castxml";
					startInfo.Arguments = "-std=c++11 --castxml-gccxml " + includeOps + " " + file + " -o " + tmp.FilePath;
					startInfo.UseShellExecute = false;
					break;

				case System.PlatformID.Win32NT:
					startInfo.FileName = "cmd.exe";
					startInfo.Arguments = "/c gccxml.exe " + includeOps +" " + file + " -fxml=" + tmp.FilePath + " 2> " + 
						gccOutputFileName.FilePath;
					break;
				}

				process.StartInfo = startInfo;
				process.Start();
				process.WaitForExit();
				process.WaitForExit();

				// get the exit code and print error message if necessary
				if(process.ExitCode != 0) {
					// there was an error coming from gcc print gcc output to the console
					String gccOutput = File.ReadAllText(gccOutputFileName.FilePath);
					Console.Write (gccOutput);
					Environment.Exit (-1);
				}
				

				File.OpenRead(tmp.FilePath).Close();
				
				
				// debug
				string xml = File.ReadAllText(tmp.FilePath);
				int indexExt = file.LastIndexOf(".");
				int indexBase = 0;
				if(outDir != null) indexBase = file.LastIndexOf('/') + 1;
				if(indexExt < indexBase) indexExt = file.Length;
				string fileBase = file.Substring(indexBase, (indexExt<0 ? file.Length : indexExt) - indexBase);
				string xmlFile = outDir + "/" + fileBase + ".xml";
				File.WriteAllText(xmlFile, xml);

				// generate the csharp file
				cppsharp.CsharpGen gen = new cppsharp.CsharpGen(new XmlTextReader(tmp.FilePath));
				gen.SourceFiles = srcFiles;
				gen.XmlFile = xmlFile;
				gen.OutDir = outDir;
				gen.DllImport = dllImport;
				
				gen.process();
				gen.write();

				foreach(KeyValuePair<string, FileInfo> pair in gen.Files)
				{
					FileInfo info = pair.Value;
					ret.Add(info.MainFileName);
				}
			}

			return ret;
		}

		public static void Main (string[] args)
		{
			// the list of files that we will be generating c# interfaces for
			List<string> inputFiles = new List<string> ();
			// list of source files for the headers above, these are used to resolve forward declarations
			List<string> sourceFiles = new List<string>();
			// *.main
			List<string> linkFiles = new List<string> ();
			// -c
			bool justCompile = false;
			// -lib
			string libName = null;
			// -import
			string dllImport = null;
			// -I
			List<string> includePaths = new List<string>();
			// -o
			string outDir = null;



			includePaths.Add("..");
			switch (Environment.OSVersion.Platform) {
			case System.PlatformID.Win32NT:
				includePaths.Add("\"C:/Program Files (x86)/Mono-2.10.8/include/mono-2.0\"");
				break;
			case System.PlatformID.Unix:
				includePaths.Add("/usr/include/mono-2.0/");
				break;
			}
//			inputFiles.Add ("Test.hpp");
//			includePaths.Add ("interop");
//			outDir = "gen";
//			libName = "sharp.dll";

			for (int i = 0; i < args.Length; i++) {
				String arg = args [i];
				if (arg.Length == 0)
					continue;

				if (arg.StartsWith ("-") || 
					arg.StartsWith ("/")) {
					// this is a command line switch, process it
					int index = arg.IndexOf (":");
					string opArg = null;
					string opString = null;
					if (index >= 0) {
						opString = arg.Substring (1, index - 1);
						opArg = arg.Substring (index + 1, (arg.Length - (index + 1)));
					} else {
						opString = arg.Substring (1);
					}

					switch (opString) {
					case "lib":		libName = opArg.EndsWith("/") ? opArg.Substring(0, opArg.LastIndexOf("/")) : opArg; break;
					case "import":	dllImport = opArg; break;
					case "c":		justCompile = true; break;
					case "I":		includePaths.Add(opArg.EndsWith("/") ? opArg.Substring(0, opArg.LastIndexOf("/")) : opArg); break;
					case "o":		outDir = opArg; break;
					case "h":
					case "help":
						{
							usage ();
							Environment.Exit (0);
							break;
						}
					} // switch(opString)
				} else if (arg.EndsWith (".hpp") || 
					arg.EndsWith (".h") || 
					arg.EndsWith (".hxx") || 
					arg.EndsWith (".H")) {
					//this is a header input file, pass the filename into the code generator
					inputFiles.Add (arg);
				} else if (arg.EndsWith (".cpp") ||
				          arg.EndsWith (".cxx") ||
				          arg.EndsWith (".C")) {
					sourceFiles.Add (arg);
				} else if (arg.EndsWith (".main")) {
					linkFiles.Add (arg);
				}

			} // for

			// TODO remove this?
//			CsharpGen.writeCppsharp(outDir, dllImport);
//			includePaths.Add (outDir);

			if (inputFiles.Count == 0 && linkFiles.Count == 0) {
				Console.WriteLine ("cppsharp: no input files");
				Environment.Exit (-1);
			}

			// if the input does not exist throw error
			List<string> missingFiles = new List<string> ();
			foreach (string fn in inputFiles)
				if (!System.IO.File.Exists (fn))
					missingFiles.Add (fn);

			foreach (string fn in linkFiles)
				if (!System.IO.File.Exists (fn))
					missingFiles.Add (fn);

			if (missingFiles.Count != 0) {
				foreach (string fn in missingFiles)
					Console.WriteLine ("Error: cannot find file \"" + fn + "\"");
				Environment.Exit (-1);
			}

			// compile the list of files
			foreach (string fn in inputFiles) {
				List<string> tmpMain = compileSource (fn, dllImport, sourceFiles, includePaths, outDir);
				foreach(string file in tmpMain)
					linkFiles.Add (file);
			}

			if(!justCompile || (inputFiles.Count == 0 && linkFiles.Count != 0)) {
				cppsharp.Linker linker = new cppsharp.Linker();
				linker.SourceFiles = sourceFiles;

				foreach(string fn in linkFiles)
					linker.AddFile (fn);

				linker.OutDir = outDir;
				linker.link(libName);
			}

			Environment.Exit (0);
		} // Main()
	} // MainClass
}
