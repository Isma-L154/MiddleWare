namespace Authorization.Common;

/// <summary>
/// Convenience facade over <see cref="Mapper"/> for single objects and sequences.
/// </summary>
public static class Converter
{
    /// <summary>Creates a shallow copy of <paramref name="source"/>.</summary>
    public static TModel? Clone<TModel>(TModel? source)
        where TModel : class, new()
        => Mapper.Map<TModel, TModel>(source);

    /// <summary>Converts a single object to <typeparamref name="TDestination"/>.</summary>
    public static TDestination? Convert<TSource, TDestination>(
        TSource? source,
        Action<TSource, TDestination>? transform = null)
        where TSource : class
        where TDestination : new()
        => Mapper.Map(source, transform);

    /// <summary>
    /// Converts a sequence, skipping any elements that map to <c>null</c>.
    /// Materialised to a list so the (already-executed) source query is only
    /// enumerated once.
    /// </summary>
    public static IReadOnlyList<TDestination> ConvertList<TSource, TDestination>(
        IEnumerable<TSource> source,
        Action<TSource, TDestination>? transform = null)
        where TSource : class
        where TDestination : new()
    {
        ArgumentNullException.ThrowIfNull(source);

        var result = new List<TDestination>();
        foreach (var element in source)
        {
            var mapped = Mapper.Map(element, transform);
            if (mapped is not null)
            {
                result.Add(mapped);
            }
        }

        return result;
    }
}
