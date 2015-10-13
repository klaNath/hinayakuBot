using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace hinayakuBotV2
{
	public partial class MainClass
	{


		public static void Main (string[] args)
		{
			"Wake up, HinayakuBot!".COut ();

			List<Task> Tasks = new List<Task> ();
			var CommandContext = new CommandContext ();
			Tasks.Add(Task.Run(() =>  StreamObservable.StreamStart(CommandContext)));
			Tasks.Add(Task.Run (() => AILogic.AI (CommandContext)));
			System.Threading.Thread.Yield ();
			Task.WhenAll (Tasks).Wait ();

			"All Done".COut ();
		}
	}

	public partial class CommandContext
	{

		private Subject<Dictionary<string,string>> _Command = new Subject<Dictionary<string, string>>();
		public Dictionary<string,string> Command
		{
			set { _Command.OnNext (value);}
		}

		public Subject<Dictionary<string,string>> GetCommand
		{
			get { return _Command;}
		}

		public CommandContext()
		{
		}

	}
}
