﻿using System;
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
	public class StreamObservable : MainClass
	{
		public async Task Stream(){
			
		}

		public CoreTweet.Tokens TokenCreate(TwitterID IDs)
		{
			return CoreTweet.Tokens.Create (IDs?.APIKey, IDs?.APISecret, IDs?.AccessToken, IDs?.AccessTokenSecret);
		}
	}
}

