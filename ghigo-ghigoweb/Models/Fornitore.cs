using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_FORNITORI")]
    public class Fornitore
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceAnagrafica { get; set; }
        public string NomeAnagrafica { get; set; }
    }
}

