using System;
using System.Collections.Generic;
using System.Text;

namespace PoorMansTSqlFormatterLib
{
    public static class Utils
    {
        public static IEnumerable<TSource> IndexRange<TSource>(
            this IList<TSource> source,
            int fromIndex, 
            int toIndex)
        {
            int currIndex = fromIndex;
            while (currIndex <= toIndex)
            {
                yield return source[currIndex];
                currIndex++;
            }
        }
    }
}
