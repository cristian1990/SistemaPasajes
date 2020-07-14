using SistemaPasajes.Models; //Colocar
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class ClienteController : Controller
    {
        // GET: Cliente
        public ActionResult Index()
        {
            List<ClienteCLS> listaClientes = null;
            using (var bd = new BDPasajeEntities())
            {
                //Utilizando LINQ
                listaClientes = (from c in bd.Cliente
                                 where c.BHABILITADO == 1
                                 select new ClienteCLS
                                 {
                                     iidcliente = c.IIDCLIENTE,
                                     nombre = c.NOMBRE,                                   
                                     apPaterno = c.APPATERNO,
                                     apMaterno = c.APMATERNO,
                                     telefonoFijo = c.TELEFONOFIJO
                                 }).ToList();
            }
            return View(listaClientes);
        }

        public ActionResult Agregar()
        {
            llenarSexo();
            ViewBag.lista = listaSexo;

            return View();
        }

        [HttpPost]
        public ActionResult Agregar(ClienteCLS oClienteCLS)
        {
            if (ModelState.IsValid)
            {
                using (var bd = new BDPasajeEntities())
                {
                    Cliente oCliente = new Cliente();
                    oCliente.NOMBRE = oClienteCLS.nombre;
                    oCliente.APPATERNO = oClienteCLS.apPaterno;
                    oCliente.APMATERNO = oClienteCLS.apMaterno;
                    oCliente.EMAIL = oClienteCLS.email;
                    oCliente.DIRECCION = oClienteCLS.direccion;
                    oCliente.IIDSEXO = oClienteCLS.iidsexo;
                    oCliente.TELEFONOCELULAR = oClienteCLS.telefonoCelular;
                    oCliente.TELEFONOFIJO = oClienteCLS.telefonoFijo;
                    oCliente.BHABILITADO = 1;
                    bd.Cliente.Add(oCliente);
                    bd.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            //Evitamos una Excepcion, ya que si el modelState no es valido, es necesario volver a cargar el ComboBox
            llenarSexo();
            ViewBag.lista = listaSexo;

            return View(oClienteCLS);
        }

        //Funcion para llenar ComboBox
        List<SelectListItem> listaSexo;
        private void llenarSexo()
        {
            using (var bd = new BDPasajeEntities())
            {
                listaSexo = (from s in bd.Sexo
                             where s.BHABILITADO == 1
                             select new SelectListItem
                             {
                                 Text = s.NOMBRE,
                                 Value = s.IIDSEXO.ToString()
                             }).ToList();
                //Indicamos que al principio se agregue el texto selecciones, del tipo SelectListItem
                listaSexo.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
            }
        }

        public ActionResult Editar(int id)
        {
            ClienteCLS oClienteCLS = new ClienteCLS();

            using (var bd = new BDPasajeEntities())
            {
                //Para asignar el valor al ComboBox de la vista
                llenarSexo();
                ViewBag.lista = listaSexo;

                //Busco y almaceno el cliente
                Cliente oCLiente = bd.Cliente.Where(p => p.IIDCLIENTE.Equals(id)).First();
                //Indico todos los campos a recuperar
                oClienteCLS.iidcliente = oCLiente.IIDCLIENTE;
                oClienteCLS.nombre = oCLiente.NOMBRE;
                oClienteCLS.apPaterno = oCLiente.APPATERNO;
                oClienteCLS.apMaterno = oCLiente.APMATERNO;
                oClienteCLS.direccion = oCLiente.DIRECCION;
                oClienteCLS.email = oCLiente.EMAIL;
                oClienteCLS.iidsexo = (int)oCLiente.IIDSEXO;
                oClienteCLS.telefonoCelular = oCLiente.TELEFONOCELULAR;
                oClienteCLS.telefonoFijo = oCLiente.TELEFONOFIJO;
            }
            return View(oClienteCLS);
        }
    }
}