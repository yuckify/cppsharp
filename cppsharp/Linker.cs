using System;
using System.Collections.Generic;
using System.IO;

namespace cppsharp
{
	public class Linker
	{
		public Linker()
		{
			//			_outFileName = outfn;
			_headerFiles = new List<string>();
		}
		
		public void AddFile(string fileName)
		{
			_headerFiles.Add (fileName);
		}

		public List<string> SourceFiles { set { _sourceFiles = value; } }
		
		public void link()
		{
			List<string> includes = new List<string>();
			List<string> call = new List<string>();

			// add the includes to includes list for the final main.cpp file
			foreach(string fileName in _headerFiles)
			{
				if(!File.Exists(fileName)) continue;
				StreamReader file = new StreamReader(fileName);
				
				while(!file.EndOfStream)
				{
					string line = file.ReadLine();
					if(line.StartsWith("#include"))
						includes.Add (line);
					else
						call.Add (line);
				}
				
			}

			using(StreamWriter mainFile = new StreamWriter(OutDir + "/cppsharp_init.cpp"))
			{
				mainFile.WriteLine ("#include<stdlib.h>");
				mainFile.WriteLine ("#include<mono/metadata/object.h>");
				mainFile.WriteLine ("#include<mono/metadata/mono-config.h>");
				
				foreach(string str in includes)
					mainFile.WriteLine (str);

				mainFile.WriteLine ("#include<mono/jit/jit.h>");
				mainFile.WriteLine ("");

				mainFile.WriteLine ("void cppsharp_init()\n{");
				foreach(string str in call)
					mainFile.WriteLine ("\t" + str);
				mainFile.WriteLine ("}\n");

//				mainFile.WriteLine ("int main(int argc, char** argv)\n{");
//				mainFile.WriteLine ("\tmono_config_parse (NULL);");
//				mainFile.WriteLine ("\tcppsharp::Domain::__domain = mono_jit_init (\"" + lib + "\");\n");

//				mainFile.WriteLine ("\n\tMonoAssembly *assembly = mono_domain_assembly_open (cppsharp::Domain::__domain, \"" + lib + "\");");
//				mainFile.WriteLine ("\tif (!assembly) exit (-2);");
//				mainFile.WriteLine ("\tinitInternalCall();");
//				mainFile.WriteLine ("\tmono_jit_exec (cppsharp::Domain::__domain, assembly, argc, argv);");
//				mainFile.WriteLine ("}");
			}
		}

		public string OutDir { get; set; }
		
		List<string> _headerFiles;

		Dictionary<string, List<string>> _reverseHeader;
		List<string> _sourceFiles;
	}
}

