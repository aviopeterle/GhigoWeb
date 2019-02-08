using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_tabella_iva")]
    public class TabellaIva
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceIva { get; set; }
        public string Descrizione { get; set; }
    }
}
