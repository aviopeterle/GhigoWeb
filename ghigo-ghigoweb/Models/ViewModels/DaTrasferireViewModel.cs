using System;
using System.Collections.Generic;
using System.Linq;

namespace GhigoWeb.Models
{
    public class DaTrasferireViewModel
    {
        public Dictionary<string, IList<Conferimento>> NumeroOrdineConferimenti { get; set; }

        public IList<PropostaTrasferimento> Proposte { get; set; }
    }
}