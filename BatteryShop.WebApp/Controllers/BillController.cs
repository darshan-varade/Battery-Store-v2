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
    }
}
