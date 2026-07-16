using System;
using System.Collections.Generic;
using BatteryShop.DataAccess.Models;

namespace BatteryShop.DataAccess.ViewModels
{
    public class BillViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 0;
        public List<int> PageSizeList = new List<int> { 3, 5, 10, 15, 20 };
        public string SearchTerm { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime? DateFrom { get; set; } = null;
        public DateTime? DateTo { get; set; } = null;
        public string SortColumn { get; set; } = "billId";
        public string SortDirection { get; set; } = "DESC";
        public List<BillModel> BillList { get; set; }
        public int TotalRows { get; set; } = 1;
    }
}
