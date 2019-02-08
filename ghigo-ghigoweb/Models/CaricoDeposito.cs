using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_CARICHI_DEPOSITO")]
    public class CaricoDeposito
    {
        [Key]
        public long RecId { get; set; }
        public string CodiceAzienda { get; set; }
        public string CodiceDeposito { get; set; }

        public string NumeroCarico { get; set; }
        public DateTime DataCarico { get; set; }

        public string CodiceFornitore { get; set; }
        public string Fornitore { get; set; }

        public string Stato { get; set; }
        public string Note { get; set; }
    }
}