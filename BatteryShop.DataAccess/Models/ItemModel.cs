using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteryShop.DataAccess.Models
{
    public class ItemModel
    {
        public int ItemId { get; set; }
        public string ItemSerialNumber { get; set; }
        public string ItemBrand { get; set; }
        public string ItemType { get; set; }
        public int TransactionId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
    }
}
