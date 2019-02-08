using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("VISTA_WEB_DDT_ARTICOLO_LOTTO")]
    public class ControlloArticoloLotto
    {
        [Key, Column(Order = 0)]
        public string CodiceAzienda { get; set; }

        [Key, Column(Order = 1)]
        public string CodiceFornitore { get; set; }

        [Key, Column(Order = 2)]
        public string TipoDdt { get; set; }

        [Key, Column(Order = 3)]
        public string NumeroDdt { get; set; }

        [Key, Column(Order = 4)]
        public int Riga { get; set; }

        public DateTime DataDdT { get; set; }

        public string CodiceArticolo { get; set; }
        public string DescrizioneArticolo { get; set; }
        public string Lotto { get; set; }
        public DateTime ScadenzaLotto { get; set; }

        public long RecIdDdt { get; set; }

        public int Quantita { get; set; }
    }
}
