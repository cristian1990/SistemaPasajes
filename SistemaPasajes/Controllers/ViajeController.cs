using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
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
            //listarCombos();
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
    }
}