using System;
using System.Web.Mvc;
using BatteryShop.DataAccess.DAL;

namespace BatteryShop.WebApp.Controllers
{
    [AuthorizeRole(Role.Admin)]
    public class AdminController : BaseController
    {
        public ActionResult PendingOwners()
        {
            AuthDAL dal = new AuthDAL();
            return View(dal.GetPendingOwners());
        }

        [HttpPost]
        public JsonResult Approve(int id)
        {
            try
            {
                AuthDAL dal = new AuthDAL();
                int ownerId = dal.ApprovePendingOwner(id, CurrentOwnerId);
                return Json(new { success = true, ownerId });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error approving pending owner {PendingOwnerId}", id);
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        [HttpPost]
        public JsonResult Reject(int id)
        {
            try
            {
                AuthDAL dal = new AuthDAL();
                dal.RejectPendingOwner(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error rejecting pending owner {PendingOwnerId}", id);
                return Json(new { success = false, message = "An error occurred" });
            }
        }
    }
}
