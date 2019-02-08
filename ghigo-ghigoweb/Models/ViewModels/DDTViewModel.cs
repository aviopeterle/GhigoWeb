using System;
using System.Collections.Generic;
using System.Linq;

namespace GhigoWeb.Models
{
    public class DDTViewModel
    {
        public string Tabella { get; set; }
        public long Recid { get; set; }
        public string StoredStampa { get; set; }
        public string NumeroDDT { get; set; }

        public DDTViewModel(System.Data.SqlClient.SqlDataReader dr)
        {
            Tabella = Convert.ToString(dr["tabella"]);
            StoredStampa = Convert.ToString(dr["storedstampa"]);
            Recid = Convert.ToInt64(dr["recid"]);
            NumeroDDT = Convert.ToString(dr["numeroddt"]);
        }
    }
}