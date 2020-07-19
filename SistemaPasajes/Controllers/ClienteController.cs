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

            return View();
        }

        [HttpPost]
        public ActionResult Agregar(ClienteCLS oClienteCLS)
        {
            //VALIDAMOS NOMBRE Y APELLIDOS
            int nregistrosEncontrados = 0;
            string nombre = oClienteCLS.nombre;
            string apPaterno = oClienteCLS.apPaterno;
            string apMaterno = oClienteCLS.apMaterno;

            using (var bd = new BDPasajeEntities())
            {
                //Buscamos la cantidad de Clientes con mismo nombre y apellidos
                nregistrosEncontrados = bd.Cliente.Where(p => p.NOMBRE.Equals(nombre) && p.APPATERNO.Equals(apPaterno)
                  && p.APMATERNO.Equals(apMaterno)).Count();

            }
            //FIN

            //Si el ModelState no es valido o ya existe un cliente con los mismo datos
            if (!ModelState.IsValid || nregistrosEncontrados >= 1)
            {
                if (nregistrosEncontrados >= 1) 
                    oClienteCLS.mensajeError = "Ya existe cliente registrado";
                
                llenarSexo();  //Evitamos una Excepcion, ya que si el modelState no es valido, es necesario volver a cargar el ComboBox
                return View(oClienteCLS);
            }

            //Si el ModelState es valido y el cliente no existe, lo agrego a la BD
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


        //Para recuperar los datos y mostrarlos en pantalla
        public ActionResult Editar(int id)
        {
            ClienteCLS oClienteCLS = new ClienteCLS();

            using (var bd = new BDPasajeEntities())
            {
                //Para asignar el valor al ComboBox de la vista
                llenarSexo();

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

        //Para realizar la edicion en la base de datos
        [HttpPost]
        public ActionResult Editar(ClienteCLS oClienteCLS)
        {
            //VALIDAMOS NOMBRE Y APELLIDOS
            int nregistradosEncontrados = 0;
            int idcliente = oClienteCLS.iidcliente;
            string nombre = oClienteCLS.nombre;
            string apPaterno = oClienteCLS.apPaterno;
            string apMaterno = oClienteCLS.apMaterno;

            using (var bd = new BDPasajeEntities())
            {
                //Buscamos la cantidad de Clientes con mismo nombre y apellidos, mientras que el Id sea distinto al actual
                nregistradosEncontrados = bd.Cliente.Where(p => p.NOMBRE.Equals(nombre) && p.APPATERNO.Equals(apPaterno)
                  && p.APMATERNO.Equals(apMaterno) && !p.IIDCLIENTE.Equals(idcliente)).Count();
            }
            //FIN

            //Si el ModelState no es valido o ya existe un cliente con los mismos datos
            if (!ModelState.IsValid || nregistradosEncontrados >= 1)
            {
                if (nregistradosEncontrados >= 1) 
                    oClienteCLS.mensajeError = "Ya existe el cliente";
                
                llenarSexo(); //lleno la lista, para evitar errores

                return View(oClienteCLS);
            }

            //Si el ModelState es valido y el cliente no existe, lo agrego a la BD
            using (var bd = new BDPasajeEntities())
            {
                //Busco el cliente por su ID, en la Base de Datos
                Cliente oCliente = bd.Cliente.Where(p => p.IIDCLIENTE.Equals(idcliente)).First();
                oCliente.NOMBRE = oClienteCLS.nombre;
                oCliente.APPATERNO = oClienteCLS.apPaterno;
                oCliente.APMATERNO = oClienteCLS.apMaterno;
                oCliente.EMAIL = oClienteCLS.email;
                oCliente.DIRECCION = oClienteCLS.direccion;
                oCliente.IIDSEXO = oClienteCLS.iidsexo;
                oCliente.TELEFONOCELULAR = oClienteCLS.telefonoCelular;
                oCliente.TELEFONOFIJO = oClienteCLS.telefonoFijo;
                bd.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        //Eliminacion logica del Cliente
        public ActionResult Eliminar(int idCliente)
        {
            using (var bd = new BDPasajeEntities())
            {
                //Busco el cliente por su ID en la base de datos
                Cliente oCliente = bd.Cliente.Where(p => p.IIDCLIENTE.Equals(idCliente)).First();
                oCliente.BHABILITADO = 0;
                bd.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        //============================================================

        //PARA LLENAR TODOS LOS COMBOBOX
        //Creo todas las listas necesarias y las paso a la vista

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
                ViewBag.lista = listaSexo;
            }
        }
    }
}