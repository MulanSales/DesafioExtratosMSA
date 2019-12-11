using System;
using System.ComponentModel.DataAnnotations;

namespace Releases.API.Models
{
    public class ReleaseRequest
    {
        [Required]
        public string Date { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public string EstablishmentName { get; set; }

        [Required]
        public Decimal Amount { get; set; }

    }
}