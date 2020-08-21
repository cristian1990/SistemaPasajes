using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaPasajes.Filtros;
using SistemaPasajes.Models;

namespace SistemaPasajes.Controllers
{
    [Acceder] //Agregamos el filtro
    public class EmpleadoController : Controller
    {
        // GET: Empleado
        public ActionResult Index(EmpleadoCLS oEmpleadoCls)
        {
            int idTipoUsuario = oEmpleadoCls.iidtipoUsuario; //Obtengo y almaceno lo ingresado
            List<EmpleadoCLS> listaEmpleado = null;
            listarTipoUsuario(); //Cargo los Combobox

            using (var bd = new BDPasajeEntities())
            {
                //Verifico si se ingreso un tipoEmpleado en busqueda
                //Si no se ingreso nada en el Combobox, su valor sera 0
                if (idTipoUsuario == 0)
                {
                    //Si no se ingreso filtro, muestro todo
                    //Utilizando LINQ
                    listaEmpleado = (from empleado in bd.Empleado
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
                else //Si se ingreso una marca para filtrar
                {
                    listaEmpleado = (from empleado in bd.Empleado
                                     join tipousuario in bd.TipoUsuario
                                     on empleado.IIDTIPOUSUARIO equals tipousuario.IIDTIPOUSUARIO
                                     join tipoContrato in bd.TipoContrato
                                     on empleado.IIDTIPOCONTRATO equals tipoContrato.IIDTIPOCONTRATO
                                     where empleado.BHABILITADO == 1
                                     && empleado.IIDTIPOUSUARIO == idTipoUsuario //Busco el tipoUsuario que tenga ese ID
                                     select new EmpleadoCLS
                                     {
                                         //Para mostrar info en pantalla
                                         iidEmpleado = empleado.IIDEMPLEADO,
                                         nombre = empleado.NOMBRE,
                                         apPaterno = empleado.APPATERNO,
                                         nombreTipoUsuario = tipousuario.NOMBRE,
                                         nombreTipoContrato = tipoContrato.NOMBRE
                                     }).ToList();
                }

            }

            return View(listaEmpleado);
        }

        public ActionResult Agregar() 
        {
            listarCombos();
            return View();
        }

        [HttpPost]
        public ActionResult Agregar(EmpleadoCLS oEmpleadoCLS)
        {
            //VALIDAR NOMBRE Y APELLIDOS
            int nregistrosAfectados = 0;
            string nombre = oEmpleadoCLS.nombre;
            string apPaterno = oEmpleadoCLS.apPaterno;
            string apMaterno = oEmpleadoCLS.apMaterno;

            using (var bd = new BDPasajeEntities())
            {
                //Verificamos la cantidad de empleados con mismo nombre y apellidos
                nregistrosAfectados = bd.Empleado.Where(
                    p => p.NOMBRE.Equals(nombre) && p.APPATERNO.Equals(apPaterno)
                    && p.APMATERNO.Equals(apMaterno)).Count();
            }
            //FIN 

            if (!ModelState.IsValid || nregistrosAfectados >= 1)
            {
                if (nregistrosAfectados >= 1) 
                    oEmpleadoCLS.mensajeError = "El empleado ya existe";
                
                listarCombos(); //Debo cargar los Combobox si el modelState no es valido para evitar un error
                return View(oEmpleadoCLS);
            }

            //Si el ModelState es valido y no existe el empleado, lo agregamos a la BD
            using (var bd = new BDPasajeEntities())
            {
                Empleado oEmpleado = new Empleado();
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

        [HttpPost]
        public ActionResult Editar(EmpleadoCLS oEmpleadoCSL)
        {
            //VALIDAR NOMBRE Y APELLIDOS
            int nregistrosAfectados = 0;
            int idEmpleado = oEmpleadoCSL.iidEmpleado;
            string nombre = oEmpleadoCSL.nombre;
            string apPaterno = oEmpleadoCSL.apPaterno;
            string apMaterno = oEmpleadoCSL.apMaterno;

            using (var bd = new BDPasajeEntities())
            {
                //Buscamos la cantidad de empleados con los mismos datos pero con distinto ID
                nregistrosAfectados = bd.Empleado.Where(
                  p => p.NOMBRE.Equals(nombre) && p.APPATERNO.Equals(apPaterno)
                  && p.APMATERNO.Equals(apMaterno) && !p.IIDEMPLEADO.Equals(idEmpleado)).Count();
            }
            //FIN

            if (!ModelState.IsValid || nregistrosAfectados >= 1)
            {
                if (nregistrosAfectados >= 1) 
                    oEmpleadoCSL.mensajeError = "Ya existe el empleado";
                
                listarCombos();
                return View(oEmpleadoCSL);
            }

            //Si el ModelState es valido y el empleado no existe, lo agrego a la BD
            using (var bd = new BDPasajeEntities())
            {
                //Busco al empleado por ID en la base de datos
                Empleado oEmpleado = bd.Empleado.Where(p => p.IIDEMPLEADO.Equals(idEmpleado)).First();
                oEmpleado.NOMBRE = oEmpleadoCSL.nombre;
                oEmpleado.APMATERNO = oEmpleadoCSL.apMaterno;
                oEmpleado.APPATERNO = oEmpleadoCSL.apPaterno;
                oEmpleado.NOMBRE = oEmpleadoCSL.nombre;
                oEmpleado.FECHACONTRATO = oEmpleadoCSL.fechaContrato;
                oEmpleado.SUELDO = oEmpleadoCSL.sueldo;
                oEmpleado.NOMBRE = oEmpleadoCSL.nombre;
                oEmpleado.IIDTIPOCONTRATO = oEmpleadoCSL.iidtipoContrato;
                oEmpleado.IIDTIPOUSUARIO = oEmpleadoCSL.iidtipoUsuario;
                oEmpleado.IIDSEXO = oEmpleadoCSL.iidSexo;

                bd.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //Eliminacion logica
        [HttpPost]
        public ActionResult Eliminar(int idEmpleado)
        {

            using (var bd = new BDPasajeEntities())
            {
                //Buscamos al empleado en la Base, mediante el ID
                Empleado emp = bd.Empleado.Where(p => p.IIDEMPLEADO.Equals(idEmpleado)).First();
                emp.BHABILITADO = 0;
                bd.SaveChanges();
            }
            return RedirectToAction("Index");
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