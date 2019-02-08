using System;
using System.Collections.Generic;
using System.Linq;

namespace GhigoWeb.Models
{
    public class ConferimentiViewModel
    {
        // conferimenti divisi per ordine
        public Dictionary<string, IList<OffertaFornitore>> NumeroOrdineConferimenti { get; set; }

        public long Quando { get; set; }
        public string Cerca { get; set; }
    }
}