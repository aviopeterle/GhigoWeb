using System;
using System.Collections.Generic;
using System.Linq;

namespace GhigoWeb.Models
{
    public class RichiamaOfferteViewModel
    {
        // offerte divise per ordine
        public Dictionary<string, IList<OffertaFornitore>> NumeroOrdineOfferte { get; set; }

        public long OrdineFornitoreDepositoRecId { get; set; }
        public string Cerca { get; set; }
    }
}