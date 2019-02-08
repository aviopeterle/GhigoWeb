using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_ORDINI_FORNITORI_DEPOSITI")]
    public class OrdineFornitoreDeposito
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceDeposito { get; set; }
        public string CodiceAzienda { get; set; }

        public string CodiceAnagrafica { get; set; }
        public string NomeAnagrafica { get; set; }

        public string NumeroOrdine { get; set; }
        public DateTime DataOrdine { get; set; }
        public string StatoOrdine { get; set; }

        public string Note { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }
}
