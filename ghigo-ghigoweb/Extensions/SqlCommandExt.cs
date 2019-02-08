using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;

namespace GhigoWeb.Extensions
{
    public static class SqlCommandExt
    {

        public static void Obj2Params(this SqlCommand cmd, NameValueCollection nvc, string user)
        {
            if(cmd.Parameters == null || cmd.Parameters.Count == 0)
            {
                SqlCommandBuilder.DeriveParameters(cmd);
            }

            foreach (SqlParameter param in cmd.Parameters)
            {
                if ((param.Direction == ParameterDirection.Input) || (param.Direction == ParameterDirection.InputOutput))
                {
                    string paramName = param.ParameterName.TrimStart('@');

                    string value = nvc.Get(paramName);

                    if (value == null && paramName == "utente")
                    {
                        value = user;
                    }

                    if (value != null)
                    {
                        if (param.SqlDbType == SqlDbType.Bit)
                        {
                            param.Value = value != "false";
                        }
                        else
                        {
                            param.Value = value;
                        }
                    }
                }
            }
        }

    }
}