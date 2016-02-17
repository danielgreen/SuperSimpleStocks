using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using SuperSimpleStocks.Data;
using SuperSimpleStocks.Model;

namespace SuperSimpleStocks
{
    public class StockEngine : IStockEngine
    {
        protected IStockData Data { get; set; }
        protected IClock Clock { get; set; }

        public StockEngine(IStockData data, IClock clock)
        {
            this.Data = data;
            this.Clock = clock;
        }
        
        public virtual decimal CalculateDividendYield(IStock stock, decimal marketPrice)
        {
            if (marketPrice == 0)
                throw new Exception("Cannot calculate dividend yield when market price is zero.");

            if (stock.StockType == Constants.StockTypePreferred && !stock.FixedDividend.HasValue)
                throw new Exception("Preferred stock must have a fixed dividend value in order to calculate the yield.");

            decimal yield = stock.StockType == Constants.StockTypePreferred
                ? (stock.FixedDividend ?? 0m * stock.ParValue * 0.01m) / marketPrice
                : stock.LastDividend / marketPrice;

            return yield;
        }

        public virtual decimal CalculatePriceEarningsRatio(IStock stock, decimal marketPrice)
        {
            if (stock.LastDividend == 0)
                throw new Exception("Cannot calculate price-earnings ratio as last dividend is zero.");

            decimal ratio = marketPrice / stock.LastDividend;

            return ratio;
        }

        public virtual void RecordTrade(string stockSymbol, int quantity, bool buy, decimal tradePrice, Instant timestamp)
        {
            var stock = Data.Stocks.FirstOrDefault(s => s.Symbol == stockSymbol);

            if (stock == null)
                throw new Exception(String.Format("Could not record trade; unknown stock symbol {0}", stockSymbol));

            var trade = new Trade
            {
                Stock = stock,
                Quantity = quantity,
                Buy = buy,
                Price = tradePrice,
                Timestamp = timestamp,
            };

            Data.Trades.Add(trade);

            Data.SaveChanges();
        }

        /// <summary>
        /// Based on trades in the past 15 minutes.
        /// </summary>
        public virtual decimal CalculateVolumeWeightedStockPrice(string stockSymbol)
        {
            Instant cutoffTimestamp = Clock.Now.Minus(Duration.FromMinutes(15));

            var trades = Data.Trades
                .Where(t => t.Stock.Symbol == stockSymbol &&
                            t.Timestamp >= cutoffTimestamp &&
                            t.Timestamp <= Clock.Now)
                .ToList();

            // Check that some trades were found in the time period.
            // We do not want to call .Sum on an empty list, as it will fail.
            if (trades.Count == 0)
                throw new Exception("Cannot calculate volume weighted stock price as there were zero trades in the time period.");

            decimal sumQuantity = trades.Sum(t => t.Quantity);

            if (sumQuantity == 0)
                throw new Exception("Cannot calculate volume weighted stock price as there were no trades with a positive quantity in the time period.");

            decimal result = trades.Sum(t => t.Price * t.Quantity) / sumQuantity;

            return result;
        }

        public virtual decimal CalculateAllShareIndex()
        {
            // Get all trades, grouped by stock
            var tradesGroupedByStock = Data.Trades.GroupBy(t => t.Stock.Symbol).ToList();

            if (tradesGroupedByStock.Count == 0)
                throw new Exception("Found no trades with which to calculate the all share index");

            // Get the most recent trade price of each stock
            List<decimal> prices = tradesGroupedByStock
                .Select(tg => tg.OrderBy(t => t.Timestamp).Last().Price)
                .ToList();
            
            // Multiply all the prices together
            decimal multipliedPrice = prices.Aggregate((agg, price) => agg * price);

            // Take the nth root of the multiplied prices, where n = the number of prices
            double result = Math.Pow((double)multipliedPrice, 1.0d / prices.Count);

            return (decimal)result;
        }
    }
}
