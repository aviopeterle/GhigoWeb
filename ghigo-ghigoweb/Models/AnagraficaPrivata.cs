using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_ANAGRAFICHE_PRIVATE")]
    public class AnagraficaPrivata
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }

        public string RagioneSociale { get; set; }
        public string Indirizzo { get; set; }
        public string Citta { get; set; }

        public string CodiceFiscale { get; set; }
        public string PartitaIva { get; set; }
    }
}
