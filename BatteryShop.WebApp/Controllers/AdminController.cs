using System;
using System.Web.Mvc;
using BatteryShop.DataAccess.DAL;

namespace BatteryShop.WebApp.Controllers
{
    [AuthorizeRole(Role.Admin)]
    public class AdminController : BaseController
    {
        public ActionResult ManageOwners()
        {
            AuthDAL dal = new AuthDAL();
            return View(dal.GetAllOwners());
        }

        [HttpPost]
        public JsonResult SetApprovalStatus(int id, byte? status)
        {
            try
            {
                new AuthDAL().SetApprovalStatus(id, status, CurrentOwnerId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error setting approval status for owner {OwnerId}", id);
                return Json(new { success = false, message = "An error occurred" });
            }
        }
    }
}
