//
//  --------------------------------------------------------------------
//  Copyright (c) 2005-2021 Arad ITC.
//
//  Author : Ammar Heidari <ammar@arad-itc.org>
//  Modified by : Somaye Azizi <azizi@arad-itc.org>
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

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
namespace Arad.Portal.GeneralLibrary.Utilities;

public class KeyVal
{
    public string Key { get; set; }

    public string Value { get; set; }
}
public abstract class UtilityLanguage
{
    #region---------- Constructors -------------------
    private UtilityLanguage()
    {
        AvailableNeutralCultures = CultureInfo.GetCultures(CultureTypes.AllCultures & CultureTypes.NeutralCultures).ToList()
                                              .Where(i => !string.IsNullOrEmpty(i.Name)).Select(i => i.Name).ToList();
    }
    static UtilityLanguage()
    {
        _activeLanguage = "en";
        AvailableNeutralCultures = CultureInfo.GetCultures(CultureTypes.AllCultures & CultureTypes.NeutralCultures).ToList()
                                              .Where(i=>!string.IsNullOrEmpty(i.Name)).Select(i=>i.Name).ToList();
    }
    #endregion

    public static List<string> AvailableNeutralCultures { get; set; }
      

    /// <summary>
    /// cultureInfo.Name
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private static string MapCultureInfo(CultureInfo info)
    {
        if(info.CultureTypes == CultureTypes.NeutralCultures)
        {
            return info.Name;
        }else
        {
            return info.Name.Split("-")[0];
        }
    }

    public static string HostingEnvironment;

    private static string _activeLanguage;

    private static Hashtable _languageDictionary = null;

    private static object _fillDictionaryCacheLock = new object(); // Used to ensure only one thread can run FillDictionaryCache




    private static void FillDictionaryCache()
    {
        lock (_fillDictionaryCacheLock)
        {
            if (_languageDictionary != null && _languageDictionary.Count > 0)
                return;

            Hashtable dictionaryCache = new Hashtable();

            string jsonPath = Path.Combine(HostingEnvironment, "Dictionaries");

            foreach (string lan in AvailableNeutralCultures)
            {
                Hashtable dictionary = new Hashtable();

                    
                LoadDictionaryHashtableFromFile(lan, jsonPath, ref dictionary);

                if (!dictionaryCache.ContainsKey(lan))
                    dictionaryCache.Add(lan, dictionary);
            }

            if (dictionaryCache.Count == 0)
                throw new Exception("Unable to load dictionary cache.");

            _languageDictionary = dictionaryCache;
        }
    }

    private static void LoadDictionaryHashtableFromFile(string languageCode, string dictionariesPath, ref Hashtable htDictionary)
    {
        string filePath = Path.Combine(dictionariesPath, languageCode + ".json");

        if (!System.IO.File.Exists(filePath))
        {
            filePath = Path.Combine(dictionariesPath, "en" + ".json");
        }
           
        string readResult = System.IO.File.ReadAllText(filePath);
        if(string.IsNullOrWhiteSpace(readResult))
        {
            return;
        }
        List<KeyVal> data = JsonConvert.DeserializeObject<List<KeyVal>>(readResult);

        try
        {
            foreach (KeyVal word in data)
            {
                htDictionary.Add(word.Key, word.Value);
            }
        }
        catch (ArgumentException ex)
        {
            throw new Exception("Duplicate key found in JSON dictionary files. " + ex.Message, ex);
        }
    }

    public static void ReloadDictionary()
    {
        _languageDictionary = null;
        FillDictionaryCache();
    }

    public static string ActiveLanguage
    {
        set
        {
            _activeLanguage = MapCultureInfo(CultureInfo.CurrentCulture);

        }
        get
        {
            return _activeLanguage;

        }
    }

    public static string GetString(string keyword)
    {
        _activeLanguage = MapCultureInfo(CultureInfo.CurrentCulture);
        string languageCode = ActiveLanguage;

        if (keyword == null)
            return string.Empty;

        if (_languageDictionary == null)
            FillDictionaryCache();

        Hashtable dictionary = (Hashtable)_languageDictionary[languageCode];

        if (dictionary == null) // This situation seems to be impossible, but it happen sometimes.
        {
            _languageDictionary = null;

            FillDictionaryCache();

            dictionary = (Hashtable)_languageDictionary[languageCode];
        }

        if (dictionary.Contains(keyword))
            return Utilities.SMS.GetStandardizeCharacters((string)dictionary[keyword]);
        else
            return keyword;
    }
}