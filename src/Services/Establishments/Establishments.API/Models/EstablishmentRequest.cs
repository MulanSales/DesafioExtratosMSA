using System;
using System.ComponentModel.DataAnnotations;

namespace Establishments.API.Models
{
    public class EstablishmentRequest 
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

    }
}