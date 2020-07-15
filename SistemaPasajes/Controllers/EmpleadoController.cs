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
                                 { //Para mostrar la info en pantalla
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

        [HttpPost]
        public ActionResult Agregar(EmpleadoCLS oEmpleadoCLS)
        {
            if (ModelState.IsValid)
            {
                using (var bd = new BDPasajeEntities())
                {
                    Empleado oEmpleado = new Empleado(); //Empleado de EF
                    oEmpleado.NOMBRE = oEmpleadoCLS.nombre;
                    oEmpleado.APPATERNO = oEmpleadoCLS.apPaterno;
                    oEmpleado.APMATERNO = oEmpleadoCLS.apMaterno;
                    oEmpleado.FECHACONTRATO = oEmpleadoCLS.fechaContrato;
                    oEmpleado.SUELDO = oEmpleadoCLS.sueldo;
                    oEmpleado.IIDTIPOUSUARIO = oEmpleadoCLS.iidtipoUsuario;
                    oEmpleado.IIDTIPOCONTRATO = oEmpleadoCLS.iidtipoContrato;
                    oEmpleado.IIDSEXO = oEmpleadoCLS.iidSexo;
                    oEmpleado.BHABILITADO = 1;

                    bd.Empleado.Add(oEmpleado);
                    bd.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            //Debo cargar los Combobox si el modelState no es valido para evitar un error
            listarCombos();
            return View(oEmpleadoCLS);
        }

        public ActionResult Editar(int id)
        {
            listarCombos(); //cargo los ComboBox
            EmpleadoCLS oEmpleadoCLS = new EmpleadoCLS();

            using (var bd = new BDPasajeEntities())
            {
                //Obtengo y almaceno el empleado a editar
                //Where devuelve varias filas, por eso se coloca First(), al final
                Empleado oEmpleado = bd.Empleado.Where(p => p.IIDEMPLEADO.Equals(id)).First();
                oEmpleadoCLS.iidEmpleado = oEmpleado.IIDEMPLEADO;
                oEmpleadoCLS.nombre = oEmpleado.NOMBRE;
                oEmpleadoCLS.apPaterno = oEmpleado.APPATERNO;
                oEmpleadoCLS.apMaterno = oEmpleado.APMATERNO;
                oEmpleadoCLS.fechaContrato = (DateTime)oEmpleado.FECHACONTRATO;
                oEmpleadoCLS.sueldo = (decimal)oEmpleado.SUELDO;
                oEmpleadoCLS.iidEmpleado = oEmpleado.IIDEMPLEADO;
                oEmpleadoCLS.iidtipoUsuario = (int)oEmpleado.IIDTIPOUSUARIO;
                oEmpleadoCLS.iidtipoContrato = (int)oEmpleado.IIDTIPOCONTRATO;
                oEmpleadoCLS.iidSexo = (int)oEmpleado.IIDSEXO;
            }
            return View(oEmpleadoCLS);
        }


        //============================================================

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