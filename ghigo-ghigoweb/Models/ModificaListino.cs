using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_MODIFICHE_LISTINO")]
    public class ModificaListino
    {
        [Key, Column(Order = 0)]
        public string CodiceAzienda { get; set; }

        [Key, Column(Order = 1)]
        public string CodiceListino { get; set; }
        public string DescrizioneListino { get; set; }

        [Key, Column(Order = 2)]
        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }
        public string NoteModifiche { get; set; }

        public decimal CostoAcquisto { get; set; }
        public decimal CostoAcquistoProposto { get; set; }
        public decimal VariazioneCosto { get; set; }

        public decimal ListinoWeb { get; set; }
        public decimal ListinoWebProposto { get; set; }
        public decimal VariazioneListinoWeb { get; set; }

        public string GruppoMinimo { get; set; }
        public string GruppoMinimoProposto { get; set; }

        public int MinimoOrdinabile { get; set; }
        public int MinimoOrdinabileProposto { get; set; }

        public bool Variato { get; set; }
        public string TipoVariazione { get; set; }
    }
}

