using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaPasajes.Models;

namespace SistemaPasajes.Controllers
{
    public class EmpleadoController : Controller
    {
        // GET: Empleado
        public ActionResult Index()
        {
            List <EmpleadoCLS> listaEmpleados = null;
            using (var bd = new BDPasajeEntities())
            {
                listaEmpleados = (from empleado in bd.Empleado
                                 join tipousuario in bd.TipoUsuario
                                 on empleado.IIDTIPOUSUARIO equals tipousuario.IIDTIPOUSUARIO
                                 join tipoContrato in bd.TipoContrato
                                 on empleado.IIDTIPOCONTRATO equals tipoContrato.IIDTIPOCONTRATO
                                 where empleado.BHABILITADO == 1
                                 select new EmpleadoCLS
                                 {
                                     iidEmpleado = empleado.IIDEMPLEADO,
                                     nombre = empleado.NOMBRE,
                                     apPaterno = empleado.APPATERNO,
                                     nombreTipoUsuario = tipousuario.NOMBRE,
                                     nombreTipoContrato = tipoContrato.NOMBRE
                                 }).ToList();
            }
                return View(listaEmpleados);
        }

        public ActionResult Agregar() 
        {
            listarCombos();
            return View();
        }


        //PARA LLENAR TODOS LOS COMBOBOX
        //Creo todas las listas necesarias y las paso a la vista
        public void listarComboSexo()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from sexo in bd.Sexo
                         where sexo.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = sexo.NOMBRE,
                             Value = sexo.IIDSEXO.ToString()
                         }).ToList();
                //Indico que al pricipio del combo aparece la palabra "Seleccione"
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaSexo = lista;
            }
        }

        public void listarTipoContrato()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.TipoContrato
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.NOMBRE,
                             Value = item.IIDTIPOCONTRATO.ToString()
                         }).ToList();

                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaTipoContrato = lista;
            }
        }

        public void listarTipoUsuario()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.TipoUsuario
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.NOMBRE,
                             Value = item.IIDTIPOUSUARIO.ToString()
                         }).ToList();
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaTipoUsuario = lista;
            }
        }

        public void listarCombos()
        {
            listarTipoUsuario();
            listarTipoContrato();
            listarComboSexo();
        }
    }
}