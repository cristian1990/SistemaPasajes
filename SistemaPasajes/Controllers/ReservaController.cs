using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class ReservaController : Controller
    {
        // GET: Reserva
        public ActionResult Index()
        {
            listarLugar();

            //Para pasar a la vista
            var pasajesId = ControllerContext.HttpContext.Request.Cookies["pasajesId"];
            var pasajesCantidad = ControllerContext.HttpContext.Request.Cookies["pasajesCantidad"];

            if (pasajesId != null) //Si tiene contenido la Cookie
            {
                ViewBag.listaId = pasajesId.Value;
                ViewBag.listaCantidad = pasajesId.Value;
            }

            using (var bd = new BDPasajeEntities())
            {
                var reserva = (from viaje in bd.Viaje //Saco info de la tabla Viaje
                               join lugar in bd.Lugar //Saco info de la tabla Lugar
                               on viaje.IIDLUGARORIGEN equals lugar.IIDLUGAR //Lo relaciono por el ID Lugar
                               join bus in bd.Bus //Saco info de la tabla Bus
                               on viaje.IIDBUS equals bus.IIDBUS //Lo relaciono por el ID Bus
                               join lugarDes in bd.Lugar //Saco info de la tabla Lugar Destino
                               on viaje.IIDLUGARDESTINO equals lugarDes.IIDLUGAR //Lo relaciono por el ID Lugar
                               where viaje.BHABILITADO == 1
                               select new ReservaCLS
                               {
                                   iidViaje = viaje.IIDVIAJE,
                                   nombreArchivo = viaje.nombrefoto,
                                   foto = viaje.FOTO,
                                   lugarOrigen = lugar.NOMBRE,
                                   lugarDestino = lugarDes.NOMBRE,
                                   precio = (decimal)viaje.PRECIO,
                                   fechaViaje = (DateTime)viaje.FECHAVIAJE,
                                   nombreBus = bus.PLACA,
                                   descripcionBus = bus.DESCRIPCION,
                                   totalAsientos = (int)bus.NUMEROCOLUMNAS * (int)bus.NUMEROFILAS,
                                   asientosDisponibles = (int)viaje.NUMEROASIENTOSDISPONIBLES,
                                   iidBus = bus.IIDBUS
                               }).ToList();
                return View(reserva);
            }

        }

        //Accion que permite almacenar la informacion de la reserva, asi se mantiene aunque se recargue la pagina
        //LAS COOKIE solo almacenan String, NO Array
        public string Agregarcookie(string idViaje, string cantidad, string fechaViaje, string lugarOrigen, string lugarDestino, string precio, int iidBus)
        {
            string rpta = ""; //Vacio es error

            try
            {
                //Defino 2 Cookie
                var pasajesId = ControllerContext.HttpContext.Request.Cookies["pasajesId"]; //pasajesId, el nombre que puse a la coockie
                var pasajesCantidad = ControllerContext.HttpContext.Request.Cookies["pasajesCantidad"];

                //Verifico si son diferente de null (osea si ya tienen un valor), y si no estan vacios
                if (pasajesId != null && pasajesCantidad != null && pasajesCantidad.Value != "" && pasajesId.Value != "")
                {
                    //Se crea la Cookie Por segunda vez
                    //Al valor actual, debemos concatenar el Id ("{" elegimos ese operador de concatenacion)
                    //5{3 (Tendia dos Id almacenados)
                    string idCookie = pasajesId.Value + "{" + idViaje;
                    string cantidadCookie = pasajesCantidad.Value + "{" + cantidad + "*" + fechaViaje + "*" + lugarOrigen + "*" + lugarDestino + "*" + precio + "*" + iidBus;

                    HttpCookie cookieId = new HttpCookie("pasajesId", idCookie);
                    HttpCookie cookieCantidad = new HttpCookie("pasajesCantidad", cantidadCookie);
                    ControllerContext.HttpContext.Response.SetCookie(cookieId);
                    ControllerContext.HttpContext.Response.SetCookie(cookieCantidad);
                }
                else //Si recien se crean la Cookie por primera vez
                {
                    //pasajesCantidad (Para almacenar toda la data, menos el IdViaje)
                    string formatoCadena = cantidad + "*" + fechaViaje + "*" + lugarOrigen + "*" + lugarDestino + "*" + precio + "*" + iidBus;
                    //Guardamos las Cookies
                    HttpCookie cookieId = new HttpCookie("pasajesId", idViaje); //En esta Cookie almaceno todos los Id
                    HttpCookie cookieCantidad = new HttpCookie("pasajesCantidad", formatoCadena);  //En esta Cookie almaceno todas las cantidades
                    ControllerContext.HttpContext.Response.SetCookie(cookieId);
                    ControllerContext.HttpContext.Response.SetCookie(cookieCantidad);
                }
                rpta = "OK";
            }
            catch (Exception)
            {
                rpta = "";
            }
            return rpta;
        }

        public string QuitarCookie(string idViaje) 
        {
            string rpta = ""; //vacio es error
            try
            {
                //Obtengo las Cookies
                var pasajesId = ControllerContext.HttpContext.Request.Cookies["pasajesId"];
                var pasajesCantidad = ControllerContext.HttpContext.Request.Cookies["pasajesCantidad"];
                //Obtengo el valor de las Cookies
                string valorId = pasajesId.Value;
                string valorCantidad = pasajesCantidad.Value;
                //Creo un array y obtengo los Id de los viajes seleccionados
                string[] arrayId = valorId.Split('{'); //Convierto a un array, quitando los '{'
                //Busco el indice donde se encuentra el IdVieja en el array
                int indiceId = Array.IndexOf(arrayId, idViaje);

                //6{7{9{2    (Texto ya esta, de ejemplo de varias reservas)

                //2
                // Caso 1: .Replace("{2","") //Elimino el indice 2, reemplazando por "" (quedaria => 6{7{9)

                //6
                //Caso 2 : .Replace("6{","") //El 6 es el primero asi que cambia el formato

                //6
                //Caso 3 :  .Replace("6","") //Si solo hay un valor (osea solo una reserva)

                //Tesxo que contiene los ID
                string nuevoId;
                if (valorId.Contains("{" + idViaje)) //Veo si contiene por ej: {2
                {
                    nuevoId = valorId.Replace("{" + idViaje, ""); //Reemplazo por vacio
                }
                else if (valorId.Contains(idViaje + "{")) //Aca busco el primero 6{
                {
                    nuevoId = valorId.Replace(idViaje + "{", ""); //Reemplazo por vacio
                }
                else
                {
                    nuevoId = valorId.Replace(idViaje, ""); //Si solo hay un valor (una reserva). Reemplazo por vacio
                }

                //Texto que tiene las cantidades
                List<string> valor = valorCantidad.Split('{').ToList();
                valor.RemoveAt(indiceId);
                string[] arrayCantidad = valor.ToArray();
                string nuevaCantidad = String.Join("{", arrayCantidad); //Vuelvo a armar la cadena para las Cookie

                HttpCookie cookieId = new HttpCookie("pasajesId", nuevoId);
                HttpCookie cookieCantidad = new HttpCookie("pasajesCantidad", nuevaCantidad);

                ControllerContext.HttpContext.Response.SetCookie(cookieId);
                ControllerContext.HttpContext.Response.SetCookie(cookieCantidad);

                rpta = "OK";

            }
            catch (Exception ex)
            {

                rpta = "";
            }

            return rpta;
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
    }
}