using System.Collections.Concurrent;
using System.Reflection;

namespace Authorization.Common;

/// <summary>
/// Lightweight convention-based object mapper: copies matching public
/// properties (by name and type) from a source object to a new destination
/// instance.
/// </summary>
/// <remarks>
/// This runs on the request hot path, so the reflection work (discovering
/// which source/destination properties line up) is computed once per
/// type-pair and cached. Only value types, strings and arrays are copied,
/// mirroring the original shallow-copy behaviour so nested reference graphs
/// are never shared by accident.
/// </remarks>
public static class Mapper
{
    private static readonly ConcurrentDictionary<(Type Source, Type Destination), PropertyPair[]> PropertyMapCache = new();

    private readonly record struct PropertyPair(PropertyInfo Source, PropertyInfo Destination);

    /// <summary>
    /// Maps <paramref name="source"/> onto a new instance of
    /// <typeparamref name="TDestination"/>.
    /// </summary>
    /// <param name="source">Object to read values from. May be <c>null</c>.</param>
    /// <param name="transform">Optional hook to apply custom rules after the automatic copy.</param>
    /// <returns>The populated destination, or the type default when <paramref name="source"/> is <c>null</c>.</returns>
    public static TDestination? Map<TSource, TDestination>(
        TSource? source,
        Action<TSource, TDestination>? transform = null)
        where TSource : class
        where TDestination : new()
    {
        if (source is null)
        {
            return default;
        }

        var destination = new TDestination();
        foreach (var pair in GetPropertyMap(typeof(TSource), typeof(TDestination)))
        {
            pair.Destination.SetValue(destination, pair.Source.GetValue(source));
        }

        transform?.Invoke(source, destination);
        return destination;
    }

    private static PropertyPair[] GetPropertyMap(Type source, Type destination) =>
        PropertyMapCache.GetOrAdd((source, destination), static key => BuildPropertyMap(key.Source, key.Destination));

    private static PropertyPair[] BuildPropertyMap(Type source, Type destination)
    {
        var destinationProperties = destination
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.Ordinal);

        var pairs = new List<PropertyPair>();
        foreach (var sourceProperty in source.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!destinationProperties.TryGetValue(sourceProperty.Name, out var destinationProperty))
            {
                continue;
            }

            if (!destinationProperty.CanWrite ||
                destinationProperty.GetIndexParameters().Length != 0 ||
                destinationProperty.PropertyType != sourceProperty.PropertyType ||
                !IsCopyable(destinationProperty.PropertyType))
            {
                continue;
            }

            pairs.Add(new PropertyPair(sourceProperty, destinationProperty));
        }

        return pairs.ToArray();
    }

    // Copy value types, strings and arrays only; skip complex reference types
    // to avoid sharing mutable nested objects between source and destination.
    private static bool IsCopyable(Type type) =>
        !type.IsClass || type == typeof(string) || type.IsArray;
}
