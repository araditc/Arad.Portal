using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public static class Ddate
    {
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

        public static string ToPersianFullDateTime(this DateTime d)
        {
            //d = d.ToLocalTime();?????
            var newDateObject = new PersianCalendar();
            var convertedDate = newDateObject.GetYear(d).ToString("0000") + "/" +
                                newDateObject.GetMonth(d).ToString("00") + "/" +
                                newDateObject.GetDayOfMonth(d).ToString("00");
            var hour = newDateObject.GetHour(d).ToString("00");
            var minutes = newDateObject.GetMinute(d).ToString("00");
            //var amPm = int.Parse(hour) >= 0 && int.Parse(hour) < 12 ? "ق.ظ" : "ب.ظ";
            var time = $"{hour}:{minutes}";


            string result = $"{convertedDate} {time}";

            return result;
        }

        public static string ToPersianLetDdate(this DateTime d)
        {
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
                var daytttt = d.ReplaceNumberEn().Split('/');
                var year = Convert.ToInt32(daytttt[0]);
                var month = Convert.ToInt32(daytttt[1]);
                var day = Convert.ToInt32(daytttt[2]);

                DateTime dt = new DateTime(year, month, day, new PersianCalendar());
                return dt;
            }
            catch (Exception e)
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime ToEnglishDateTime(this string d)
        {
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
            var newDateObject = new PersianCalendar();
            var convertedDate = newDateObject.GetYear(d).ToString("00") +
                                newDateObject.GetMonth(d).ToString("00") +
                                newDateObject.GetDayOfMonth(d).ToString("00");
            return convertedDate.Substring(2);
        }

        public static string ReplaceNumberEn(this string str)
        {
            // Arabic
            str = str.Replace('٠', '0');
            str = str.Replace('١', '1');
            str = str.Replace('٢', '2');
            str = str.Replace('٣', '3');
            str = str.Replace('٤', '4');
            str = str.Replace('٥', '5');
            str = str.Replace('٦', '6');
            str = str.Replace('٧', '7');
            str = str.Replace('٨', '8');
            str = str.Replace('٩', '9');

            // Persian
            str = str.Replace('۰', '0');
            str = str.Replace('۱', '1');
            str = str.Replace('۲', '2');
            str = str.Replace('۳', '3');
            str = str.Replace('۴', '4');
            str = str.Replace('۵', '5');
            str = str.Replace('۶', '6');
            str = str.Replace('۷', '7');
            str = str.Replace('۸', '8');
            str = str.Replace('۹', '9');

            return str;
        }
    }
}
