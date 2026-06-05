using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteryShop.DataAccess.ViewModels
{
    public class ItemUpdateViewModel
    {
        public int ItemId { get; set; } = 0;
        public int? BrandId { get; set; } = null;
        public List<BrandListViewModel> BrandList { get; set; }
        public int? TypeId { get; set; } = null;
        public List<TypeListViewModel> TypeList { get; set; }
        public string SerialNumber { get; set; } = "";
        public int? TransactionId { get; set; } = null;
    }
}
