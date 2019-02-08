using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_offerte_clienti_proposta_depositi")]
    public class PropostaOrdineClienteDestinazioni
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceAnagrafica { get; set; }
        public string CodiceAzienda { get; set; }
        public string CodiceArticolo { get; set; }
        public string CodiceListino { get; set; }
        public string CodiceCliente { get; set; }

        public string CodiceDestinazione { get; set; }
        public int Quantita { get; set; }
    }
}
