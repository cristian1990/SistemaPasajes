using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class RolController : Controller
    {
        // GET: Rol
        public ActionResult Index()
        {
            List<RolCLS> listaRol = new List<RolCLS>();
            using (var bd = new BDPasajeEntities())
            {
                //Utilizando LINQ
                listaRol = (from rol in bd.Rol
                            where rol.BHABILITADO == 1
                            select new RolCLS
                            {
                                //Datos a mostrar
                                iidRol = rol.IIDROL,
                                nombre = rol.NOMBRE,
                                descripcion = rol.DESCRIPCION
                            }).ToList();
            }

            return View(listaRol);
        }

        public ActionResult Filtro(string nombreRol) //Debe llamarse igual que el texbo de la vista
        {
            List<RolCLS> listaRol = new List<RolCLS>();
            using (var bd = new BDPasajeEntities())
            {
                if (nombreRol == null) //Si no se ingreso nada para filtrar
                { 
                    //Listo todos los roles
                    listaRol = (from rol in bd.Rol
                                where rol.BHABILITADO == 1
                                select new RolCLS
                                {
                                    iidRol = rol.IIDROL,
                                    nombre = rol.NOMBRE,
                                    descripcion = rol.DESCRIPCION
                                }).ToList();
                }
                else //Si se ingreso un rol en el filtro
                {
                    listaRol = (from rol in bd.Rol
                                where rol.BHABILITADO == 1
                                && rol.NOMBRE.Contains(nombreRol) //Busco el Rol, que contenga lo ingresado
                                select new RolCLS
                                {
                                    iidRol = rol.IIDROL,
                                    nombre = rol.NOMBRE,
                                    descripcion = rol.DESCRIPCION
                                }).ToList();
                }
                    
            }
            //Retorno la Vista Parcial, y envio la lista de Roles
            return PartialView("_TablaRol", listaRol);
        }

        public string Guardar(RolCLS oRolCLS, int titulo)
        {
            //Vacio indica error
            string rpta = string.Empty;

            try
            {
                if (!ModelState.IsValid)
                {
                    var query = (from state in ModelState.Values //Obtengo los estados de cada propiedad
                                 from error in state.Errors //Obtengo los mjes de error
                                 select error.ErrorMessage).ToList(); //En caso de haber errores los almaceno en una lista
                                                                      //Formo el HTML, para mostrar los errores en la vista
                    rpta += "<ul class='list-group'>";
                    foreach (var item in query)
                    {
                        rpta += "<li class='list-group-item'>" + item + "</li>";
                    }
                    rpta += "</ul>";
                }
                else
                {
                    using (var bd = new BDPasajeEntities())
                    {
                        //Si titulo = -1, es que intento agregar
                        if (titulo.Equals(-1)) //Este -1, lo recibo del Html.Hidden("titulo") del Index
                        {
                            Rol oRol = new Rol();
                            oRol.NOMBRE = oRolCLS.nombre;
                            oRol.DESCRIPCION = oRolCLS.descripcion;
                            oRol.BHABILITADO = 1;
                            bd.Rol.Add(oRol);
                            //SaveChanges, devuelve un int del numero de filas afectadas
                            rpta = bd.SaveChanges().ToString(); //Almaceno el numero de filas afectadas
                            
                            //si rpta es 0, es porque no se almaceno nada en la BD
                            if (rpta == "0") rpta = string.Empty; //Sobreescribo con vacio para indicar error
                        }
                        else //Si el titulo es >=1, es que quiero editar
                        {
                            Rol oRol = bd.Rol.Where(p => p.IIDROL == titulo).First(); //Buscamos el rol a editar
                            oRol.NOMBRE = oRolCLS.nombre;
                            oRol.DESCRIPCION = oRolCLS.descripcion;
                            //Si no se edita ningun valor el SaveChanges, valdra cero, si no vale el numero de filas afectadas
                            rpta = bd.SaveChanges().ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                rpta = string.Empty;
            }
           //Si todo se guardo bien enviara a la vista un "1", porque se afecto 1 fila
           return rpta;
        }

        //Busco la entidad para llenar los datos en el Popups Editar
        public JsonResult recuperarDatos(int titulo)
        {
            RolCLS oRolCLS = new RolCLS();
            using (var bd = new BDPasajeEntities())
            {
                Rol oRol = bd.Rol.Where(p => p.IIDROL == titulo).First(); //Busco el Rol
                oRolCLS.nombre = oRol.NOMBRE; //Asigno los datos al texbox
                oRolCLS.descripcion = oRol.DESCRIPCION;
            }
            //Serializo el objeto a Json
            return Json(oRolCLS, JsonRequestBehavior.AllowGet);
        }
    }
}