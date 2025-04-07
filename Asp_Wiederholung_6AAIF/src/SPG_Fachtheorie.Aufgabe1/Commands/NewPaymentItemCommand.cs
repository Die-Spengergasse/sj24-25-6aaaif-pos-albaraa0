using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record NewPaymentItemCommand(
        [Required(ErrorMessage = "The article name is required.")]
        [StringLength(100, ErrorMessage = "The article name must not exceed 100 characters.")]
        string ArticleName,

        [Range(1, int.MaxValue, ErrorMessage = "The amount must be at least 1.")]
        int Amount,

        [Range(0.01, double.MaxValue, ErrorMessage = "The price must be greater than zero.")]
        decimal Price,

        [Required(ErrorMessage = "A valid payment reference is required.")]
        Payment Payment
    );
}
