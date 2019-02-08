using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_TIPI_PAGAMENTO")]
    public class TabellaTipiPagamento
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceTipoPagamento { get; set; }
        public string TipoPagamento { get; set; }
    }
}
