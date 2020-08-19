using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class RolPaginaController : Controller
    {
        // GET: RolPagina
        public ActionResult Index()
        {
            listarComboRol();
            listarComboPagina();
            List<RolPaginaCLS> listaRol = new List<RolPaginaCLS>();
            using (var bd = new BDPasajeEntities())
            {
                //Utilizando LINQ
                listaRol = (from rolpagina in bd.RolPagina
                            join rol in bd.Rol
                            on rolpagina.IIDROL equals rol.IIDROL
                            join pagina in bd.Pagina
                            on rolpagina.IIDPAGINA equals pagina.IIDPAGINA
                            where rolpagina.BHABILITADO == 1
                            select new RolPaginaCLS //Creo un objeto
                            {   //Le asigno valores al objeto
                                iidrolpagina = rolpagina.IIDROLPAGINA,
                                nombreRol = rol.NOMBRE,
                                nombreMensaje = pagina.MENSAJE
                            }).ToList();
            }
            return View(listaRol);
        }

        public ActionResult Filtrar(int? iidrolFiltro) //Puede aceptar null
        {

            List<RolPaginaCLS> listaRol = new List<RolPaginaCLS>();
            using (var bd = new BDPasajeEntities())
            {
                //Si no se ingreso nada para filtrar
                if (iidrolFiltro == null)
                {
                    //Listamos todo
                    listaRol = (from rolpagina in bd.RolPagina
                                join rol in bd.Rol
                                on rolpagina.IIDROL equals
                                rol.IIDROL
                                join pagina in bd.Pagina
                                on rolpagina.IIDPAGINA equals
                                pagina.IIDPAGINA
                                where rolpagina.BHABILITADO == 1
                                select new RolPaginaCLS
                                {
                                    iidrolpagina = rolpagina.IIDROLPAGINA,
                                    nombreRol = rol.NOMBRE,
                                    nombreMensaje = pagina.MENSAJE
                                }).ToList();
                }
                else //Si se ingreso algo para filtrar
                {
                    listaRol = (from rolpagina in bd.RolPagina
                                join rol in bd.Rol
                                on rolpagina.IIDROL equals
                                rol.IIDROL
                                join pagina in bd.Pagina
                                on rolpagina.IIDPAGINA equals
                                pagina.IIDPAGINA
                                where rolpagina.BHABILITADO == 1
                                && rolpagina.IIDROL == iidrolFiltro //Filtro por el idRol ingresado
                                select new RolPaginaCLS
                                {
                                    iidrolpagina = rolpagina.IIDROLPAGINA,
                                    nombreRol = rol.NOMBRE,
                                    nombreMensaje = pagina.MENSAJE
                                }).ToList();
                }

            }
            //Retorno a la vista parcial y envio la lista
            return PartialView("_TablaRolPagina", listaRol);
        }

        public string Guardar(RolPaginaCLS oRolPaginaCLS, int titulo)
        {
            //Nro de registros afectados (0 indica error)
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
                        if (titulo == -1) //Si es -1, se quiere agregar
                        {
                            RolPagina oRolPagina = new RolPagina();
                            oRolPagina.IIDROL = oRolPaginaCLS.iidrol;
                            oRolPagina.IIDPAGINA = oRolPaginaCLS.iidpagina;
                            oRolPagina.BHABILITADO = 1;
                            bd.RolPagina.Add(oRolPagina);
                            //SaveChanges: devuelve el numero de registros afectados
                            rpta = bd.SaveChanges().ToString();
                            //En guardar no se permite que la rpta sea 0, pero si en editar
                            if (rpta == "0") rpta = ""; //Sobrescribimos el valor
                        }
                        else //Si no, si intentamos editar
                        {
                            RolPagina oRolPagina = bd.RolPagina.Where(p => p.IIDROLPAGINA == titulo).First(); //Buscamos la entidad por su ID
                            oRolPagina.IIDROL = oRolPaginaCLS.iidrol; //Asignamos el nuevo valor
                            oRolPagina.IIDPAGINA = oRolPaginaCLS.iidpagina;
                            rpta = bd.SaveChanges().ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                rpta = "";
            }
            return rpta;
        }

        //Accion para recuperrar la info y mostrarla cargada en el Popups al editar
        public JsonResult recuperarInformacion(int idRolPagina) //El id, se recibe de la pagina parcial
        {
            RolPaginaCLS oRolPaginaCLS = new RolPaginaCLS();
            using (var bd = new BDPasajeEntities())
            {
                RolPagina oRolPagina = bd.RolPagina.Where(p => p.IIDROLPAGINA == idRolPagina).First(); //Buscamos la entidad por su Id
                oRolPaginaCLS.iidrol = (int)oRolPagina.IIDROL; //Asignamos los valores
                oRolPaginaCLS.iidpagina = (int)oRolPagina.IIDROLPAGINA;
            }
            //Serializo el objeto a JSON
            return Json(oRolPaginaCLS, JsonRequestBehavior.AllowGet);
        }


        //Metodo para hacer el borrado logico de la pagina
        public int EliminarPagina(int iidpagina) //Aca recibo un id, en la otra recibiamos el objeto
        {
            int rpta = 0; //Error
            try
            {
                using (var bd = new BDPasajeEntities())
                {
                    Pagina oPagina = bd.Pagina.Where(p => p.IIDPAGINA == iidpagina).First(); //Obtengo la pagina por su Id
                    oPagina.BHABILITADO = 0;
                    rpta = bd.SaveChanges(); //Tendra 1 si se pudo hacer la operacion
                }
            }
            catch (Exception ex)
            {
                rpta = 0;
            }
            return rpta;
        }

        //Accion para realizar el eliminado logico
        public int EliminarRolPagina(int iidrolpagina) //Aca pasamos un Id, en otro un objeto
        {
            //Error
            int rpta = 0;
            try
            {
                using (var bd = new BDPasajeEntities())
                {
                    RolPagina oRolPagina = bd.RolPagina.Where(p => p.IIDROLPAGINA == iidrolpagina).First(); //Busco la entidad
                    oRolPagina.BHABILITADO = 0;
                    rpta = bd.SaveChanges(); //rpta tendra 1, si va bien porque afecta una fila
                }
            }
            catch (Exception ex)
            {
                rpta = 0;
            }
            return rpta;
        }

        //============================================================

        //PARA LLENAR TODOS LOS COMBOBOX
        //Creo todas las listas necesarias y las paso a la vista
        public void listarComboRol()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.Rol
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.NOMBRE,
                             Value = item.IIDROL.ToString()
                         }).ToList();
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaRol = lista;
            }
        }

        public void listarComboPagina()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.Pagina
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.MENSAJE,
                             Value = item.IIDPAGINA.ToString()
                         }).ToList();
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaPagina = lista;
            }
        }
    }
}