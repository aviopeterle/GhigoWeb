using System;
using System.Collections.Generic;
using System.Linq;

namespace GhigoWeb.Models
{
    public class OfferteViewModel
    {
        // offerte divise per ordine
        public Dictionary<string, IList<OffertaFornitore>> NumeroOrdineOfferte { get; set; }

        public long Quando { get; set; }
        public string Cerca { get; set; }
        public string ApriOrdine { get; set; }

        public bool OrdiniFornitoriDepositiDisponibili { get; set; }
    }
}