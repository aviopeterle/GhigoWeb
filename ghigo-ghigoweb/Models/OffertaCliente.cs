using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_offerte_clienti")]
    public class OffertaCliente
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string NumeroOfferta { get; set; }
        public int VersioneOfferta { get; set; }

        public DateTime DataOfferta { get; set; }
        public DateTime DataConsegna { get; set; }

        public string CodiceAnagrafica { get; set; }
        public string CodiceAzienda { get; set; }

        public string CodiceAgente { get; set; }

        public int StatoOfferta { get; set; }

        public DateTime DataRichiestaCliente { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }

        public bool Ricorrente { get; set; }
        public bool RicorrenzaAttiva { get; set; }

        public bool PagamentoEffettuato { get; set; }
        public string DatiPagamento { get; set; }

        public decimal Totale { get; set; }

        public string DescrizioneRicorrenza { get; set; }

    }
}
