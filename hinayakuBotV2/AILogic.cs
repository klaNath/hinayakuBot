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
	public class AILogic : MainClass
	{
		public static readonly string NagaokaID = "150020";
		public static readonly string KanagawaID = "130010";
		public static readonly string WeatherUrl = "http://weather.livedoor.com/forecast/webservice/json/v1?city=";
		public static readonly string NO_VALUE = "---";

		public static async Task AI(CommandContext C, StatusContext S){

			var stream = S.GetStatus.Publish();
			var context = C.GetCommand;

			stream
				.Where(y => y.Status.Text.Contains("Yo")
					&& y.Status.Text.Contains("hinayakuBot"))
				.Select(x => new{Text = x.Status.Text,Id = x.Status.Id, Name = x.Status.User.ScreenName})
				.Subscribe(x => 
					{
						var dict = new Dictionary<string,string>()
						{
							{Constant.TwText,"@{x.Name} Yo"},
							{Constant.TwId,x.Id.ToString()}
						};
						context.OnNext(dict);
					},
					z => 
					{
						Console.WriteLine(z.Message);
					},
					() => 
					{
						Console.WriteLine(DateTime.Now + " : これはOnComplete at Yo");
					});

			stream
				.Where(x => !(x.Status.IsRetweeted.HasValue && x.Status.IsRetweeted.Value) )
				.Select (x => new TwString{Name = x.Status.User.ScreenName, Text = x.Status.Text, Id = x.Status.Id})
				.Subscribe(x => 
					{
						Console.WriteLine(x.Name + "¥n" + x.Text + "¥n");
					},
					(Exception y) => Console.WriteLine(y.Message),
					() => 
					{
						Console.WriteLine(DateTime.Now + " : これはOnComplete at Stream");
					});


			stream.OfType<Error> ()
				.Subscribe (x => Console.WriteLine(x.Message),z => Console.WriteLine(z.Message));

			while(true){
				
			}
			
		}
	}
}

