using GhigoWeb.Extensions;
using GhigoWeb.Filters;
using GhigoWeb.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "Fornitore"), GhigoAccess]
    public class DepositoController : Controller
    {

        public ActionResult Index(string cerca, bool? ordina_scadenza)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.DepositoInterno = user.DepositoInterno;

                var sm = gc.SaldiDeposito
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => o.CodiceDeposito == user.CodiceFornitore)
                    .Where(o => o.Quantita != 0) // quantita non nulla
                    .AsQueryable();

                ViewBag.Cerca = string.Empty;

                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    sm = sm.Where(o => o.CodiceArticolo.Contains(filtro)
                        || o.Descrizione.Contains(filtro)
                        || o.Ean13.Contains(filtro)
                        || o.Code32.Contains(filtro));
                }

                bool ordinato_scadenza = ordina_scadenza.HasValue && ordina_scadenza.Value;
                ViewBag.OrdinatoScadenza = ordinato_scadenza;

                if (ordinato_scadenza)
                {
                    return View(sm.OrderBy(o => o.ScadenzaLotto).ThenBy(o => o.Descrizione).ToArray()); 
                }

                return View(sm.OrderBy(o => o.Descrizione).ToArray());
            }
        }

        [HttpPost]
        public ActionResult Rimuovi()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                    var recid = long.Parse(Request.Form["recid"]);

                    var cf = gc.SaldiDeposito
                            .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                            .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                            .Where(o => o.CodiceDeposito == user.CodiceFornitore)
                            .Where(o => o.RecId == recid).Single();
                    cf.Prenotato = 0;
                    gc.SaveChanges();

                    return Json(new
                    {
                        Successo = true,
                        RecId = recid,
                        Prenotato = 0,
                        Quantita = cf.Quantita,
                        Messaggio = string.Format("Hai correttamente rimosso le prenotazioni di {0}", cf.Descrizione)
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = "Errore: " + ex.Message
                });
            }
        }

        [HttpPost]
        public ActionResult Prenota()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                    var recid = long.Parse(Request.Form["recid"]);
                    var qta = int.Parse(Request.Form["qta"]);

                    var cf = gc.SaldiDeposito
                            .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                            .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                            .Where(o => o.CodiceDeposito == user.CodiceFornitore)
                            .Where(o => o.RecId == recid).Single();

                    if ((qta <= 0) || (cf.Prenotato + qta > cf.Quantita))
                        throw new Exception("Quantità non valida.");

                    cf.Prenotato += qta;
                    gc.SaveChanges();

                    return Json(new
                    {
                        Successo = true,
                        RecId = recid,
                        Prenotato = cf.Prenotato,
                        Quantita = cf.Quantita,
                        Messaggio = string.Format("Hai correttamente prenotato {1} pz. di {0}", 
                            cf.Descrizione, qta)
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = "Errore: " + ex.Message
                });
            }
        }

    }
}
