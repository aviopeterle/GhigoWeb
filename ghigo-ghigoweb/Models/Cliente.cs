using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_ANAGRAFICHE")]
    public class Cliente
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceAnagrafica { get; set; }

        public string RagioneSociale { get; set; }
        public string RagioneSociale1 { get; set; }
        public string Indirizzo { get; set; }
        public string Indirizzo1 { get; set; }
        public string Localita { get; set; }
        public string Cap { get; set; }

        public string Comune { get; set; }

        public string CodiceFiscale { get; set; }
        public string PartitaIva { get; set; }

        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Fax { get; set; }
    }
}
