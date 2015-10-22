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
	public class StreamObservable
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


			var botstatus = Token.Users.Show (Constant.hinayakuUserId);

			var stats = new BotStatus () {
				Name = botstatus.Name,
				Location = botstatus.Location,
				Description = botstatus.Description,
				Url = botstatus.Url
			};

			try{
				var latesthinayaku = Token.Statuses.UserTimeline(Constant.hinayakuUserId,10);
				Console.WriteLine(latesthinayaku.First().Text);
				await Token.Statuses.UpdateAsync(".@hinayaku おはようひなやく",latesthinayaku.First().Id);
			}
			catch(Exception ex){
				Console.WriteLine (ex.Message);
			}
			while(true){
				try{
					RetryFlag = false;
					var stream = Token.Streaming.UserAsObservable ()
						.OfType<StatusMessage> ()
						.Timeout (TimeSpan.FromSeconds (30))
						.Where (x => !x.Status.User.ScreenName.Contains (@"hinayakuBot"))
						.Retry (5)
						.Catch((Exception e) => {
							Console.WriteLine(e.Message);
							if(e.StackTrace != null) Console.WriteLine(e.StackTrace);
							return Observable.Never<StatusMessage>();
						})					
						.Publish ();

					stream
						.OfType<StatusMessage>()
						.Select (x => new TwString{Name = x.Status.User.ScreenName, Text = x.Status.Text, Id = x.Status.Id})
						.Subscribe (x => Console.WriteLine(x.Text));

					//				stream
					//					.Where (x => !(x.Status.IsRetweeted.HasValue && x.Status.IsRetweeted.Value))
					//					.Select (x => new TwString{Name = x.Status.User.ScreenName, Text = x.Status.Text, Id = x.Status.Id})
					//					.Subscribe (x => StatusContext.OnNext(x));

					var ApiLimit = await Token.Application.RateLimitStatusAsync ();
					foreach(var rateLimit in ApiLimit["statuses"])
					{
						Console.WriteLine("{0}: {1} {2}",
							rateLimit.Key, rateLimit.Value.Remaining, 
							rateLimit.Value.Limit);
						Console.WriteLine("---------");
					}

					context
						.Where (x => x.Keys.Any (y => y == Constant.Cmd))
						.Subscribe (async x => {
							if(x[Constant.Cmd] == Constant.CmdReBorn)RetryFlag = true;
							else if(x[Constant.Cmd] == Constant.CmdEnd){
								//C.Command = new Dictionary<string, string>(){{Constant.Cmd,Constant.CmdAck}};
								await SayonaraHinayakuAsync(Token);
								return;
							}
							else if(x[Constant.Cmd] == Constant.CmdSuicide){
								await Token.Statuses.UpdateAsync($"Stop By @{x[Constant.TwName]}",long.Parse(x[Constant.TwId]));
							}
							else if(x[Constant.Cmd] == Constant.CmdTweet){
								await Token.Statuses.UpdateAsync(x[Constant.TwText],long.Parse(x[Constant.TwId]));
							}
						});

					while(RetryFlag != true){
						await Task.Delay (TimeSpan.FromSeconds (1));
					}
				}catch(Exception ex){
					ex.Message.COut ();
				}
				
			}


		}

		static async Task SayonaraHinayakuAsync (Tokens token)
		{
			try{
				var latesthinayaku = token.Statuses.UserTimeline(Constant.hinayakuUserId,10);
				Console.WriteLine(latesthinayaku.First().Text);
				await token.Statuses.UpdateAsync(".@hinayaku さよならひなやく",latesthinayaku.First().Id);
			}
			catch(Exception ex){
				Console.WriteLine(ex.Message);
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

