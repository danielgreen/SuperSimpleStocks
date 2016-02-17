using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSimpleStocks.Model
{
    public class Stock : IStock
    {
        public string Symbol { get; set; }
        public string StockType { get; set; }
        public decimal LastDividend { get; set; }
        public decimal? FixedDividend { get; set; }
        public decimal ParValue { get; set; }
    }
}
