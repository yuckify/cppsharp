using System;


namespace cppsharp
{
	public class PlatformInfo
	{
		public enum ArchType
		{
			Empty,
			x86,
			x64
		}

		public enum OsType
		{
			Windows,
			Unix
		}

		public PlatformInfo()
		{
			_arch = ArchType.Empty;
		}

		public PlatformInfo(ArchType archtype)
		{
			_arch = archtype;
		}

		public OsType Os
		{
			get {
				OperatingSystem os = Environment.OSVersion;
				PlatformID     pid = os.Platform;
				switch (pid) 
				{
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
					return OsType.Windows;
				case PlatformID.Unix:
					return OsType.Unix;
				default:
					throw new System.AggregateException("could not identify os");
				}
			}
		}

		public ArchType Arch
		{
			get
			{
				if(_arch != ArchType.Empty)
					return _arch;
				else if(System.Environment.Is64BitOperatingSystem)
					return ArchType.x64;
				else
					return ArchType.x86;
			}
		}

		ArchType _arch;

	} // class PlatformType
}

