using GhigoWeb.Filters;
using GhigoWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "GestioneListino")]
    public class GestioneListinoController : Controller
    {
        public class CsvTableResult
        {
            public int Riga { get; set; }
            public string Codice { get; set; }
            public string Descrizione { get; set; }
            public string QuantitaMinima { get; set; }
            public string PrezzoAcquisto { get; set; }
            public string PrezzoVendita { get; set; }
            public string Ricarico { get; set; }
            public string GruppoAssortito { get; set; }
            public string Note { get; set; }
            public string Valido { get; set; }
        }

        public class ModificheViewModel
        {
            public IEnumerable<ModificaListino> Modifiche { get; set; }
            public IEnumerable<Tuple<string,string,string>> Testate { get; set; }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ImportaModifiche()
        {
            return View();
        }

        public ActionResult ImportaModificheStep2()
        {
            return View();
        }

        public ActionResult ImportaModificheStep3()
        {
            if (!Request.UserAgent.Contains("Chrome"))
            {
                return View("BrowserNotSupported");
            }

            var listini = new List<SelectListItem>();
            listini.Add(new SelectListItem() 
            { 
                Text = "[Seleziona il listino per proseguire]", 
                Value = string.Empty, 
                Selected = true 
            });
            listini.Add(new SelectListItem()
            {
                Text = "*** NUOVO LISTINO ***",
                Value = "[Nuovo]",
                Selected = false
            });

            using (GhigoContext gc = new GhigoContext())
            {
                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "APP_SP_WEB_SELECT_LISTINI_WEB_MODIFICABILI";

                cmd.Connection.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        listini.Add(new SelectListItem()
                        {
                            Text = Convert.ToString(dr["Descrizione"]),
                            Value = Convert.ToString(dr["CodiceListino"]),
                            Selected = false
                        });
                    }
                }
            }

            ViewBag.Listini = listini;

            return View();
        }

        [HttpPost]
        public ActionResult ConcludiImportazioneModifiche(string csv, string codiceListino, string nomeListino, string note)
        {
            try
            {
                if (string.IsNullOrEmpty(csv))
                    throw new Exception("Nessun dato da importare!");

                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "APP_SP_WEB_IMPORTA_CSV_MODIFICHE_LISTINO";
                    cmd.Parameters.AddWithValue("@csv", csv);
                    cmd.Parameters.AddWithValue("@codiceListino", codiceListino);
                    cmd.Parameters.AddWithValue("@nomeListino", nomeListino);
                    cmd.Parameters.AddWithValue("@note", note);

                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new
                {
                    Successo = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = ex.Message
                });
            }
        }

        public ActionResult ImportaModificheCompletata()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ControllaCsvModifiche(string csv)
        {
            try
            {
                if (string.IsNullOrEmpty(csv))
                    throw new Exception("Nessun dato da importare!");

                List<CsvTableResult> res = new List<CsvTableResult>();

                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "APP_SP_WEB_CONTROLLA_CSV_MODIFICHE_LISTINO";
                    cmd.Parameters.AddWithValue("@csv", csv);

                    cmd.Connection.Open();

                    using(var dr = cmd.ExecuteReader())
                    {
                        while(dr.Read())
                        {
                            res.Add(new CsvTableResult()
                            {
                                Riga = Convert.ToInt32(dr["Riga"]),
                                Codice = Convert.ToString(dr["Codice"]),
                                Descrizione = Convert.ToString(dr["Descrizione"]),
                                QuantitaMinima = Convert.ToString(dr["QuantitaMinima"]),
                                PrezzoAcquisto = Convert.ToString(dr["PrezzoAcquisto"]),
                                PrezzoVendita = Convert.ToString(dr["PrezzoVendita"]),
                                Ricarico = Convert.ToString(dr["Ricarico"]),
                                GruppoAssortito = Convert.ToString(dr["GruppoAssortito"]),
                                Note = Convert.ToString(dr["Note"]),
                                Valido = Convert.ToString(dr["Valido"])
                            });
                        }
                    }
                }

                // creazione table
                ViewData.Model = res;
                using (StringWriter sw = new StringWriter())
                {
                    ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, "_CsvResultTable");
                    ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                    viewResult.View.Render(viewContext, sw);

                    return Json(new
                    {
                        Successo = true,
                        Avanti = res.Count > 0 && res.Count(r => !string.IsNullOrEmpty(r.Valido)) == 0,
                        Messaggio = "Ok!",
                        Table = sw.GetStringBuilder().ToString()
                    }); 
                }
            } 
            catch(Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = ex.Message
                });
            }
        }

        public ActionResult ValutaModifiche()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                // modifiche in ordine di descrizione articolo
                var modifiche = gc.ModificheListino.Where(m => m.Variato).OrderBy(m => m.Descrizione).ToList();

                // testate dei listini
                var testate = modifiche.GroupBy(m => m.CodiceListino).Select(m => new Tuple<string, string, string>(m.Key, m.First().DescrizioneListino, m.First().NoteModifiche));

                return View(new ModificheViewModel() { Modifiche = modifiche, Testate = testate });
            }
        }

        [HttpPost]
        public ActionResult ConfermaModifica(string codice_listino, string codice_articolo, bool accetta)
        {
            try
            {
                if (string.IsNullOrEmpty(codice_listino))
                    throw new Exception("Manca il codice listino.");

                if (string.IsNullOrEmpty(codice_articolo))
                    throw new Exception("Manca il codice articolo.");

                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "APP_SP_WEB_CONFERMA_MODIFICA_LISTINO";
                    cmd.Parameters.AddWithValue("@codiceListino", codice_listino);
                    cmd.Parameters.AddWithValue("@codiceArticolo", codice_articolo);
                    cmd.Parameters.AddWithValue("@accetta", accetta);

                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new
                {
                    Successo = true,
                    Messaggio = String.Format("Ok, la modifica dell'articolo {0} è stata {1}", codice_articolo, (accetta ? "accettata." : "rifiutata."))
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = ex.Message
                });
            }
        }
    }
}
