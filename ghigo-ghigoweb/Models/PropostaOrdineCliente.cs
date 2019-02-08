using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_offerte_clienti_proposta")]
    public class PropostaOrdineCliente
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceAnagrafica { get; set; }
        public string CodiceAzienda { get; set; }

        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }

        public string Ean13 { get; set; }
        public string Code32 { get; set; }

        public int Quantita { get; set; }
        public decimal Prezzo { get; set; }
        public decimal PrezzoProposto { get; set; }

        public DateTime ScadenzaLotto { get; set; }

        public string CodiceListino { get; set; }
        public string CodiceCliente { get; set; }
    }
}
