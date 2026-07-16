namespace BatteryShop.DataAccess.ViewModels
{
    public class VehicleModelViewModel
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int BrandId { get; set; }
        public decimal? itemPrice { get; set; }
        public decimal? oldItemPrice { get; set; }
    }
}
