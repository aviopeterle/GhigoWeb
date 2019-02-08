using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_ddt_trasferimenti_fornitori_proposta")]
    public class PropostaTrasferimentoTestata
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }

        public string Descrizione { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }

    [Table("app_ddt_trasferimenti_fornitori_proposta_dettaglio")]
    public class PropostaTrasferimento
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public long RecIdTestata { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }

        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }

        public string Ean13 { get; set; }
        public string Code32 { get; set; }

        public int Quantita { get; set; }
        public string Lotto { get; set; }
        public DateTime ScadenzaLotto { get; set; }

        public decimal CostoNettoUnitario { get; set; }
        public decimal PrezzoVendita { get; set; }

        [Column("ultima_modifica")]
        public DateTime UltimaModifica { get; set; }
    }
}
