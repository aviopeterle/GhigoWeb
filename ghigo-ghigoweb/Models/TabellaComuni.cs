using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_tabella_comuni")]
    public class TabellaComuni
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceComune { get; set; }
        public string Comune { get; set; }

        public string CodiceProvincia { get; set; }
    }
}
