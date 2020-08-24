using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SistemaPasajes.Models
{
    public class ReservasRealizadasCLS
    {
        [Display(Name = "Id Venta")]
        public int iidventa { get; set; }

        [Display(Name = "Fecha Venta")]
        public DateTime fechaVenta { get; set; }

        [Display(Name = "Total Pagar")]
        public decimal total { get; set; }
    }
}