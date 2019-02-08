using GhigoWeb.Models;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using GhigoWeb.Filters;
using GhigoWeb.Extensions;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "Fornitore"), GhigoAccess]
    public class ConferimentiController : Controller
    {

        public ActionResult Index(string cerca)
        {
            var model = new ConferimentiViewModel();

            using (GhigoContext gc = new GhigoContext())
            {
                var pubblicato = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Pubblicato").StatoOrdine;
                var offerta_chiusa = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Offerta chiusa").StatoOrdine;
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                var of = gc.OfferteFornitore
                    .Where(o => o.StatoOrdine == offerta_chiusa || o.StatoOrdine == pubblicato) // solo offerte chiuse
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => o.QuantitaDaConferire > 0) // di cui e' ancora necessario un conferimento
                    .AsQueryable();

                model.Cerca = string.Empty;

                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    model.Cerca = filtro;

                    of = of.Where(o => o.CodiceArticolo.Contains(filtro)
                        || o.Descrizione.Contains(filtro)
                        || o.Ean13.Contains(filtro)
                        || o.Code32.Contains(filtro));
                }

                model.NumeroOrdineConferimenti = new Dictionary<string, IList<OffertaFornitore>>();
                foreach (var item in of.OrderByDescending(o => o.NumeroOrdine).ThenBy(o => o.Descrizione))
                {
                    if (!model.NumeroOrdineConferimenti.ContainsKey(item.NumeroOrdine))
                    {
                        model.NumeroOrdineConferimenti.Add(item.NumeroOrdine, new List<OffertaFornitore>());
                    }
                    model.NumeroOrdineConferimenti[item.NumeroOrdine].Add(item);
                }

                model.Quando = of.Count() > 0 ?
                    of.Max(o => o.UltimaModifica).Ticks : 0;

                return View(model);
            }
        }

        public ActionResult InAttesa(string cerca)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.DepositoInterno = user.DepositoInterno;

                var cf = gc.Conferimenti
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => string.IsNullOrEmpty(o.Ddt)) // nessun ddt associato
                    .AsQueryable();

                ViewBag.Cerca = string.Empty;

                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    cf = cf.Where(o => o.CodiceArticolo.Contains(filtro)
                        || o.Descrizione.Contains(filtro)
                        || o.Ean13.Contains(filtro)
                        || o.Code32.Contains(filtro));
                }

                var model = new Dictionary<string, IList<Conferimento>>();

                foreach (var item in cf.OrderByDescending(o => o.NumeroOrdine).ThenBy(o => o.Descrizione))
                {
                    if (!model.ContainsKey(item.NumeroOrdine))
                    {
                        model.Add(item.NumeroOrdine, new List<Conferimento>());
                    }
                    model[item.NumeroOrdine].Add(item);
                }

                // depositi di destinazione (es. SEDE, RISERVATO)
                var depositi = cf.Select(o => new { Codice = o.CodiceDeposito, Descrizione = o.DescrizioneDepositoWeb }).Distinct().ToList();

                ViewBag.Depositi = depositi.Select(d => new SelectListItem() { Value = d.Codice, Text = d.Descrizione });

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult CreaFattura(FormCollection fc)
        {
            string msg = string.Empty;
            bool successo = false;

            try
            {
                long recid = long.Parse(fc["ddtrecid"]);
                string numeroFattura = fc["NumeroFattura"];

                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_GENERA_FATTURA_ACCOMPAGNATORIA_DA_DDT_CONFERIMENTO";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@rec_id", recid);
                    cmd.Parameters.AddWithValue("@numerofattura", numeroFattura);
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();
                    cmd.ExecuteNonQuery();

                    successo = true;
                    msg = "Fattura correttamente creata.";
                }
            }
            catch (Exception ex)
            {
                successo = false;
                msg = string.Format("Errore nella generazione della fattura: <br/> {0}",
                    ex.Message.Replace("\n", "<br/>"));
            }

            return Json(new
            {
                Successo = successo,
                Messaggio = msg
            });
        }

        [HttpPost]
        public ActionResult SalvaConferimento()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var recid = long.Parse(Request.Form["recid"]);
                    var value = int.Parse(Request.Form["value"]);
                    var lotto = Request.Form["lotto"];

                    string msg = string.Empty;
                    bool successo = false;

                    var pubblicato = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Pubblicato").StatoOrdine;
                    var offerta_chiusa = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Offerta chiusa").StatoOrdine;
                    var of = gc.OfferteFornitore.Single(o => o.RecId == recid);

                    try
                    {
                        // se non e' impostato il costo, impostiamo 0
                        string costoStr = Request.Form["costo"];
                        decimal costoNettoUnitario = 0;
                        if (!decimal.TryParse(costoStr, out costoNettoUnitario)) costoNettoUnitario = 0;

                        // scadenza
                        int mese = int.Parse(Request.Form["scadenzalottomese"]);
                        int anno = int.Parse(Request.Form["scadenzalottoanno"]);
                        if (anno < 100) anno += 2000;

                        // fine mese
                        DateTime scadenzalotto = new DateTime(anno, mese, 1);
                        scadenzalotto = scadenzalotto.AddMonths(1).AddDays(-1);

                        var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                        cmd.CommandText = "APP_SP_WEB_INVIA_CONFERIMENTO";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@recid", recid);
                        cmd.Parameters.AddWithValue("@qta", value);
                        cmd.Parameters.AddWithValue("@lotto", lotto);
                        cmd.Parameters.AddWithValue("@scadenzalotto", scadenzalotto);
                        cmd.Parameters.AddWithValue("@costonettounitario", costoNettoUnitario);
                        if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                            cmd.Connection.Open();
                        cmd.ExecuteNonQuery();

                        successo = true;
                        msg = string.Format("Hai conferito {0} pz di {1}.", value, of.Descrizione);
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Non e' stato possibile conferire {0} pz di {1}. {2}",
                            value, of.Descrizione, ex.Message);
                    }
                    gc.Entry<OffertaFornitore>(of).Reload();

                    return Json(new
                    {
                        Successo = successo,
                        Messaggio = msg,
                        RecId = recid,
                        QuantitaConfermata = of.QuantitaConfermata,
                        QuantitaConferita = of.QuantitaConferita,
                        QuantitaDaConferire = of.QuantitaDaConferire,
                        Visibile = (of.StatoOrdine == offerta_chiusa || of.StatoOrdine == pubblicato) && (of.QuantitaDaConferire > 0)
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = "Errore: " + ex.Message,
                    RecId = 0
                });
            }
        }

        [HttpPost]
        public ActionResult AggiornaElencoConferimenti()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var pubblicato = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Pubblicato").StatoOrdine;
                var offerta_chiusa = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Offerta chiusa").StatoOrdine;
                var quando = new DateTime(long.Parse(Request.Form["quando"]));
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                var ofs = gc.OfferteFornitore
                    .Where(o => o.UltimaModifica > quando)
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore)
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda)
                    .AsQueryable();

                var res = ofs.OrderBy(o => o.RecId).ToArray()
                    .Select(o => new
                    {
                        RecId = o.RecId,
                        QuantitaConfermata = o.QuantitaConfermata,
                        QuantitaConferita = o.QuantitaConferita,
                        QuantitaDaConferire = o.QuantitaDaConferire,
                        Visibile = (o.StatoOrdine == offerta_chiusa || o.StatoOrdine == pubblicato) && (o.QuantitaDaConferire > 0)
                    });

                var ticks = ofs.Count() > 0 ?
                    ofs.Max(o => o.UltimaModifica).Ticks : 0;

                return Json(new { Quando = ticks, Items = res });
            }
        }

    }
}
