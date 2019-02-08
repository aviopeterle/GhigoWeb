using GhigoWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GhigoWeb.Extensions
{
    public class GhigoAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // se l'utente è autenticato
            if(filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // aggiorniamo il suo ultimo accesso
                using(var gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == filterContext.HttpContext.User.Identity.Name);

                    // e controlliamo se ha accettato la privacy
                    if(!user.AccettazionePrivacy)
                    {
                        filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                        {
                            { "controller", "Home" },
                            { "action", "Privacy" }
                        });
                    }
                }

                base.OnActionExecuting(filterContext);
            }
        }
    }
}