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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace hinayakuBotV2
{
	public static class WeatherTweet
	{

		public static readonly string NagaokaID = "150020";
		public static readonly string KanagawaID = "130010";
		public static readonly string WeatherUrl = "http://weather.livedoor.com/forecast/webservice/json/v1?city=";
		public static readonly string NO_VALUE = "---";

		public static async Task<Dictionary<string,string>> WeatherHinayakuAsync(TwString x){
			
			Console.WriteLine (x.Text + x.Id);
			var req = WebRequest.Create (WeatherUrl + KanagawaID);
			using (var res = await req.GetResponseAsync())
			using (var s = res.GetResponseStream())
			{

				dynamic json = Codeplex.Data.DynamicJson.Parse(s);

				//天気(今日)
				dynamic today = json.forecasts[0];
				if(today.temperature.max == null) today = json.forecast[1];
				//string dateLabel = today.dateLabel;
				string date = today.date;
				string telop = today.telop;



				var sbTempMax = new StringBuilder();

				dynamic todayTemperatureMax = today.temperature.max;
				if (todayTemperatureMax != null)
				{
					sbTempMax.AppendFormat("{0}℃", todayTemperatureMax.celsius);
				}
				else
				{
					sbTempMax.Append(NO_VALUE);
				}

				var sbTempMin = new StringBuilder();
				dynamic todayTemperatureMin = today.temperature.min;
				if (todayTemperatureMin != null)
				{
					sbTempMin.AppendFormat("{0}℃", todayTemperatureMin.celsius);
				}
				else
				{
					sbTempMin.Append(NO_VALUE);
				}

				//天気概況文
				var situation = json.description.text;

				//Copyright
				var link = json.copyright.link;
				var title = json.copyright.title;
				string text = $"@{x.Name} {date}の天気予報は\n最高気温:{sbTempMax.ToString()} 最低気温:{sbTempMin.ToString()}\n天気は{telop}らしいぞ";

				Console.WriteLine(string.Format("{0}\n天気 {1}\n最高気温 {2}\n最低気温 {3}\n\n{4}\n\n{5}\n{6}",
					date,
					telop,
					sbTempMax.ToString(),
					sbTempMin.ToString(),
					situation,
					link,
					title
				));

				return new Dictionary<string,string>(){
					{Constant.TwText,text},
					{Constant.TwId,x.Id.ToString()},
					{Constant.Cmd, Constant.CmdTweet}
				};
			}
		}
	}
}

