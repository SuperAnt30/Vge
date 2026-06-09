using System;
using System.Collections.Generic;
using System.IO;
using Vge.Util;

/// <summary>
/// Объект отвечающий за перевод языка на сервере
/// Language. Чтоб быстро было L.T() || L.S() || L.I()
/// </summary>
public sealed class L
{
    /// <summary>
    /// Массив всех клиентский слов, GUI и подобное
    /// </summary>
    private readonly static Dictionary<string, string> _listGui = new Dictionary<string, string>();
    /// <summary>
    /// Массив всех серверных cлов
    /// </summary>
    private readonly static Dictionary<string, string> _listServer = new Dictionary<string, string>();
    /// <summary>
    /// Массив всех cлов предметов
    /// </summary>
    private readonly static Dictionary<string, string> _listItems = new Dictionary<string, string>();

    /// <summary>
    /// Обновить теста языка
    /// </summary>
    public static void UpdateLanguage()
    {
        string postfix = Options.Language;
        string strGui = _LoadFileText(Options.PathTexts + postfix + ".txt");
        string strServer = _LoadFileText(Options.PathTexts + postfix + "Server.txt");
        string strItems = _LoadFileText(Options.PathTexts + postfix + "Items.txt");
        L.SetLanguage(strGui, strServer, strItems);
    }

    private static string _LoadFileText(string fileName)
    {
        if (File.Exists(fileName))
        {
            // Читаем весь текст
            return File.ReadAllText(fileName);
        }
        else
        {
            throw new Exception(Sr.GetString(Sr.TheTextFileIsMissing, fileName));
        }
    }

    /// <summary>
    /// Выбрать язык
    /// </summary>
    public static void SetLanguage(string strGui, string strServer, string strItems)
    {
        _FillList(strGui, _listGui);
        _FillList(strServer, _listServer);
        _FillList(strItems, _listItems);

        // Предметам присвоение языка
        Ce.Items?.InitLanguages();
    }

    /// <summary>
    /// Заполнить массив словами тикущего языка
    /// </summary>
    private static void _FillList(string strAll, Dictionary<string, string> list)
    {
        string[] stringSeparators = new string[] { Ce.Br };
        string[] strs = strAll.Split(stringSeparators, StringSplitOptions.None);
        list.Clear();
        foreach (string strLine in strs)
        {
            // комментарий
            if (Sundry.ChekComment(strLine)) continue;
            // Разделитель ключа и текста
            int index = strLine.IndexOf(":");
            if (index > 0)
            {
                string key = strLine.Substring(0, index);
                if (!list.ContainsKey(key))
                {
                    list.Add(strLine.Substring(0, index), strLine.Substring(index + 1));
                }
            }
        }
    }

    /// <summary>
    /// Получить строку перевода для GUI
    /// </summary>
    public static string T(string key) => _listGui.ContainsKey(key) ? _listGui[key] : key;
    /// <summary>
    /// Получить строку перевода для GUI
    /// </summary>
    public static string T(string key, params object[] args) => string.Format(T(key), args);
    
    /// <summary>
    /// Получить строку перевода для Server
    /// </summary>
    public static string S(string key) => _listServer.ContainsKey(key) ? _listServer[key] : key;
    /// <summary>
    /// Получить строку перевода для Server
    /// </summary>
    public static string S(string key, params object[] args) => string.Format(S(key), args);
    
    /// <summary>
    /// Получить строку перевода для Items
    /// </summary>
    public static string I(string key) => _listItems.ContainsKey(key) ? _listItems[key] : key;
    /// <summary>
    /// Получить строку перевода для Items
    /// </summary>
    //public static string I(string key, params object[] args) => string.Format(I(key), args);
}
