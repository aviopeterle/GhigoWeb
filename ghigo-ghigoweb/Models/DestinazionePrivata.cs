using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_DESTINAZIONI_PRIVATE")]
    public class DestinazionePrivata
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }
        public string CodiceFiscale { get; set; }

        public string DestinazioneRagioneSociale { get; set; }
        public string DestinazioneIndirizzo { get; set; }
        public string DestinazioneCitta { get; set; }
    }
}
