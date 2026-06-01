using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatteryShop.DataAccess.Models;
using BatteryShop.DataAccess.ViewModels;

namespace BatteryShop.DataAccess.ViewModels
{
    public class ItemViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 0;
        public List<int> PageSizeList = new List<int> { 3,5,10,15,20 };
        public string SerialNumber { get; set; } = "";
        public int? BrandId { get; set; } = null;   
        public List<BrandListViewModel> BrandList { get; set; }
        public int? TypeId { get; set; } = null;
        public List<TypeListViewModel> TypeList { get; set; }
        public int? TransactionId { get; set; } = null;
        public List<ItemModel> ItemList { get; set; }
        public int TotalRows { get; set; } = 1;
    }
}
