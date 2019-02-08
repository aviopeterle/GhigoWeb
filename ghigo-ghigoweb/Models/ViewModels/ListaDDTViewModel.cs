using System;
using System.Collections.Generic;
using System.Linq;

namespace GhigoWeb.Models
{
    public class ListaDDTViewModel
    {
        public string Cerca { get; set; }
        public IList<DDTConferimento> Conferimenti { get; set; }
        public IList<DDTLibero> FarmaciaGrossista { get; set; }
        public IList<DDTVendita> GrossistaFarmacia { get; set; }
        public IList<DDTVendita> Vendite { get; set; }
    }
}