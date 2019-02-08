using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace GhigoWeb.Models
{
    public class GhigoContext : DbContext
    {
        public GhigoContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<OffertaFornitore> OfferteFornitore { get; set; }
        public DbSet<TabellaStatoOrdine> TabellaStatoOrdine { get; set; }
        public DbSet<TabellaStatoOffertaCliente> TabellaStatoOffertaCliente { get; set; }

        public DbSet<Conferimento> Conferimenti { get; set; }
        public DbSet<PropostaTrasferimentoTestata> ProposteTrasferimentiTestate { get; set; }
        public DbSet<PropostaTrasferimento> ProposteTrasferimenti { get; set; }

        public DbSet<OrdineFornitoreDeposito> OrdiniFornitoriDepositi { get; set; }
        public DbSet<OrdineFornitoreDepositoDettaglio> OrdiniFornitoriDepositiDettaglio { get; set; }
        
        public DbSet<DDTConferimento> DDTConf { get; set; }
        public DbSet<DDTLibero> DDTFarmGros { get; set; }
        public DbSet<DDTVendita> DDTVendite { get; set; }
        public DbSet<ControlloArticoloLotto> ControlliArticoloLotto { get; set; }

        public DbSet<Articolo> Articoli { get; set; }
        public DbSet<ArticoloGlobale> ArticoliGlobali { get; set; }
        public DbSet<Listino> Listino { get; set; }

        public DbSet<DatiLotto> DatiLotti { get; set; }
        public DbSet<DatiCosto> DatiCosti { get; set; }
        public DbSet<LottoInvendibile> LottiInvendibili { get; set; }

        public DbSet<SaldoDeposito> SaldiDeposito { get; set; }

        public DbSet<AnagraficaPrivata> AnagrafichePrivate { get; set; }
        public DbSet<DestinazionePrivata> DestinazioniPrivate { get; set; }
        public DbSet<FatturaDeposito> FattureDeposito { get; set; }
        public DbSet<FatturaPrivata> FatturePrivate { get; set; }
        public DbSet<FatturaAccompagnatoriaDeposito> FattureAccompagnatorieDeposito { get; set; }
        public DbSet<NotaAccreditoPrivata> NoteAccreditoPrivate { get; set; }
        public DbSet<NotaAccreditoDeposito> NoteAccreditoDeposito { get; set; }

        public DbSet<OrdineCliente> OrdiniCliente { get; set; }
        public DbSet<OrdineClienteAperto> OrdiniClienteAperti { get; set; }
        public DbSet<OrdineClienteChiuso> OrdiniClienteChiusi { get; set; }
        public DbSet<OrdineClienteDettaglio> DettagliOrdiniCliente { get; set; }

        public DbSet<PropostaOrdineCliente> ProposteOrdiniCliente { get; set; }
        public DbSet<PropostaOrdineClienteDestinazioni> ProposteOrdiniClienteDestinazioni { get; set; }
        public DbSet<DestinazioneCliente> DestinazioniCliente { get; set; }

        public DbSet<OffertaCliente> OfferteCliente { get; set; }
        public DbSet<OffertaClienteDettaglio> DettagliOfferteCliente { get; set; }
        public DbSet<AgenteClienti> AgentiClienti { get; set; }

        public DbSet<ClientePotenziale> ClientiPotenziali { get; set; }
        public DbSet<Cliente> Clienti { get; set; }
        public DbSet<Fornitore> Fornitori { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<StatoConferimento> StatoConferimenti { get; set; }
        public DbSet<StatoOrdineFornitoreDeposito> StatoOrdiniFornitoreDeposito { get; set; }

        public DbSet<ModificaListino> ModificheListino { get; set; }

        public DbSet<CaricoDeposito> CarichiDeposito { get; set; }

        // altro
        public DbSet<TabellaIva> ElencoTabellaIva { get; set; }
        public DbSet<TabellaTipiPagamento> ElencoTabellaTipiPagamento { get; set; }
        public DbSet<TabellaPeriodi> ElencoTabellaPeriodi { get; set; }
        public DbSet<TabellaComuni> ElencoTabellaComuni { get; set; }
    }
}
