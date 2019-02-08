using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GhigoWeb.Models
{
    public class StatoOrdineFornitoreDeposito
    {
        [Key]
        public long RecId { get; set; }

        public string CodiceDeposito { get; set; }

        public string NumeroOrdine { get; set; }
        public string CodiceFornitore { get; set; }
        public string Fornitore { get; set; }

        public DateTime DataOrdine { get; set; }
        public DateTime DataConsegna { get; set; }

        public string Note { get; set; }

        public string CodicePagamento { get; set; }
        public string CodiceDestinazione { get; set; }
        public string DestinazioneRagioneSociale { get; set; }

        public bool OrdineChiuso { get; set; }
        public bool OrdineChiusoTotalmente { get; set; }

        public string StatoOrdine { get; set; }

        public int Riga { get; set; }

        public string CodiceArticolo { get; set; }
        public string DescrizioneArticolo { get; set; }
        public string CodiceProduttore { get; set; }

        public decimal Quantita { get; set; }
        public decimal QuantitaConfermata { get; set; }
        public decimal QuantitaEvasa { get; set; }
        public decimal QuantitaResidua { get; set; }

        public decimal ValoreUnitario { get; set; }

        public string RifOffertaCodiceFornitore { get; set; }
        public string RifOffertaNumeroOrdine { get; set; }
    }
}