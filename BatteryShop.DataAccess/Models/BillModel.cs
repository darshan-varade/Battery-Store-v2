using System;

namespace BatteryShop.DataAccess.Models
{
    public class BillModel
    {
        public int BillId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserPhone { get; set; }
        public DateTime DateOfSale { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount => TotalAmount - PaidAmount;
    }
}
