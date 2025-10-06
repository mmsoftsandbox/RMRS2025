using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace RMRS.ConsoleApp.Helpers;

public static class PropertyAccessor
{
    /// <summary>
    /// Concurrency aware словарь для кеширования PropertyInfo
    /// </summary>
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>> _cache =
        new ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>>();

    public static TValue GetPropertyValue<TInstance, TValue>(this TInstance instance, string propertyName)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var type = typeof(TInstance);
        var properties = _cache.GetOrAdd(type, _ => new ConcurrentDictionary<string, PropertyInfo>());

        var property = properties.GetOrAdd(propertyName, name =>
        {
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

            if (prop == null)
                throw new ArgumentException($"Property '{name}' not found in type {type.Name}");

            if (!prop.CanRead)
                throw new ArgumentException($"Property '{name}' does not have a getter");

            if (!typeof(TValue).IsAssignableFrom(prop.PropertyType))
                throw new InvalidCastException($"Property type {prop.PropertyType.Name} is not compatible with {typeof(TValue).Name}");

            return prop;
        });

        return (TValue)property.GetValue(instance);
    }

    public static object? GetPropertyValue<TInstance>(this TInstance instance, string propertyName)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var type = typeof(TInstance);
        var properties = _cache.GetOrAdd(type, _ => new ConcurrentDictionary<string, PropertyInfo>());

        var property = properties.GetOrAdd(propertyName, name =>
        {
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

            if (prop == null)
                throw new ArgumentException($"Property '{name}' not found in type {type.Name}");

            if (!prop.CanRead)
                throw new ArgumentException($"Property '{name}' does not have a getter");

            return prop;
        });

        return property.GetValue(instance);
    }
}