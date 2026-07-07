using System;

namespace BatteryShop.DataAccess.Models
{
    public class OwnerModel
    {
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
