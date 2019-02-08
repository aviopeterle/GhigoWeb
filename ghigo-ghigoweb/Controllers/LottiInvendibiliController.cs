using GhigoWeb.Extensions;
using GhigoWeb.Filters;
using GhigoWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "Fornitore"), GhigoAccess]
    public class LottiInvendibiliController : Controller
    {
        //
        // GET: /LottiInvendibili/

        public ActionResult Index(string cerca)
        {
            GhigoContext gc = new GhigoContext();

            var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
            user.UltimoAccesso = DateTime.Now;
            ViewBag.MessaggioWeb = user.MessaggioWeb;
            gc.SaveChanges();

            var lotti = gc.LottiInvendibili
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .OrderBy(o => o.Descrizione).ThenBy(o => o.Lotto)
                    .AsQueryable();

            ViewBag.Cerca = string.Empty;

            if (!string.IsNullOrEmpty(cerca))
            {
                string filtro = cerca.Trim().ToLower();
                ViewBag.Cerca = filtro;

                lotti = lotti.Where(o => o.CodiceProdotto.Contains(filtro)
                    || o.Descrizione.Contains(filtro)
                    || o.Invendibilita.Contains(filtro)
                    || o.Lotto.Contains(filtro));
            }

            return View(lotti);
        }

    }
}
