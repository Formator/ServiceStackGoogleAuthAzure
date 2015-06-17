using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ServiceStack;
using ServiceStack.Mvc;

namespace GoogleAuthAzure.Controllers
{
    public class HomeController : ServiceStackController
    {
        public ActionResult Index()
        {
            if (base.IsAuthenticated)
                return Redirect("/Home/Result");
            return View();
        }

        public ActionResult Result()
        {
            if (!base.IsAuthenticated)
                return Redirect("/");

            return View();
        }

        public ActionResult Logout()
        {
            Execute(new Authenticate { provider = "logout" });
            FormsAuthentication.SignOut();

            return Redirect("/");
        }

    }
}