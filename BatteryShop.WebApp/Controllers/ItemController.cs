using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using BatteryShop.DataAccess.DAL;
using BatteryShop.DataAccess.ViewModels;

namespace BatteryShop.WebApp.Controllers
{
    public class ItemController : Controller
    {

        private List<BrandListViewModel> GetCachedBrands()
        {
            string key = "BrandList";
            var cached = HttpRuntime.Cache[key] as List<BrandListViewModel>;
            if (cached != null) return cached;

            var dal = new ItemDAL();
            var list = dal.ItemFetchBrand();
            HttpRuntime.Cache.Insert(key, list, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration);
            return list;
        }
        
        private List<TypeListViewModel> GetCachedTypes()
        {
            string key = "TypeList";
            var cached = HttpRuntime.Cache[key] as List<TypeListViewModel>;
            if (cached != null) return cached;

            var dal = new ItemDAL();
            var list = dal.ItemFetchType();
            HttpRuntime.Cache.Insert(key, list, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration);
            return list;
        }
        // GET: Item
        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult ItemList()
        {
            ItemViewModel ItemVM = new ItemViewModel();
            ItemVM.BrandList = GetCachedBrands();
            ItemVM.TypeList = GetCachedTypes();
            
            return View(ItemVM);
        }

        [HttpPost]
        [ActionName("ItemList")]
        public ActionResult ItemListPost(ItemViewModel ItemVM)
        {
            
            ItemDAL item = new ItemDAL();
            ItemVM.ItemList = item.ItemGetList(ItemVM);
            return PartialView("_ItemListPartial", ItemVM);
          
        }

        [HttpPost]
        public JsonResult ItemDelete(int id)
        {
            try
            {
                ItemDAL item = new ItemDAL();
                item.deleteItem(id);
                return Json(new
                {
                    success = true,
                    message = "Item deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public JsonResult ItemGet(int id)
        {
            ItemDAL item = new ItemDAL();
            var vm = item.GetItemForUpdate(id);
            return Json(new
            {
                itemId = vm.ItemId,
                brandId = vm.BrandId,
                typeId = vm.TypeId,
                serialNumber = vm.SerialNumber,
                transactionId = vm.TransactionId
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ItemAdd(ItemViewModel ItemVM)
        {
            ItemVM.BrandList = GetCachedBrands();
            ItemVM.TypeList = GetCachedTypes();
            return View(ItemVM);
        }

        [HttpPost]
        public JsonResult ItemAdd(ItemAddViewModel addItemList)
        {
            try
            {
                ItemDAL itemDal = new ItemDAL();
                itemDal.addItems(addItemList);
                return Json(new
                {
                    success = true,
                    message = "Items added successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public ActionResult ItemUpdate(int id)
        {
            ItemDAL item = new ItemDAL();

            ItemUpdateViewModel vm = item.GetItemForUpdate(id);

            vm.BrandList = GetCachedBrands();
            vm.TypeList = GetCachedTypes();

            return View(vm);
        }

        [HttpPost]
        public JsonResult ItemUpdate(ItemUpdateViewModel vm)
        {
            try
            {
                ItemDAL item = new ItemDAL();

                item.UpdateItem(vm);

                return Json(new
                {
                    success = true,
                    message = "Item updated successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}