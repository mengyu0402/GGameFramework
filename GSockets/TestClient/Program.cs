using System;

namespace TestClient
{
	public class TestClass
	{
		string name = "hello";

		public TestClass(string s)
		{
			name = s;
		}
		public void TestRun()
		{
			Console.WriteLine($"{name} world");
		}
	}


	class MainClass
	{
		

		public static void Main(string[] args)
		{
			TestClass @class = null;
			@class = @class??new TestClass("cccc");
			TestClass @class1 = new TestClass("wwwww");

			@class?.TestRun();

			TestClass o = @class ?? class1;

			o?.TestRun();
			Console.WriteLine("Hello World!");
			Console.ReadKey();
		}
	}
}
