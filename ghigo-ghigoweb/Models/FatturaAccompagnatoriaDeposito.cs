using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_fatture_accompagnatorie_depositi")]
    public class FatturaAccompagnatoriaDeposito
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }

        public string NumeroDocumento { get; set; }
        public DateTime DataDocumento { get; set; }
        public string DestinazioneRagioneSociale { get; set; }

        public decimal TotaleDocumento { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }
}
