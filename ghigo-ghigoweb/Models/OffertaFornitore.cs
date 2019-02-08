using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_offerte_fornitori")]
    public class OffertaFornitore
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string NumeroOrdine { get; set; }
        public string CodiceDeposito { get; set; }
        public string DescrizioneDepositoWeb { get; set; }

        public DateTime DataChiusura { get; set; }
        public DateTime DataConsegna { get; set; }
        public string NoteFornitore { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }

        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }

        public string Ean13 { get; set; }
        public string Code32 { get; set; }

        public int QuantitaMinimaLotto { get; set; }
        public DateTime DataScadenza { get; set; }

        public decimal PercentualeRicaricoFornitore { get; set; }
        public decimal PrezzoFornitore { get; set; }
        public decimal CostoBase { get; set; }

        public int QuantitaRichiestaResidua { get; set; }
        public int QuantitaOfferta { get; set; }
        public int QuantitaConfermata { get; set; }
        public int QuantitaConferita { get; set; }
        public int QuantitaDaConferire { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }

        public string Lotto { get; set; }
        public DateTime ScadenzaLotto { get; set; }

        public string StatoOrdine { get; set; }

        public int QuantitaDaOrdinare { get; set; }
        public decimal PrezzoDaOrdinare { get; set; }
    }
}
