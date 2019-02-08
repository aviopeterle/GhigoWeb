using GhigoWeb.Extensions;
using GhigoWeb.Filters;
using GhigoWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Xml;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "GestioneOrdiniFornitoriDeposito"), GhigoAccess]
    public class OrdiniFornitoreDepositoController : Controller
    {
        public ActionResult Index(bool? tutti, string cerca)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                bool bTutti = tutti.HasValue && tutti.Value;
                ViewBag.Tutti = bTutti;

                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewData["CodiceAzienda"] = user.CodiceAzienda;
                ViewData["CodiceDeposito"] = user.CodiceFornitore;

                var pr = gc.OrdiniFornitoriDepositi
                    .Where(o => o.CodiceDeposito == user.CodiceFornitore)
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    ;

                ViewBag.Cerca = string.Empty;

                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    pr = pr.Where(x => x.NumeroOrdine.Contains(filtro) || x.Note.Contains(filtro));
                }

                // limit
                if(!bTutti)
                {
                    pr = pr.Take(10);
                }

                // ordinamento
                pr = pr 
                    .OrderByDescending(o => o.DataOrdine)
                    .ThenByDescending(o => o.NumeroOrdine);

                var model = pr.ToList();

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

        // [CodiceAzienda], [CodiceDeposito], [NumeroOrdine]
        public ActionResult Dettagli(string codicedeposito, string numeroordine)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.NumeroOrdine = numeroordine;
                ViewBag.CodiceDeposito = codicedeposito;
                ViewBag.CodiceAzienda = user.CodiceAzienda;

                var dettagli = gc.OrdiniFornitoriDepositiDettaglio
                    .Where(d => d.CodiceAzienda == user.CodiceAzienda)
                    .Where(d => d.CodiceDeposito == codicedeposito)
                    .Where(d => d.NumeroOrdine == numeroordine)
                    .OrderBy(d => d.Riga)
                    .ToList();

                return View(dettagli);
            }
        }

        [HttpPost]
        public ActionResult SalvaDettaglioOrdine(FormCollection fc)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "APP_SP_WEB_AGGIORNA_ORDINE_FORNITORE_DEPOSITO";

                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    cmd.Parameters.AddWithValue("@codiceazienda", fc["CodiceAzienda"]);
                    cmd.Parameters.AddWithValue("@codicedeposito", fc["CodiceDeposito"]);

                    var xml = new XmlDocument();
                    var root = xml.CreateElement("rows");
                    xml.AppendChild(root);

                    foreach(var key in fc.AllKeys)
                    {
                        if(key.StartsWith("txtQuantita"))
                        {
                            string recid = key.Replace("txtQuantita_", "");
                            var row = xml.CreateElement("row");
                            row.Attributes.Append(xml.CreateAttribute("recid")).Value = recid;
                            row.Attributes.Append(xml.CreateAttribute("quantita")).Value = fc[key];
                            row.Attributes.Append(xml.CreateAttribute("prezzo")).Value = fc[key.Replace("txtQuantita", "txtPrezzo")].Replace(",",".");
                            root.AppendChild(row);
                        }
                    }

                    cmd.Parameters.AddWithValue("@rows", xml.InnerXml);
                    cmd.ExecuteNonQuery();

                    return Json(new
                    {
                        Successo = true,
                        Messaggio = "Ordine aggiornato correttamente"
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
        public ActionResult NuovoArticolo()
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var codicearticolo = Request.Form["txtArticolo"];
                    var numeroordine = Request.Form["NumeroOrdine"];
                    var codicedeposito = Request.Form["CodiceDeposito"];
                    var codiceazienda = Request.Form["CodiceAzienda"];
                    var qta = int.Parse(Request.Form["txtQuantita"]);
                    var prezzo = Request.Form["txtPrezzo"].Replace(",", ".");

                    string msg = string.Empty;
                    bool successo = false;

                    var or = gc.OrdiniFornitoriDepositi
                            .Where(o => o.CodiceAzienda == codiceazienda)
                            .Where(o => o.CodiceDeposito == codicedeposito)
                            .Where(o => o.NumeroOrdine == numeroordine)
                            .Single();

                    var ar = gc.Articoli.First(a => a.CodiceArticolo == codicearticolo);

                    try
                    {
                        var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                        cmd.CommandText = "APP_SP_WEB_INVIA_ORDINE_FORNITORE_DEPOSITO";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@codicearticolo", codicearticolo);
                        cmd.Parameters.AddWithValue("@ordine_recid", or.RecId);
                        cmd.Parameters.AddWithValue("@qta", qta);
                        cmd.Parameters.AddWithValue("@prezzo", prezzo);

                        if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                            cmd.Connection.Open();
                        cmd.ExecuteNonQuery();

                        successo = true;
                        msg = string.Format("Hai ordinato {0} pz di {1}.", qta, ar.Descrizione);
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Non e' stato possibile ordinare {0} pz di {1}. {2}",
                            qta, ar.Descrizione, ex.Message);
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
                    Messaggio = "Errore: " + ex.Message,
                    RecId = 0
                });
            }
        }

        [HttpPost]
        public ActionResult Nuovo(FormCollection fc)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "APP_SP_WEB_GENERA_ORDINE_FORNITORE_DEPOSITO";

                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    SqlCommandBuilder.DeriveParameters(cmd);
                    cmd.Obj2Params(fc, string.Empty);

                    int recid = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Connection.Close();

                    var nuovo_ordine = gc.OrdiniFornitoriDepositi.Single(o => o.RecId == recid);
                    string descrizione = string.Format("Ordine {0} di {1}", nuovo_ordine.NumeroOrdine, nuovo_ordine.NomeAnagrafica);

                    return Json(new
                    {
                        Successo = true,
                        Messaggio = "Ordine creato correttamente",
                        RecId = recid,
                        Descrizione = descrizione
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

        public ActionResult CambiaStato(long recid, string stato)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "APP_SP_WEB_CAMBIA_STATO_ORDINE_FORNITORE_DEPOSITO";

                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@rec_id", recid);
                cmd.Parameters.AddWithValue("@stato", stato);

                cmd.ExecuteNonQuery();

                return RedirectToAction("Index");
            }
        }

        public ActionResult Tabellone(string ordineFornitore, string codiceFornitore, string articolo,
            bool? residuo, bool? anche_chiusi)
        {
            ordineFornitore = ordineFornitore ?? string.Empty;
            codiceFornitore = codiceFornitore ?? string.Empty;
            articolo = articolo ?? string.Empty;

            ordineFornitore = string.IsNullOrEmpty(ordineFornitore) ? string.Empty : ordineFornitore.Trim();
            ViewBag.OrdineFornitore = ordineFornitore;

            codiceFornitore = string.IsNullOrEmpty(codiceFornitore) ? string.Empty : codiceFornitore.Trim();
            ViewBag.CodiceFornitore = codiceFornitore;

            articolo = string.IsNullOrEmpty(articolo) ? string.Empty : articolo.Trim();
            ViewBag.Articolo = articolo;

            bool filtro_residuo = residuo ?? false;
            ViewBag.Residuo = filtro_residuo;

            bool filtro_anche_chiusi = anche_chiusi ?? false;
            ViewBag.AncheChiusi = filtro_anche_chiusi;

            using (var gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                var items = gc.Database.SqlQuery<StatoOrdineFornitoreDeposito>(
                    "EXEC APP_SP_WEB_STATO_ORDINI_FORNITORE_DEPOSITO @codiceFornitore, @ordineFornitore, @codiceDeposito, @articolo, @codiceAzienda, @residuo, @anche_chiusi",
                    new SqlParameter("codiceFornitore", codiceFornitore),
                    new SqlParameter("ordineFornitore", ordineFornitore),
                    new SqlParameter("codiceDeposito", user.CodiceFornitore),
                    new SqlParameter("articolo", articolo),
                    new SqlParameter("codiceAzienda", "GHIGO"),
                    new SqlParameter("residuo", filtro_residuo),
                    new SqlParameter("anche_chiusi", filtro_anche_chiusi)
                ).ToList();

                return View(items);
            }
        }

        [HttpPost]
        public ActionResult CreaCaricoEsterno(FormCollection fc)
        {
            try
            {
                XmlDocument xmldoc = new XmlDocument();

                string numero_documento_fornitore = fc["numero_documento_fornitore"];
                string data_documento = fc["data_documento"];

                var root = xmldoc.CreateElement("rows");
                xmldoc.AppendChild(root);

                foreach (string key in fc.Keys)
                {
                    if (key.StartsWith("lista_qta"))
                    {
                        var recid = key.Substring(key.IndexOf('[') + 1).TrimEnd(']');
                        var qta = fc[key];
                        var valore = fc[key.Replace("lista_qta", "lista_valori")];

                        var child = xmldoc.CreateElement("row");
                        root.AppendChild(child);

                        child.Attributes.Append(xmldoc.CreateAttribute("recid")).Value = recid;
                        child.Attributes.Append(xmldoc.CreateAttribute("qta")).Value = qta;
                        child.Attributes.Append(xmldoc.CreateAttribute("valore")).Value = valore;
                    }
                }

                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_CREA_CARICO_ESTERNO_DEPOSITO";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@xml", xmldoc.InnerXml);
                    cmd.Parameters.AddWithValue("@numero_documento_fornitore", numero_documento_fornitore);
                    cmd.Parameters.AddWithValue("@data_documento", data_documento);

                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string ddt_numero = Convert.ToString(dr["numeroddt"]);
                            long ddt_recid = Convert.ToInt64(dr["recid"]);

                            return Json(new
                            {
                                Successo = true,
                                Messaggio = string.Format("Ho generato il ddt numero {0}", ddt_numero),
                                RecId = ddt_recid
                            });
                        }
                    }
                }

                throw new Exception("Qualcosa è andato storto....");
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

        public ActionResult ElencoCarichi(bool? tutti, string cerca)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                gc.SaveChanges();

                bool bTutti = tutti.HasValue && tutti.Value;

                ViewBag.Tutti = bTutti;

                var carichi = gc.CarichiDeposito
                    .Where(c => c.CodiceAzienda == user.CodiceAzienda)
                    .Where(c => c.CodiceDeposito == user.CodiceFornitore)
                    .OrderByDescending(c => c.DataCarico)
                    .ThenByDescending(c => c.NumeroCarico)
                    .AsQueryable();

                // filtro cerca
                ViewBag.Cerca = string.Empty;
                if (!string.IsNullOrEmpty(cerca))
                {
                    bTutti = true;
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    carichi = carichi.Where(c => c.NumeroCarico.Contains(filtro));
                }

                var model = bTutti ? carichi.ToArray() : carichi.Take(10).ToArray();

                return View(model);
            }
        }
    }
}
