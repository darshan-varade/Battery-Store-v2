using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using BatteryShop.DataAccess.DAL;
using BatteryShop.DataAccess.ViewModels;
using Serilog;

namespace BatteryShop.WebApp.Controllers
{
    public class CustomerController : Controller
    {
        private List<CityListViewModel> GetCachedCities()
        {
            string key = "CityList";
            var cached = HttpRuntime.Cache[key] as List<CityListViewModel>;
            if (cached != null) return cached;

            var dal = new CustomerDAL();
            var list = dal.CustomerGetDistinctCities();
            HttpRuntime.Cache.Insert(key, list, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration);
            return list;
        }

        public ActionResult CustomerList()
        {
            CustomerViewModel vm = new CustomerViewModel();
            vm.CityList = GetCachedCities();
            return View(vm);
        }

        [HttpPost]
        [ActionName("CustomerList")]
        public ActionResult CustomerListPost(CustomerViewModel vm)
        {
            try
            {
                CustomerDAL dal = new CustomerDAL();
                vm.CustomerList = dal.CustomerGetList(vm);
                return PartialView("_CustomerListPartial", vm);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerListPost");
                return Content("<div class='alert alert-danger'>Error loading data: " + ex.Message + "</div>");
            }
        }

        [HttpGet]
        public JsonResult CustomerGet(int id)
        {
            try
            {
                CustomerDAL dal = new CustomerDAL();
                var vm = dal.CustomerGetById(id);

                if (vm == null)
                {
                    return Json(new { success = false, message = "Customer not found." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    userId = vm.UserId,
                    userFullName = vm.UserFullName,
                    userPhone = vm.UserPhone,
                    cityName = vm.CityName,
                    userBalance = vm.UserBalance
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerGet");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult CustomerSearch(string term)
        {
            try
            {
                CustomerDAL dal = new CustomerDAL();
                var list = dal.CustomerSearch(term ?? "");

                var results = list.Select(c => new
                {
                    id = c.UserId,
                    text = c.UserFullName,
                    phone = c.UserPhone,
                    cityId = c.CityId,
                    cityName = c.UserCity
                });

                return Json(results, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerSearch");
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult CustomerSearchByPhone(string term)
        {
            try
            {
                CustomerDAL dal = new CustomerDAL();
                var list = dal.CustomerSearchByPhone(term ?? "");

                var results = list.Select(c => new
                {
                    id = c.UserPhone,
                    text = c.UserPhone
                });

                return Json(results, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerSearchByPhone");
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CustomerAdd(CustomerUpdateViewModel vm)
        {
            try
            {
                CustomerDAL dal = new CustomerDAL();
                dal.CustomerAdd(vm);
                var customers = dal.CustomerSearchByPhone(vm.UserPhone);
                int userId = 0;
                if (customers != null && customers.Count > 0)
                {
                    userId = customers[0].UserId;
                }
                return Json(new { success = true, message = "Customer added successfully", userId = userId });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerAdd");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CustomerUpdate(CustomerUpdateViewModel vm)
        {
            try
            {
                CustomerDAL dal = new CustomerDAL();
                dal.CustomerUpdate(vm);
                return Json(new { success = true, message = "Customer updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerUpdate");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CustomerDelete(int id)
        {
            try
            {
                CustomerDAL dal = new CustomerDAL();
                dal.CustomerDelete(id);
                return Json(new { success = true, message = "Customer deleted successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerDelete");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
