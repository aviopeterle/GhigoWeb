using GhigoWeb.Filters;
using GhigoWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GhigoWeb.Extensions;
using System.Xml;


namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "Cliente"), GhigoAccess]
    public class OrdiniController : Controller
    {
        class ProduttoriSelectListItem : SelectListItem
        {
            private string _produttore;
            private string _codice;

            public string Produttore
            {
                get
                {
                    return _produttore;
                }
                set
                {
                    Text = _produttore = value;
                }
            }

            public string CodiceProduttore
            {
                get
                {
                    return _codice;
                }
                set
                {
                    Value = _codice = value;
                }
            }
        }

        public ActionResult ReteVendita()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "APP_SP_REPORT_RETE_XML";
                cmd.Parameters.AddWithValue("@CODICEAGENTE", user.CodiceFornitore);
                cmd.Parameters.AddWithValue("@procedura", "APP_SP_FATTURATO_RETE_XML");
                cmd.Parameters.AddWithValue("@padre", string.Empty);
                cmd.Parameters.AddWithValue("@percorso", string.Empty);
                cmd.Parameters.AddWithValue("@parametri", "WHERE isnull(OrdineChiuso,0)=1");
                var xmlparam = cmd.Parameters.Add(new SqlParameter("@xml", System.Data.SqlDbType.Xml, 1));
                xmlparam.Direction = System.Data.ParameterDirection.InputOutput;
                xmlparam.Value = Convert.DBNull;

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();

                XmlDocument xml = new XmlDocument();

                string xmlstr = Convert.ToString(xmlparam.Value);
                if(!string.IsNullOrEmpty(xmlstr))
                {
                    xml.LoadXml(xmlstr);
                }

                return View(xml);
            }
        }

        public ActionResult Index(bool? tutti, string cerca)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.CodiceAnagrafica = user.CodiceFornitore;
                ViewBag.CodiceAzienda = user.CodiceAzienda;

                bool bTutti = tutti.HasValue && tutti.Value;
                ViewBag.Tutti = bTutti;

                var o = gc.OrdiniClienteAperti
                    .Where(x => x.CodiceAnagrafica == user.CodiceFornitore || x.CodiceAgente == user.CodiceFornitore)
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .OrderByDescending(x => x.DataOrdine)
                    .ThenByDescending(x => x.NumeroOrdine)
                    .AsQueryable();

                ViewBag.Cerca = string.Empty;

                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    o = o.Where(x => x.NumeroOrdine.Contains(filtro)
                        || x.Note.Contains(filtro));
                }

                return View(bTutti ? o.ToArray() : o.Take(10).ToArray());
            }
        }

        public ActionResult OrdiniChiusi(bool? tutti, string cerca)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.CodiceAnagrafica = user.CodiceFornitore;
                ViewBag.CodiceAzienda = user.CodiceAzienda;

                bool bTutti = tutti.HasValue && tutti.Value;
                ViewBag.Tutti = bTutti;

                var o = gc.OrdiniClienteChiusi
                    .Where(x => x.CodiceAnagrafica == user.CodiceFornitore || x.CodiceAgente == user.CodiceFornitore)
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .OrderByDescending(x => x.DataOrdine)
                    .ThenByDescending(x => x.NumeroOrdine)
                    .AsQueryable();

                ViewBag.Cerca = string.Empty;

                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    o = o.Where(x => x.NumeroOrdine.Contains(filtro)
                        || x.Note.Contains(filtro));
                }

                return View(bTutti ? o.ToArray() : o.Take(10).ToArray());
            }
        }

        public ActionResult DettaglioOrdine(string numeroordine)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var ordine = gc.OrdiniCliente.SingleOrDefault(x =>
                        x.CodiceAzienda == user.CodiceAzienda
                    &&  (x.CodiceAnagrafica == user.CodiceFornitore || x.CodiceAgente == user.CodiceFornitore)
                    &&  x.NumeroOrdine == numeroordine
                    );
                if(ordine == null)
                    return new HttpNotFoundResult();

                var o = gc.DettagliOrdiniCliente
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .Where(x => x.NumeroOrdine == numeroordine)
                    .OrderBy(x => x.Descrizione)
                    .AsQueryable();

                return View(o.ToArray());
            }
        }

        public ActionResult Offerte(string cerca)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.CodiceAnagrafica = user.CodiceFornitore;
                ViewBag.CodiceAzienda = user.CodiceAzienda;

                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandText = "APP_SP_WEB_DATI_BONIFICO";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();

                using(SqlDataReader dr = cmd.ExecuteReader())
                {
                    if(dr.Read())
                    {
                        ViewBag.BonificoBanca = Convert.ToString(dr["Banca"]);
                        ViewBag.BonificoIban = Convert.ToString(dr["Iban"]);
                    }
                }

                var o = gc.OfferteCliente
                    .Where(x => x.CodiceAnagrafica == user.CodiceFornitore || x.CodiceAgente == user.CodiceFornitore)
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .Where(x => x.StatoOfferta == 2 // da valutare da parte di ghigo
                            ||  x.StatoOfferta == 3 // da valutare da parte del cliente
                    ).OrderByDescending(x => x.DataOfferta)
                    .ThenByDescending(x => x.NumeroOfferta)
                    .AsQueryable();

                ViewBag.DaValutareGhigo = gc.TabellaStatoOffertaCliente.Single(s => s.Codice == 2).Descrizione;
                ViewBag.DaValutareCliente = gc.TabellaStatoOffertaCliente.Single(s => s.Codice == 3).Descrizione;
                ViewBag.PagamentoAttivo = user.PagamentoAttivo;

                ViewBag.Cerca = string.Empty;

                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    o = o.Where(x => x.NumeroOfferta.Contains(filtro));
                }

                return View(o.ToArray());
            }
        }

        public ActionResult OfferteRifiutate(string cerca)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.CodiceAnagrafica = user.CodiceFornitore;
                ViewBag.CodiceAzienda = user.CodiceAzienda;

                var o = gc.OfferteCliente
                    .Where(x => x.CodiceAnagrafica == user.CodiceFornitore || x.CodiceAgente == user.CodiceFornitore)
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .Where(x => x.StatoOfferta == 6 // rifiuto da ghigo
                            || x.StatoOfferta == 8 // rifiutato da cliente
                    ).OrderByDescending(x => x.DataOfferta)
                    .ThenByDescending(x => x.NumeroOfferta)
                    .AsQueryable();

                ViewBag.RifiutoGhigo = gc.TabellaStatoOffertaCliente.Single(s => s.Codice == 6).Descrizione;
                ViewBag.RifiutoCliente = gc.TabellaStatoOffertaCliente.Single(s => s.Codice == 8).Descrizione;

                ViewBag.Cerca = string.Empty;

                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    o = o.Where(x => x.NumeroOfferta.Contains(filtro));
                }

                return View(o.ToArray());
            }
        }

        public ActionResult OfferteRicorrenti(string cerca)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                var o = gc.OfferteCliente
                    .Where(x => x.CodiceAnagrafica == user.CodiceFornitore || x.CodiceAgente == user.CodiceFornitore)
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .Where(x => x.StatoOfferta ==  9) // stato ricorrenza
                    .Where(x => x.Ricorrente)
                    .OrderByDescending(x => x.DataOfferta)
                    .ThenByDescending(x => x.NumeroOfferta)
                    .AsQueryable(); 

                ViewBag.Cerca = string.Empty;

                if (!string.IsNullOrEmpty(cerca))
                {
                    string filtro = cerca.Trim().ToLower();
                    ViewBag.Cerca = filtro;

                    o = o.Where(x => x.NumeroOfferta.Contains(filtro));
                }

                return View(o.ToArray());
            }
        }

        public ActionResult AnnullaOffertaRicorrente(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var offerta = gc.OfferteCliente.SingleOrDefault(x => x.RecId == recid);

                offerta.StatoOfferta = 8; // annullata cliente
                gc.SaveChanges();

                return RedirectToAction("OfferteRicorrenti");
            }
        }

        public ActionResult AttivaDisattivaOffertaRicorrente(long recid)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                    var off = gc.OfferteCliente.Single(o => o.RecId == recid);

                    off.RicorrenzaAttiva = !off.RicorrenzaAttiva;

                    gc.SaveChanges();

                    return Json(new
                    {
                        Successo = true,
                        Messaggio = "Offerta " + (off.RicorrenzaAttiva ? "ATTIVATA" : "DISATTIVATA") + " con successo.",
                        Attivo = off.RicorrenzaAttiva,
                        RecId = recid
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = "Errore: " + ex.Message,
                    RecId = recid
                });
            }
        }

        public ActionResult CopiaOffertaRicorrenteInProposta(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var offerta = gc.OfferteCliente.Single(o => o.RecId == recid);

                if (offerta != null)
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_COPIA_OFFERTA_IN_PROPOSTA";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();
                    cmd.Parameters.AddWithValue("@recid", recid);
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Proposta");
            }
        }

        public ActionResult DettaglioOfferta(string numeroofferta, int versioneofferta)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var offerta = gc.OfferteCliente
                    .SingleOrDefault(x => x.NumeroOfferta == numeroofferta 
                        && x.VersioneOfferta == versioneofferta
                        && x.CodiceAzienda == user.CodiceAzienda
                        && (x.CodiceAnagrafica == user.CodiceFornitore || x.CodiceAgente == user.CodiceFornitore));

                if(offerta == null) return new HttpNotFoundResult();

                ViewBag.OffertaRecId = offerta.RecId;

                var o = gc.DettagliOfferteCliente
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .Where(x => x.NumeroOfferta == numeroofferta)
                    //.Where(x => x.VersioneOfferta == versioneofferta)
                    .OrderBy(x => x.Riga)
                    .ToArray();

                if(offerta.StatoOfferta == 3)
                    return View("DettaglioOffertaValutazioneCliente", o);

                if (offerta.StatoOfferta == 2)
                    return View("DettaglioOffertaValutazioneGhigo", o);

                if(offerta.StatoOfferta == 9)
                    return View("DettaglioOffertaRicorrente", o);

                return View("DettaglioOffertaSolaLettura", o);
            }
        }

        public ActionResult ElencoProposte(string cerca)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.CodiceAzienda = user.CodiceAzienda;
                ViewBag.CodiceAnagrafica = user.CodiceFornitore;

                cerca = cerca ?? string.Empty;
                ViewBag.Cerca = cerca = cerca.Trim().ToLower();

                // clienti che è possibile scegliere
                var clienti = gc.Database.SqlQuery<AgenteClienti>("EXEC APP_SP_WEB_AGENTI_CLIENTI @codiceazienda, @login, @codiceanagrafica, @cerca",
                    new SqlParameter("codiceazienda", user.CodiceAzienda),
                    new SqlParameter("login", user.UserName),
                    new SqlParameter("codiceanagrafica", user.CodiceFornitore),
                    new SqlParameter("cerca", cerca)).ToList();

                // se nessun cliente, andiamo direttamente alla proposta
                if (clienti.Count() == 0 && string.IsNullOrEmpty(cerca))
                {
                    return RedirectToAction("Proposta");
                }

                // proposte attive per tutti i clienti
                var proposte = gc.ProposteOrdiniCliente
                    .Where(x => x.CodiceAnagrafica == user.CodiceFornitore)
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .OrderBy(x => x.Descrizione);

                ViewBag.Proposte = proposte;

                var clienti_con_proposta = proposte.Select(p => p.CodiceCliente).Distinct().ToList();

                ViewBag.ClientiConProposta = clienti.Where(c => clienti_con_proposta.Contains(c.CodiceCliente)).ToArray();
                ViewBag.ClientiSenzaProposta = clienti.Where(c => !clienti_con_proposta.Contains(c.CodiceCliente)).ToArray();

                return View();
            }

        }

        public ActionResult Proposta(string codiceCliente)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;
                gc.SaveChanges();

                ViewBag.CodiceAzienda = user.CodiceAzienda;
                ViewBag.CodiceAnagrafica = user.CodiceFornitore;
                ViewBag.PagamentoAttivo = user.PagamentoAttivo;
                ViewBag.Agente = user.Agente;

                // abbiamo specificato un cliente?
                if (string.IsNullOrEmpty(codiceCliente))
                {
                    // prendiamo l'ultimo se possibile
                    string ultimo_cliente = Convert.ToString(Session["_ultimoClienteScelto"]);

                    if(string.IsNullOrEmpty(ultimo_cliente))
                    {
                        // altrimenti noi stessi
                        codiceCliente = user.CodiceFornitore;
                    }
                    else
                    {
                        codiceCliente = ultimo_cliente;
                    }
                }
                else
                {
                    Session["_ultimoClienteScelto"] = codiceCliente;
                }

                ViewBag.CodiceCliente = codiceCliente;

                if(codiceCliente.StartsWith("P"))
                {
                    var clipot = gc.ClientiPotenziali.SingleOrDefault(c => c.CodiceAnagrafica == codiceCliente);
                    TabellaComuni comune = gc.ElencoTabellaComuni.SingleOrDefault(c => c.CodiceComune == clipot.CodiceComune);
                    ViewData["CodiceComune"] = comune == null ? "-" : string.Format("{0} ({1})", comune.Comune, comune.CodiceProvincia);
                    ViewBag.Cliente = clipot;
                }
                else
                {
                    var cli = gc.Clienti.SingleOrDefault(c => c.CodiceAnagrafica == codiceCliente);
                    ViewData["CodiceComune"] = cli.Comune;
                    ViewBag.Cliente = cli;
                }

                // proposta del cliente selezionato
                var p = gc.ProposteOrdiniCliente
                    .Where(x => x.CodiceAnagrafica == user.CodiceFornitore)
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .Where(x => x.CodiceCliente == codiceCliente)
                    .OrderBy(x => x.Descrizione)
                    .AsQueryable();

                decimal imponibile = 0;
                decimal imposta = 0;
                decimal trasporto = 0;
                decimal totale = 0;

                bool trasportoGratuito = Session["_trasportoGratuito"] == null ? false : (bool)Session["_trasportoGratuito"];
                ViewBag.TrasportoGratuito = trasportoGratuito;

                CalcolaTotaliProposta(codiceCliente, gc, user, trasportoGratuito, ref imponibile, ref imposta, ref trasporto, ref totale);

                ViewBag.Imponibile = imponibile.ToString("C");
                ViewBag.Imposta = imposta.ToString("C");
                ViewBag.Trasporto = trasporto.ToString("C");
                ViewBag.Totale = totale.ToString("C");

                // produttori
                string ultimoProduttoreScelto = (string)Session["_ultimoProduttoreScelto"] ?? string.Empty;

                var produttori = gc.Database.SqlQuery<ProduttoriSelectListItem>
                    ("EXEC APP_SP_WEB_LISTINI @codiceAzienda, @codiceAnagrafica, @codiceCliente",
                new SqlParameter("codiceAzienda", user.CodiceAzienda),
                new SqlParameter("codiceAnagrafica", user.CodiceFornitore),
                new SqlParameter("codiceCliente", codiceCliente)
                ).ToList();

                ViewBag.Produttori = new SelectList(produttori, "Value", "Text", ultimoProduttoreScelto);

                // destinazioni
                var destinazioni = gc.DestinazioniCliente
                    .Where(x => x.CodiceAnagrafica == codiceCliente)
                    .OrderBy(x => x.NomeDestinazione)
                    .Select(pr => new SelectListItem() { Text = pr.NomeDestinazione, Value = pr.CodiceDestinazione })
                    .ToArray();
                ViewBag.Destinazioni = destinazioni.Length > 0 ? 
                    new SelectList(destinazioni, "Value", "Text", destinazioni[0].Value) : 
                    new SelectList(destinazioni, "Value", "Text");

                // periodi di ricorrenza
                var periodi = gc.ElencoTabellaPeriodi
                    .OrderBy(pr => pr.Id)
                    .Select(pr => new SelectListItem() { Text = pr.Descrizione, Value = pr.Codice })
                    .ToArray();
                ViewBag.Periodi = new SelectList(periodi, "Value", "Text");

                return View(p.ToArray());
            }
        }

        private static void CalcolaTotaliProposta(string codiceCliente, GhigoContext gc, UserProfile user, bool trasportoGratuito, ref decimal imponibile, ref decimal imposta,
            ref decimal trasporto, ref decimal totale)
        {
            var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
            cmd.CommandText = "APP_SP_WEB_TOTALI_PROPOSTA";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
            cmd.Parameters.AddWithValue("@codiceanagrafica", user.CodiceFornitore);
            cmd.Parameters.AddWithValue("@codicecliente", codiceCliente);
            cmd.Parameters.AddWithValue("@trasportoGratuito", trasportoGratuito);

            if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                cmd.Connection.Open();

            using (var dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    imponibile = Convert.ToDecimal(dr["imponibile"]);
                    imposta = Convert.ToDecimal(dr["imposta"]);
                    trasporto = Convert.ToDecimal(dr["trasporto"]);
                    totale = Convert.ToDecimal(dr["totale"]);
                }
            }
        }

        public ActionResult Listino(string cerca, string produttore, string codiceCliente, string evidenzia_articolo, string codiceCategoria, string tipoCategoria)
        {
            /*
            // impediamo la visione del listino senza produttore
            if(string.IsNullOrEmpty(produttore))
            {
                return RedirectToAction("Proposta");
            }
            */
            produttore = produttore ?? string.Empty;

            GhigoContext gc = new GhigoContext();

            var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
            user.UltimoAccesso = DateTime.Now;
            ViewBag.MessaggioWeb = user.MessaggioWeb;
            gc.SaveChanges();

            // se non c'è il codiceCliente, verifichiamo se ne abbiamo disponibili
            if(string.IsNullOrEmpty(codiceCliente))
            {
                // clienti che è possibile scegliere
                var clienti = gc.AgentiClienti
                    .Where(x => x.CodiceAnagrafica == user.CodiceFornitore)
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .OrderBy(x => x.Cliente)
                    .ToArray();

                if(clienti.Length == 0)
                {
                    // nessuno, usiamo noi stessi
                    codiceCliente = user.CodiceFornitore;
                }
                else
                {
                    // ce nè almeno 1, dobbiamo purtroppo tornare alla proposta
                    return RedirectToAction("Proposta");
                }
            }

            Session["_ultimoProduttoreScelto"] = produttore;

            ViewBag.CodiceAzienda = user.CodiceAzienda;
            ViewBag.CodiceAnagrafica = user.CodiceFornitore;
            ViewBag.CodiceCliente = codiceCliente;

            string filtro_produttore = produttore.Trim().ToLower();
            ViewBag.Produttore = filtro_produttore;

            ViewBag.Cerca = string.Empty;
            if(!string.IsNullOrEmpty(cerca))
            {
                cerca = cerca.Trim().ToLower();
                ViewBag.Cerca = cerca;
            }

            var listino = gc.Database.SqlQuery<Listino>
                ("EXEC APP_SP_WEB_LISTINO @codiceAzienda, @codiceAnagrafica, @codiceProduttore, @codiceCliente, @cerca, @tipoCategoria",
                new SqlParameter("codiceAzienda", user.CodiceAzienda),
                new SqlParameter("codiceAnagrafica", user.CodiceFornitore),
                new SqlParameter("codiceProduttore", filtro_produttore),
                new SqlParameter("codiceCliente", codiceCliente),
                new SqlParameter("cerca", cerca ?? string.Empty),
                new SqlParameter("tipoCategoria", tipoCategoria ?? string.Empty)
                ).ToList();

            ViewBag.EvidenziaArticolo = evidenzia_articolo ?? string.Empty;
            ViewBag.Descrizione = filtro_produttore;
            ViewBag.CodiceCategoria = string.Empty;
            ViewBag.TipoCategoria = (tipoCategoria ?? string.Empty).ToLower();

            if(listino.Count()>0)
            {
                ViewBag.Descrizione = listino.First().Produttore;
                if(string.IsNullOrEmpty(filtro_produttore))
                {
                    ViewBag.Produttore = listino.First().CodiceProduttore;
                }
            }

            // controlliamo se ci sono delle categorie
            var categorie = new Dictionary<string, string>();
            var coloreCategoria = new Dictionary<string, string>();
            foreach (var item in listino.Where(l => !string.IsNullOrEmpty(l.CodiceCategoria)).OrderBy(l => l.OrdinamentoCategoria))
            {
                if(!categorie.ContainsKey(item.CodiceCategoria))
                {
                    categorie[item.CodiceCategoria] = item.Categoria;
                    coloreCategoria[item.CodiceCategoria] = item.ColoreCategoria;
                }
            }

            if (categorie.Count > 0)
            {
                if (string.IsNullOrEmpty(codiceCategoria) && string.IsNullOrEmpty(cerca))
                {
                    if (categorie.Count > 1)
                    {
                        // dobbiamo andare all'elenco delle categorie
                        ViewBag.ColoreCategoria = coloreCategoria;
                        return View("ElencoCategorieListino", categorie);
                    }

                    codiceCategoria = categorie.Single().Key;
                }

                if(!string.IsNullOrEmpty(codiceCategoria))
                {
                    listino = listino.Where(l => l.CodiceCategoria == codiceCategoria).ToList();
                }

                ViewBag.Descrizione = string.Format("{0} Categoria: {1}", ViewBag.Descrizione, (listino.Count == 0 ? "Categoria generica" : listino.First().Categoria));
                ViewBag.CodiceCategoria = codiceCategoria;
            }

            // destinazioni
            var destinazioni = gc.DestinazioniCliente
                .Where(x => x.CodiceAnagrafica == codiceCliente)
                .OrderBy(x => x.NomeDestinazione)
                .ToArray();
            ViewBag.Destinazioni = destinazioni;
            ViewBag.DividiQta = destinazioni.Length > 1;

            // primo check
            var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
            cmd.CommandText = "APP_SP_WEB_INVIA_PROPOSTA_OFFERTA_CLIENTE";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@codart", string.Empty);
            cmd.Parameters.AddWithValue("@qta", 0);
            cmd.Parameters.AddWithValue("@prezzoProposto", 0);
            cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
            cmd.Parameters.AddWithValue("@codiceanagrafica", user.CodiceFornitore);
            cmd.Parameters.AddWithValue("@codicelistino", filtro_produttore);
            cmd.Parameters.AddWithValue("@codiceCliente", codiceCliente);
            if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                cmd.Connection.Open();

            ViewBag.Hint = Convert.ToString(cmd.ExecuteScalar());


            return View(listino);
        }

        [HttpPost]
        public JsonResult TotaliProposta(string codiceCliente, bool trasportoGratuito)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                    Session["_trasportoGratuito"] = trasportoGratuito;

                    decimal imponibile = 0;
                    decimal imposta = 0;
                    decimal trasporto = 0;
                    decimal totale = 0;

                    CalcolaTotaliProposta(codiceCliente, gc, user, trasportoGratuito, ref imponibile, ref imposta, ref trasporto, ref totale);

                    return Json(new
                    {
                        Successo = true,
                        Imponibile = imponibile.ToString("C"),
                        Imposta = imposta.ToString("C"),
                        Trasporto = trasporto.ToString("C"),
                        Totale = totale.ToString("C")
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
        public JsonResult EliminaProposta(long recid)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                    var prop = gc.ProposteOrdiniCliente.Single(f => f.RecId == recid &&
                        f.CodiceAzienda == user.CodiceAzienda &&
                        f.CodiceAnagrafica == user.CodiceFornitore);

                    string codiceCliente = prop.CodiceCliente;

                    SalvaQta(prop.CodiceArticolo, "0", "0", prop.CodiceCliente, prop.CodiceListino, string.Empty);

                    decimal imponibile = 0;
                    decimal imposta = 0;
                    decimal trasporto = 0;
                    decimal totale = 0;

                    bool trasportoGratuito = Session["_trasportoGratuito"] == null ? false : (bool)Session["_trasportoGratuito"];

                    CalcolaTotaliProposta(codiceCliente, gc, user, trasportoGratuito, ref imponibile, ref imposta, ref trasporto, ref totale);

                    return Json(new
                    {
                        Successo = true,
                        Messaggio = "Proposta rimossa con successo.",
                        Imponibile = imponibile.ToString("C"),
                        Imposta = imposta.ToString("C"),
                        Trasporto = trasporto.ToString("C"),
                        Totale = totale.ToString("C"),
                        RecId = recid
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = "Errore: " + ex.Message,
                    RecId = recid
                });
            }
        }

        public ActionResult SvuotaProposta(string codiceCliente)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                user.UltimoAccesso = DateTime.Now;
                ViewBag.MessaggioWeb = user.MessaggioWeb;

                var p = gc.ProposteOrdiniCliente
                    .Where(x => x.CodiceAnagrafica == user.CodiceFornitore)
                    .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                    .Where(x => x.CodiceCliente == codiceCliente)
                    .OrderBy(x => x.Descrizione)
                    .AsQueryable();

                foreach (var item in p)
                {
                    SalvaQta(item.CodiceArticolo, "0", "0", codiceCliente, item.CodiceListino, string.Empty);
                }
                gc.SaveChanges();

                return RedirectToAction("Proposta", new { codiceCliente = codiceCliente });
            }
        }

        /*
        [HttpPost]
        public JsonResult OttieniDatiArticolo(string articolo)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var art = gc.Listino.SingleOrDefault(a => a.CodiceArticolo == articolo);

                    // se non trovato
                    if (art == null)
                    {
                        // cerco di copiare l'articolo
                        var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                        cmd.CommandText = "APP_SP_COPIA_FARMA_PRODOTTI";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@codiceazienda", string.Empty);
                        cmd.Parameters.AddWithValue("@codicearticolo", articolo);
                        if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                            cmd.Connection.Open();
                        cmd.ExecuteNonQuery();

                        art = gc.Listino.Single(a => a.CodiceArticolo == articolo);
                    }

                    return Json(new
                    {
                        Trovato = true,
                        Descrizione = art.Descrizione,
                        Prezzo = art.Prezzo.ToString("N"),
                        PrezzoPubblico = art.PrezzoPubblico.ToString("N"),
                        ScontoLordo = art.ScontoLordo.ToString("N"),
                        ScontoNetto = art.ScontoNetto.ToString("N"),
                        MinimoOrdinabile = art.MinimoOrdinabile,
                        Note = art.Note
                    });
                }
            }
            catch (Exception)
            {
                return Json(new
                {
                    Trovato = false,
                    Descrizione = "NON DISPONIBILE"
                });
            }
        }

        [HttpPost]
        public JsonResult InserisciProposta()
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var codart = Request.Form["codart"];
                var qta = int.Parse(Request.Form["qta"]);
                string prezzoPropostoStr = Request.Form["prezzoProposto"];
                decimal prezzoProposto = 0;
                if (!decimal.TryParse(prezzoPropostoStr, out prezzoProposto)) prezzoProposto = 0;

                // scadenza
                int mese = int.Parse(Request.Form["scadenzalottomese"]);
                int anno = int.Parse(Request.Form["scadenzalottoanno"]);

                // fine mese
                DateTime scadenzalotto = new DateTime(1900, 1, 1);
                if (mese != 0 || anno != 0)
                {
                    if (anno < 100) anno += 2000;
                    scadenzalotto = new DateTime(anno, mese, 1);
                    scadenzalotto = scadenzalotto.AddMonths(1).AddDays(-1);
                }

                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var art = gc.Articoli
                    .Single(o => o.CodiceAzienda == user.CodiceAzienda && o.CodiceArticolo == codart);

                string msg = string.Empty;
                bool successo = false;
                long new_recid = 0;

                try
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_INVIA_PROPOSTA_OFFERTA_CLIENTE";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codart", codart);
                    cmd.Parameters.AddWithValue("@qta", qta);
                    cmd.Parameters.AddWithValue("@prezzoProposto", prezzoProposto);
                    cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
                    cmd.Parameters.AddWithValue("@codiceanagrafica", user.CodiceFornitore);
                    cmd.Parameters.AddWithValue("@scadenzalotto", scadenzalotto);
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();
                    new_recid = Convert.ToInt64(cmd.ExecuteScalar());

                    successo = true;
                    msg = string.Format("Hai richiesto {0} pz di {1}.", qta, art.Descrizione);
                }
                catch (Exception ex)
                {
                    successo = false;
                    msg = string.Format("Non e' stato possibile prenotare {0} pz di {1}. {2}",
                        qta, art.Descrizione, ex.Message);
                }

                return Json(new
                {
                    Successo = successo,
                    Messaggio = msg,
                    RecId = new_recid
                });
            }
        }
                */

        [HttpPost]
        public ActionResult InviaOfferta(FormCollection fc)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    string msg = string.Empty;
                    bool successo = false;

                    try
                    {
                        var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                        cmd.CommandText = "APP_SP_WEB_GENERA_OFFERTA_CLIENTE";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                            cmd.Connection.Open();

                        SqlCommandBuilder.DeriveParameters(cmd);
                        cmd.Obj2Params(fc, string.Empty);
                        cmd.ExecuteNonQuery();

                        // resettiamo il flag del trasporto
                        Session["_trasportoGratuito"] = null;

                        successo = true;
                        msg = "Offerta correttamente creata.";
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Errore nella generazione dell'offerta: <br/> {0}",
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

        public ActionResult AnnullaOfferta(long recid)
        {
            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                var offerta = gc.OfferteCliente
                    .SingleOrDefault(x => x.RecId == recid
                        && x.CodiceAzienda == user.CodiceAzienda
                        && x.CodiceAnagrafica == user.CodiceFornitore
                        && (x.StatoOfferta == 2 || x.StatoOfferta == 3));

                if (offerta != null)
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_ANNULLA_OFFERTA_CLIENTE";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();
                    cmd.Parameters.AddWithValue("@recid", recid);
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Offerte");
            }
        }

        [HttpPost]
        public ActionResult ModificaOfferta(FormCollection fc)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    long recid = long.Parse(fc["hidOffertaRecId"]);

                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                    var offerta = gc.OfferteCliente
                        .SingleOrDefault(x => x.RecId == recid
                            && x.CodiceAzienda == user.CodiceAzienda
                            && x.CodiceAnagrafica == user.CodiceFornitore
                            && x.StatoOfferta == 3);

                    if (offerta == null)
                        throw new Exception("Offerta non trovata oppure non modificabile.");

                    OffertaClienteDettaglio dettaglio = null;

                    foreach (string key in fc.Keys)
                    {
                        if (key.StartsWith("txtQuantita_recid_"))
                        {
                            recid = long.Parse(key.Substring("txtQuantita_recid_".Length));
                            dettaglio = gc.DettagliOfferteCliente.SingleOrDefault(x => x.RecId == recid);
                            if (dettaglio != null)
                            {
                                dettaglio.QuantitaRichiesta = int.Parse(fc[key]);
                            }
                        }
                        else
                        {
                            if (key.StartsWith("txtPrezzoRichiesto_recid_"))
                            {
                                recid = long.Parse(key.Substring("txtPrezzoRichiesto_recid_".Length));
                                dettaglio = gc.DettagliOfferteCliente.SingleOrDefault(x => x.RecId == recid);
                                if (dettaglio != null)
                                {
                                    dettaglio.PrezzoRichiesto = decimal.Parse(fc[key]);
                                }
                            }
                        }
                    }

                    offerta.StatoOfferta = 2;
                    offerta.DataRichiestaCliente = DateTime.Now;
                    offerta.UltimaModifica = DateTime.Now;

                    gc.SaveChanges();

                    return Json(new
                    {
                        Successo = true ,
                        Messaggio = "Offerta modificata correttamente."
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
        public ActionResult SalvaQta(string codart, string qta, string prezzo, string codiceCliente, string codiceProduttore, string divisione)
        {
            int quantita_prec = 0;
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_DATI_ARTICOLO_PROPOSTA";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codart", codart);
                    cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
                    cmd.Parameters.AddWithValue("@codiceanagrafica", user.CodiceFornitore);
                    cmd.Parameters.AddWithValue("@codicecliente", codiceCliente);
                    cmd.Parameters.AddWithValue("@codiceproduttore", codiceProduttore);

                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    string articolo_descrizione = codart;

                    using (var dr = cmd.ExecuteReader())
                    {
                        if(dr.Read())
                        {
                            quantita_prec = Convert.ToInt32(dr["inproposta"]);
                            articolo_descrizione = Convert.ToString(dr["descrizione"]);
                        }
                    }


                    int iQta = int.Parse(qta);
                    decimal dPrezzo = decimal.Parse(prezzo.Replace('.', ','));

                    cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_INVIA_PROPOSTA_OFFERTA_CLIENTE";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codart", codart);
                    cmd.Parameters.AddWithValue("@qta", iQta);
                    cmd.Parameters.AddWithValue("@prezzoProposto", dPrezzo);
                    cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
                    cmd.Parameters.AddWithValue("@codiceanagrafica", user.CodiceFornitore);
                    cmd.Parameters.AddWithValue("@codicelistino", codiceProduttore);
                    cmd.Parameters.AddWithValue("@codiceCliente", codiceCliente);
                    cmd.Parameters.AddWithValue("@divisione", divisione);
                    // cmd.Parameters.AddWithValue("@scadenzalotto", scadenzalotto);

                    string hint = Convert.ToString(cmd.ExecuteScalar());

                    gc.SaveChanges();

                    return Json(new
                    {
                        Successo = true,
                        Messaggio = string.Format("Quantità di {0} modificate correttamente.", articolo_descrizione),
                        Quantita = iQta,
                        CodiceArticolo = codart,
                        Hint = hint ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Successo = false,
                    Messaggio = "Errore: " + ex.Message,
                    Quantita = quantita_prec,
                    CodiceArticolo = codart,
                    Hint = string.Empty
                });
            }
        }

        [HttpPost]
        public ActionResult LeggiDivisioneQta(string codart, string qta, string codiceProduttore, string codiceCliente)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                    int iQta = int.Parse(qta);

                    var divisioni = gc.ProposteOrdiniClienteDestinazioni
                        .Where(x => x.CodiceAnagrafica == user.CodiceFornitore)
                        .Where(x => x.CodiceAzienda == user.CodiceAzienda)
                        .Where(x => x.CodiceListino == codiceProduttore)
                        .Where(x => x.CodiceCliente == codiceCliente)
                        .Where(x => x.CodiceArticolo == codart)
                        .ToList();

                    Dictionary<string, int> divisione = new Dictionary<string, int>();
                    foreach(var item in divisioni)
                    {
                        divisione.Add(item.CodiceDestinazione, item.Quantita);
                    }

                    return Json(new
                    {
                        Successo = true,
                        Divisione = divisione
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

        public ActionResult ModificaClientePotenziale(long cli_recid)
        {
            ViewBag.CliRecId = cli_recid;

            using (GhigoContext gc = new GhigoContext())
            {
                var user = gc.UserProfiles.Single(u => u.UserName == User.Identity.Name);
                ViewBag.CodiceAzienda = user.CodiceAzienda;
                ViewBag.CodiceFornitore = user.CodiceFornitore;

                var cli = gc.ClientiPotenziali.Find(cli_recid);

                if (cli == null)
                {
                    cli = new ClientePotenziale();
                    cli.CodiceAnagrafica = string.Empty;
                }

                var comuni = gc.ElencoTabellaComuni.OrderBy(s => s.Comune).ToList().Select(d => new SelectListItem()
                    {
                        Value = d.CodiceComune,
                        Text = string.Format("{0} ({1})", d.Comune, d.CodiceProvincia)
                    }).ToArray();

                ViewData["selCodiceComune"] = new SelectList(comuni, "Value", "Text", cli.CodiceComune);

                return View(cli);
            }
        }

        [HttpPost]
        public ActionResult ModificaClientePotenziale(FormCollection fc)
        {
            try
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    string msg = string.Empty;
                    bool successo = false;
                    string nuovo_codice = string.Empty;

                    try
                    {
                        var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                        cmd.CommandText = "APP_SP_WEB_AGGIORNA_CLIENTE_POTENZIALE";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                            cmd.Connection.Open();

                        SqlCommandBuilder.DeriveParameters(cmd);
                        cmd.Obj2Params(fc, string.Empty);
                        nuovo_codice = Convert.ToString(cmd.ExecuteScalar());

                        successo = true;
                        msg = "Cliente correttamente registrato.";
                    }
                    catch (Exception ex)
                    {
                        successo = false;
                        msg = string.Format("Errore nella registrazione del cliente: <br/> {0}",
                            ex.Message.Replace("\n", "<br/>"));
                    }

                    return Json(new
                    {
                        Successo = successo,
                        Messaggio = msg,
                        CodiceCliente = nuovo_codice
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
