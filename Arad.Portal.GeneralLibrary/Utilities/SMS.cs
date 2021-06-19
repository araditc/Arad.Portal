// --------------------------------------------------------------------
// Copyright (c) 2005-2021 Arad ITC.
//
// Author : Ammar Heidari <ammar@arad-itc.org>
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0 
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// --------------------------------------------------------------------

using System;
using System.Net;
using System.Text.RegularExpressions;


namespace Arad.Portal.GeneralLibrary.Utilities
{
    public static class SMS
    {
		public static string GetStandardizeCharacters(string inputString)
		{
			return GetStandardizeCharacters(inputString, "persian");
		}

		public static string GetStandardizeCharacters(string inputString, string persianKeyboardBehavior)
		{
			string outputString = inputString.Replace("'", "`").Replace("ی", "ي");
			// This is the third shape of charachter 'ی'.

			if (persianKeyboardBehavior == "arabic")
				outputString = outputString.Replace('ى', 'ي').Replace('ک', 'ك');
			else
				outputString = outputString.Replace('ي', 'ی').Replace('ك', 'ک');
			return outputString;
		}
		public static bool HasUniCodeCharacter(string text)
		{
			return Regex.IsMatch(text, "[^\u0000-\u00ff]");
		}

		public static int GetSmsCount(string text)
		{
			int standardSmsLen = 160;
			int standardUdhLen = 7;

			if (HasUniCodeCharacter(text))
			{
				standardSmsLen = 70;
				standardUdhLen = 4;
			}

			double smsLen = text.Replace("\r\n", "\n").Length;
			double smsCount = 0;

			if (smsLen > standardSmsLen)
				smsCount = Math.Ceiling(smsLen / (standardSmsLen - standardUdhLen));
			else
				smsCount = Math.Ceiling(smsLen / standardSmsLen);

			return (int)smsCount;
		}

		public static string GetLocalPrivateNumber(string number)
		{
			if (number.StartsWith("098") || number.StartsWith("+98"))
				return number.Substring(3);
			else if (number.StartsWith("0098"))
				return number.Substring(4);
			else if (number.StartsWith("98"))
				return number.Substring(2);
			else
				return number;
		}

		public static bool CheckServiceAvailable(string url)
		{
			try
			{
				WebClient client = new WebClient();
				var request = (HttpWebRequest)WebRequest.Create(url);
				var response = (HttpWebResponse)request.GetResponse();
				if (response.StatusCode == HttpStatusCode.OK)
					return true;
				else
					return false;
			}
			catch (Exception ex)
			{
				//LogController<>.LogInFile(string.Format("{0} ===>> Error : {1}", "CheckServiceAvailable", ex.Message));
				return false;
			}
		}
	}
}
