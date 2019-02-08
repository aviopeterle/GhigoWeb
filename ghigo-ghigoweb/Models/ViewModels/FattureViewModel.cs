using System;
using System.Collections.Generic;
using System.Linq;

namespace GhigoWeb.Models
{
    public class FattureViewModel
    {
        public string Cerca { get; set; }
        public IList<FatturaDeposito> FattureDeposito { get; set; }
        public IList<FatturaPrivata> FatturePrivate { get; set; }
        public IList<FatturaAccompagnatoriaDeposito> FattureAccompagnatorieDeposito { get; set; }
        public IList<NotaAccreditoPrivata> NoteAccreditoPrivate { get; set; }
        public IList<NotaAccreditoDeposito> NoteAccreditoDeposito { get; set; }
    }
}