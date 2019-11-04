using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XRayBuilder.Core.Libraries.Enumerables.Extensions
{
    public static class EnumerableExtensions
    {
        // https://stackoverflow.com/a/50244393
        public static async Task<IEnumerable<T>> Where<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
        {
            var results = await Task.WhenAll(source.Select(async x => (x, await predicate(x))));
            return results.Where(x => x.Item2).Select(x => x.Item1);
        }
    }
}