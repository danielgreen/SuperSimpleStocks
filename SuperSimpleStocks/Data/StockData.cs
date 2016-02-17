using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using SuperSimpleStocks.Model;

namespace SuperSimpleStocks.Data
{
    public class StockData : IStockData
    {
        public StockData(string connectionString)
        {
            Stocks = Enumerable.Empty<IStock>().ToList();
            Trades = Enumerable.Empty<ITrade>().ToList();

            // Database access implementation omitted
        }

        public IList<IStock> Stocks { get; protected set; }

        public IList<ITrade> Trades { get; protected set; }

        public virtual void SaveChanges()
        {
            var txOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted,
                Timeout = TimeSpan.FromSeconds(30)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, txOptions))
            {
                // Database access implementation omitted
                scope.Complete();
            }
        }
    }
}
