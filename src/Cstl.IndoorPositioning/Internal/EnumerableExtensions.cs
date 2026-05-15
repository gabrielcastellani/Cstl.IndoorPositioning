namespace Cstl.IndoorPositioning.Internal
{
    internal static class EnumerableExtensions
    {
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return source as IReadOnlyList<T> ?? source.ToList();
        }
    }
}
