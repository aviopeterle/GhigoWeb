using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_PERIODI")]
    public class TabellaPeriodi
    {
        [Key, Column("rec_id", Order=0)]
        public long RecId { get; set; }
        public string Id { get; set; }
        public string Codice { get; set; }
        public string Descrizione { get; set; }
    }
}
