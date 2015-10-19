//
//  WeatherTweet.cs
//
//  Author:
//       kazusa Okuda <kazusa@klamath.jp>
//
//  Copyright (c) 2015 kazusa Okuda
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Reactive;
using System.Linq;
using CoreTweet.Streaming;

namespace hinayakuBotV2
{
	public partial class MainClass
	{


		static void Main (string[] args)
		{
			"Wake up, HinayakuBot!".COut ();
			bool ShutDownFlag = false;
			List<Task> Tasks = new List<Task> ();
			var CommandContext = new CommandContext ();
			var StatusContext = new StatusContext ();
			var cts = new System.Threading.CancellationTokenSource ();
			Tasks.Add(Task.Run(() =>  StreamObservable.StreamStart(CommandContext,StatusContext),cts.Token));
			Tasks.Add(Task.Run (() => AILogic.AI (CommandContext,StatusContext),cts.Token));
			Tasks.Add (Task.Run (() => UserInterface (CommandContext),cts.Token));
			System.Threading.Thread.Yield ();
			Task.WhenAll (Tasks).ContinueWith (x => ShutDownFlag = true);
			CommandContext.GetCommand.Subscribe (x => {
				if(x.Keys.Any(y => y == Constant.Cmd)&& x[Constant.Cmd] == Constant.CmdSuicide) ShutDownFlag = true ;
			});
			while(true){
				if (ShutDownFlag == true){
					cts.Cancel ();
					break;
				}
			}

			"All Done".COut ();
		}

		static Task UserInterface (CommandContext commandContext)
		{
			var context = commandContext;
			while(true){
				var key = Console.ReadLine ();
				if (key.ToUpper () == "Q")
					context.Command = new Dictionary<string, string>{ { Constant.Cmd,Constant.CmdSuicide } };
			}
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
	public partial class StatusContext
	{

		private Subject<StatusMessage> _Status = new Subject<StatusMessage>();
		public StatusMessage Status
		{
			set { _Status.OnNext (value);}
		}

		public Subject<StatusMessage> GetStatus
		{
			get { return _Status;}
		}

		public StatusContext()
		{
		}

	}
}
