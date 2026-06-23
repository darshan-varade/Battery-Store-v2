namespace BatteryShop.DataAccess.ViewModels
{
    public class BillAddRequest
    {
        public int UserId { get; set; }
        public string DateOfSale { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string ItemsJson { get; set; }
    }
}
