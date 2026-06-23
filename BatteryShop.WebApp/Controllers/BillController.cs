using System;
using System.Linq;
using System.Web.Mvc;
using BatteryShop.DataAccess.DAL;
using BatteryShop.DataAccess.ViewModels;
using Serilog;

namespace BatteryShop.WebApp.Controllers
{
    public class BillController : Controller
    {
        public ActionResult BillList()
        {
            BillViewModel vm = new BillViewModel();
            return View(vm);
        }

        [HttpPost]
        [ActionName("BillList")]
        public ActionResult BillListPost(BillViewModel vm)
        {
            try
            {
                BillDAL dal = new BillDAL();
                vm.BillList = dal.BillGetList(vm);
                return PartialView("_BillListPartial", vm);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BillListPost");
                return Content("<div class='alert alert-danger'>Error loading data: " + ex.Message + "</div>");
            }
        }

        [HttpGet]
        public JsonResult BillGet(int id)
        {
            try
            {
                BillDAL dal = new BillDAL();
                var vm = dal.BillGetById(id);

                if (vm == null)
                {
                    return Json(new { success = false, message = "Bill not found." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    billId = vm.BillId,
                    userId = vm.UserId,
                    userFullName = vm.UserFullName,
                    userPhone = vm.UserPhone,
                    dateOfSale = vm.DateOfSale.ToString("yyyy-MM-dd"),
                    totalAmount = vm.TotalAmount,
                    paidAmount = vm.PaidAmount,
                    dueAmount = vm.TotalAmount - vm.PaidAmount
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BillGet");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult BillDelete(int id)
        {
            try
            {
                BillDAL dal = new BillDAL();
                dal.BillDelete(id);
                return Json(new { success = true, message = "Bill deleted successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BillDelete");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult BillAdd()
        {
            try
            {
                BillDAL dal = new BillDAL();
                ItemDAL itemDal = new ItemDAL();
                CustomerDAL custDal = new CustomerDAL();

                BillAddViewModel vm = new BillAddViewModel
                {
                    CityList = custDal.CustomerGetDistinctCities(),
                    BrandList = itemDal.ItemFetchBrand().Select(b => new VehicleBrandViewModel
                    {
                        BrandId = b.BrandId,
                        BrandName = b.BrandName
                    }).ToList(),
                    TypeList = dal.GetBillItemTypes(),
                    OldItemStatusList = dal.GetOldItemStatusList(),
                    VehicleBrandList = dal.GetVehicleBrands()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BillAdd GET");
                return Content("<div class='alert alert-danger'>Error: " + ex.Message + "</div>");
            }
        }

        [HttpPost]
        public JsonResult BillAdd(BillAddRequest request)
        {
            try
            {
                BillDAL dal = new BillDAL();
                DateTime dt = DateTime.Parse(request.DateOfSale);
                int billId = dal.BillAdd(request.UserId, dt, request.TotalAmount, request.PaidAmount, request.ItemsJson);
                return Json(new { success = true, billId = billId });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BillAdd POST");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetAvailableSerials(int brandId, int typeId, int count)
        {
            try
            {
                BillDAL dal = new BillDAL();
                var list = dal.FetchAvailableSerials(brandId, typeId, count);
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetAvailableSerials");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetDiscount(int itemTypeId, string oldItemDateOfSale)
        {
            try
            {
                BillDAL dal = new BillDAL();
                DateTime dt = DateTime.Parse(oldItemDateOfSale);
                decimal pct = dal.GetDiscountPercentage(itemTypeId, dt);
                return Json(new { discountPercent = pct }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetDiscount");
                return Json(new { discountPercent = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetVehicleBrands()
        {
            try
            {
                BillDAL dal = new BillDAL();
                var list = dal.GetVehicleBrands();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetVehicleBrands");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetVehicleModels(int brandId)
        {
            try
            {
                BillDAL dal = new BillDAL();
                var list = dal.GetVehicleModelsByBrand(brandId);
                return Json(list.Select(m => new { VehicleModelId = m.VehicleModelId, VehicleModelName = m.VehicleModelName }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetVehicleModels");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult VehicleInfoAdd(int modelId, string regNumber)
        {
            try
            {
                BillDAL dal = new BillDAL();
                int id = dal.AddVehicleInfo(modelId, regNumber, 1);
                return Json(new { success = true, vehicleInformationId = id });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in VehicleInfoAdd");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
