using System;

namespace BatteryShop.DataAccess.Models
{
    public class RefreshTokenModel
    {
        public int RefreshTokenId { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public string RoleName { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
