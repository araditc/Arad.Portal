//
//  --------------------------------------------------------------------
//  Copyright (c) 2005-2021 Arad ITC.
//
//  Author : Davood Ghashghaei <ghashghaei@arad-itc.org>
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

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Arad.Portal.GeneralLibrary.Utilities
{
    public static class DateHelper
    {
        /// <summary>
        /// Converts DateTime to unix milliseconds
        /// </summary>
        public static long ToUnixMilliseconds(this DateTime date)
        {
            TimeSpan span = date - new DateTime(1970, 1, 1);
            return Convert.ToInt64(span.TotalMilliseconds);
        }

        /// <summary>
        /// Returns total milliseconds between UTC now and the date
        /// </summary>
        public static long LifetimeMilliseconds(this DateTime date)
        {
            return Convert.ToInt64((DateTime.UtcNow - date).TotalMilliseconds);
        }

        public static string GetEnglishDatesForPersianStartAndEndDatesOfCurrentMonth()
        {
            var date = DateTime.Now;
            var newDateObject = new PersianCalendar();
            var persianDayOfMonth = newDateObject.GetDayOfMonth(date);
            var thisMonth = newDateObject.GetMonth(date);
            var thisYear = newDateObject.GetYear(date);
            var startDate = date.Subtract(new TimeSpan(persianDayOfMonth - 1, 0, 0, 0));
            var daysOfThisMonth = newDateObject.GetDaysInMonth(thisYear, thisMonth);
            var endDate = date.AddDays(daysOfThisMonth - persianDayOfMonth);

            return $"{startDate}%{endDate}";
        }



        public static string ToPersianVeryFullDateTime(this DateTime d)
        {

            if (d == DateTime.MinValue)
            {
                return string.Empty;
            }

            d = d.ToLocalTime();
            var newDateObject = new PersianCalendar();
            var convertedDate = newDateObject.GetYear(d).ToString("0000") + "/" +
                                newDateObject.GetMonth(d).ToString("00") + "/" +
                                newDateObject.GetDayOfMonth(d).ToString("00");
            var hour = newDateObject.GetHour(d).ToString("00");
            var minutes = newDateObject.GetMinute(d).ToString("00");
            var amPm = int.Parse(hour) >= 0 && int.Parse(hour) < 12 ? "ق.ظ" : "ب.ظ";
            var time = $"{hour}:{minutes} {amPm}";


            string result = $"{convertedDate} ساعت {time}";

            return result;
        }


        public static string ToPersianLetDateTime(this DateTime d)
        {
            if (d == DateTime.MinValue)
            {
                return string.Empty;
            }

            d = d.ToLocalTime();
            string perYear = string.Empty;
            string perMonth = string.Empty;
            string perDay = string.Empty;
            string perDayOfWeek = string.Empty;
            var newDateObject = new PersianCalendar();
            var year = newDateObject.GetYear(d).ToString("0000");
            var month = newDateObject.GetMonth(d).ToString("00");
            var day = newDateObject.GetDayOfMonth(d).ToString("00");
            var hour = newDateObject.GetHour(d).ToString("00");
            var mins = newDateObject.GetMinute(d).ToString("00");
            if (day.StartsWith("0"))
            {
                day = day.Substring(1);
            }
            var dayOfWeek = newDateObject.GetDayOfWeek(d).ToString();

            switch (dayOfWeek)
            {
                case "Saturday":
                    perDayOfWeek = "شنبه";
                    break;

                case "Sunday":
                    perDayOfWeek = "یکشنبه";
                    break;

                case "Monday":
                    perDayOfWeek = "دوشنبه";
                    break;

                case "Tuesday":
                    perDayOfWeek = "سه شنبه";
                    break;

                case "Wednesday":
                    perDayOfWeek = "چهار شنبه";
                    break;

                case "Thursday":
                    perDayOfWeek = "پنج شنبه";
                    break;

                case "Friday":
                    perDayOfWeek = "جمعه";
                    break;
            }


            switch (month)
            {
                case "01":
                    perMonth = "فروردین";
                    break;

                case "02":
                    perMonth = "اردیبهشت";
                    break;

                case "03":
                    perMonth = "خرداد";
                    break;

                case "04":
                    perMonth = "تیر";
                    break;

                case "05":
                    perMonth = "مرداد";
                    break;

                case "06":
                    perMonth = "شهریور";
                    break;

                case "07":
                    perMonth = "مهر";
                    break;

                case "08":
                    perMonth = "آبان";
                    break;

                case "09":
                    perMonth = "آذر";
                    break;

                case "10":
                    perMonth = "دی";
                    break;

                case "11":
                    perMonth = "بهمن";
                    break;

                case "12":
                    perMonth = "اسفند";
                    break;
            }

            perDay = day.ToPersianString();
            perYear = year.ToPersianString();

            string result = $"{perDayOfWeek} ، {perDay} {perMonth} {perYear} ، ساعت {hour}:{mins}";

            return result;
        }


        public static string ToPersianLetDateTimeWithSeconds(this DateTime d)
        {
            if (d == DateTime.MinValue)
            {
                return string.Empty;
            }

            d = d.ToLocalTime();
            string perYear = string.Empty;
            string perMonth = string.Empty;
            string perDay = string.Empty;
            string perDayOfWeek = string.Empty;
            var newDateObject = new PersianCalendar();
            var year = newDateObject.GetYear(d).ToString("0000");
            var month = newDateObject.GetMonth(d).ToString("00");
            var day = newDateObject.GetDayOfMonth(d).ToString("00");
            var hour = newDateObject.GetHour(d).ToString("00");
            var mins = newDateObject.GetMinute(d).ToString("00");
            var secs = newDateObject.GetSecond(d).ToString("00");

            if (day.StartsWith("0"))
            {
                day = day.Substring(1);
            }
            var dayOfWeek = newDateObject.GetDayOfWeek(d).ToString();

            switch (dayOfWeek)
            {
                case "Saturday":
                    perDayOfWeek = "شنبه";
                    break;

                case "Sunday":
                    perDayOfWeek = "یکشنبه";
                    break;

                case "Monday":
                    perDayOfWeek = "دوشنبه";
                    break;

                case "Tuesday":
                    perDayOfWeek = "سه شنبه";
                    break;

                case "Wednesday":
                    perDayOfWeek = "چهارشنبه";
                    break;

                case "Thursday":
                    perDayOfWeek = "پنج شنبه";
                    break;

                case "Friday":
                    perDayOfWeek = "جمعه";
                    break;
            }


            switch (month)
            {
                case "01":
                    perMonth = "فروردین";
                    break;

                case "02":
                    perMonth = "اردیبهشت";
                    break;

                case "03":
                    perMonth = "خرداد";
                    break;

                case "04":
                    perMonth = "تیر";
                    break;

                case "05":
                    perMonth = "مرداد";
                    break;

                case "06":
                    perMonth = "شهریور";
                    break;

                case "07":
                    perMonth = "مهر";
                    break;

                case "08":
                    perMonth = "آبان";
                    break;

                case "09":
                    perMonth = "آذر";
                    break;

                case "10":
                    perMonth = "دی";
                    break;

                case "11":
                    perMonth = "بهمن";
                    break;

                case "12":
                    perMonth = "اسفند";
                    break;
            }

            perDay = day.ToPersianString();
            perYear = year.ToPersianString();

            string result = $"{perDayOfWeek} ، {perDay} {perMonth} {perYear} ، ساعت {hour}:{mins}:{secs}";

            return result;
        }

        public static string ToPersianLetDate(this DateTime d)
        {
            d = d.ToLocalTime();
            if (d == DateTime.MinValue)
            {
                return string.Empty;
            }

            string perYear = string.Empty;
            string perMonth = string.Empty;
            string perDay = string.Empty;
            string perDayOfWeek = string.Empty;


            var newDateObject = new PersianCalendar();
            var year = newDateObject.GetYear(d).ToString("0000");
            var month = newDateObject.GetMonth(d).ToString("00");
            var day = newDateObject.GetDayOfMonth(d).ToString("00");

            if (day.StartsWith("0"))
            {
                day = day.Substring(1);
            }
            var dayOfWeek = newDateObject.GetDayOfWeek(d).ToString();

            switch (dayOfWeek)
            {
                case "Saturday":
                    perDayOfWeek = "شنبه";
                    break;

                case "Sunday":
                    perDayOfWeek = "یکشنبه";
                    break;

                case "Monday":
                    perDayOfWeek = "دوشنبه";
                    break;

                case "Tuesday":
                    perDayOfWeek = "سه شنبه";
                    break;

                case "Wednesday":
                    perDayOfWeek = "چهار شنبه";
                    break;

                case "Thursday":
                    perDayOfWeek = "پنج شنبه";
                    break;

                case "Friday":
                    perDayOfWeek = "جمعه";
                    break;
            }


            switch (month)
            {
                case "01":
                    perMonth = "فروردین";
                    break;

                case "02":
                    perMonth = "اردیبهشت";
                    break;

                case "03":
                    perMonth = "خرداد";
                    break;

                case "04":
                    perMonth = "تیر";
                    break;

                case "05":
                    perMonth = "مرداد";
                    break;

                case "06":
                    perMonth = "شهریور";
                    break;

                case "07":
                    perMonth = "مهر";
                    break;

                case "08":
                    perMonth = "آبان";
                    break;

                case "09":
                    perMonth = "آذر";
                    break;

                case "10":
                    perMonth = "دی";
                    break;

                case "11":
                    perMonth = "بهمن";
                    break;

                case "12":
                    perMonth = "اسفند";
                    break;
            }

            perDay = day.ToPersianString();
            perYear = year.ToPersianString();

            string result = $"امروز {perDayOfWeek} ، {perDay} {perMonth} {perYear}";

            return result;
        }

        public static string ToPersianDdate(this DateTime d)
        {

            d = d.ToLocalTime();

            var newDateObject = new PersianCalendar();
            var convertedDate = newDateObject.GetYear(d).ToString("0000") + "/" +
                    newDateObject.GetMonth(d).ToString("00") + "/" +
                    newDateObject.GetDayOfMonth(d).ToString("00");



            return convertedDate;
        }

        public static DateTime ToEnglishDate(this string d)
        {
            try
            {
                d = d.ToEngishString();
                var daytttt = d.Split('/');
                var year = int.Parse(daytttt[0]);
                var month = int.Parse(daytttt[1]);
                var day = int.Parse(daytttt[2]);
                //var dt = DateTime.Parse(new DateTime(year, month, day, new PersianCalendar()).ToString(), null, DateTimeStyles.None);
                var dt = new DateTime(year, month, day, new PersianCalendar());


                return dt;
            }
            catch (Exception e)
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime ToEnglishDateTime(this string d)
        {
            d = d.ToEngishString();
            var daytttt = d.Split('/');
            var dayy = daytttt[2].Split(' ');
            var times = dayy[1].Split(':');
            var hour = Convert.ToInt32(times[0]);
            var mintute = Convert.ToInt32(times[1]);
            var second = Convert.ToInt32(times[2]);
            var year = int.Parse(daytttt[0]);
            var month = int.Parse(daytttt[1]);
            var day = int.Parse(dayy[0]);

            DateTime dt = new DateTime(year, month, day, hour, mintute, second, new PersianCalendar());
            return dt;
        }


        public static string ToPersianDdateNoBackSlash(this DateTime d)
        {
            d = d.ToLocalTime();


            var newDateObject = new PersianCalendar();
            var convertedDate = newDateObject.GetYear(d).ToString("00") +
                                newDateObject.GetMonth(d).ToString("00") +
                                newDateObject.GetDayOfMonth(d).ToString("00");
            return convertedDate.Substring(2);
        }
    }
}
