using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace GhigoWeb.Models
{

    [Table("app_tabella_stato_ordini")]
    public class TabellaStatoOrdine
    {
        [Key]
        public string StatoOrdine { get; set; }

        public string Descrizione { get; set; }
    }
}
