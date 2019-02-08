using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_note_credito_private")]
    public class NotaAccreditoPrivata
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string NumeroDocumento { get; set; }
        public DateTime DataDocumento { get; set; }

        public string CodiceFornitore { get; set; }
        public string CodiceAzienda { get; set; }

        public string RagioneSociale { get; set; }
        public string RagioneSociale1 { get; set; }
        public string Indirizzo { get; set; }
        public string Indirizzo1 { get; set; }
        public string Citta { get; set; }
        public string Cap { get; set; }

        public string ItaliaEstero { get; set; }
        public string CodiceFiscale { get; set; }
        public string PartitaIva { get; set; }

        public string EsenzioneIva { get; set; }
        public DateTime ScadenzaEsenzione { get; set; }

        public string NumeroLetteraIntento { get; set; }
        public DateTime DataLetteraIntento { get; set; }

        public string IdTracciabilitaFarmaco { get; set; }
        public string NomenclaturaCombinata { get; set; }

        public string CodicePagamento { get; set; }

        public string DestinazioneRagioneSociale { get; set; }
        public string DestinazioneRagioneSociale1 { get; set; }
        public string DestinazioneIndirizzo { get; set; }
        public string DestinazioneIndirizzo1 { get; set; }
        public string DestinazioneCitta { get; set; }
        public string DestinazioneCap { get; set; }

        public string Note1 { get; set; }

        public int NumeroColli { get; set; }
        public decimal PesoKg { get; set; }

        public string CodiceAspetto { get; set; }
        public string CodicePorto { get; set; }
        public string CodiceACura { get; set; }

        public string NumeroFatturaAcconto { get; set; }
        public DateTime DataFatturaAcconto { get; set; }
        public decimal ImportoFatturaAcconto { get; set; }

        public bool AllegatoPackingList { get; set; }
        public bool MerceOrigineItaliana { get; set; }

        public decimal TotaleDocumento { get; set; }
    }
}
