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

	public static class ExtensionMethods
	{
		public static void COut(this String s)
		{
			Console.WriteLine (s);
		}
	}
}

