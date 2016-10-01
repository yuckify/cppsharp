using System;

namespace Work
{
	public class CallbackTest
	{
		public CallbackTest()
		{}
		
		public void OnCreate()
		{
			Console.WriteLine("\tOnCreate");
		}
		
		public void OnUpdate()
		{
			Console.WriteLine("\tOnUpdate");
		}
	}
}