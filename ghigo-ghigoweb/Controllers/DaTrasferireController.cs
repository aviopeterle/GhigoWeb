using GhigoWeb.Extensions;
using GhigoWeb.Filters;
using GhigoWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "Fornitore"), GhigoAccess]
    public class DaTrasferireController : Controller
    {
        public ActionResult ElencoProposte()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                var pr = gc.ProposteTrasferimentiTestate
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .OrderBy(o => o.Descrizione);

                var model = pr.ToList();

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult CreaProposta(string txtDescrizione)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var pr = new PropostaTrasferimentoTestata();

                pr.Descrizione = string.IsNullOrEmpty(txtDescrizione) ? "Nessuna descrizione" : txtDescrizione;
                pr.CodiceAzienda = user.CodiceAzienda;
                pr.CodiceFornitore = user.CodiceFornitore;
                pr.UltimaModifica = DateTime.Now;

                gc.ProposteTrasferimentiTestate.Add(pr);
                gc.SaveChanges();

                return RedirectToAction("ElencoProposte", new { numeroproposta = pr.RecId });
            }
        }

        public ActionResult EliminaProposta(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var pr = gc.ProposteTrasferimentiTestate
                    .Where(p => p.CodiceAzienda == user.CodiceAzienda)
                    .Where(p => p.CodiceFornitore == user.CodiceFornitore)
                    .Where(p => p.RecId == recid)
                    .SingleOrDefault();

                if(pr == null)
                {
                    return new HttpNotFoundResult();
                }

                // rimuoviamo le righe
                foreach(var riga in gc.ProposteTrasferimenti.Where(p => p.RecIdTestata == recid))
                {
                    gc.ProposteTrasferimenti.Remove(riga);
                }
                // rimuoviamo la testata
                gc.ProposteTrasferimentiTestate.Remove(pr);

                gc.SaveChanges();

                return RedirectToAction("ElencoProposte");
            }
        }

        public ActionResult Index(string cerca, long numeroproposta)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                var te = gc.ProposteTrasferimentiTestate.SingleOrDefault(p => p.RecId == numeroproposta);
                if (te == null)
                {
                    return new HttpNotFoundResult();
                }

                ViewBag.DepositoInterno = user.DepositoInterno;

                ViewBag.NumeroProposta = te.RecId;
                ViewBag.DescrizioneProposta = te.Descrizione;

                var pr = gc.ProposteTrasferimenti
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => o.RecIdTestata == numeroproposta) // della proposta interessata
                    .AsQueryable();

                var model = pr.OrderBy(c => c.Descrizione).ToArray();

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Sposta()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var recid = long.Parse(Request.Form["recid"]);
                    var dove = int.Parse(Request.Form["dove"]);

                    var cf = gc.Conferimenti.Single(o => o.RecId == recid);
                    cf.Prenotato = dove == 1;
                    gc.SaveChanges();

                    return Json(new
                    {
                        Successo = true,
                        Messaggio = string.Format("Hai {0} correttamente {1} pz. di {2}",
                            cf.Prenotato ? "prenotato" : "rimosso", cf.Quantita, cf.Descrizione)
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
        public ActionResult RimuoviConferimento()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    string msg = string.Empty;
                    bool successo = false;

                    try
                    {
                        var recid = long.Parse(Request.Form["recid"]);

                        var conf = gc.Conferimenti.Single(a => a.RecId == recid);
                        gc.Conferimenti.Remove(conf);
                        gc.SaveChanges();

                        successo = true;
                        msg = string.Format("Hai rimosso correttamente {0} pz di {1}.", conf.Quantita, conf.Descrizione);
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Non e' stato possibile rimuovere la riga. {0}", ex.Message);
                    }

                    return Json(new
                    {
                        Successo = successo,
                        Messaggio = msg
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
        public ActionResult InserisciLibero()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                    var codiceArticolo = Request.Form["articolo"];
                    var value = int.Parse(Request.Form["value"]);
                    var lotto = Request.Form["lotto"];

                    // numero proposta
                    long numeroproposta = long.Parse(Request.Form["numeroproposta"]);

                    // scadenza
                    int mese = int.Parse(Request.Form["scadenzalottomese"]);
                    int anno = int.Parse(Request.Form["scadenzalottoanno"]);
                    if (anno < 100) anno += 2000;

                    // fine mese
                    DateTime scadenzalotto = new DateTime(anno, mese, 1);
                    scadenzalotto = scadenzalotto.AddMonths(1).AddDays(-1);

                    // se non e' impostato il costo, impostiamo 0
                    string costoStr = Request.Form["costo"];
                    decimal costoNettoUnitario = 0;
                    if (!decimal.TryParse(costoStr, out costoNettoUnitario)) costoNettoUnitario = 0;

                    // se non e' impostato il prezzo, impostiamo come il costo
                    string prezzoStr = Request.Form["prezzo"];
                    decimal prezzoVendita = 0;
                    if (!decimal.TryParse(prezzoStr, out prezzoVendita)) prezzoVendita = costoNettoUnitario;

                    string msg = string.Empty;
                    bool successo = false;
                    long new_recid = 0;

                    try
                    {
                        var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                        cmd.CommandText = "APP_SP_WEB_INSERISCI_PROPOSTA";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@codiceArticolo", codiceArticolo);
                        cmd.Parameters.AddWithValue("@qta", value);
                        cmd.Parameters.AddWithValue("@lotto", lotto);
                        cmd.Parameters.AddWithValue("@scadenzalotto", scadenzalotto);
                        cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
                        cmd.Parameters.AddWithValue("@codicefornitore", user.CodiceFornitore);
                        cmd.Parameters.AddWithValue("@costonettounitario", costoNettoUnitario);
                        cmd.Parameters.AddWithValue("@prezzovendita", prezzoVendita);
                        cmd.Parameters.AddWithValue("@numeroproposta", numeroproposta);
                        if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                            cmd.Connection.Open();
                        new_recid = Convert.ToInt64(cmd.ExecuteScalar());

                        successo = true;
                        msg = string.Format("Hai inserito {0} pz di {1}.", value, codiceArticolo);
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Non e' stato possibile inserire {0} pz di {1}. {2}",
                            value, codiceArticolo, ex.Message);
                    }

                    return Json(new
                    {
                        Successo = successo,
                        Messaggio = msg,
                        Recid = new_recid
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = "Errore: " + ex.Message,
                    Recid = 0
                });
            }
        }

        [HttpPost]
        public ActionResult RimuoviLibero()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    string msg = string.Empty;
                    bool successo = false;

                    try
                    {
                        var recid = long.Parse(Request.Form["recid"]);

                        var proposta = gc.ProposteTrasferimenti.Single(a => a.RecId == recid);
                        gc.ProposteTrasferimenti.Remove(proposta);
                        gc.SaveChanges();

                        successo = true;
                        msg = string.Format("Hai rimosso correttamente {0} pz di {1}.", proposta.Quantita, proposta.Descrizione);
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Non e' stato possibile rimuovere la riga. {0}", ex.Message);
                    }

                    return Json(new
                    {
                        Successo = successo,
                        Messaggio = msg
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
        public ActionResult OttieniLibero()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var recid = long.Parse(Request.Form["recid"]);

                var proposta = gc.ProposteTrasferimenti.Single(a => a.RecId == recid);

                return PartialView("_LiberoPartial", proposta);
            }
        }

        [HttpPost]
        public ActionResult OttieniDescrizioneArticolo()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var codiceArticolo = Request.Form["articolo"];
                    var articolo = gc.ArticoliGlobali.Single(a => a.CodiceArticolo == codiceArticolo);

                    return Json(new
                    {
                        Trovato = true,
                        Descrizione = articolo.Descrizione
                    });
                }
            }
            catch (Exception)
            {
                return Json(new
                {
                    Trovato = false,
                    Descrizione = "NON TROVATO"
                });
            }
        }

        [HttpPost]
        public ActionResult OttieniDatiLotto()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                    var codiceArticolo = Request.Form["articolo"];
                    var lotto = Request.Form["lotto"];

                    if (string.IsNullOrEmpty(codiceArticolo) || string.IsNullOrEmpty(lotto))
                        throw new ArgumentException();

                    var dati_costo = gc.DatiCosti
                        .Single(a =>
                           (a.CodiceArticolo == codiceArticolo.ToUpper())
                        && (a.CodiceAzienda == user.CodiceAzienda)
                        && (a.CodiceFornitore == user.CodiceFornitore)
                        );

                    var dati_lotto = gc.DatiLotti
                        .SingleOrDefault(a => 
                           (a.CodiceArticolo == codiceArticolo.ToUpper())
                        && (a.Lotto == lotto.ToUpper())
                        && (a.CodiceAzienda == user.CodiceAzienda)
                        && (a.CodiceFornitore == user.CodiceFornitore)
                        );

                    return Json(new
                    {
                        Successo = true,
                        MeseScadenza = dati_lotto == null ? 0 : dati_lotto.ScadenzaLotto.Month,
                        AnnoScadenza = dati_lotto == null ? 0 : dati_lotto.ScadenzaLotto.Year,
                        Costo = dati_costo.CostoNettoUnitario.ToString("N"),
                        Prezzo = dati_costo.PrezzoVendita.ToString("N")
                    });
                }
            }
            catch (Exception)
            {
                return Json(new
                {
                    Successo = false
                });
            }
        }
    }
}
