using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_ordini_clienti_dettaglio")]
    public class OrdineClienteDettaglio
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceAzienda { get; set; }
        public string NumeroOrdine { get; set; }

        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }

        public decimal PrezzoVendita { get; set; }
        public decimal Netto { get; set; }
        public decimal Quantita { get; set; }
        public decimal QuantitaEvasa { get; set; }
        public decimal QuantitaResidua { get; set; }
        public int QuantitaRaccolta { get; set; }

        public DateTime DataScadenza { get; set; }
    }
}
