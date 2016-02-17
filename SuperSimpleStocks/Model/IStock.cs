using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSimpleStocks.Model
{
    public interface IStock
    {
        string Symbol { get; set; }
        string StockType { get; set; }
        decimal LastDividend { get; set; }
        decimal? FixedDividend { get; set; }
        decimal ParValue { get; set; }
    }
}
