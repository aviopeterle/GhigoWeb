using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_offerte_clienti_dettaglio")]
    public class OffertaClienteDettaglio
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceAzienda { get; set; }
        public string NumeroOfferta { get; set; }
        public int VersioneOfferta { get; set; }
        public int Riga { get; set; }

        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }

        public decimal PrezzoRichiesto { get; set; }
        public decimal QuantitaRichiesta { get; set; }

        public decimal PrezzoConfermato { get; set; }
        public decimal QuantitaConfermata { get; set; }

        public DateTime ScadenzaLotto { get; set; }
    }
}
