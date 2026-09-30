using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Http;
using BatteryShop.DataAccess.DAL;
using BatteryShop.DataAccess.Models;
using BatteryShop.DataAccess.ViewModels;
using BatteryShop.WebApp.Infrastructure;
using Serilog;

namespace BatteryShop.WebApp.Controllers
{
    [JwtWebApiAuthenticationFilter]
    [RoutePrefix("api/ItemApi")]
    public class ItemApiController : ApiController
    {
        [HttpPost]
        [Route("ItemGetList")]
        public IHttpActionResult ItemGetList(ItemViewModel itemVM)
        {
            try
            {
                if (itemVM == null)
                {
                    itemVM = new ItemViewModel();
                }

                int ownerId = GetCurrentOwnerId();
                string roleName = GetCurrentRoleName();

                ItemDAL itemDAL = new ItemDAL();

                List<ItemModel> itemList =
                    itemDAL.ItemGetList(
                        itemVM,
                        ownerId,
                        roleName
                    );

                return Ok(new
                {
                    success = true,
                    data = itemList,
                    totalRows = itemVM.TotalRows,
                    pageNumber = itemVM.PageNumber,
                    pageSize = itemVM.PageSize
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ItemGetList API");

                return InternalServerError(ex);
            }
        }

        private int GetCurrentOwnerId()
        {
            ClaimsPrincipal principal =
                RequestContext.Principal as ClaimsPrincipal;

            if (principal == null)
            {
                return 0;
            }

            Claim claim =
                principal.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                return 0;
            }

            int ownerId;

            if (int.TryParse(claim.Value, out ownerId))
            {
                return ownerId;
            }

            return 0;
        }

        private string GetCurrentRoleName()
        {
            ClaimsPrincipal principal =
                RequestContext.Principal as ClaimsPrincipal;

            if (principal == null)
            {
                return "";
            }

            Claim claim =
                principal.FindFirst(ClaimTypes.Role);

            if (claim == null)
            {
                return "";
            }

            return claim.Value;
        }
    }
}