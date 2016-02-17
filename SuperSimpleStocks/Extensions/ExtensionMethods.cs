using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSimpleStocks.Extensions
{
    public static class ExtensionMethods
    {
        public static IList<T> AddFluent<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return list;
        }
    }
}
