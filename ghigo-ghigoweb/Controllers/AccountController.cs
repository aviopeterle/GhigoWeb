using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using GhigoWeb.Filters;
using GhigoWeb.Models;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // Se si arriva a questo punto, significa che si è verificato un errore, rivisualizzare il form
            ModelState.AddModelError("", "Il nome utente o la password fornita non è corretta.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Tentare di registrare l'utente
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // Se si arriva a questo punto, significa che si è verificato un errore, rivisualizzare il form
            return View(model);
        }

        // chiamata per creare utente da documents
        //[HttpPost]
        [AllowAnonymous]
        public ActionResult DocumentsRegister(string utente, string password, string codiceazienda, string codicefornitore)
        {
            // Tentare di registrare l'utente
            try
            {
                if (string.IsNullOrEmpty(utente)) throw new Exception("Nome utente non valido.");
                if (string.IsNullOrEmpty(password)) throw new Exception("Password non valida.");
                if (string.IsNullOrEmpty(codiceazienda)) throw new Exception("Codice azienda non valido.");
                if (string.IsNullOrEmpty(codicefornitore)) throw new Exception("Codice fornitore non valido.");

                WebSecurity.CreateUserAndAccount(utente, password);
                using (GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.Single(u => u.UserName == utente);
                    user.CodiceAzienda = codiceazienda;
                    user.CodiceFornitore = codicefornitore;
                    gc.SaveChanges();
                }

                return PartialView("_DocumentsResponse", "OK");
            }
            catch (Exception e)
            {
                return PartialView("_DocumentsResponse", string.Format("NO.{0}", e.Message));
            }
        }

        // chiamata per resettare una password
        //[HttpPost]
        [AllowAnonymous]
        public ActionResult DocumentsResetPassword(string utente, string password)
        {
            // Tentare di registrare l'utente
            try
            {
                if (string.IsNullOrEmpty(utente)) throw new Exception("Nome utente non valido.");
                if (string.IsNullOrEmpty(password)) throw new Exception("Password non valida.");

                var token = WebSecurity.GeneratePasswordResetToken(utente);
                var result = WebSecurity.ResetPassword(token, password);

                return PartialView("_DocumentsResponse", "OK");
            }
            catch (Exception e)
            {
                return PartialView("_DocumentsResponse", string.Format("NO.{0}", e.Message));
            }
        }
        
        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Dissociare l'account solo se l'utente attualmente connesso è il proprietario
            if (ownerAccount == User.Identity.Name)
            {
                // Utilizzare una transazione per impedire all'utente di eliminare l'ultima credenziale di accesso utilizzata
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Cambiamento password completato."
                : message == ManageMessageId.SetPasswordSuccess ? "Impostazione password completata."
                : message == ManageMessageId.RemoveLoginSuccess ? "L'account di accesso esterno è stato rimosso."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword genererà un'eccezione anziché restituire false in alcuni scenari di errore.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "La password corrente non è corretta o la nuova password non è valida.");
                    }
                }
            }
            else
            {
                // L'utente non dispone di una password locale. Rimuovere quindi gli eventuali errori di convalida dovuti a un
                // campo OldPassword mancante
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // Se si arriva a questo punto, significa che si è verificato un errore, rivisualizzare il form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // Se l'utente corrente ha eseguito l'accesso, aggiungere il nuovo account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // L'utente è nuovo, chiedere di specificare il nome di appartenenza desiderato
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Inserisce un nuovo utente nel database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Verifica se l'utente esiste già
                    if (user == null)
                    {
                        // Inserire il nome nella tabella dei profili
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "Il nome utente esiste già. Immettere un nome utente differente.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helper
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // Vedere http://go.microsoft.com/fwlink/?LinkID=177550 per
            // un elenco completo di codici di stato.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Il nome utente esiste già. Immettere un nome utente differente.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Un nome utente per l'indirizzo di posta elettronica esiste già. Immettere un nome utente differente.";

                case MembershipCreateStatus.InvalidPassword:
                    return "La password fornita non è valida. Immettere un valore valido per la password.";

                case MembershipCreateStatus.InvalidEmail:
                    return "L'indirizzo di posta elettronica fornito non è valido. Controllare il valore e riprovare.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "La risposa fornita per il recupero della password non è valida. Controllare il valore e riprovare.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "La domanda fornita per il recupero della password non è valida. Controllare il valore e riprovare.";

                case MembershipCreateStatus.InvalidUserName:
                    return "Il nome utente fornito non è valido. Controllare il valore e riprovare.";

                case MembershipCreateStatus.ProviderError:
                    return "Il provider di autenticazione ha restituito un errore. Verificare l'immissione e riprovare. Se il problema persiste, contattare l'amministratore di sistema.";

                case MembershipCreateStatus.UserRejected:
                    return "La richiesta di creazione dell'utente è stata annullata. Verificare l'immissione e riprovare. Se il problema persiste, contattare l'amministratore di sistema.";

                default:
                    return "Si è verificato un errore sconosciuto. Verificare l'immissione e riprovare. Se il problema persiste, contattare l'amministratore di sistema.";
            }
        }
        #endregion
    }
}
