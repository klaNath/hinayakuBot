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
using CoreTweet;
using CoreTweet.Streaming;
using CoreTweet.Core;
using System.IO;
using System.Reactive.Linq;
using CoreTweet.Streaming.Reactive;


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
				if(x.Keys.Any(y => y == Constant.Cmd)) x[Constant.Cmd].COut();
				else if(x.Keys.Any(y => y == Constant.Cmd)&& x[Constant.Cmd] == Constant.CmdEnd) ShutDownFlag = true ;
			});


			var IDs = GetIdFromXml ();
			var Token = TokenCreate (IDs);




			var stream = Token.Streaming.UserAsObservable()
				.Timeout (TimeSpan.FromSeconds (30))
				.Retry (5)
				.Catch((Exception e) => {
					Console.WriteLine(e.Message);
					if(e.StackTrace != null) Console.WriteLine(e.StackTrace);
					return Observable.Never<StatusMessage>();
				})					
				.Publish ();

			stream
				.OfType<StatusMessage>()
				.Where (x => !x.Status.User.ScreenName.Contains (@"hinayakuBot"))
				.Select (x => new TwString{Name = x.Status.User.ScreenName, Text = x.Status.Text, Id = x.Status.Id})
				.Subscribe (x => Console.WriteLine(x.Text));


			while(true){
				if (ShutDownFlag == true){
					Task.Delay (TimeSpan.FromSeconds (15)).Wait ();
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
					context.Command = new Dictionary<string, string>{ { Constant.Cmd,Constant.CmdEnd} };
			}
		}

		static TwitterID GetIdFromXml(){
			try{
				var ser = new System.Xml.Serialization.XmlSerializer(typeof(TwitterID));
				string path = $"{Directory.GetCurrentDirectory()}{System.IO.Path.DirectorySeparatorChar}TwitterIDs.xml";
				using (var fs = new System.IO.FileStream (path, FileMode.Open)) 
				{
					return (TwitterID)ser.Deserialize (fs);
				}
			}
			catch(Exception ex){
				Console.WriteLine (ex.Message);
				return null;
			}
		}

		static Tokens TokenCreate(TwitterID IDs)
		{
			try{
				return CoreTweet.Tokens.Create (IDs?.APIKey, IDs?.APISecret, IDs?.AccessToken, IDs?.AccessTokenSecret);
			}
			catch(Exception ex){
				Console.WriteLine (ex.Message);
				return null;
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

		private Subject<TwString> _Status = new Subject<TwString>();
		public TwString Status
		{
			set { _Status.OnNext (value);}
		}

		public Subject<TwString> GetStatus
		{
			get { return _Status;}
		}

		public StatusContext()
		{
		}

	}
}
