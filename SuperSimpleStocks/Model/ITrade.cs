using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace SuperSimpleStocks.Model
{
    public interface ITrade
    {
        IStock Stock { get; set; }
        int Quantity { get; set; }
        bool Buy { get; set; }
        decimal Price { get; set; }
        Instant Timestamp { get; set; }
    }
}
