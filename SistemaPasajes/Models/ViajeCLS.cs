using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SistemaPasajes.Models
{
    public class ViajeCLS
    {
        [Display(Name = "Id viaje")]
        public int iidViaje { get; set; }

        [Display(Name = "Lugar origen")]
        [Required]
        public int iidLugarOrigen { get; set; }

        [Display(Name = "Lugar destino")]
        [Required]
        public int iidLugarDestino { get; set; }

        [Display(Name = "Precio")]
        [Required]
        [Range(0, 100000, ErrorMessage = "Rango fuera de indices")]
        public decimal precio { get; set; }

        [Display(Name = "Fecha Viaje")]
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fechaViaje { get; set; }

        [Display(Name = "Bus")]
        [Required]
        public int iidBus { get; set; }

        [Display(Name = "Numero Asientos Disponibles")]
        [Required]
        public int numeroAsientosDisponibles { get; set; }

        //Propiedades adicionales
        [Display(Name = "Lugar Origen")]

        public string nombreLugarOrigen { get; set; }

        [Display(Name = "Lugar destino")]
        public string nombreLugarDestino { get; set; }

        [Display(Name = "Nombre Bus")]
        public string nombreBus { get; set; }


        //Propiedades adicionales
        public string nombreFoto { get; set; }

        public string mensaje { get; set; }

        //Para transformar el formato de la fecha
        public string fechaViajeCadena { get; set; }

        //Para mostrar la foto, debemos obtener la extension
        public string extension { get; set; }

        //Para convertir el tipo de la boto de Byte A Base64
        public string fotoRecuperCadena { get; set; }
    }
}