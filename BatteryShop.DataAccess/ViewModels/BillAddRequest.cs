namespace BatteryShop.DataAccess.ViewModels
{
    public class BillAddRequest
    {
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerCity { get; set; }
        public string DateOfSale { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string ItemsJson { get; set; }
    }
}
