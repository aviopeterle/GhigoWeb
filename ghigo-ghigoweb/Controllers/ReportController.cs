using GhigoWeb.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GhigoWeb.Controllers
{
    [InitializeSimpleMembership, Authorize]
    public class ReportController : Controller
    {
        public ActionResult Print()
        {
            try
            {
                using(WebClient webClient = new WebClient())
                {
                    bool loginParameterFound = false;
                    foreach (var key in HttpContext.Request.QueryString.AllKeys)
                    {
                        string value = HttpContext.Request.QueryString[key];
                        webClient.QueryString.Add(key, value);

                        if ("login".Equals(key, StringComparison.InvariantCultureIgnoreCase))
                            loginParameterFound = true;
                    }

                    if(!loginParameterFound)
                    {
                        webClient.QueryString.Add("login", User.Identity.Name);
                    }

                    byte[] result = webClient.DownloadData("http://localhost:8023/EseguiStoredWeb.ashx");
                    string contentType = webClient.ResponseHeaders[HttpResponseHeader.ContentType];

                    string contentDisposition = webClient.ResponseHeaders["Content-Disposition"];
                    if(!string.IsNullOrEmpty(contentDisposition))
                        Response.Headers.Add("Content-Disposition", contentDisposition);

                    return File(result, contentType);
                }
            }
            catch (WebException ex)
            {
                var statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                return new HttpStatusCodeResult(statusCode);
            }
        }
    }
}
