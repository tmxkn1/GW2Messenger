using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2Messenger
{
    class Gw2Data
    {
        public abstract class Gw2Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Outfit : Gw2Item
        {
            public int GemCost { get; set; }
            public int UnlockableId { get; set; }
            public bool IsOnSale { get; set; }
            public int GemDiscount { get; set; }
        }

        public class Sale : Gw2Item
        {
            public int GemCost { get; set; }
            public int GemDiscount { get; set; }
            public string GemDiscountArray { get; set; }
            public string EndDate { get; set; }
        }
    }
}
