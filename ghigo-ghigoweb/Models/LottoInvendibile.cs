using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_LOTTI_INVENDIBILI")]
    public class LottoInvendibile
    {
        [Key, Column("rec_id", Order=0)]
        public long RecId { get; set; }

        public string CodiceProdotto { get; set; }
        public string Descrizione { get; set; }

        public string Invendibilita { get; set; }
        public DateTime DataVendibilita { get; set; }
        public string LottiVendibili { get; set; }

        public int Giacenza { get; set; }

        [Key, Column("Lotto", Order = 1)]
        public string Lotto { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }
    }
}
