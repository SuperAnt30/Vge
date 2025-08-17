using System;
using System.Collections.Generic;
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
    private readonly static Dictionary<string, string> listGui = new Dictionary<string, string>();
    /// <summary>
    /// Массив всех серверных cлов
    /// </summary>
    private readonly static Dictionary<string, string> listServer = new Dictionary<string, string>();
    /// <summary>
    /// Массив всех cлов предметов
    /// </summary>
    private readonly static Dictionary<string, string> listItems = new Dictionary<string, string>();

    /// <summary>
    /// Выбрать язык
    /// </summary>
    public static void SetLanguage(string strGui, string strServer, string strItems)
    {
        FillList(strGui, listGui);
        FillList(strServer, listServer);
        FillList(strItems, listItems);
    }

    /// <summary>
    /// Заполнить массив словами тикущего языка
    /// </summary>
    private static void FillList(string strAll, Dictionary<string, string> list)
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
    public static string T(string key) => listGui.ContainsKey(key) ? listGui[key] : key;
    /// <summary>
    /// Получить строку перевода для GUI
    /// </summary>
    public static string T(string key, params object[] args) => string.Format(T(key), args);
    
    /// <summary>
    /// Получить строку перевода для Server
    /// </summary>
    public static string S(string key) => listServer.ContainsKey(key) ? listServer[key] : key;
    /// <summary>
    /// Получить строку перевода для Server
    /// </summary>
    public static string S(string key, params object[] args) => string.Format(S(key), args);
    
    /// <summary>
    /// Получить строку перевода для Items
    /// </summary>
    public static string I(string key) => listItems.ContainsKey(key) ? listItems[key] : key;
    /// <summary>
    /// Получить строку перевода для Items
    /// </summary>
    public static string I(string key, params object[] args) => string.Format(I(key), args);
}
