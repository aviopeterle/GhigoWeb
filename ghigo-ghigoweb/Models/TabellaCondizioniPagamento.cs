using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhigoWeb.Models
{
    [Table("app_tabella_condizioni_pagamento")]
    public class TabellaCondizioniPagamento
    {
        [Key, Column("rec_id")]
        public long RecId { get; set; }

        public string CodicePagamento { get; set; }
        public string Pagamento { get; set; }
    }
}