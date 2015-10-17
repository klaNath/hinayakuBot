﻿//
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoreTweet;
using Newtonsoft.Json;
using System.Reactive.Linq;
using System.IO;
using System.Net;
using System.Xml;
using System.Dynamic;
using System.Reactive;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;
using Codeplex;
using Microsoft.CSharp;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Reactive.PlatformServices;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace hinayakuBotV2
{
	public static class StreamObservable
	{
		public static async Task StreamStart(CommandContext C,StatusContext S){
			
			var IDs = GetIdFromXml ();
			if (IDs == null) {
				Console.WriteLine ("Xml dose not parse. Close.");
				C.Command = new Dictionary<string,string> (){ { Constant.Cmd,Constant.CmdEnd } };
				return;
			}
			bool RetryFlag = false;
			var context = C.GetCommand;
			var StatusContext = S.GetStatus;
			var Token = TokenCreate (IDs);
			C.Command = new Dictionary<string,string> (){ { Constant.Cmd,Constant.CmdStart } };


			var botstatus = Token.Users.Show (Constant.hinayakuBotUserId);

			var stats = new BotStatus () {
				Name = botstatus.Name,
				Location = botstatus.Location,
				Description = botstatus.Description,
				Url = botstatus.Url
			};

			try{
				var latesthinayaku = Token.Statuses.UserTimeline(Constant.hinayakuBotUserId,10);
				Console.WriteLine(latesthinayaku.First().Text);
				await Token.Statuses.UpdateAsync(".@hinayaku おはようひなやく",latesthinayaku.First().Id);
			}
			catch(Exception ex){
				Console.WriteLine (ex.Message);
			}
			while(true){
				RetryFlag = false;
				var stream = Token.Streaming.UserAsObservable()
					.OfType<StatusMessage>()
					.Timeout(TimeSpan.FromMinutes(30))
					.Where(x => !x.Status.User.ScreenName.Contains(@"hinayakuBot"))
					.Retry(5)
					.Catch((Exception e) => {
						Console.WriteLine(e.Message);
						e.StackTrace?.COut();
						return Observable.Never<StatusMessage>();
					})					
					.Publish();

				stream.OfType<Error> ()
					.Subscribe (x => Console.WriteLine(x.Message),z => Console.WriteLine(z.Message));





				stream
					.Where (x => !(x.Status.IsRetweeted.HasValue && x.Status.IsRetweeted.Value))
					.Subscribe (StatusContext.OnNext);
				
				while(RetryFlag != true){
					context
						.Where (x => x.Keys.Any (y => y == Constant.Cmd))
						.Subscribe (async x => {
							if(x[Constant.Cmd] == Constant.CmdReBorn)RetryFlag = true;
							else if(x[Constant.Cmd] == Constant.CmdEnd){
								C.Command = new Dictionary<string, string>(){{Constant.Cmd,Constant.CmdAck}};
								return;
							}
							else if(x[Constant.Cmd] == Constant.CmdSuicide){
								stream.Connect().Dispose();
								await Token.Statuses.UpdateAsync($"Suicide. Bye @{x[Constant.TwName]}",long.Parse(x[Constant.TwId]));
							}
							else if(x[Constant.Cmd] == Constant.CmdTweet){
								await Token.Statuses.UpdateAsync(x[Constant.TwText],long.Parse(x[Constant.TwId]));
							}
						});
				}
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

		public static Tokens TokenCreate(TwitterID IDs)
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
}

