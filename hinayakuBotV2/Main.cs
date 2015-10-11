using System;
using System.Threading.Tasks;

namespace hinayakuBotV2
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Wake Up, HinayakuBot!");
			var TwitterTask = Task.Run(() => TwitterStream ());
			var BotTask = Task.Run (() => AI ());
			System.Threading.Thread.Yield ();
			t.Wait ();		
		}


	}
}
