using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatteryShop.DataAccess.DAL;
using BatteryShop.DataAccess.ViewModels;

namespace BatteryShop.WebApp.Controllers
{
    public class ItemController : Controller
    {
        // GET: Item
        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult ItemList()
        {
            ItemViewModel ItemVM = new ItemViewModel();
            ItemDAL item = new ItemDAL();
            ItemVM.BrandList = item.ItemFetchDistinct();
            return View(ItemVM);
        }

        [HttpPost]
        public ActionResult ItemList(ItemViewModel ItemVM)
        {
            
            ItemDAL item = new ItemDAL();
            ItemVM.ItemList = item.ItemGetList(ItemVM);
            return PartialView("_ItemListPartial", ItemVM);
          
        }

        //public int PageNumberList(int PageSize)
        //{
        //    ItemDAL item = new ItemDAL();

        //    return item.FindPages(PageSize);
        //}

    }
}