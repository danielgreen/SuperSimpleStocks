using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace SuperSimpleStocks.Model
{
    public class Trade : ITrade
    {
        public IStock Stock { get; set; }
        public int Quantity { get; set; }
        public bool Buy { get; set; }
        public decimal Price { get; set; }
        public Instant Timestamp { get; set; }
    }
}
