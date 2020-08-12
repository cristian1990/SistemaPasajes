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

        public ActionResult Filtrar(int? iidrol) //Puede aceptar null
        {

            List<RolPaginaCLS> listaRol = new List<RolPaginaCLS>();
            using (var bd = new BDPasajeEntities())
            {
                //Si no se ingreso nada para filtrar
                if (iidrol == null)
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
                                && rolpagina.IIDROL == iidrol //Filtro por el idRol ingresado
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

        public int Guardar(RolPaginaCLS oRolPaginaCLS, int titulo)
        {
            int rpta = 0; //Nro de registros afectados

            using (var bd = new BDPasajeEntities())
            {
                if (titulo == 1) //Si es 1, se quiere agregar
                {
                    RolPagina oRolPagina = new RolPagina();
                    oRolPagina.IIDROL = oRolPaginaCLS.iidrol;
                    oRolPagina.IIDPAGINA = oRolPaginaCLS.iidpagina;
                    oRolPagina.BHABILITADO = 1;
                    bd.RolPagina.Add(oRolPagina);
                    //SaveChanges: devuelve el numero de registros afectados
                    rpta = bd.SaveChanges();
                }
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