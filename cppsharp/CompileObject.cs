using System;

namespace cppsharp
{
	// TODO reimplement this class as interface
	public abstract class CompileObject
	{
		public CompileObject ()
		{
		}

		// TODO delete this
		public abstract string DebugTag { get; }

		public abstract Arg Args { get; set; }

	}
}

