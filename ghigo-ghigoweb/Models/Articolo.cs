using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_articoli")]
    public class Articolo
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceAzienda { get; set; }

        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }

        public string Ean13 { get; set; }
        public string Code32 { get; set; }
    }

    [Table("app_farma_prodotti")]
    public class ArticoloGlobale
    {
        [Key, Column("CodiceProdotto")]
        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }
    }
}
