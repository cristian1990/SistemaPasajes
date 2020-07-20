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
        public ActionResult Index(BusCLS oBusCls)
        {
            listarCombos(); //Listo los ComboBox
            List<BusCLS> listaRpta = new List<BusCLS>();
            List<BusCLS> listaBus = null;

            using (var bd = new BDPasajeEntities())
            {
                //Hago el listado de todos los Bus Habilitados
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
                                iidModelo = tipoModelo.IIDMODELO, //Importante para filtrar
                                iidSucursal = sucursal.IIDSUCURSAL, //Importante para filtrar
                                iidTipoBus = tipoBus.IIDTIPOBUS //Importante para filtrar

                            }).ToList();

                //Si no se ingreso ningun filtro de busqueda
                if (oBusCls.iidBus == 0 && oBusCls.placa == null
                    && oBusCls.iidModelo == 0 && oBusCls.iidSucursal == 0
                    && oBusCls.iidTipoBus == 0)
                {
                    //Se muestran todos los Bus habilidatos
                    listaRpta = listaBus;
                }
                else //Filtros sin Predicate (otra manera)
                {
                    //Filtro por Bus
                    if (oBusCls.iidBus != 0)
                    {
                        //Se asigna la lista filtrada a listaBus
                        listaBus = listaBus.Where(p => p.iidBus.ToString().Contains(oBusCls.iidBus.ToString())).ToList();
                    }
                    //Filtro por Placa
                    if (oBusCls.placa != null)
                    {
                        //Si se ingresa otro parametro para filtrar se hace sobre "listaBus" que ya filtramos anterior (Asi se logra multiples filtros)
                        listaBus = listaBus.Where(p => p.placa.Contains(oBusCls.placa)).ToList();
                    }
                    //Filtro por Modelo
                    if (oBusCls.iidModelo != 0)
                    {
                        listaBus = listaBus.Where(p => p.iidModelo.ToString().Contains(oBusCls.iidModelo.ToString())).ToList();

                    }
                    //Filtro por Sucursak
                    if (oBusCls.iidSucursal != 0)
                    {
                        listaBus = listaBus.Where(p => p.iidSucursal.ToString().Contains(oBusCls.iidSucursal.ToString())).ToList();
                    }
                    //Filtro por Tipo Bus
                    if (oBusCls.iidTipoBus != 0)
                    {
                        listaBus = listaBus.Where(p => p.iidTipoBus.ToString().Contains(oBusCls.iidTipoBus.ToString())).ToList();
                    }

                    //Asigno el contenido de listaBus a listaRpta
                    listaRpta = listaBus;
                }
            }

            return View(listaRpta);
        }


        public ActionResult Agregar()
        {
            //Listo a la info de loc ComboBox
            listarCombos();
            return View();
        }

        [HttpPost]
        public ActionResult Agregar(BusCLS oBusCls)
        {
            //VALIDAR PLACA
            int nregistrosEncontrados = 0;
            string placa = oBusCls.placa;

            using (var bd = new BDPasajeEntities())
            {
                //Verifico la cantidad de Bus con la misma placa
                nregistrosEncontrados = bd.Bus.Where(p => p.PLACA.Equals(placa)).Count();
            }
            //FIN

            if (!ModelState.IsValid || nregistrosEncontrados >= 1)
            {
                if (nregistrosEncontrados >= 1) 
                    oBusCls.mensajeError = "Ya existe el bus";
                
                listarCombos();
                return View(oBusCls);
            }

            //Si no el ModelState es valido y la placa no existe, agrego en la BD
            using (var bd = new BDPasajeEntities())
            {
                Bus oBus = new Bus();
                oBus.IIDSUCURSAL = oBusCls.iidSucursal;
                oBus.IIDTIPOBUS = oBusCls.iidTipoBus;
                oBus.PLACA = oBusCls.placa;
                oBus.FECHACOMPRA = oBusCls.fechaCompra;
                oBus.IIDMODELO = oBusCls.iidModelo;
                oBus.NUMEROFILAS = oBusCls.numeroFilas;
                oBus.NUMEROCOLUMNAS = oBusCls.numeroColumnas;
                oBus.DESCRIPCION = oBusCls.descripcion;
                oBus.OBSERVACION = oBusCls.observacion;
                oBus.IIDMARCA = oBusCls.iidmarca;
                oBus.BHABILITADO = 1;
                bd.Bus.Add(oBus);

                bd.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        //El id, lo pasamos desde la vista
        //Para recuperar los datos y mostrarlos en pantalla
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

        //Para realizar la edicion en la base de datos
        [HttpPost]
        public ActionResult Editar(BusCLS oBusCLS)
        {
            int nregistrosEncontrados = 0;
            int idBus = oBusCLS.iidBus;
            string placa = oBusCLS.placa;

            using (var bd = new BDPasajeEntities())
            {
                //Verifico la cantidad de placas, con un ID distinto
                nregistrosEncontrados = bd.Bus.Where(p => p.PLACA.Equals(placa) && !p.IIDBUS.Equals(idBus)).Count();
            }

            if (!ModelState.IsValid || nregistrosEncontrados >= 1)
            {
                if (nregistrosEncontrados >= 1)
                    oBusCLS.mensajeError = "El bus ya existe";

                listarCombos();
                return View(oBusCLS);
            }

            //Si no el ModelState es valido y la placa no existe, agrego en la BD
            using (var bd = new BDPasajeEntities())
            {
                //Busco el Bus por su ID en la base de datos
                Bus oBus = bd.Bus.Where(p => p.IIDBUS.Equals(idBus)).First();
                oBus.IIDSUCURSAL = oBusCLS.iidSucursal;
                oBus.IIDTIPOBUS = oBusCLS.iidTipoBus;
                oBus.PLACA = oBusCLS.placa;
                oBus.FECHACOMPRA = oBusCLS.fechaCompra;
                oBus.IIDMODELO = oBusCLS.iidModelo;
                oBus.NUMEROCOLUMNAS = oBusCLS.numeroColumnas;
                oBus.NUMEROFILAS = oBusCLS.numeroFilas;
                oBus.DESCRIPCION = oBusCLS.descripcion;
                oBus.OBSERVACION = oBusCLS.observacion;
                oBus.IIDMARCA = oBusCLS.iidmarca;

                bd.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult Eliminar(int idBus)
        {

            using (var bd = new BDPasajeEntities())
            {
                //Busco el Bus en la BD, por su ID
                Bus oBus = bd.Bus.Where(p => p.IIDBUS.Equals(idBus)).First();
                oBus.BHABILITADO = 0;
                bd.SaveChanges();

            }
            return RedirectToAction("Index");
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