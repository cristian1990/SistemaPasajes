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
            //VALIDAR EL NOMBRE DE LA SUCURSAL
            int nregistrosEncontrados = 0;
            string nombreSucursal = oSucursalCLS.nombre;

            using (var bd = new BDPasajeEntities())
            {
                nregistrosEncontrados = bd.Sucursal.Where(p => p.NOMBRE.Equals(nombreSucursal)).Count();
            }
            //FIN

            if (!ModelState.IsValid || nregistrosEncontrados >= 1)
            {
                if (nregistrosEncontrados >= 1) 
                    oSucursalCLS.mensajeError = "Ya existe la sucursal a ingresar";

                return View(oSucursalCLS);
            }

            //Si el ModelState es valido y el nombre de la Sucursal no se repite, guardamos en BD
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

        //Para recuperar los datos y mostrarlos en pantalla
        public ActionResult Editar(int id) //El id, lo recibo de la vista Index
        {
            SucursalCLS oSucursalCLS = new SucursalCLS();
            using (var bd = new BDPasajeEntities())
            {  //Buscamos la sucursal y la almacenamos en oSucursal
                Sucursal oSucursal = bd.Sucursal.Where(p => p.IIDSUCURSAL.Equals(id)).First();
                oSucursalCLS.iidsucursal = oSucursal.IIDSUCURSAL;
                oSucursalCLS.nombre = oSucursal.NOMBRE;
                oSucursalCLS.direccion = oSucursal.DIRECCION;
                oSucursalCLS.telefono = oSucursal.TELEFONO;
                oSucursalCLS.email = oSucursal.EMAIL;
                oSucursalCLS.fechaApertura = (DateTime)oSucursal.FECHAAPERTURA;
            }
            return View(oSucursalCLS);
        }

        //Para realizar la edicion en la base de datos
        [HttpPost]
        public ActionResult Editar(SucursalCLS oSucursalCLS)
        {
            int nregistrosAfectados = 0;
            int idSucursal = oSucursalCLS.iidsucursal;
            string nombreSucursal = oSucursalCLS.nombre;

            using (var bd = new BDPasajeEntities())
            {
                //Buscamos la cantidad de registros en la base con el mismo nombre y distinto ID
                nregistrosAfectados = bd.Sucursal.Where(p => p.NOMBRE.Equals(nombreSucursal) && !p.IIDSUCURSAL.Equals(idSucursal)).Count();
            }
            
            if (!ModelState.IsValid || nregistrosAfectados >= 1)
            {
                if (nregistrosAfectados >= 1) oSucursalCLS.mensajeError = "Ya existe la sucursal";
                return View(oSucursalCLS);
            }

            using (var bd = new BDPasajeEntities())
            {
                Sucursal oSucursal = bd.Sucursal.Where(p => p.IIDSUCURSAL.Equals(idSucursal)).First();
                oSucursal.NOMBRE = oSucursalCLS.nombre;
                oSucursal.DIRECCION = oSucursalCLS.direccion;
                oSucursal.TELEFONO = oSucursalCLS.telefono;
                oSucursal.EMAIL = oSucursalCLS.email;
                oSucursal.FECHAAPERTURA = oSucursalCLS.fechaApertura;
                bd.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Eliminar(int id) //Recibo el ID de la vista
        {
            using (var bd = new BDPasajeEntities())
            {
                //Busco la sucursal por su ID y la almaceno
                Sucursal oSucursal = bd.Sucursal.Where(p => p.IIDSUCURSAL.Equals(id)).First();
                oSucursal.BHABILITADO = 0;
                bd.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}