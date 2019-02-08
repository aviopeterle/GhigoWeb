using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace GhigoWeb.Models
{

    [Table("app_tabella_stato_offerta_cliente")]
    public class TabellaStatoOffertaCliente
    {
        [Key]
        public int Codice { get; set; }

        public string Descrizione { get; set; }
    }
}
