using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class ViajeController : Controller
    {
        // GET: Viaje
        public ActionResult Index()
        {
            List<ViajeCLS> listaViaje = null;
            listarCombos();
            using (var bd = new BDPasajeEntities())
            {//Debo crear una variable de Rango para acceder a las tablas (ej: viaje para bd.Viaje)
                listaViaje = (from viaje in bd.Viaje
                              join lugarOrigen in bd.Lugar //Creo variable de Rango
                              on viaje.IIDLUGARORIGEN equals lugarOrigen.IIDLUGAR //Hago la union
                              join lugarDestino in bd.Lugar //Creo variable de Rango
                              on viaje.IIDLUGARDESTINO equals lugarDestino.IIDLUGAR //Hago la union
                              join bus in bd.Bus //Creo variable de Rango
                              on viaje.IIDBUS equals bus.IIDBUS //Hago la union
                              where viaje.BHABILITADO == 1
                              select new ViajeCLS
                              {
                                  //Selecciono lo que voy a mostrar
                                  iidViaje = viaje.IIDVIAJE,
                                  nombreBus = bus.PLACA,
                                  nombreLugarOrigen = lugarOrigen.NOMBRE,
                                  nombreLugarDestino = lugarDestino.NOMBRE

                              }).ToList();

            }
            return View(listaViaje);
        }

        public ActionResult Agregar()
        {
            listarCombos();
            return View();
        }

        //HttpPostedFileBase: para indicar que recibimos una foto
        public string Guardar(ViajeCLS oViajeCls, HttpPostedFileBase foto, int titulo)
        {
            //Error
            string rpta = string.Empty;

            try
            {
                if (!ModelState.IsValid || (foto == null && titulo == -1))
                {
                    //Obtengo el estado y el mensaje del error
                    var query = (from state in ModelState.Values
                                 from error in state.Errors
                                 select error.ErrorMessage).ToList();
                    if (foto == null && titulo == -1) //Si no tengo foto y quiero agregar (Foto solo obligatoria al agregar)
                    {
                        oViajeCls.mensaje = "La foto es obligatoria";
                        rpta += "<ul><li> Debe ingresar la foto</li></ul>";
                    }
                    rpta += "<ul class='list-group'>";
                    foreach (var item in query)
                    {
                        rpta += "<li class='list-group-item'>" + item + "</li>";
                    }
                    rpta += "</ul>";
                }
                else
                {
                    byte[] fotoBD = null;
                    if (foto != null) //Osea se ingreso una imagen
                    {
                        BinaryReader lector = new BinaryReader(foto.InputStream); //Leemos el archivo
                        fotoBD = lector.ReadBytes((int)foto.ContentLength); //Indicamos que lea todo el archivo
                    }

                    using (var bd = new BDPasajeEntities())
                    {
                        if (titulo == -1) //-1 indica que quiero Agregar
                        {
                            //Creo un nuevo objeto y asigno la informacion
                            Viaje oViaje = new Viaje();
                            oViaje.IIDBUS = oViajeCls.iidBus;
                            oViaje.IIDLUGARDESTINO = oViajeCls.iidLugarDestino;
                            oViaje.IIDLUGARORIGEN = oViajeCls.iidLugarOrigen;
                            oViaje.PRECIO = oViajeCls.precio;
                            oViaje.FECHAVIAJE = oViajeCls.fechaViaje;
                            oViaje.NUMEROASIENTOSDISPONIBLES = oViajeCls.numeroAsientosDisponibles;
                            oViaje.FOTO = fotoBD; //Aca asigno la foto leida anteriormente
                            oViaje.nombrefoto = oViajeCls.nombreFoto;
                            oViaje.BHABILITADO = 1;
                            bd.Viaje.Add(oViaje);
                            rpta = bd.SaveChanges().ToString(); //Almacenamos la cant de filas afectadas
                            if (rpta == "0") rpta = ""; //Sobrescribimos si es 0, (solo se permite en el editar)
                        }
                        else //Si no es que queremos editar
                        {
                            //Construyo y lleno el objeto con los datos
                            Viaje oViaje = bd.Viaje.Where(p => p.IIDVIAJE == titulo).First(); //Busco la entidad por su Id (Titulo tendra en Id)            
                            oViaje.IIDLUGARDESTINO = oViajeCls.iidLugarDestino;
                            oViaje.IIDLUGARORIGEN = oViajeCls.iidLugarOrigen;
                            oViaje.PRECIO = oViajeCls.precio;
                            oViaje.FECHAVIAJE = oViajeCls.fechaViaje;
                            oViaje.IIDBUS = oViajeCls.iidBus;
                            oViaje.NUMEROASIENTOSDISPONIBLES = oViajeCls.numeroAsientosDisponibles;
                            if (foto != null) oViaje.FOTO = fotoBD; //Si no, mas arriba esta la logica para mostrar un mensaje
                            rpta = bd.SaveChanges().ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                rpta = "";
            }

            //Si todo salio bien se enviara un "1" a la vista
            return rpta;
        }

        //Accion para filtrar por lugar de destino
        public ActionResult Filtrar(int? lugarDestinoParametro) //int? = Permite valores null (lugarDestinoParametro viene de la vista Index)
        {
            List<ViajeCLS> listaViaje = new List<ViajeCLS>();

            using (var bd = new BDPasajeEntities())
            {
                if (lugarDestinoParametro == null) //Si no se ingreso nada para filtrar
                {
                    //Traemos todos los viajes
                    listaViaje = (from viaje in bd.Viaje
                                  join lugarOrigen in bd.Lugar
                                  on viaje.IIDLUGARORIGEN equals lugarOrigen.IIDLUGAR
                                  join lugarDestino in bd.Lugar
                                  on viaje.IIDLUGARDESTINO equals lugarDestino.IIDLUGAR
                                  join bus in bd.Bus
                                  on viaje.IIDBUS equals bus.IIDBUS
                                  where viaje.BHABILITADO == 1
                                  select new ViajeCLS
                                  {
                                      iidViaje = viaje.IIDVIAJE,
                                      nombreBus = bus.PLACA,
                                      nombreLugarOrigen = lugarOrigen.NOMBRE,
                                      nombreLugarDestino = lugarDestino.NOMBRE

                                  }).ToList();
                }
                else //Si se ingreso algo para filtrar
                {
                    //Listamos los viajes habilitados con el Filtro ingresado
                    listaViaje = (from viaje in bd.Viaje
                                  join lugarOrigen in bd.Lugar
                                  on viaje.IIDLUGARORIGEN equals lugarOrigen.IIDLUGAR
                                  join lugarDestino in bd.Lugar
                                  on viaje.IIDLUGARDESTINO equals lugarDestino.IIDLUGAR
                                  join bus in bd.Bus
                                  on viaje.IIDBUS equals bus.IIDBUS
                                  where viaje.BHABILITADO == 1
                                  && viaje.IIDLUGARDESTINO == lugarDestinoParametro //Filtro por el valor ingresado
                                  select new ViajeCLS
                                  {
                                      iidViaje = viaje.IIDVIAJE,
                                      nombreBus = bus.PLACA,
                                      nombreLugarOrigen = lugarOrigen.NOMBRE,
                                      nombreLugarDestino = lugarDestino.NOMBRE

                                  }).ToList();
                }
            }
            //Retornamos una vista parcial a la que le enviamos la listaViaje
            return PartialView("_TablaViaje", listaViaje);
        }

        //Recuperar informacion de la entidad, para luego mostrarla al editar
        public JsonResult recuperarInformacion(int idViaje) //idViaje: lo recibo de la vista parcial
        {
            ViajeCLS oViajeCls = new ViajeCLS();

            using (var bd = new BDPasajeEntities())
            {
                Viaje oViaje = bd.Viaje.Where(p => p.IIDVIAJE == idViaje).First(); //Busco la entidad por su Id
                oViajeCls.iidViaje = oViaje.IIDVIAJE;
                oViajeCls.iidBus = (int)oViaje.IIDBUS;
                oViajeCls.iidLugarDestino = (int)oViaje.IIDLUGARDESTINO;
                oViajeCls.iidLugarOrigen = (int)oViaje.IIDLUGARORIGEN;
                oViajeCls.precio = (decimal)oViaje.PRECIO;
                //año-mes-dia  (El navegador pide)
                //dia-mes-anio (Viene de BD)
                //Por eso hago una conversion del formato de la fecha
                oViajeCls.fechaViajeCadena = oViaje.FECHAVIAJE != null ?
                    ((DateTime)oViaje.FECHAVIAJE).ToString("yyyy-MM-ddTHH:mm") : "";
                oViajeCls.numeroAsientosDisponibles = (int)oViaje.NUMEROASIENTOSDISPONIBLES;
                oViajeCls.nombreFoto = oViaje.nombrefoto;

                oViajeCls.extension = Path.GetExtension(oViaje.nombrefoto); //Obtenemos la extension de la imagen
                oViajeCls.fotoRecuperCadena = Convert.ToBase64String(oViaje.FOTO); //Convertimos la foto a Base64

            }
            //Serializo el objeto a JSON (Luego con JS, obtenemos el contenido en la Vista Index)
            return Json(oViajeCls, JsonRequestBehavior.AllowGet);
        }

        //Accion para realizar la eliminacion logica una entidad
        public int EliminarViaje(int idViaje)
        {
            //0 = Error
            int nregistrosAfectados = 0;
            try
            {
                using (var bd = new BDPasajeEntities())
                {
                    Viaje oViaje = bd.Viaje.Where(p => p.IIDVIAJE == idViaje).First(); //Busco la entidad por si Id
                    oViaje.BHABILITADO = 0; //HYago la eliminacion logica
                    nregistrosAfectados = bd.SaveChanges(); //Guardo el numero de filas afectadas, si fue bien tendra un (1)
                }

            }
            catch (Exception ex)
            {
                nregistrosAfectados = 0;
            }

            return nregistrosAfectados;
        }

        //============================================================

        //PARA LLENAR TODOS LOS COMBOBOX
        //Creo todas las listas necesarias y las paso a la vista

        public void listarLugar()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.Lugar
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.NOMBRE,
                             Value = item.IIDLUGAR.ToString()
                         }).ToList();
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaLugar = lista;
            }
        }

        public void listarBus()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.Bus
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.PLACA,
                             Value = item.IIDBUS.ToString()
                         }).ToList();
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaBus = lista;
            }
        }

        public void listarCombos()
        {
            listarLugar();
            listarBus();
        }
    }
}