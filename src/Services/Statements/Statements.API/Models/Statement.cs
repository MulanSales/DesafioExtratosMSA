using System;

namespace Statements.API.Models {

    public class Statement
    {
        public string Date { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string Type { get; set; }

        public Decimal TotalAmount { get; set; }

    }
}