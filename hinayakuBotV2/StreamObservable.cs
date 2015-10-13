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
		public static async Task StreamStart(CommandContext C){
			
			var IDs = GetIdFromXml ();
			if (IDs == null) {
				Console.WriteLine ("Xml dose not parse. Close.");
				C.Command = new Dictionary<string,string> (){ { Constant.Cmd,Constant.CmdEnd } };
				return;
			}
			bool RetryFlag = false;
			var context = C.GetCommand;

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
					.Where(x => !(x.Status.IsRetweeted.HasValue && x.Status.IsRetweeted.Value) )
					.Select (x => new TwString{Name = x.Status.User.ScreenName, Text = x.Status.Text, Id = x.Status.Id})
					.Subscribe(x => 
						{
							Console.WriteLine(x.Name + "/n" + x.Text + "/n");
						},
						(Exception y) => Console.WriteLine(y.Message),
						() => 
						{
							Console.WriteLine(DateTime.Now + " : これはOnComplete at Stream");
						});

				stream
					.Where(y => y.Status.Text.Contains("Yo")
						&& y.Status.Text.Contains("hinayakuBot"))
					.Select(x => new{Text = x.Status.Text,Id = x.Status.Id, Name = x.Status.User.ScreenName})
					.Subscribe(x => Token.Statuses.Update($"@{x.Name} Yo",x.Id ),
						z => 
						{
							Console.WriteLine(z.Message);
						},
						() => 
						{
							Console.WriteLine(DateTime.Now + " : これはOnComplete at Yo");
						});
				while(RetryFlag != true){
					
					context
						.Where (x => x.Keys.Any (Constant.Cmd))
						.Subscribe (x => {
							if(x[Constant.Cmd] == Constant.CmdReBorn)RetryFlag = true;
							else if(x[Constant.Cmd] == Constant.CmdEnd){
								C.Command = new Dictionary<string, string>(){{Constant.Cmd,Constant.CmdAck}};
								return;
							}
							else if(x[Constant.Cmd] == Constant.CmdSuicide){
								stream.Connect().Dispose();
							}
						});
				}
			}


		}

		private static TwitterID GetIdFromXml(){
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

		public static CoreTweet.Tokens TokenCreate(TwitterID IDs)
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

