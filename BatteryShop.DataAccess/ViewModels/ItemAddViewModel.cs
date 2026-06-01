using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteryShop.DataAccess.ViewModels
{
    public class ItemAddViewModel
    {
        public int TransactionId { get; set; }

        public List<ItemAddDetailsViewModel> Items { get; set; }
    }
}
