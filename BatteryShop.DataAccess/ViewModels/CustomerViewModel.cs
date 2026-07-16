using System.Collections.Generic;
using BatteryShop.DataAccess.Models;

namespace BatteryShop.DataAccess.ViewModels
{
    public class CustomerViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 0;
        public List<int> PageSizeList = new List<int> { 3, 5, 10, 15, 20 };
        public string SearchTerm { get; set; } = "";
        public string Phone { get; set; } = "";
        public int? CityId { get; set; } = null;
        public string SortColumn { get; set; } = "userId";
        public string SortDirection { get; set; } = "ASC";
        public List<CityListViewModel> CityList { get; set; }
        public List<CustomerModel> CustomerList { get; set; }
        public int TotalRows { get; set; } = 1;
    }
}
