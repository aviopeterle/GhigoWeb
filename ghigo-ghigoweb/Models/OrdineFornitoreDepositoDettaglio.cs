using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_ORDINI_FORNITORI_DEPOSITI_DETTAGLIO")]
    public class OrdineFornitoreDepositoDettaglio
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceDeposito { get; set; }
        public string CodiceAzienda { get; set; }

        public string NumeroOrdine { get; set; }
        public int Riga { get; set; }

        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }

        public string NumeroOfferta { get; set; }
        public DateTime DataChiusura { get; set; }
        public DateTime DataConsegna { get; set; }
        public string NoteFornitore { get; set; }

        public int Quantita { get; set; }
        public decimal Netto { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }
}
