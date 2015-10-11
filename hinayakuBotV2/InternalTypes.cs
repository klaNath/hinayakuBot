using System;
using System.Collections.Generic;

namespace hinayakuBotV2
{
	public class TwitterID
	{
		public string APIKey{ get; set; }
		public string APISecret{ get; set; }
		public string AccessToken{ get; set; }
		public string AccessTokenSecret{ get; set; }
	}

	class TwString{
		public string Name{ get; set; }
		public string Text{ get; set; }	
		public long Id{ get; set; }
		public Dictionary<string,string> Parameters{ get; set; }
	}

	class BotStatus
	{
		public string Name{ get; set; }
		public string Url{ get; set; }
		public string Location { get; set; }
		public string Description { get; set; }
	}
}

