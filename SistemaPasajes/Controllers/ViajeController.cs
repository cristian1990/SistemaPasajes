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

        public ActionResult Agregar()
        {
            listarCombos();
            return View();
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