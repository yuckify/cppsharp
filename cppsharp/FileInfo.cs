using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;

namespace cppsharp
{
	public class FileInfo
	{
		public FileInfo(XmlTextReader reader, string outDir)
		{
			_fileName = reader["name"];
			_id = reader["id"];
			_write = false;
			_outDir = outDir;

			//setup the output directory
			int indexExt = _fileName.LastIndexOf(".");
			int indexBase = 0;
			if(outDir != null) indexBase = _fileName.LastIndexOf('/') + 1;
			if(indexExt < indexBase) indexExt = _fileName.Length;
			string fileBase = _fileName.Substring(indexBase, (indexExt<0 ? _fileName.Length : indexExt) - indexBase);

			// set the output file names for the files being generated
			_cHeaderFileName = _outDir + "/" + fileBase + "_cppsharp.hpp";
			_cSourceFileName = _outDir + "/" + fileBase + "_cppsharp.cpp";
			_csFileName = _outDir + "/" + fileBase + ".cs";
			_mainFileName = _outDir + "/" + fileBase + ".main";
			
			_cHeaderString = new StringBuilder();
			_cHeaderWriter = new StringWriter(_cHeaderString);
			_cSourceString = new StringBuilder();
			_cSourceWriter = new StringWriter(_cSourceString);
			_csString = new StringBuilder();
			_csWriter = new StringWriter(_csString);
			_mainString = new StringBuilder();
			_mainWriter = new StringWriter(_mainString);
		}

		public FileInfo(string name, string outDir)
		{
			_fileName = name;
			_id = null;
			_write = true;
			_outDir = outDir;

			// set the output file names for the files being generated
			_cHeaderFileName = _outDir + "/" + _fileName + ".h";
			_cSourceFileName = _outDir + "/" + _fileName + ".cpp";
			_csFileName = _outDir + "/" + _fileName + ".cs";
			_mainFileName = _outDir + "/" + _fileName + ".main";
			
			_cHeaderString = new StringBuilder();
			_cHeaderWriter = new StringWriter(_cHeaderString);
			_cSourceString = new StringBuilder();
			_cSourceWriter = new StringWriter(_cSourceString);
			_csString = new StringBuilder();
			_csWriter = new StringWriter(_csString);
			_mainString = new StringBuilder();
			_mainWriter = new StringWriter(_mainString);
		}
		
		public string Id { get { return _id; } }
		public string FileName { get { return _fileName; } }
		public StringWriter CHeaderWriter { get { return _cHeaderWriter; } }
		public StringWriter CSourceWriter { get { return _cSourceWriter; } }
		public StringWriter CsWriter { get { return _csWriter; } }
		public StringWriter MainWriter { get { return _mainWriter; } }
		public string CHeaderFileName { get { return (_cHeaderFileName.LastIndexOf("/") >= 0 ? 
				                                              _cHeaderFileName.Substring(_cHeaderFileName.LastIndexOf("/") + 1) : 
				                                              _cHeaderFileName); } }
		public string CSourceFileName { get { return _cSourceFileName; } }
		public string CsFileName { get {return _csFileName; } }
		public string MainFileName { get { return _mainFileName; } }
		public bool Write { get { return _write; } set { _write = value; } }

		public void WriteFiles ()
		{
			if (!Write)
				return;

			if (_cHeaderString.ToString ().Length != 0)
				using (System.IO.StreamWriter cHeaderFile = new StreamWriter(_cHeaderFileName))
					cHeaderFile.Write (_cHeaderString.ToString ());

			if(_cSourceString.ToString().Length != 0)
				using(System.IO.StreamWriter cSourceFile = new StreamWriter(_cSourceFileName))
					cSourceFile.Write (_cSourceString.ToString());

			if(_csString.ToString().Length != 0)
				using(System.IO.StreamWriter csFile = new StreamWriter(_csFileName))
					csFile.Write (_csString.ToString());
			
			if(_mainString.ToString().Length != 0)
				using(System.IO.StreamWriter mainFile = new StreamWriter(_mainFileName))
					mainFile.Write (_mainString.ToString());
		}
		
		string _id;
		string _fileName;
		bool _write;

		string _outDir;
		
		// c header file
		string _cHeaderFileName;
		StringWriter _cHeaderWriter;
		StringBuilder _cHeaderString;

		// c source file
		string _cSourceFileName;
		StringWriter _cSourceWriter;
		StringBuilder _cSourceString;

		// cs
		string _csFileName;
		StringWriter _csWriter;
		StringBuilder _csString;
		
		// main
		string _mainFileName;
		StringWriter _mainWriter;
		StringBuilder _mainString;
	}
}