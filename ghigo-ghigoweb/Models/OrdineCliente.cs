using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_ordini_clienti")]
    public class OrdineCliente
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string NumeroOrdine { get; set; }
        public DateTime DataOrdine { get; set; }
        public DateTime DataConsegna { get; set; }

        public string Note { get; set; }

        public string CodiceAnagrafica { get; set; }
        public string CodiceAzienda { get; set; }

        public string CodiceAgente { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }

    [Table("VISTA_WEB_ORDINI_CLIENTI_APERTI")]
    public class OrdineClienteAperto
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string NumeroOrdine { get; set; }
        public DateTime DataOrdine { get; set; }
        public DateTime DataConsegna { get; set; }

        public string Note { get; set; }

        public string CodiceAnagrafica { get; set; }
        public string CodiceAzienda { get; set; }

        public string CodiceAgente { get; set; }
        public string NomeAgente { get; set; }

        public string NomeCliente { get; set; }

        public decimal Importo { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }

    [Table("VISTA_WEB_ORDINI_CLIENTI_CHIUSI")]
    public class OrdineClienteChiuso
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string NumeroOrdine { get; set; }
        public DateTime DataOrdine { get; set; }
        public DateTime DataConsegna { get; set; }

        public string Note { get; set; }

        public string CodiceAnagrafica { get; set; }
        public string CodiceAzienda { get; set; }

        public string CodiceAgente { get; set; }
        public string NomeAgente { get; set; }

        public string NomeCliente { get; set; }

        public decimal Importo { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }
}
