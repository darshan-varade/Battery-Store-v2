using System;

namespace BatteryShop.DataAccess.ViewModels
{
    public class PendingOwnerViewModel
    {
        public int PendingOwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
