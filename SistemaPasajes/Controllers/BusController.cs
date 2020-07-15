using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class BusController : Controller
    {
        // GET: Bus
        public ActionResult Index()
        {
            List<BusCLS> listaBus = null;
            using (var bd = new BDPasajeEntities())
            {
                listaBus = (from bus in bd.Bus
                            join sucursal in bd.Sucursal
                            on bus.IIDSUCURSAL equals sucursal.IIDSUCURSAL
                            join tipoBus in bd.TipoBus
                            on bus.IIDTIPOBUS equals tipoBus.IIDTIPOBUS
                            join tipoModelo in bd.Modelo
                            on bus.IIDMODELO equals tipoModelo.IIDMODELO
                            where bus.BHABILITADO == 1
                            select new BusCLS
                            {
                                iidBus = bus.IIDBUS,
                                placa = bus.PLACA,
                                nombreModelo = tipoModelo.NOMBRE,
                                nombreSucursal = sucursal.NOMBRE,
                                nombreTipoBus = tipoBus.NOMBRE,
                                iidModelo = tipoModelo.IIDMODELO,
                                iidSucursal = sucursal.IIDSUCURSAL,
                                iidTipoBus = tipoBus.IIDTIPOBUS

                            }).ToList();
            }
                return View(listaBus);
        }

        public ActionResult Agregar()
        {
            //Listo a la info de loc ComboBox
            listarCombos();
            return View();
        }

        [HttpPost]
        public ActionResult Agregar(BusCLS oBusClS)
        {
            if (ModelState.IsValid)
            {
                using (var bd = new BDPasajeEntities())
                {
                    Bus oBus = new Bus();
                    oBus.IIDSUCURSAL = oBusClS.iidSucursal;
                    oBus.IIDTIPOBUS = oBusClS.iidTipoBus;
                    oBus.PLACA = oBusClS.placa;
                    oBus.FECHACOMPRA = oBusClS.fechaCompra;
                    oBus.IIDMODELO = oBusClS.iidModelo;
                    oBus.NUMEROFILAS = oBusClS.numeroFilas;
                    oBus.NUMEROCOLUMNAS = oBusClS.numeroColumnas;
                    oBus.DESCRIPCION = oBusClS.descripcion;
                    oBus.OBSERVACION = oBusClS.observacion;
                    oBus.IIDMARCA = oBusClS.iidmarca;
                    oBus.BHABILITADO = 1;
                    bd.Bus.Add(oBus);
                    bd.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            listarCombos();
            return View(oBusClS);
        }

        public ActionResult Editar(int id)
        {
            //Listo a la info de los ComboBox, necesario para poder editar
            listarCombos();
            BusCLS oBusCls = new BusCLS();
            using (var bd = new BDPasajeEntities())
            {
                Bus obus = bd.Bus.Where(p => p.IIDBUS.Equals(id)).First();
                oBusCls.iidBus = obus.IIDBUS;
                oBusCls.iidSucursal = (int)obus.IIDSUCURSAL;
                oBusCls.iidTipoBus = (int)obus.IIDTIPOBUS;
                oBusCls.placa = obus.PLACA;
                oBusCls.fechaCompra = (DateTime)obus.FECHACOMPRA;
                oBusCls.iidModelo = (int)obus.IIDMODELO;
                oBusCls.numeroColumnas = (int)obus.NUMEROCOLUMNAS;
                oBusCls.numeroFilas = (int)obus.NUMEROFILAS;
                oBusCls.descripcion = obus.DESCRIPCION;
                oBusCls.observacion = obus.OBSERVACION;
                oBusCls.iidmarca = (int)obus.IIDMARCA;
            }
            return View(oBusCls);
        }

        //============================================================

        //PARA LLENAR TODOS LOS COMBOBOX
        //Creo todas las listas necesarias y las paso a la vista
        public void listarTipoBus()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.TipoBus
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.NOMBRE,
                             Value = item.IIDTIPOBUS.ToString()
                         }).ToList();
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaTipoBus = lista;
            }
        }

        public void listarMarca()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.Marca
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.NOMBRE,
                             Value = item.IIDMARCA.ToString()
                         }).ToList();
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaMarca = lista;
            }
        }

        public void listarModelo()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.Modelo
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.NOMBRE,
                             Value = item.IIDMODELO.ToString()
                         }).ToList();
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaModelo = lista;
            }
        }

        public void listarSucursal()
        {
            //agregar
            List<SelectListItem> lista;
            using (var bd = new BDPasajeEntities())
            {
                lista = (from item in bd.Sucursal
                         where item.BHABILITADO == 1
                         select new SelectListItem
                         {
                             Text = item.NOMBRE,
                             Value = item.IIDSUCURSAL.ToString()
                         }).ToList();
                lista.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaSucursal = lista;
            }
        }

        public void listarCombos()
        {
            listarSucursal();
            listarModelo();
            listarMarca();
            listarTipoBus();
        }
    }
}