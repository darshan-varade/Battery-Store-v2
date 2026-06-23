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
    }
}
