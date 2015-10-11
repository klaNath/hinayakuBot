using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hinayakuBotV2
{
	class MainClass
	{

		public static readonly string TwSuicide = "Suicide";
		public static readonly string TwCommands = "Commands";
		public static readonly long hinayakuBotUserId = 3088293410;
		public static readonly long hinayakuUserId = 125017187;

		public static readonly string ScreenNameBot = "hinayakuBot";

		public static void Main (string[] args)
		{
			Console.WriteLine ("Wake Up, HinayakuBot!");
			List<Task> Tasks = new List<Task> ();
			var TwitterTask = Task.Run(() =>  Stream());
			var BotTask = Task.Run (() => AI ());
			System.Threading.Thread.Yield ();
			Task.WhenAll (Tasks);
		}
	}
}
