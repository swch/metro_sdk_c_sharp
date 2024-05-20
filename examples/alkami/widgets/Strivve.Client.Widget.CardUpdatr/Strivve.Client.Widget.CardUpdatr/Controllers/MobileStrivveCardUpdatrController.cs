using Alkami.Client.Framework.Mvc;
using Alkami.Common;
using Alkami.Security.Common.Claims;
using Common.Logging;
using System;
using System.Web.Mvc;

namespace Strivve.Client.Widget.CardUpdatr.Controllers
{
    [ClaimsAuthorizationFilter(PermissionNames.NoPermissions)]
    public class MobileStrivveCardUpdatrController : BaseController
    {
        /// <summary>
        /// Gets logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger<MobileStrivveCardUpdatrController>();

        /// <summary>
        /// Standard widget entry route
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            try
            {
                Logger.DebugFormat("[GET] Controller/Index");
                return View("Index");
            }
            catch (Exception e)
            {
                Logger.Error("Error [GET] Controller/Index", e);
                return View("Error");
            }
        }
    }
}