using System.Collections.Generic;
using BatteryShop.DataAccess.Models;

namespace BatteryShop.DataAccess.ViewModels
{
    public class BillAddViewModel
    {
        public List<CityListViewModel> CityList { get; set; }
        public List<VehicleBrandViewModel> BrandList { get; set; }
        public List<VehicleModelViewModel> TypeList { get; set; }
        public List<OldItemStatusViewModel> OldItemStatusList { get; set; }
        public List<VehicleBrandViewModel> VehicleBrandList { get; set; }

        public int BillId { get; set; } = 0;
        public int? EditUserId { get; set; }
        public string EditCustomerName { get; set; }
        public string EditCustomerPhone { get; set; }
        public string EditCustomerCity { get; set; }
        public string EditDateOfSale { get; set; }
        public decimal EditPaidAmount { get; set; }
        public string EditItemsJson { get; set; } = "[]";
    }
}
