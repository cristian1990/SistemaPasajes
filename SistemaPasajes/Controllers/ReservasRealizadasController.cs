using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class ReservasRealizadasController : Controller
    {
        // GET: ReservasRealizadas 
        public ActionResult Index()
        {
            //Obtener el Id del usuario logueado (Obtenemos la sesion creada en el controlador Login)
            Usuario ousuario = (Usuario)Session["Usuario"];
            int iidusuario = ousuario.IIDUSUARIO; //Obtengo el ID del usuario Logueado
            List<ReservasRealizadasCLS> listaReserva = new List<ReservasRealizadasCLS>();
            
            using (BDPasajeEntities db = new BDPasajeEntities())
            {
                //Obtengo la lista de reservas del usuario Logueado
                listaReserva = (from venta in db.VENTA
                                where venta.BHABILITADO == 1 && venta.IIDUSUARIO == iidusuario
                                select new ReservasRealizadasCLS //Llenamos la entidad con los datos
                                {
                                    iidventa = venta.IIDVENTA,
                                    fechaVenta = (DateTime)venta.FECHAVENTA,
                                    total = (decimal)venta.TOTAL
                                }).ToList();
            }
            return View(listaReserva);
        }
    }
}