using GhigoWeb.Filters;
using GhigoWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership]
    public class MagazzinoController : Controller
    {
        //
        // GET: /Magazzino/

        public ActionResult Index(string ordineFornitore, string cliente, string codiceFornitore, string articolo,
            bool? residuo, bool? da_vendere, bool? anche_chiusi)
        {
            // solo dalla rete locale
            string ip = GetVisitorIPAddress(true) ?? string.Empty;
            if (!ip.StartsWith("192.168"))
                return RedirectToAction("Index", "Home");

            ordineFornitore = string.IsNullOrEmpty(ordineFornitore) ? string.Empty : ordineFornitore.Trim();
            ViewBag.OrdineFornitore = ordineFornitore;

            cliente = string.IsNullOrEmpty(cliente) ? string.Empty : cliente.Trim();
            ViewBag.Cliente = cliente;

            codiceFornitore = string.IsNullOrEmpty(codiceFornitore) ? string.Empty : codiceFornitore.Trim();
            ViewBag.CodiceFornitore = codiceFornitore;

            articolo = string.IsNullOrEmpty(articolo) ? string.Empty : articolo.Trim();
            ViewBag.Articolo = articolo;

            bool filtro_residuo = residuo ?? false;
            ViewBag.Residuo = filtro_residuo;

            bool filtro_da_vendere = da_vendere ?? false;
            ViewBag.DaVendere = filtro_da_vendere;

            bool filtro_anche_chiusi = anche_chiusi ?? false;
            ViewBag.AncheChiusi = filtro_anche_chiusi;

            using (var gc = new GhigoContext())
            {
                var items = gc.Database.SqlQuery<StatoConferimento>(
                    "EXEC APP_SP_WEB_STATO_CONFERIMENTI @codiceFornitore, @ordineFornitore, @cliente, @articolo, @codiceAzienda, @residuo, @da_vendere, @anche_chiusi",
                    new SqlParameter("codiceFornitore", codiceFornitore),
                    new SqlParameter("ordineFornitore", ordineFornitore),
                    new SqlParameter("cliente", cliente),
                    new SqlParameter("articolo", articolo),
                    new SqlParameter("codiceAzienda", "GHIGO"),
                    new SqlParameter("residuo", filtro_residuo),
                    new SqlParameter("da_vendere", filtro_da_vendere),
                    new SqlParameter("anche_chiusi", filtro_anche_chiusi)
                ).ToList();

                return View(items);
            }
        }

        // indirizzo del visitatore
        public string GetVisitorIPAddress(bool GetLan = false)
        {
            string visitorIPAddress = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Request.ServerVariables["REMOTE_ADDR"];

            if (string.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Request.UserHostAddress;

            if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
            {
                GetLan = true;
                visitorIPAddress = string.Empty;
            }

            if (GetLan)
            {
                if (string.IsNullOrEmpty(visitorIPAddress))
                {
                    //This is for Local(LAN) Connected ID Address
                    string stringHostName = Dns.GetHostName();
                    //Get Ip Host Entry
                    IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
                    //Get Ip Address From The Ip Host Entry Address List
                    IPAddress[] arrIpAddress = ipHostEntries.AddressList;

                    try
                    {
                        visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
                    }
                    catch
                    {
                        try
                        {
                            visitorIPAddress = arrIpAddress[0].ToString();
                        }
                        catch
                        {
                            try
                            {
                                arrIpAddress = Dns.GetHostAddresses(stringHostName);
                                visitorIPAddress = arrIpAddress[0].ToString();
                            }
                            catch
                            {
                                visitorIPAddress = "127.0.0.1";
                            }
                        }
                    }
                }
            }
            return visitorIPAddress;
        }

        [HttpPost]
        public ActionResult AggiornaNoteUfficio(string attribute, string value)
        {
            long recid = long.Parse(attribute);

            using (GhigoContext gc = new GhigoContext())
            {
                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandText = "APP_SP_WEB_AGGIORNA_NOTE_UFFICIO_STATO_CONFERIMENTO";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@recid", recid);
                cmd.Parameters.AddWithValue("@value", value);
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }

            return Content(value);
        }

        [HttpPost]
        public ActionResult AggiornaNoteMagazzino(string attribute, string value)
        {
            long recid = long.Parse(attribute);

            using (GhigoContext gc = new GhigoContext())
            {
                var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                cmd.CommandText = "APP_SP_WEB_AGGIORNA_NOTE_MAGAZZINO_STATO_CONFERIMENTO";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@recid", recid);
                cmd.Parameters.AddWithValue("@value", value);
                if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                    cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }

            return Content(value);
        }

        [HttpPost]
        public ActionResult CreaCaricoEsterno(FormCollection fc)
        {
            try
            {
                XmlDocument xmldoc = new XmlDocument();

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
                    cmd.CommandText = "APP_SP_WEB_CREA_CARICO_ESTERNO";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@xml", xmldoc.InnerXml);

                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    using(var dr = cmd.ExecuteReader())
                    {
                        if(dr.Read())
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
    }
}
