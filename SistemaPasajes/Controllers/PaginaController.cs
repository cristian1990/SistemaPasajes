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

        public string Guardar(PaginaCLS oPaginaCLS, int titulo)
        {
            //rpta: almacena el numero de registros afectados (Si esta vacio es error)
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
                else //Si el model es valido
                {
                    using (var bd = new BDPasajeEntities())
                    {
                        //-1 cuando queremos agregar
                        if (titulo == -1)
                        {
                            Pagina oPagina = new Pagina();
                            oPagina.MENSAJE = oPaginaCLS.mensaje;
                            oPagina.ACCION = oPaginaCLS.accion;
                            oPagina.CONTROLADOR = oPaginaCLS.controlador;
                            oPagina.BHABILITADO = 1;
                            bd.Pagina.Add(oPagina);
                            //SaveChanges retorna el numero de filas afectadas, y lo guardo en rpta
                            rpta = bd.SaveChanges().ToString();
                            //Si rpta es 0, es que no se inserto nada
                            if (rpta == "0") rpta = string.Empty; //Sobreescribo a vacio
                        }
                        else //si no, queremos editar (tendra el Id del rol)
                        {
                            Pagina oPagina = bd.Pagina.Where(p => p.IIDPAGINA == titulo).First();
                            oPagina.MENSAJE = oPaginaCLS.mensaje;
                            oPaginaCLS.controlador = oPaginaCLS.controlador;
                            oPaginaCLS.accion = oPaginaCLS.accion;
                            rpta = bd.SaveChanges().ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                rpta = string.Empty;
            }
            return rpta;
        }

        //Para recuperar informacion y luego con JS agregarla al popups
        public JsonResult recuperarInformacion(int idPagina)
        {
            PaginaCLS oPaginaCLS = new PaginaCLS();

            using (var bd = new BDPasajeEntities())
            {
                //Busco la pagina por su Id
                Pagina oPagina = bd.Pagina.Where(p => p.IIDPAGINA == idPagina).First();
                oPaginaCLS.mensaje = oPagina.MENSAJE; //Asigno los valores para mostrar en los textbox
                oPaginaCLS.accion = oPagina.ACCION;
                oPaginaCLS.controlador = oPagina.CONTROLADOR;
            }
            //Serializo el objeto a Json
            return Json(oPaginaCLS, JsonRequestBehavior.AllowGet);
        }

        //Metodo para hacer el borrado logico de la pagina
        public int EliminarPagina(int iidpagina) //Aca reciubo un id, en la otra recibiamos el objeto
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
    }
}