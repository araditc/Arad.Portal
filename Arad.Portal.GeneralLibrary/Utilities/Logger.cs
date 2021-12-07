//
//  --------------------------------------------------------------------
//  Copyright (c) 2005-2021 Arad ITC.
//
//  Author : Ammar Heidari <ammar@arad-itc.org>
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0 
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  --------------------------------------------------------------------
//                if (Logger.LogFlag)
//                {
//                    Logger.WriteLogFile(ServiceName, logPath);
//                }

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;

namespace Arad.Portal.GeneralLibrary.Utilities
{
    public static class Logger
    {
        public static ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();
        public static bool LogFlag = true;
        public static string LogFileName = "";

        public static void WriteLogFile(string ServiceName, string logPath)
        {
            //log = log + ServiceName;
            LogFlag = false;

            try
            {
                if (LogQueue.Count > 0)
                {
                    string l = "";
                    while (LogQueue.TryDequeue(out l))
                    {
                        using (StreamWriter sw =
                            new StreamWriter(string.Format("{0}_{1:yyyyMMdd}.txt", logPath + ServiceName, DateTime.Now), true))
                        {
                            sw.WriteLine(l);
                        }
                    }
                }
            }
            catch
            {

            }
            LogFlag = true;
        } //WriteLogFile()

        public static void WriteLogFile(string uMessage)
        {
            try
            {
                LogQueue.Enqueue(string.Format("{0:HH:mm:ss.fff}\t{1}",
                    DateTime.Now, uMessage.Replace("\r", "\\r").Replace("\n", "\\n")));
            }
            catch
            {

            }
        } //WriteLogFile(string uMessage)


        public static string ReplaceInvalidChars(string uText)
        {
            string ret = "";

            try
            {

                for (int i = 0; i < uText.Length; i++)
                {
                    int c = (int)uText[i];
                    if (c < 32 && c != 10)
                    {
                        ret += (char)32;
                    }
                    else
                    {
                        ret += (char)c;
                    }
                }
            }
            catch
            {
            }
            return ret;
        } //ReplaceInvalidChars

        public static int GetSmsLength(string message, bool farsi)
        {
            try
            {
                if (farsi)
                    if (message.Length <= 70)
                        return 1;
                    else
                        return (message.Length / 66) + ((message.Length % 66 == 0) ? 0 : 1);
                else
                if (message.Length <= 160)
                    return 1;
                else
                    return (message.Length / 152) + ((message.Length % 152 == 0) ? 0 : 1);

            }
            catch
            {

            }
            return 1;
        } //GetSmsLength

        public static bool HasUniCodeCharacter(string text)
        {
            return Regex.IsMatch(text, "[^\u0000-\u00ff]");
        }
    }
}
