using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_AGENTI_CLIENTI")]
    public class AgenteClienti
    {
        public string CodiceAzienda { get; set; }
        public string CodiceAnagrafica { get; set; }

        [Key]
        public string CodiceCliente { get; set; }
        public string Cliente { get; set; }

        public string PartitaIva { get; set; }
        public string CodiceFiscale { get; set; }

        public string ComuneCompleto { get; set; }
    }
}

