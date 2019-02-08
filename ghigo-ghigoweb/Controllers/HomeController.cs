using GhigoWeb.Extensions;
using GhigoWeb.Filters;
using GhigoWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Roles.IsUserInRole("Fornitore"))
                return RedirectToAction("Index", "Offerte");

            if (Roles.IsUserInRole("Cliente"))
                return RedirectToAction("Index", "Ordini");

            throw new Exception("Nessun ruolo valido");
        }

        public ActionResult Privacy(string ok)
        {
            if (string.IsNullOrEmpty(ok))
            {
                return View();
            }
            else
            {
                using (var gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == HttpContext.User.Identity.Name);
                    user.AccettazionePrivacy = true;
                    gc.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
        }
    }
}
