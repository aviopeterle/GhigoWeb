using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_dati_costi")]
    public class DatiCosto
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }
        public string CodiceArticolo { get; set; }

        public decimal CostoNettoUnitario { get; set; }
        public decimal PrezzoVendita { get; set; }
    }
}
