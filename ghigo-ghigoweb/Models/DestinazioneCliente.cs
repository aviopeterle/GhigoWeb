using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_anagrafiche_destinazioni")]
    public class DestinazioneCliente
    {
        [Key,Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceAnagrafica { get; set; }

        public string CodiceDestinazione { get; set; }
        public string NomeDestinazione { get; set; }
    }
}
