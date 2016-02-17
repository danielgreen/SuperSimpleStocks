using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NodaTime;
using SuperSimpleStocks;
using SuperSimpleStocks.Data;
using SuperSimpleStocks.Extensions;
using SuperSimpleStocks.Model;

namespace SuperSimpleStocks.Tests
{
    [TestClass]
    public class StockTests
    {
        protected IStockData Data { get; set; }

        protected StockEngine Engine { get; set; }

        protected IClock Clock { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            var stockTestData = new List<IStock>
            {
                new Stock { Symbol = "TEA", StockType = Constants.StockTypeCommon, LastDividend = 0m, ParValue = 100m, },
                new Stock { Symbol = "POP", StockType = Constants.StockTypeCommon, LastDividend = 8m, ParValue = 100m, },
                new Stock { Symbol = "ALE", StockType = Constants.StockTypeCommon, LastDividend = 23m, ParValue = 60m, },
                new Stock { Symbol = "GIN", StockType = Constants.StockTypePreferred, LastDividend = 8m, FixedDividend = 2m, ParValue = 100m, },
                new Stock { Symbol = "JOE", StockType = Constants.StockTypeCommon, LastDividend = 13m, ParValue = 250m, },
            };

            var tradeTestData = new List<ITrade>();

            var mock = new Mock<IStockData>();
            mock.Setup(d => d.Stocks).Returns(stockTestData);
            mock.Setup(d => d.Trades).Returns(tradeTestData);

            Data = mock.Object;

            Clock = SystemClock.Instance;
            Engine = new StockEngine(Data, Clock);
        }

        [TestMethod]
        public void TestCommonDividendYieldCalculation()
        {
            var stock = new Stock
            {
                Symbol = "ALE",
                StockType = Constants.StockTypeCommon,
                LastDividend = 23m,
                ParValue = 60m,
            };

            decimal yield = Engine.CalculateDividendYield(stock, 2m);

            Assert.AreEqual(yield, 11.5m);
        }

        [TestMethod]
        public void TestPreferredDividendYieldCalculation()
        {
            var stock = new Stock
            {
                Symbol = "GIN",
                StockType = Constants.StockTypePreferred,
                LastDividend = 8m,
                FixedDividend = 2m,
                ParValue = 100m,
            };

            decimal yield = Engine.CalculateDividendYield(stock, 10m);

            Assert.AreEqual(yield, 0.2m);
        }

        [TestMethod]
        public void TestPreferredDividendYieldCalculationFailsWhenMarketPriceIsZero()
        {
            var stock = new Stock
            {
                Symbol = "GIN",
                StockType = Constants.StockTypePreferred,
                LastDividend = 8m,
                FixedDividend = null,
                ParValue = 100m,
            };

            try
            {
                decimal yield = Engine.CalculateDividendYield(stock, 0m);
                Assert.Fail();
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestPreferredDividendYieldCalculationFailsWhenNoFixedDividendSet()
        {
            var stock = new Stock
            {
                Symbol = "GIN",
                StockType = Constants.StockTypePreferred,
                LastDividend = 8m,
                FixedDividend = null,
                ParValue = 100m,
            };

            try
            {
                decimal yield = Engine.CalculateDividendYield(stock, 10m);
                Assert.Fail();
            }
            catch (Exception)
            {
                
            }
        }

        [TestMethod]
        public void TestPriceEarningsRatioCalculation()
        {
            var stock = new Stock
            {
                Symbol = "ALE",
                StockType = Constants.StockTypeCommon,
                LastDividend = 23m,
                ParValue = 60m,
            };

            decimal yield = Engine.CalculatePriceEarningsRatio(stock, 46m);

            Assert.AreEqual(yield, 2m);
        }

        [TestMethod]
        public void TestPriceEarningsRatioCalculationFailsWhenLastDividedIsZero()
        {
            var stock = new Stock
            {
                Symbol = "ALE",
                StockType = Constants.StockTypeCommon,
                LastDividend = 0m,
                ParValue = 60m,
            };

            try
            {
                decimal yield = Engine.CalculatePriceEarningsRatio(stock, 66m);
                Assert.Fail();
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestRecordingTrade()
        {
            var tradeTimestamp = SystemClock.Instance.Now.Minus(Duration.FromMinutes(5));

            var tradeCount = Data.Trades.Count;

            Engine.RecordTrade("JOE", 3000, true, 18.53m, tradeTimestamp);

            // Confirm that the total number of trades has increased by one
            Assert.AreEqual(tradeCount + 1, Data.Trades.Count);
            
            // Confirm that SaveChanges was called
            Mock.Get(Data).Verify(d => d.SaveChanges(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void TestRecordingTradeFailsForUnknownStockSymbol()
        {
            var tradeTimestamp = SystemClock.Instance.Now.Minus(Duration.FromMinutes(5));

            try
            {
                Engine.RecordTrade("XYZ", 3000, true, 18.53m, tradeTimestamp);
                Assert.Fail();
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void TestCalculateVolumeWeightedStockPrice()
        {
            // Insert some trades for two different stocks
            var teaStock = Data.Stocks.First(s => s.Symbol == "TEA");
            var popStock = Data.Stocks.First(s => s.Symbol == "POP");

            var tradeTimestamp = Clock.Now.Minus(Duration.FromMinutes(16));
            Data.Trades.Add(new Trade { Stock = teaStock, Buy = true, Price = 100m, Quantity = 20, Timestamp = tradeTimestamp });

            tradeTimestamp = Clock.Now.Minus(Duration.FromMinutes(14));
            
            Data.Trades
                .AddFluent(new Trade { Stock = teaStock, Buy = false, Price = 100m, Quantity = 50, Timestamp = tradeTimestamp })
                .AddFluent(new Trade { Stock = teaStock, Buy = true, Price = 50m, Quantity = 15, Timestamp = tradeTimestamp })
                .AddFluent(new Trade { Stock = teaStock, Buy = false, Price = 25m, Quantity = 10, Timestamp = tradeTimestamp })
                .AddFluent(new Trade { Stock = teaStock, Buy = true, Price = 15m, Quantity = 30, Timestamp = tradeTimestamp })
                .AddFluent(new Trade { Stock = popStock, Buy = true, Price = 5000m, Quantity = 1000, Timestamp = tradeTimestamp });

            tradeTimestamp = Clock.Now.Plus(Duration.FromMinutes(5));
            Data.Trades.Add(new Trade { Stock = teaStock, Buy = false, Price = 100m, Quantity = 50, Timestamp = tradeTimestamp });
            
            // If the test passes, it means the result is correct for the relevant stock.
            // Therefore trades for the other stock had no impact, and trades outwith the time window had no impact.
            decimal result = Engine.CalculateVolumeWeightedStockPrice("TEA");
            const decimal expectedResult = 6450m/105m;
            
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void TestCalculateVolumeWeightedStockPriceFailsWhenNoRecentTrades()
        {
            // Insert a trade with a timestamp of 3 days ago i.e. outwith the window checked by the method
            var stock = Data.Stocks.First();
            var tradeTimestamp = Clock.Now.Minus(Duration.FromStandardDays(3));
            Data.Trades.Add(new Trade { Stock = stock, Buy = true, Price = 100m, Quantity = 33, Timestamp = tradeTimestamp });

            try
            {
                decimal result = Engine.CalculateVolumeWeightedStockPrice(stock.Symbol);
                Assert.Fail();
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void TestCalculateAllShareIndex()
        {
            // Insert some trades for two different stocks
            var teaStock = Data.Stocks.First(s => s.Symbol == "TEA");
            var popStock = Data.Stocks.First(s => s.Symbol == "POP");
            var aleStock = Data.Stocks.First(s => s.Symbol == "ALE");

            var olderTimestamp = Clock.Now.Minus(Duration.FromMinutes(30));
            var newerTimestamp = Clock.Now.Minus(Duration.FromMinutes(10));
            
            Data.Trades
                .AddFluent(new Trade { Stock = teaStock, Buy = false, Price = 100m, Quantity = 50, Timestamp = olderTimestamp })
                .AddFluent(new Trade { Stock = teaStock, Buy = false, Price = 150m, Quantity = 50, Timestamp = newerTimestamp })
                .AddFluent(new Trade { Stock = popStock, Buy = true,  Price = 200m, Quantity = 50, Timestamp = olderTimestamp })
                .AddFluent(new Trade { Stock = popStock, Buy = false, Price = 450m, Quantity = 50, Timestamp = newerTimestamp })
                .AddFluent(new Trade { Stock = aleStock, Buy = false, Price = 180m, Quantity = 50, Timestamp = olderTimestamp })
                .AddFluent(new Trade { Stock = aleStock, Buy = true,  Price = 130m, Quantity = 50, Timestamp = newerTimestamp });

            decimal result = Engine.CalculateAllShareIndex();

            double expectedResult = Math.Pow(150*450*130, 1.0d/3);

            Assert.AreEqual(result, (decimal)expectedResult);
        }

        [TestMethod]
        public void TestCalculateAllShareIndexFailsWhenNoTradesExist()
        {
            try
            {
                decimal result = Engine.CalculateAllShareIndex();
                Assert.Fail();
            }
            catch (Exception)
            {
            }
        }
    }
}
