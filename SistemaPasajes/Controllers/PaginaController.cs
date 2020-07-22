using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class PaginaController : Controller
    {
        // GET: Pagina
        public ActionResult Index()
        {
            List<PaginaCLS> listaPagina = new List<PaginaCLS>();

            using (var bd = new BDPasajeEntities())
            {
                //Utilizando LINQ
                listaPagina = (from pagina in bd.Pagina
                               where pagina.BHABILITADO == 1
                               select new PaginaCLS
                               {
                                   //Info que quiero mostrar
                                   iidpagina = pagina.IIDPAGINA,
                                   mensaje = pagina.MENSAJE,
                                   controlador = pagina.CONTROLADOR,
                                   accion = pagina.ACCION
                               }).ToList();
            }

            return View(listaPagina);
        }

        public ActionResult Filtrar(PaginaCLS oPaginaCLS)
        {
            string mensaje = oPaginaCLS.mensajeFiltro;
            List<PaginaCLS> listaPagina = new List<PaginaCLS>();

            using (var bd = new BDPasajeEntities())
            {
                if (mensaje == null) //Si no se ingreso nada para filtrar
                {
                    //Listo todos las Paginas
                    listaPagina = (from pagina in bd.Pagina
                                   where pagina.BHABILITADO == 1
                                   select new PaginaCLS
                                   {
                                       iidpagina = pagina.IIDPAGINA,
                                       mensaje = pagina.MENSAJE,
                                       controlador = pagina.CONTROLADOR,
                                       accion = pagina.ACCION
                                   }).ToList();
                }
                else //Si se ingreso un rol en el filtro
                {
                    listaPagina = (from pagina in bd.Pagina
                                   where pagina.BHABILITADO == 1
                                   && pagina.MENSAJE.Contains(mensaje) //Busco la Pagina, que contenga lo ingresado
                                   select new PaginaCLS
                                   {
                                       iidpagina = pagina.IIDPAGINA,
                                       mensaje = pagina.MENSAJE,
                                       controlador = pagina.CONTROLADOR,
                                       accion = pagina.ACCION
                                   }).ToList();
                }
            }

            //Retorno la Vista Parcial, y envio la lista de Paginas
            return PartialView("_TablaPagina", listaPagina);
        }

        public int Guardar(PaginaCLS oPaginaCLS, int titulo)
        {
            //rpta: almacena el numero de registros afectados
            int rpta = 0;

            using (var bd = new BDPasajeEntities())
            {
                if (titulo == 1)
                {
                    Pagina oPagina = new Pagina();
                    oPagina.MENSAJE = oPaginaCLS.mensaje;
                    oPagina.ACCION = oPaginaCLS.accion;
                    oPagina.CONTROLADOR = oPaginaCLS.controlador;
                    oPagina.BHABILITADO = 1;
                    bd.Pagina.Add(oPagina);
                    //SaveChanges retorna el numero de filas afectadas, y lo guardo en rpta
                    rpta = bd.SaveChanges();
                }
            }

            return rpta;
        }
    }
}