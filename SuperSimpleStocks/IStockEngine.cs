using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using SuperSimpleStocks.Model;

namespace SuperSimpleStocks
{
    interface IStockEngine
    {
        decimal CalculateDividendYield(IStock stock, decimal marketPrice);

        decimal CalculatePriceEarningsRatio(IStock stock, decimal marketPrice);

        void RecordTrade(string stockSymbol, int quantity, bool buy, decimal tradePrice, Instant timestamp);

        decimal CalculateVolumeWeightedStockPrice(string stockSymbol);

        decimal CalculateAllShareIndex();
    }
}
