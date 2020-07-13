using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaPasajes.Models; //Colocar

namespace SistemaPasajes.Controllers
{
    public class SucursalController : Controller
    {
        // GET: Sucursal
        public ActionResult Index()
        {
            List<SucursalCLS> listaSucursal = null;
            using (var bd = new BDPasajeEntities())
            {
                //Utilizando LINQ
                listaSucursal = (from s in bd.Sucursal
                                 where s.BHABILITADO == 1
                                 select new SucursalCLS
                                 {
                                     iidsucursal = s.IIDSUCURSAL,
                                     nombre = s.NOMBRE,
                                     telefono = s.TELEFONO,
                                     email = s.EMAIL
                                 }).ToList();
            }
            return View(listaSucursal);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Agregar(SucursalCLS oSucursalCLS)
        {
            if (ModelState.IsValid)
            {
                using (var bd = new BDPasajeEntities())
                {
                    Sucursal oSucursal = new Sucursal();
                    oSucursal.NOMBRE = oSucursalCLS.nombre;
                    oSucursal.DIRECCION = oSucursalCLS.direccion;
                    oSucursal.TELEFONO = oSucursalCLS.telefono;
                    oSucursal.EMAIL = oSucursalCLS.email;
                    oSucursal.FECHAAPERTURA = oSucursalCLS.fechaApertura;
                    oSucursal.BHABILITADO = 1;
                    bd.Sucursal.Add(oSucursal);
                    bd.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(oSucursalCLS);
        }
    }
}