using System;

namespace BatteryShop.DataAccess.Models
{
    public class CustomerModel
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserPhone { get; set; }
        public int CityId { get; set; }
        public string UserCity { get; set; }
        public decimal UserBalance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }
}
