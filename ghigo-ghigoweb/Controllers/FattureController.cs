using GhigoWeb.Filters;
using GhigoWeb.Models;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using GhigoWeb.Extensions;
using System.Text;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "Fornitore"), GhigoAccess]
    public class FattureController : Controller
    {
        public ActionResult Index(bool? tutti, string tabella, string stampa, long? recid, string cerca)
        {
            var model = new FattureViewModel();

            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.DepositoInterno = user.DepositoInterno;

                bool bTutti = tutti.HasValue && tutti.Value;
                ViewBag.Tutti = bTutti;

                var fd = gc.FattureDeposito
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .OrderByDescending(o => o.DataDocumento)
                    .ThenByDescending(o => o.NumeroDocumento)
                    .AsQueryable();

                var fp = gc.FatturePrivate
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .OrderByDescending(o => o.DataDocumento)
                    .ThenByDescending(o => o.NumeroDocumento)
                    .AsQueryable();

                var fa = gc.FattureAccompagnatorieDeposito
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .OrderByDescending(o => o.DataDocumento)
                    .ThenByDescending(o => o.NumeroDocumento)
                    .AsQueryable();

                var nc = gc.NoteAccreditoPrivate
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .OrderByDescending(o => o.DataDocumento)
                    .ThenByDescending(o => o.NumeroDocumento)
                    .AsQueryable();

                var nd = gc.NoteAccreditoDeposito
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .OrderByDescending(o => o.DataDocumento)
                    .ThenByDescending(o => o.NumeroDocumento)
                    .AsQueryable();

                // filtro cerca
                model.Cerca = string.Empty;
                if (!string.IsNullOrEmpty(cerca))
                {
                    bTutti = true;
                    string filtro = cerca.Trim().ToLower();
                    model.Cerca = filtro;

                    fd = fd.Where(d => d.NumeroDocumento.Contains(filtro));
                    fp = fp.Where(d => d.NumeroDocumento.Contains(filtro));
                    fa = fa.Where(d => d.NumeroDocumento.Contains(filtro));
                    nc = nc.Where(d => d.NumeroDocumento.Contains(filtro));
                    nd = nd.Where(d => d.NumeroDocumento.Contains(filtro));
                }


                model.FattureDeposito = bTutti ? fd.ToArray() : fd.Take(10).ToArray();
                model.FatturePrivate = bTutti ? fp.ToArray() : fp.Take(10).ToArray();
                model.FattureAccompagnatorieDeposito = bTutti ? fa.ToArray() : fa.Take(10).ToArray();
                model.NoteAccreditoPrivate = bTutti ? nc.ToArray() : nc.Take(10).ToArray();
                model.NoteAccreditoDeposito = bTutti ? nd.ToArray() : nd.Take(10).ToArray();
                     
                if (recid.HasValue)
                {
                    ViewBag.RecId = recid;
                    ViewBag.Tabella = tabella;
                    ViewBag.Stampa = stampa;
                }
                else
                {
                    ViewBag.RecId = 0;
                }

                return View(model);
            }
        }

        public ActionResult NuovaFatturaPrivata_SceltaAnagrafica(long ddt_recid, string cerca, bool nota_accredito, long numeroproposta)
        {
            ViewBag.DdtRecId = ddt_recid;
            ViewBag.NotaAccredito = nota_accredito;
            ViewBag.NumeroProposta = numeroproposta;

            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var ddt = gc.DDTFarmGros.Find(ddt_recid);

                if ((ddt == null) && !nota_accredito) return HttpNotFound();

                // cerchiamo le anagrafiche private di questo fornitore
                var ana = gc.AnagrafichePrivate
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => !string.IsNullOrEmpty(o.RagioneSociale))
                    .Where(o => !string.IsNullOrEmpty(o.CodiceFiscale))
                    .OrderByDescending(o => o.RagioneSociale)
                    .AsQueryable();

                // filtro
                ViewBag.Cerca = string.Empty;
                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    ana = ana.Where(o => o.RagioneSociale.Contains(filtro)
                        || o.Indirizzo.Contains(filtro)
                        || o.Citta.Contains(filtro)
                        || o.CodiceFiscale.Contains(filtro)
                        || o.PartitaIva.Contains(filtro));
                }

                return View(ana.ToArray());
            }
        }

        public ActionResult NuovaFatturaPrivata_SceltaDestinazione(long ddt_recid, long ana_recid, string cerca, bool nota_accredito, long numeroproposta)
        {
            ViewBag.DdtRecId = ddt_recid;
            ViewBag.AnaRecId = ana_recid;
            ViewBag.NotaAccredito = nota_accredito;
            ViewBag.NumeroProposta = numeroproposta;

            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var ddt = gc.DDTFarmGros.Find(ddt_recid);
                var ana = gc.AnagrafichePrivate.Find(ana_recid);

                if (((ddt == null) && !nota_accredito) || (ana == null)) return HttpNotFound();

                // cerchiamo le destinazioni di questa anagrafica privata
                var dest = gc.DestinazioniPrivate
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => o.CodiceFiscale == ana.CodiceFiscale) // codice fiscale
                    .Where(o => !string.IsNullOrEmpty(o.DestinazioneRagioneSociale))
                    .OrderByDescending(o => o.RecId)
                    .AsQueryable();

                // filtro
                ViewBag.Cerca = string.Empty;
                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    dest = dest.Where(o => o.DestinazioneRagioneSociale.Contains(filtro)
                        || o.DestinazioneIndirizzo.Contains(filtro)
                        || o.DestinazioneCitta.Contains(filtro)
                        || o.CodiceFiscale.Contains(filtro));
                }

                var data = dest.ToArray();
                if ((data.Length == 0) && string.IsNullOrEmpty(cerca))
                    return RedirectToAction("NuovaFatturaPrivata_ModificaDati",
                        new { ddt_recid = ddt_recid, ana_recid = ana_recid, nota_accredito = nota_accredito, numeroproposta = numeroproposta });

                return View(dest.ToArray());
            }
        }

        public ActionResult NuovaFatturaPrivata_ModificaDati(long ddt_recid, long ana_recid, bool nota_accredito, long numeroproposta)
        {
            ViewBag.DdtRecId = ddt_recid;
            ViewBag.AnaRecId = ana_recid;
            ViewBag.NotaAccredito = nota_accredito;
            ViewBag.NumeroProposta = numeroproposta;

            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                ViewBag.CodiceAzienda = user.CodiceAzienda;
                ViewBag.CodiceFornitore = user.CodiceFornitore;

                var ana = gc.FatturePrivate.Find(ana_recid);

                if (ana == null)
                {
                    ana = new FatturaPrivata();
                    ana.CodicePorto = "A"; // assegnato
                    ana.CodiceAspetto = "S"; // scatole
                }
                else
                {
                    ana.NumeroDocumento = string.Empty;
                    ana.DataDocumento = DateTime.Now;
                }

                var esenzioni = gc.ElencoTabellaIva.ToList();
                esenzioni.Insert(0, new TabellaIva() { CodiceIva = string.Empty, Descrizione = "[Nessuna Esenzione]" });

                ViewData["EsenzioneIva"] = new SelectList(
                    esenzioni.ToArray(), "CodiceIva", "Descrizione", ana.EsenzioneIva);

                ViewData["CodicePagamento"] = new SelectList(
                    gc.ElencoTabellaTipiPagamento.OrderBy(e => e.TipoPagamento).ToArray(),
                    "CodiceTipoPagamento", "TipoPagamento", ana.CodicePagamento);

                ViewData["ItaliaEstero"] = new SelectList(new SelectListItem[]
                { new SelectListItem() { Text = "Italia", Value = "I" },
                  new SelectListItem() { Text = "Estero", Value = "E" } },
                  "Value", "Text", ana.ItaliaEstero);

                ViewData["TipoMittenteDestinatario"] = new SelectList(new SelectListItem[]
                { new SelectListItem() { Text = "Distributore", Value = "D" },
                  new SelectListItem() { Text = "Produttore", Value = "P" },
                  new SelectListItem() { Text = "Estero", Value = "E" } },
                    "Value", "Text", ana.TipoMittenteDestinatario);

                return View(ana);
            }
        }

        [HttpPost]
        public ActionResult NuovaFatturaPrivata_ModificaDati(FormCollection fc)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    string msg = string.Empty;
                    bool successo = false;
                    bool nota_accredito = fc["nota_accredito"] == "1";

                    try
                    {
                        var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                        if (nota_accredito)
                        {
                            cmd.CommandText = "APP_SP_WEB_GENERA_NOTA_CREDITO_PRIVATA";
                        }
                        else
                        {
                            cmd.CommandText = "APP_SP_WEB_GENERA_FATTURA_ACCOMPAGNATORIA_PRIVATA";
                        }
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                            cmd.Connection.Open();

                        SqlCommandBuilder.DeriveParameters(cmd);
                        cmd.Obj2Params(fc, string.Empty);
                        cmd.ExecuteNonQuery();

                        successo = true;
                        msg = "Fattura/Nota di accredito correttamente creata.";
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Errore nella generazione della fattura/nota di accredito: <br/> {0}",
                            ex.Message.Replace("\n", "<br/>"));
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

        public ActionResult EliminaFatturaPrivata(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var fatt = gc.FatturePrivate.SingleOrDefault(f => f.RecId == recid &&
                    f.CodiceAzienda == user.CodiceAzienda &&
                    f.CodiceFornitore == user.CodiceFornitore);

                if (fatt != null)
                {
                    gc.FatturePrivate.Remove(fatt);
                    gc.SaveChanges();
                }

                return RedirectToAction("Index");
            }
        }

        public ActionResult EliminaFatturaAccDeposito(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var fatt = gc.FattureAccompagnatorieDeposito.SingleOrDefault(f => f.RecId == recid &&
                    f.CodiceAzienda == user.CodiceAzienda &&
                    f.CodiceFornitore == user.CodiceFornitore);

                if (fatt != null)
                {
                    gc.FattureAccompagnatorieDeposito.Remove(fatt);
                    gc.SaveChanges();
                }

                return RedirectToAction("Index");
            }
        }

        public ActionResult EliminaNotaAccreditoPrivata(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var nc = gc.NoteAccreditoPrivate.SingleOrDefault(f => f.RecId == recid &&
                    f.CodiceAzienda == user.CodiceAzienda &&
                    f.CodiceFornitore == user.CodiceFornitore);

                if (nc != null)
                {
                    gc.NoteAccreditoPrivate.Remove(nc);
                    gc.SaveChanges();
                }

                return RedirectToAction("Index");
            }
        }

        public ActionResult EliminaNotaAccreditoDeposito(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var nc = gc.NoteAccreditoDeposito.SingleOrDefault(f => f.RecId == recid &&
                    f.CodiceAzienda == user.CodiceAzienda &&
                    f.CodiceFornitore == user.CodiceFornitore);

                if (nc != null)
                {
                    gc.NoteAccreditoDeposito.Remove(nc);
                    gc.SaveChanges();
                }

                return RedirectToAction("Index");
            }
        }

        public ActionResult FileTracciabilita_FatturaPrivata(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandText = "APP_SP_REPORT_TRACCIABILITA_FATTURA_ACCOMPAGNATORIA_PRIVATA_XML";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();
                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@rec_id"].Value = recid;
                cmd.ExecuteNonQuery();

                string xml = Convert.ToString(cmd.Parameters["@xml"].Value);

                return File(Encoding.UTF8.GetBytes(xml), "text/xml", "file_tracciabilita.xml");
            }
        }

        public ActionResult FileTracciabilita_FatturaAccompagnatoriaDeposito(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandText = "APP_SP_REPORT_TRACCIABILITA_FATTURA_ACCOMPAGNATORIA_DEPOSITO_XML";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();
                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@rec_id"].Value = recid;
                cmd.ExecuteNonQuery();

                string xml = Convert.ToString(cmd.Parameters["@xml"].Value);

                return File(Encoding.UTF8.GetBytes(xml), "text/xml", "file_tracciabilita.xml");
            }
        }
    }
}
