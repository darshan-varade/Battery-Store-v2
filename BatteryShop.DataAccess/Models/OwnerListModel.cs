using System;

namespace BatteryShop.DataAccess.Models
{
    public class OwnerListModel
    {
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }
        public string RoleName { get; set; }
        public byte? IsApproved { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ProfileImage { get; set; }
    }
}
