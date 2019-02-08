namespace GhigoWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using GhigoWeb.Models;
    using System.Security.Cryptography;
    using GhigoWeb.Extensions;
    using System.Data.SqlClient;
    using System.Text;

    /// <summary>
    /// Controller per la condivisione di documenti xml
    /// </summary>
    public class XmlController : Controller
    {
        private RijndaelManaged CreateCipher(string hash)
        {
            RijndaelManaged cipher = new RijndaelManaged();
            cipher.KeySize = 256;
            cipher.BlockSize = 128;
            cipher.Padding = PaddingMode.ISO10126;
            cipher.Mode = CipherMode.CBC;
            byte[] key = hash.ToByteArray();
            cipher.Key = key;
            return cipher;
        }

        public ActionResult ListinoDeposito(string userName)
        {
            try
            {
                using(GhigoContext gc = new GhigoContext())
                {
                    var user = gc.UserProfiles.SingleOrDefault(u => u.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase));
                    if (user == null)
                        return HttpNotFound();

                    var cmd = gc.Database.Connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = "APP_SP_WEB_LISTINO_DEPOSITO_XML";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codiceazienda", user.CodiceAzienda);
                    cmd.Parameters.AddWithValue("@codicefornitore", user.CodiceFornitore);

                    if (cmd.Connection.State == System.Data.ConnectionState.Closed)
                        cmd.Connection.Open();

                    string xml = cmd.ExecuteScalar() as string;
                    if(string.IsNullOrEmpty(xml))
                    {
                        throw new Exception("nessun risultato");
                    }

                    byte[] plain = Encoding.UTF8.GetBytes(xml);

                    var cipher = CreateCipher(user.HashKey);
                    ICryptoTransform cryptoTransform = cipher.CreateEncryptor();
                    byte[] cipherText = cryptoTransform.TransformFinalBlock(plain, 0, plain.Length);

                    byte[] message = new byte[cipher.IV.Length + cipherText.Length];
                    cipher.IV.CopyTo(message, 0);
                    cipherText.CopyTo(message, cipher.IV.Length);
                    return File(message, "application/octet-stream", "listino_deposito.enc");

                    /*
                    var iv = cipher.IV;
                    cipher = CreateCipher(user.HashKey);
                    cipher.IV = iv;
                    byte[] decoded = cipher.CreateDecryptor().TransformFinalBlock(message, cipher.IV.Length, message.Length - cipher.IV.Length);
                    string res = Encoding.UTF8.GetString(decoded);

                    return File(decoded, "text/xml", "listino_deposito.xml");
                     */
                }
            }
            catch(Exception /*ignore*/)
            {
                return HttpNotFound();
            }

        }

    }
}
