using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_LISTINO")]
    public class Listino
    {
        public string CodiceAzienda { get; set; }
        public string CodiceAnagrafica { get; set; }

        [Key]
        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }

        public string CodiceProduttore { get; set; }
        public string Produttore { get; set; }

        public decimal Prezzo { get; set; }
        public decimal PrezzoPubblico { get; set; }
        public decimal PrezzoProposto { get; set; }

        public decimal ScontoLordo { get; set; }
        public decimal ScontoNetto { get; set; }
        public int MinimoOrdinabile { get; set; }
        public int InProposta { get; set; }

        public int Disponibilita { get; set; }

        public string Note { get; set; }

        public string Ean13 { get; set; }
        public string Code32 { get; set; }

        public long Ordinamento { get; set; }

        public string CodiceCategoria { get; set; }
        public string Categoria { get; set; }
        public string OrdinamentoCategoria { get; set; }
        public string ColoreCategoria { get; set; }

        public string GruppoProvvigione { get; set; }
    }
}

