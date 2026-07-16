namespace BatteryShop.DataAccess.ViewModels
{
    public class CustomerUpdateViewModel
    {
        public int UserId { get; set; } = 0;
        public string UserFullName { get; set; } = "";
        public string UserPhone { get; set; } = "";
        public string CityName { get; set; } = "";
        public decimal UserBalance { get; set; } = 0;
    }
}
