using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_ddt_conferimenti_fornitori")]
    public class DDTConferimento
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }

        public string NumeroDocumento { get; set; }
        public DateTime DataDDT { get; set; }

        public string NumeroDdtVendita { get; set; }
        public string NumeroFattura { get; set; }
        public string DestinazioneRagioneSociale { get; set; }

        public bool Stampata { get; set; }
        public bool DifferenzeSpunta { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }
}
