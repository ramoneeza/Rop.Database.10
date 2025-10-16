namespace Rop.Database10
{
    /// <summary>
    /// Helper Class for Linq. OrderBy and ThenBy with Descending option
    /// </summary>
    public static class LinqHelper
    {
        public static IOrderedEnumerable<T> OrderBy<T, K>(this IEnumerable<T> lst, Func<T, K> fnorder, bool desc)
        {
            if (!desc)
                return lst.OrderBy(fnorder);
            else
                return lst.OrderByDescending(fnorder);
        }
        public static IOrderedEnumerable<T> ThenBy<T, K>(this IOrderedEnumerable<T> lst, Func<T, K> fnorder, bool desc)
        {
            if (!desc)
                return lst.ThenBy(fnorder);
            else
                return lst.ThenByDescending(fnorder);
        }

    }
}
