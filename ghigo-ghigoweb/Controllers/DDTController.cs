using GhigoWeb.Models;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using GhigoWeb.Filters;
using System.Text;
using GhigoWeb.Extensions;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "Fornitore"), GhigoAccess]
    public class DDTController : Controller
    {
        public ActionResult Index(bool? tutti, string tabella, string stampa, long? recid, string cerca, bool? da_fatturare)
        {
            var model = new ListaDDTViewModel();

            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.DepositoInterno = user.DepositoInterno;

                bool bDaFatturare = da_fatturare.HasValue && da_fatturare.Value;
                bool bTutti = tutti.HasValue && tutti.Value;

                ViewBag.Tutti = bTutti;
                ViewBag.DaFatturare = bDaFatturare;

                var ddtconf = gc.DDTConf
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .OrderByDescending(o => o.DataDDT)
                    .ThenByDescending(o => o.NumeroDocumento)
                    .AsQueryable();

                var ddtlib = gc.DDTFarmGros
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .OrderByDescending(o => o.DataDDT)
                    .ThenByDescending(o => o.NumeroDocumento)
                    .AsQueryable();

                var ddtven = gc.DDTVendite
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => o.CausaleDDT == "DDTVDG" || o.CausaleDDT == "DDTVDR") // causale vendita
                    .OrderByDescending(o => o.DataDDT)
                    .ThenByDescending(o => o.NumeroDocumento)
                    .AsQueryable();

                var ddtgf = gc.DDTVendite
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => o.CausaleDDT == "DDTGF") // causale grossista farmacia
                    .OrderByDescending(o => o.DataDDT)
                    .ThenByDescending(o => o.NumeroDocumento)
                    .AsQueryable();

                // filtro cerca
                model.Cerca = string.Empty;
                if (!string.IsNullOrEmpty(cerca))
                {
                    bTutti = true;
                    string filtro = cerca.Trim().ToLower();
                    model.Cerca = filtro;

                    ddtconf = ddtconf.Where(d => d.NumeroDocumento.Contains(filtro));
                    ddtlib = ddtlib.Where(d => d.NumeroDocumento.Contains(filtro));
                    ddtgf = ddtgf.Where(d => d.NumeroDocumento.Contains(filtro));
                    ddtven = ddtven.Where(d => d.NumeroDocumento.Contains(filtro));
                }

                model.Conferimenti = bDaFatturare ? ddtconf.Where(d => string.IsNullOrEmpty(d.NumeroFattura)).ToArray() :
                    bTutti ? ddtconf.ToArray() : ddtconf.Take(10).ToArray();
                model.Vendite = bDaFatturare ? ddtven.Where(d => string.IsNullOrEmpty(d.NumeroFattura)).ToArray() :
                    bTutti ? ddtven.ToArray() : ddtven.Take(10).ToArray();
                model.FarmaciaGrossista = bDaFatturare ? ddtlib.Where(d => string.IsNullOrEmpty(d.NumeroFattura)).ToArray() :
                    bTutti ? ddtlib.ToArray() : ddtlib.Take(10).ToArray();
                model.GrossistaFarmacia = bDaFatturare ? new DDTVendita[0] : 
                    bTutti ? ddtgf.ToArray() : ddtgf.Take(10).ToArray();

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

        public ActionResult ControlloArticoloLotto(string articolo, string lotto)
        {
            // se nessun articolo, esci
            if (string.IsNullOrEmpty(articolo))
                return View();

            ViewBag.Articolo = articolo;
            ViewBag.Lotto = lotto;

            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                var controllo_articolo_lotto = gc.ControlliArticoloLotto
                    .Where(o => o.CodiceFornitore == user.CodiceFornitore) // del fornitore specifico
                    .Where(o => o.CodiceAzienda == user.CodiceAzienda) // dell'azienda specifica
                    .Where(o => o.CodiceArticolo.Equals(articolo, StringComparison.InvariantCultureIgnoreCase));

                if(!string.IsNullOrEmpty(lotto))
                {
                    controllo_articolo_lotto = controllo_articolo_lotto
                        .Where(o => o.Lotto.Contains(lotto));
                }

                return View(controllo_articolo_lotto.OrderBy(o => o.DataDdT).ToList());
            }
        }

        public ActionResult GeneraDDTConferimento()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                int numeroColli = int.Parse(Request.Form["txtNumeroColli"]);
                string numeroDDT = Request.Form["txtNumeroDDT"];
                string codiceDeposito = Request.Form["selCodiceDeposito"];
                string note = Request.Form["txtNote"];
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                using (var cmd = gc.Database.Connection.CreateCommand() as SqlCommand)
                {
                    cmd.CommandText = "APP_SP_WEB_GENERA_DDT_CONFERIMENTO";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
                    cmd.Parameters.AddWithValue("@codicefornitore", user.CodiceFornitore);
                    cmd.Parameters.AddWithValue("@numerocolli", numeroColli);
                    cmd.Parameters.AddWithValue("@numeroddt", numeroDDT);
                    cmd.Parameters.AddWithValue("@codicedeposito", codiceDeposito);
                    cmd.Parameters.AddWithValue("@note", note);
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            DDTViewModel dvm = new DDTViewModel(dr);
                            return RedirectToAction("Index", new { tabella = dvm.Tabella, stampa = dvm.StoredStampa, recid = dvm.Recid });
                        }

                        return RedirectToAction("Index");
                    }
                }
            }
        }

        public ActionResult GeneraDDTLibero()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                int numeroColli = int.Parse(Request.Form["txtNumeroColli"]);
                long numeroproposta = long.Parse(Request.Form["hidNumeroProposta"]);
                string numeroDDT = Request.Form["txtNumeroDDT"];
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                using (var cmd = gc.Database.Connection.CreateCommand() as SqlCommand)
                {
                    cmd.CommandText = "APP_SP_WEB_GENERA_DDT_LIBERO";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
                    cmd.Parameters.AddWithValue("@codicefornitore", user.CodiceFornitore);
                    cmd.Parameters.AddWithValue("@numerocolli", numeroColli);
                    cmd.Parameters.AddWithValue("@numeroddt", numeroDDT);
                    cmd.Parameters.AddWithValue("@numeroproposta", numeroproposta);
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            DDTViewModel dvm = new DDTViewModel(dr);
                            return RedirectToAction("Index", new { tabella = dvm.Tabella, stampa = dvm.StoredStampa, recid = dvm.Recid });
                        }

                        return RedirectToAction("Index");
                    }
                }
            }
        }

        public ActionResult GeneraDDTDeposito()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                int numeroColli = int.Parse(Request.Form["txtNumeroColli"]);
                string numeroDDT = Request.Form["txtNumeroDDT"];
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                using (var cmd = gc.Database.Connection.CreateCommand() as SqlCommand)
                {
                    cmd.CommandText = "APP_SP_WEB_GENERA_DDT_DEPOSITO";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
                    cmd.Parameters.AddWithValue("@codicefornitore", user.CodiceFornitore);
                    cmd.Parameters.AddWithValue("@numerocolli", numeroColli);
                    cmd.Parameters.AddWithValue("@numeroddt", numeroDDT);
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            DDTViewModel dvm = new DDTViewModel(dr);
                            return RedirectToAction("Index", new { tabella = dvm.Tabella, stampa = dvm.StoredStampa, recid = dvm.Recid });
                        }

                        return RedirectToAction("Index");
                    }
                }
            }
        }

        public ActionResult FileTracciabilita_DdtVenditaDeposito(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandText = "APP_SP_REPORT_TRACCIABILITA_DDT_CONFERIMENTI_XML";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();
                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@rec_id"].Value = 0;
                cmd.Parameters["@rec_id_numeroddtvendita"].Value = recid;
                cmd.ExecuteNonQuery();

                string xml = Convert.ToString(cmd.Parameters["@xml"].Value);

                return File(Encoding.UTF8.GetBytes(xml), "text/xml", "file_tracciabilita.xml");
            }
        }

        public ActionResult FileTracciabilita_DdtGrossistaFarmacia(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandText = "APP_SP_REPORT_TRACCIABILITA_DDT_TRASFERIMENTI_DEPOSITI_XML";
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
