using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GhigoWeb.Models
{
    public class StatoConferimento
    {
        [Key]
        public long RecId { get; set; }

        public string RiferimentoCliente { get; set; }

        public string DescrizioneOrdineFornitore { get; set; }
        public string NumeroOrdineFornitore { get; set; }
        public string CodiceFornitore { get; set; }
        public string Fornitore { get; set; }
        public int RigaOrdineFornitore { get; set; }

        public DateTime DataCreazioneOrdineFornitore { get; set; }
        public DateTime DataChiusuraOrdineFornitore { get; set; }

        public string CodiceArticolo { get; set; }
        public string DescrizioneArticolo { get; set; }

        public decimal QtaRichiesta { get; set; }
        public decimal QtaOfferta { get; set; }
        public decimal QtaOffertaTotale { get; set; }
        public decimal QtaConferita { get; set; }
        public decimal QtaDaVendere { get; set; }
        public decimal QtaResidua { get; set; }
        public decimal QtaEsterna { get; set; }

        public decimal ValoreUnitario { get; set; }

        public string NoteMagazzino { get; set; }
        public string NoteUfficio { get; set; }
    }
}