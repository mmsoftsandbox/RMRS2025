using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RMRS.ConsoleApp.Helpers;

/// <summary>
/// Утилиты работы со строками
/// </summary>
public static class StringHelper
{

    public static string AsNotDefinedIfNull<T>(T value, string defaultText = "N/A")
    {
        return value == null ? defaultText : value.ToString() ?? string.Empty;
    }

    public static string PadLeft(object? value, int width, char paddingChar = ' ')
    {
        var str = value?.ToString();
        return str != null ?
            str.PadLeft(width, paddingChar).Substring(0, width) :
            new string(paddingChar, width);
    }
}