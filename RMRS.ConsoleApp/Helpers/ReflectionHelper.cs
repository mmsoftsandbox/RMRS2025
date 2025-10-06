using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace RMRS.ConsoleApp.Helpers;

public static class ReflectionHelper
{
    public static string GetDisplayName(this Type type, string propName)
    {
        var field = type.GetProperty(propName);
        if (field == null)
        {
            return string.Empty;
        }

        var displayNameAttribute = field
            .GetCustomAttributes(typeof(DisplayNameAttribute), false)
            .FirstOrDefault() as DisplayNameAttribute;

        return displayNameAttribute?.DisplayName ?? propName;
    }

    public static TAttribute? GetCustomAttribute<TAttribute>(this Type type)
        where TAttribute : Attribute
    {
        return type.GetCustomAttributes(typeof(TAttribute), inherit: false)
                   .FirstOrDefault() as TAttribute;
    }
}