using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_DDT_TRASFERIMENTI")]
    public class DDTVendita
    {
        [Key, Column("rec_id", Order=0)]
        public long RecId { get; set; }

        [Key, Column(Order=1)]
        public string TipoDdt { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }

        public string CausaleDDT { get; set; }

        public string NumeroDocumento { get; set; }
        public DateTime DataDDT { get; set; }

        public string NumeroFattura { get; set; }
        public string DestinazioneRagioneSociale { get; set; }

        public bool Tracciabilita { get; set; }
        public bool DifferenzeSpunta { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }
}
