using System;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace cppsharp
{
	public class Attributes
	{
		public Attributes (XmlTextReader reader)
		{
			string id = reader["id"];
			_attr = reader["attributes"];
			_enabled = false;
			_export = false;
			_import = false;

			if(_attr == null) return;

			_export = _attr.Contains("gccxml(export)");
			_import = _attr.Contains("gccxml(import)");
			_nodtor = _attr.Contains ("gccxml(nodtor)");

			string regex = "gccxml\\((\\S*)\\)([\\s\\S]*)";
			string regexText = _attr;
			Match match = null;
			while((match = Regex.Match(regexText, regex)).Success)
			{
				Match propMatch = Regex.Match (match.Groups[1].ToString(), "(get|set),(\\S+)");
				string propType = propMatch.Groups[1].ToString();
				string propName = propMatch.Groups[2].ToString();
				switch(propType)
				{
				case "set":	_set = propName; _setId = id; break;
				case "get": _get = propName; _getId = id; break;
				}
				regexText = match.Groups[2].ToString();
			}
		}

		public Attributes(Attributes attr)
		{
			_enabled = attr._enabled;
			_getId = attr._getId;
			_setId = attr._setId;
			_attr = attr._attr;
			_nodtor = attr._nodtor;
			_export = attr._export;
			_import = attr._import;
			_get = attr._get;
			_set = attr._set;
		}

		public void validate(CompileObject obj, bool export, bool import, bool getter, bool setter, bool nodtor)
		{
			if(_export && !export)
			{
				Console.WriteLine (obj.DebugTag + " : cannot be exported");
				Environment.Exit(-1);
			}

			if(_import && !import)
			{
				Console.WriteLine (obj.DebugTag + " : cannot be imported");
				Environment.Exit(-1);
			}

			if(_nodtor && !nodtor)
			{
				Console.WriteLine (obj.DebugTag + " : only functions can be tagged with __nodtor");
				Environment.Exit(-1);
			}

			if((getter == false) & (_get != null))
			{
				Console.WriteLine (obj.DebugTag + " : cannot generate a get property");
				Environment.Exit(-1);
			}

			if((setter == false) & (_set != null))
			{
				Console.WriteLine (obj.DebugTag + " : cannot generate a set property");
				Environment.Exit(-1);
			}
		}

		public void merge(Attributes attr)
		{
			if(!_import) _import = attr.Import;
			if(!_export) _export = attr.Export;
			if(_get == null) 
			{
				_get = attr.Get;
				_getId = attr.GetId;
			}
			if(_set == null)
			{
				_set = attr.Set;
				_setId = attr.SetId;
			}
		}

		public bool Export { get { return _export; } set { _export = value; } }
		public bool Import { get { return _import; } }
		public bool NoDestructor { get { return _nodtor; } }
		public string Get { get { return _get; } }
		public string Set { get { return _set; } }
		public string Name { get {
				if(_set != null) return _set;
				return _get;
			}}
		public bool Enabled { get { return _enabled; } set { _enabled = value; } }
		public string GetId { get { return _getId; } set { _getId = value; } }
		public string SetId { get { return _setId; } set { _setId = value; } }

		bool _enabled;

		string _getId;
		string _setId;

		string _attr;

		bool _nodtor;
		bool _export;
		bool _import;
		string _get;
		string _set;
	}
}

