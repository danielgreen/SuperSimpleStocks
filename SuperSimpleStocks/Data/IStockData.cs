using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSimpleStocks.Model;

namespace SuperSimpleStocks.Data
{
    public interface IStockData
    {
        IList<IStock> Stocks { get; }

        IList<ITrade> Trades { get; }

        void SaveChanges();
    }
}
