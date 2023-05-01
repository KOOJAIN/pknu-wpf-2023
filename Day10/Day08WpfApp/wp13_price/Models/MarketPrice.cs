using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wp13_price.Models
{
    internal class MarketPrice
    {
        public string MidName { get; set; }
        public string GoodName { get; set; }
        public double Danq { get; set; }
        public string Dan { get; set; }
        public string Poj { get; set; }
        public string SizeName { get; set; }
        public string Lv { get; set; }
        public int MinCost { get; set; }
        public int MaxCost { get; set; }
        public int AveCost { get; set; }
        public DateTime Saledate { get; set; }
        public string CmpName { get; set; }
        public string LargeName { get; set; }
    }
}
