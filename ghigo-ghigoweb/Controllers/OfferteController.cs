using GhigoWeb.Models;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using GhigoWeb.Extensions;
using GhigoWeb.Filters;
using System.Globalization;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "Fornitore"), GhigoAccess]
    public class OfferteController : Controller
    {

        public ActionResult Index(string cerca, string apri_ordine)
        {
            var model = new OfferteViewModel();

            using (GhigoContext gc = new GhigoContext())
            {
                var pubblicato = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Pubblicato").StatoOrdine;

                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                var of = gc.OfferteFornitore
                    .Where(o => o.StatoOrdine == pubblicato) // offerte pubblicate
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => (o.QuantitaRichiestaResidua != 0) || (o.QuantitaOfferta != 0))
                    .Where(o => o.QuantitaConferita < o.QuantitaRichiestaResidua)
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

                model.ApriOrdine = string.Empty;

                if (!string.IsNullOrEmpty(apri_ordine))
                {
                    model.ApriOrdine = apri_ordine.Trim().ToLower();
                }

                model.NumeroOrdineOfferte = new Dictionary<string, IList<OffertaFornitore>>();
                foreach (var item in of.OrderByDescending(o => o.NumeroOrdine).ThenBy(o => o.Descrizione))
                {
                    if (!model.NumeroOrdineOfferte.ContainsKey(item.NumeroOrdine))
                    {
                        model.NumeroOrdineOfferte.Add(item.NumeroOrdine, new List<OffertaFornitore>());
                    }
                    model.NumeroOrdineOfferte[item.NumeroOrdine].Add(item);
                }

                model.Quando = of.Count() > 0 ?
                    of.Max(o => o.UltimaModifica).Ticks : 0;

                // ordini fornitori depositi
                var ordini = gc.OrdiniFornitoriDepositi
                    .Where(o => o.CodiceDeposito == user.CodiceFornitore) // del deposito associato al fornitore loggato
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => o.StatoOrdine == "NUOVO") // ordini nuovi
                    .OrderByDescending(o => o.DataOrdine)
                    .ThenByDescending(o => o.NumeroOrdine)
                    .ToList();
                model.OrdiniFornitoriDepositiDisponibili = ordini.Count > 0;

                ViewData["CodiceAzienda"] = user.CodiceAzienda;
                ViewData["CodiceDeposito"] = user.CodiceFornitore;

                ViewData["OrdiniFornitoriDepositiDisponibili"] = new SelectList(
                    ordini.Select(o => new {
                            RecId = o.RecId,
                            Descrizione = string.Format("Ordine {0} di {1}", o.NumeroOrdine, o.NomeAnagrafica) }),
                    "RecId", "Descrizione", string.Empty);

                ViewData["CodicePagamento"] = new SelectList(
                    gc.ElencoTabellaTipiPagamento.OrderBy(e => e.TipoPagamento).ToArray(),
                    "CodiceTipoPagamento", "TipoPagamento", string.Empty);

                var fornitore = gc.Database.SqlQuery<Fornitore>(
                    "EXEC APP_SP_WEB_SELECT_FORNITORI @CODICEAZIENDA, @CODICEANAGRAFICA",
                    new SqlParameter("CODICEAZIENDA", user.CodiceAzienda),
                    new SqlParameter("CODICEANAGRAFICA", user.CodiceFornitore));

                ViewData["CodiceAnagrafica"] = new SelectList(
                    fornitore.OrderBy(e => e.NomeAnagrafica).ToArray(),
                    "CodiceAnagrafica", "NomeAnagrafica", string.Empty);

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult SalvaQtaOfferta()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var recid = long.Parse(Request.Form["recid"]);
                    var value = int.Parse(Request.Form["value"]);

                    string msg = string.Empty;
                    bool successo = false;

                    var pubblicato = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Pubblicato").StatoOrdine;
                    var of = gc.OfferteFornitore.Single(o => o.RecId == recid);

                    try
                    {
                        var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                        cmd.CommandText = "APP_SP_WEB_INVIA_OFFERTA";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@recid", recid);
                        cmd.Parameters.AddWithValue("@qtaofferta", value);
                        if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                            cmd.Connection.Open();
                        cmd.ExecuteNonQuery();

                        successo = true;
                        msg = string.Format("Hai prenotato {0} pz di {1}.", value, of.Descrizione);
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Non e' stato possibile prenotare {0} pz di {1}. {2}",
                            value, of.Descrizione, ex.Message);
                    }

                    gc.Entry<OffertaFornitore>(of).Reload();

                    return Json(new
                    {
                        Successo = successo,
                        Messaggio = msg,
                        RecId = recid,
                        QuantitaConfermata = of.QuantitaConfermata,
                        QuantitaOfferta = of.QuantitaOfferta,
                        QuantitaRichiestaResidua = of.QuantitaRichiestaResidua,
                        Lotto = of.Lotto,
                        ScadenzaLotto = of.ScadenzaLotto.ToGhigoShortDateString(),
                        Visibile = of.StatoOrdine == pubblicato,
                        QuantitaDaOrdinare = of.QuantitaDaOrdinare,
                        PrezzoDaOrdinare = of.PrezzoDaOrdinare
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
        public ActionResult SalvaQtaDaOrdinare()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var recid = long.Parse(Request.Form["recid"]);
                    var ordine = long.Parse(Request.Form["ordine"]);
                    var qta = int.Parse(Request.Form["qta"]);
                    var prezzo = decimal.Parse(Request.Form["prezzo"], CultureInfo.InvariantCulture);

                    string msg = string.Empty;
                    bool successo = false;

                    var pubblicato = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Pubblicato").StatoOrdine;
                    var of = gc.OfferteFornitore.Single(o => o.RecId == recid);

                    try
                    {
                        var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                        cmd.CommandText = "APP_SP_WEB_INVIA_ORDINE_FORNITORE_DEPOSITO";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@offerta_recid", recid);
                        cmd.Parameters.AddWithValue("@ordine_recid", ordine);
                        cmd.Parameters.AddWithValue("@qta", qta);
                        cmd.Parameters.AddWithValue("@prezzo", prezzo);
     
                        if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                            cmd.Connection.Open();
                        cmd.ExecuteNonQuery();

                        successo = true;
                        msg = string.Format("Hai ordinato {0} pz di {1}.", qta, of.Descrizione);
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Non e' stato possibile ordinare {0} pz di {1}. {2}",
                            qta, of.Descrizione, ex.Message);
                    }

                    gc.Entry<OffertaFornitore>(of).Reload();

                    return Json(new
                    {
                        Successo = successo,
                        Messaggio = msg,
                        RecId = recid,
                        QuantitaConfermata = of.QuantitaConfermata,
                        QuantitaOfferta = of.QuantitaOfferta,
                        QuantitaRichiestaResidua = of.QuantitaRichiestaResidua,
                        Lotto = of.Lotto,
                        ScadenzaLotto = of.ScadenzaLotto.ToGhigoShortDateString(),
                        Visibile = of.StatoOrdine == pubblicato,
                        QuantitaDaOrdinare = of.QuantitaDaOrdinare,
                        PrezzoDaOrdinare = of.PrezzoDaOrdinare
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
        public ActionResult AggiornaElencoOfferte()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var pubblicato = gc.TabellaStatoOrdine.Single(s => s.Descrizione == "Pubblicato").StatoOrdine;
                var quando = new DateTime(long.Parse(Request.Form["quando"]));
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                // ordini successivi all'ultima data di modifica
                var ofs = gc.OfferteFornitore
                    .Where(o => o.UltimaModifica > quando)
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore)
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .AsQueryable();

                var res = ofs.OrderBy(o => o.RecId).ToArray()
                    .Select(o => new
                    {
                        RecId = o.RecId,
                        QuantitaConfermata = o.QuantitaConfermata,
                        QuantitaOfferta = o.QuantitaOfferta,
                        QuantitaRichiestaResidua = o.QuantitaRichiestaResidua,
                        Lotto = o.Lotto,
                        ScadenzaLotto = o.ScadenzaLotto.ToGhigoShortDateString(),
                        Visibile = o.StatoOrdine == pubblicato,
                        QuantitaDaOrdinare = o.QuantitaDaOrdinare,
                        PrezzoDaOrdinare = o.PrezzoDaOrdinare
                    });

                var ticks = ofs.Count() > 0 ?
                    ofs.Max(o => o.UltimaModifica).Ticks : 0;

                return Json(new { Quando = ticks, Items = res });
            }
        }

    }
}
