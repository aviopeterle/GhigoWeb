using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_conferimenti_fornitori")]
    public class Conferimento
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

        public int Quantita { get; set; }
        public string Lotto { get; set; }
        public DateTime ScadenzaLotto { get; set; }

        public string Ddt { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }

        public bool Prenotato { get; set; }
        public decimal CostoNettoUnitario { get; set; }
    }
}
