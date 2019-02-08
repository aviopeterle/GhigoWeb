using GhigoWeb.Filters;
using GhigoWeb.Models;
using PayPal;
using PayPal.Api.Payments;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using NLog;
using System.Collections.Specialized;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize(Roles = "Cliente")]
    public class PagamentoController : Controller
    {
        private Logger _logger = LogManager.GetLogger("Payment");

        public enum Method
        {
            PayPal,
            GestPay,
            CartaSi
        }

        public ActionResult EffettuaPagamento(string document, Method method)
        {
            _logger.Info("EffettuaPagamento: document={0}, method={1}, user={2}", 
                document, Enum.GetName(typeof(Method), method), User.Identity.Name);

            if (string.IsNullOrEmpty(document))
            {
                _logger.Fatal("\tdocument vuoto..esco");
                return View("PagamentoErrore");
            }
            
            // formato DOCUMENTO.CODICEAZIENDA.NUMERO
            string[] parti = document.ToUpper().Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if(parti.Length != 3)
            {
                _logger.Fatal("\tdocument non valido..esco");
                return View("PagamentoErrore");
            }

            string tipo_documento = parti[0];
            string codice_azienda = parti[1];
            string numero_documento = parti[2];

            _logger.Info("\ttipo_documento={0}, codice_azienda={1}, numero_documento={2}", tipo_documento, codice_azienda, numero_documento);

            decimal amount = 0;
            using(GhigoContext gc = new GhigoContext())
            {
                switch (tipo_documento)
                {
                    case "OFFERTA_CLIENTE":
                        amount = gc.OfferteCliente.Single(o => o.CodiceAzienda == codice_azienda && o.NumeroOfferta == numero_documento).Totale;
                        _logger.Info("\tAmount = {0}", amount);
                        switch(method)
                        {
                            case Method.GestPay:
                                return PagamentoGestPay(amount, document);
                            case Method.PayPal:
                                return PagamentoPayPal(tipo_documento, codice_azienda, numero_documento, amount, document);
                            case Method.CartaSi:
                                return PagamentoCartaSi(tipo_documento, codice_azienda, numero_documento, amount, document);
                        }
                        break;
                }
            }

            _logger.Fatal("\ttipo documento non riconosciuto oppure errore metodo di pagamento..esco");
            return View("PagamentoErrore");
        }

        #region GestPay

        // http://localhost:2056/Pagamento/PagamentoGestPayOK?a=GESPAY60603&b=URvBKfqyEV
        public ActionResult PagamentoGestPayOk()
        {
            var nvc = new NameValueCollection(Request.QueryString);
            nvc.Add("method", "GESTPAY");

            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                        .ToArray();

            return Redirect(String.Format("{0}?{1}", Url.Action("PagamentoOK"), string.Join("&", array)));
        }

        public ActionResult PagamentoGestPayOkServer2Server()
        {
            var nvc = new NameValueCollection(Request.QueryString);
            nvc.Add("method", "GestPay");
            nvc.Add("server2server", "server2server");

            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                        .ToArray();

            return Redirect(String.Format("{0}?{1}", Url.Action("PagamentoOK"), string.Join("&", array)));
        }

        private ActionResult PagamentoGestPay(decimal decAmount, string document)
        {
            try
            {
                string shopLogin = ConfigurationManager.AppSettings["gestpay_shopLogin"];
                _logger.Info("PagamentoGestPay amount={0}, document={1}, shopLogin={2}", decAmount, document, shopLogin);

                BancaSellaWS.WSCryptDecryptSoapClient wc = new BancaSellaWS.WSCryptDecryptSoapClient();
                var res = wc.Encrypt(
                        shopLogin
                       , "242"
                       , decAmount.ToString("0.00", new CultureInfo("en-US"))
                       , document
                       , string.Empty
                       , string.Empty
                       , string.Empty
                       , string.Empty
                       , string.Empty
                       , string.Empty
                       , string.Empty
                       , string.Empty
                       , string.Empty
                       , string.Empty
                       , new BancaSellaWS.ShippingDetails()
                       , new string[0]
                       , new BancaSellaWS.PaymentTypeDetail()
                       , string.Empty
                       , new BancaSellaWS.RedCustomerInfo()
                       , new BancaSellaWS.RedShippingInfo()
                       , new BancaSellaWS.RedBillingInfo()
                       , new BancaSellaWS.RedCustomerData()
                       , new string[0]
                       , new BancaSellaWS.RedItems()
                        );
                _logger.Info("\tres: {0}", res.OuterXml);

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(res.OuterXml);

                // transaction
                bool TransactionResult = xml.GetElementsByTagName("TransactionResult")[0].InnerText == "OK";
                if (TransactionResult)
                {
                    string CryptDecryptString = xml.GetElementsByTagName("CryptDecryptString")[0].InnerText;
                    string host = wc.Endpoint.Address.Uri.Host;
                    string url = string.Format("https://{2}/gestpay/pagam.asp?a={0}&b={1}", shopLogin, CryptDecryptString, host);
                    _logger.Info("\tRedirecting to: {0}", url);
                    return Redirect(url);
                }

                throw new Exception("TransactionResult = KO");
            }
            catch (Exception ex)
            {
                _logger.Fatal("\tErrore fatale: {0}", ex.Message);
                return View("PagamentoErrore");
            }
        }

        #endregion

        #region PayPal
        private ActionResult PagamentoPayPal(string tipo_documento, string codice_azienda, string numero_documento, decimal decAmount, string document)
        {
            try
            {
                _logger.Info("PagamentoPayPal tipo_documento={0}, codice_azienda={1}, numero_documento={2}, amount={3}, document={4}",
                tipo_documento, codice_azienda, numero_documento, decAmount, document);

                var clientId = ConfigurationManager.AppSettings["paypal_ClientID"];
                var secretToken = ConfigurationManager.AppSettings["paypal_ClientSecret"];
                string accessToken = new OAuthTokenCredential(clientId, secretToken).GetAccessToken();
                _logger.Info("\tPayPal clientId={0}, secretToken={1}, accessToken={2}", clientId, secretToken, accessToken);

                var details = new Details
                {
                    subtotal = "0.00",
                    shipping = "0.00",
                    tax = "0.00",
                    fee = "0.00",
                };

                var amount = new Amount
                {
                    currency = "EUR",
                    details = details,
                    total = decAmount.ToString("0.00", new CultureInfo("en-US"))
                };

                var payment = new Payment
                {
                    transactions = new List<Transaction> 
                    { 
                        new Transaction
                        {
                            amount = amount,
                            description = String.Format("{0} num. {1} ({2}) Totale: {3}", tipo_documento, numero_documento, codice_azienda, string.Format("{0:C}", decAmount))
                        } 
                    },
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    redirect_urls = new RedirectUrls
                    {
                        cancel_url = this.Url.Action("PagamentoKO", "Pagamento", new { document = document, method = "PayPal" }, this.Request.Url.Scheme),
                        return_url = this.Url.Action("PagamentoOK", "Pagamento", new { document = document, method = "PayPal" }, this.Request.Url.Scheme)
                    }
                };

                payment = payment.Create(accessToken);
                Session.Add("payment_" + document, payment.id);
                _logger.Info("\tPaypal document={0}, id={1}", document, payment.id);

                var links = payment.links.GetEnumerator();

                while (links.MoveNext())
                {
                    Links lnk = links.Current;
                    if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                    {
                        _logger.Info("\tRedirecting to: {0}", lnk.href);
                        return Redirect(lnk.href);
                    }
                }

                throw new Exception("No link found");
            }
            catch (Exception ex)
            {
                _logger.Fatal("\tErrore fatale: {0}", ex.Message);
                return View("PagamentoErrore");
            }
        }
        #endregion

        #region CartaSi
        private ActionResult PagamentoCartaSi(string tipo_documento, string codice_azienda, string numero_documento, decimal decAmount, string document)
        {
            try
            {
                _logger.Info("PagamentoCartaSi tipo_documento={0}, codice_azienda={1}, numero_documento={2}, amount={3}, document={4}",
                tipo_documento, codice_azienda, numero_documento, decAmount, document);

                var cartasi_url = ConfigurationManager.AppSettings["cartasi_url"];
                var cartasi_alias = ConfigurationManager.AppSettings["cartasi_alias"];
                var cartasi_mac = ConfigurationManager.AppSettings["cartasi_mac"];
                /*
                 * 
                 * ?alias=valore&importo=5000&divisa=EUR&codTrans=990101-00001&mail=xxx@xxxx.it&url=http://www.xxxxx.it&session_id=xxxxxxxx&mac=yyyy&languageId=ITA
                 * 
                 */
                var nvc = new NameValueCollection();
                nvc.Add("alias", cartasi_alias);
                int importo = Convert.ToInt32(decAmount * 100);

                nvc.Add("importo", importo.ToString());
                nvc.Add("divisa", "EUR");

                // tentativo dal db
                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_PAGAMENTO_SELECT_TENTATIVO";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    cmd.Parameters.AddWithValue("@radDatiDocumento", document);

                    int tentativo = Convert.ToInt32(cmd.ExecuteScalar());
                    _logger.Info("Nuovo tentativo: {0}", tentativo);

                    numero_documento = String.Format("{0}-{1}", numero_documento, tentativo);
                }

                nvc.Add("codTrans", numero_documento);

                nvc.Add("url", this.Url.Action("PagamentoOK", "Pagamento", new { document = document, method = "CartaSi" }, this.Request.Url.Scheme));
                nvc.Add("url_back", this.Url.Action("PagamentoKO", "Pagamento", new { document = document, method = "CartaSi" }, this.Request.Url.Scheme));
                nvc.Add("languageId", "ITA");

                // mac
                string testo_da_codificare = string.Format("codTrans={0}divisa=EURimporto={1}{2}", numero_documento, importo, cartasi_mac);
                HashAlgorithm sha1 = SHA1.Create();
                StringBuilder mac = new StringBuilder();
                foreach(byte b in sha1.ComputeHash(Encoding.UTF8.GetBytes(testo_da_codificare)))
                {
                    mac.Append(b.ToString("X2"));
                }
                nvc.Add("mac", mac.ToString().ToLower());

                string querystring = String.Join("&",nvc.AllKeys.Select(a => a + "=" + HttpUtility.UrlEncode(nvc[a])));
                string url = String.Format("{0}?{1}", cartasi_url, querystring);
                _logger.Info("PagamentoCartaSi url {0}", url);

                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.Fatal("\tErrore fatale: {0}", ex.Message);
                return View("PagamentoErrore");
            }
        }
        #endregion

        // http://localhost:2056/Pagamento/PagamentoOK?document=OFFERTA_CLIENTE.GHIGO.2014000117&token=EC-2VA83563MF440223W&PayerID=NPMAZJUGFMSJL
        public ActionResult PagamentoOK(string document, string method)
        {
            try
            {
                _logger.Info("PagamentoOK document={0}, method={1}, QueryString={2}", document, method, Request.QueryString);

                bool server2server = !string.IsNullOrEmpty(Request.QueryString["server2server"]);

                string dati_pagamento = string.Format("Pagamento via {0} effettuato il {1}", method, DateTime.Now.ToShortDateString());

                bool handled = false;

                if (method.Equals("PAYPAL", StringComparison.InvariantCultureIgnoreCase))
                {
                    var clientId = ConfigurationManager.AppSettings["paypal_ClientID"];
                    var secretToken = ConfigurationManager.AppSettings["paypal_ClientSecret"];
                    string accessToken = new OAuthTokenCredential(clientId, secretToken).GetAccessToken();

                    string paymentId = Convert.ToString(Session["payment_" + document]);
                    string payerId = Request.QueryString["PayerID"];

                    _logger.Info("\tPayPal clientId={0}, secretToken={1}, accessToken={2}, paymentId={3}, payerId={4}", 
                        clientId, secretToken, accessToken, paymentId, payerId);

                    var payment = new Payment() { id = paymentId };

                    dati_pagamento += string.Format(" (payment {0})", payment.id);

                    PaymentExecution pymntExecution = new PaymentExecution() { payer_id = payerId };

                    payment.Execute(accessToken, pymntExecution);

                    handled = true;
                }

                if(method.Equals("GESTPAY", StringComparison.InvariantCultureIgnoreCase))
                {
                    var shopLogin = Request.QueryString["a"];
                    var cryptedString = Request.QueryString["b"];

                    _logger.Info("\tGestPay shopLogin={0}, cryptedString={1}", shopLogin, cryptedString);

                    BancaSellaWS.WSCryptDecryptSoapClient wc = new BancaSellaWS.WSCryptDecryptSoapClient();
                    var res = wc.Decrypt(shopLogin, cryptedString);

                    _logger.Info("\tres: {0}", res.OuterXml);

                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(res.OuterXml);

                    // transaction
                    bool TransactionResult = xml.GetElementsByTagName("TransactionResult")[0].InnerText == "OK";
                    if (TransactionResult)
                    {
                        document = xml.GetElementsByTagName("ShopTransactionID")[0].InnerText;
                        string authorizationCode = xml.GetElementsByTagName("AuthorizationCode")[0].InnerText;
                        dati_pagamento += string.Format(" (authorizationCode {0})", authorizationCode);
                    }

                    handled = true;
                }

                if(method.Equals("CARTASI", StringComparison.InvariantCultureIgnoreCase))
                {
                    string codTrans = Request.QueryString["codTrans"];
                    string esito = Request.QueryString["esito"];

                    if (!"OK".Equals(esito, StringComparison.InvariantCultureIgnoreCase))
                        return RedirectToAction("PagamentoKO", new { document = document, method = method });

                    string importo = Request.QueryString["importo"];
                    string divisa = Request.QueryString["divisa"];
                    string data = Request.QueryString["data"];
                    string orario = Request.QueryString["orario"];
                    string codAut = Request.QueryString["codAut"];
                    string cartasi_mac = ConfigurationManager.AppSettings["cartasi_mac"];

                    // mac
                    string testo_da_codificare = string.Format("codTrans={0}esito={1}importo={2}divisa={3}data={4}orario={5}codAut={6}{7}",
                        codTrans, esito, importo, divisa, data, orario, codAut, cartasi_mac);

                    HashAlgorithm sha1 = SHA1.Create();
                    StringBuilder mac_calcolato = new StringBuilder();
                    foreach (byte b in sha1.ComputeHash(Encoding.UTF8.GetBytes(testo_da_codificare)))
                    {
                        mac_calcolato.Append(b.ToString("X2"));
                    }

                    string mac_passato = Request.QueryString["mac"];
                    if(!mac_calcolato.ToString().Equals(mac_passato, StringComparison.InvariantCultureIgnoreCase))
                        throw new Exception("Il mac passato non corrisponde.");

                    handled = true;
                }

                if (!handled)
                    throw new Exception("Tipo di pagamento non gestito.");

                // avvisiamo il db
                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_PAGAMENTO_EFFETTUATO";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    cmd.Parameters.AddWithValue("@radDatiDocumento", document);
                    cmd.Parameters.AddWithValue("@radPagamento", method);
                    cmd.Parameters.AddWithValue("@radDatiPagamento", dati_pagamento);

                    cmd.ExecuteNonQuery();
                }

                if (server2server)
                    return new EmptyResult();

                return View();

            } catch(Exception ex)
            {
                _logger.Fatal("\tErrore fatale: {0}", ex.Message);
                return View("PagamentoErrore");
            }
        }

        public ActionResult PagamentoKO(string document, string method)
        {
            _logger.Info("PagamentoKO QueryString={0}", Request.QueryString);

            // avvisiamo il db
            if (!string.IsNullOrEmpty(document) && !string.IsNullOrEmpty(method))
            {
                using (GhigoContext gc = new GhigoContext())
                {
                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_PAGAMENTO_FALLITO";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    cmd.Parameters.AddWithValue("@radDatiDocumento", document);
                    cmd.Parameters.AddWithValue("@radPagamento", method);
                    cmd.Parameters.AddWithValue("@radDatiPagamento", Request.QueryString.ToString());

                    cmd.ExecuteNonQuery();
                }
            }

            return View();
        }
    }
}
