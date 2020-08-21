using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions; //Lo agregue desde las referencias
using System.Security.Cryptography;
using System.Text; //Para Byte

namespace SistemaPasajes.Controllers
{
    public class UsuarioController : Controller
    {
        // GET: Usuario
        public ActionResult Index()
        {
            listaPersonas();
            listarRol();
            List<UsuarioCLS> listaUsuario = new List<UsuarioCLS>();

            using (var bd = new BDPasajeEntities())
            {
                //Obtenemos la lista de usuarios del tipo Cliente
                List<UsuarioCLS> listaUsuarioCliente = (from usuario in bd.Usuario
                                                        join cliente in bd.Cliente
                                                        on usuario.IID equals
                                                        cliente.IIDCLIENTE
                                                        join rol in bd.Rol
                                                        on usuario.IIDROL equals rol.IIDROL
                                                        where usuario.bhabilitado == 1
                                                        && usuario.TIPOUSUARIO == "C"
                                                        select new UsuarioCLS
                                                        {
                                                            iidusuario = usuario.IIDUSUARIO,
                                                            nombrePersona = cliente.NOMBRE + " " + cliente.APPATERNO + " " + cliente.APMATERNO,
                                                            nombreusuario = usuario.NOMBREUSUARIO,
                                                            nombreRol = rol.NOMBRE,
                                                            nombreTipoEmpleado = "Cliente"
                                                        }).ToList();

                //Obtenemos la lista de usuarios del tipo Empleado
                List<UsuarioCLS> listaUsuarioEmpleado = (from usuario in bd.Usuario
                                                         join empleado in bd.Empleado
                                                         on usuario.IID equals
                                                         empleado.IIDEMPLEADO
                                                         join rol in bd.Rol
                                                         on usuario.IIDROL equals rol.IIDROL
                                                         where usuario.bhabilitado == 1
                                                         && usuario.TIPOUSUARIO == "E"
                                                         select new UsuarioCLS
                                                         {
                                                             iidusuario = usuario.IIDUSUARIO,
                                                             nombrePersona = empleado.NOMBRE + " " + empleado.APPATERNO + " " + empleado.APMATERNO,
                                                             nombreusuario = usuario.NOMBREUSUARIO,
                                                             nombreRol = rol.NOMBRE,
                                                             nombreTipoEmpleado = "Empleado"
                                                         }).ToList();
                //Combino ambas listas en una sola
                listaUsuario.AddRange(listaUsuarioCliente);
                listaUsuario.AddRange(listaUsuarioEmpleado);
                listaUsuario = listaUsuario.OrderBy(p => p.iidusuario).ToList(); //Odeno la lista por ID
            }

            return View(listaUsuario);
        }

        //Utilizo transacciones, porque voy a tener que manipular en 2 tablas, para insertar y actualizar
        //la de Usuario y la de cliente o Empleado
        public string Guardar(UsuarioCLS oUsuarioCLS, int titulo) //titulo viene de la vista (tendra -1 o el Id del usuario)
        {
            //Vacio es error
            string rpta = string.Empty; //Representa los registros afectados

            try
            {
                if (!ModelState.IsValid) //Si el modelo NO es valido
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
                else //Si es valido
                {
                    int cantidad = 0;

                    using (var bd = new BDPasajeEntities())
                    {
                        //Al agregar un usuario, realizo un Insert y un Update a la tabla Cliente o Empleado
                        //Por ese motivo debemos utilizar Transacciones, porque hago mas de 1 operacion
                        //Abro un bloque de transaccion
                        using (var transaccion = new TransactionScope()) //Debo agregar una referencia
                        {
                            if (titulo.Equals(-1)) //1: significa Agregar
                            {
                                //Para saber si el usuario se repite en la base
                                cantidad = bd.Usuario.Where(p => p.NOMBREUSUARIO == oUsuarioCLS.nombreusuario).Count();
                                if (cantidad >= 1)
                                {
                                    rpta = "-1"; //Ya esta en la base
                                }
                                else
                                {
                                    Usuario oUsuario = new Usuario();
                                    oUsuario.NOMBREUSUARIO = oUsuarioCLS.nombreusuario;
                                    SHA256Managed sha = new SHA256Managed(); //Para cifrar la clave
                                    byte[] byteContra = Encoding.Default.GetBytes(oUsuarioCLS.contra); //Convierto a array de Byte
                                    byte[] byteContraCifrado = sha.ComputeHash(byteContra); //Cifro
                                    string cadenaContraCifrada = BitConverter.ToString(byteContraCifrado).Replace("-", ""); //Convierto a cadena, para guardar en la BD (Saco los -)
                                    oUsuario.CONTRA = cadenaContraCifrada; //Almaceno la contraseña cifrada
                                    oUsuario.TIPOUSUARIO = oUsuarioCLS.nombrePersonaEnviar.Substring(oUsuarioCLS.nombrePersonaEnviar.Length - 2, 1); //Para obtener la penultima posicion ("C" o "E")
                                    oUsuario.IID = oUsuarioCLS.iid;
                                    oUsuario.IIDROL = oUsuarioCLS.iidrol;
                                    oUsuario.bhabilitado = 1; //Habilitamos el usuario
                                    bd.Usuario.Add(oUsuario);

                                    //Hacemos la 2da. operacion
                                    //Verificamos que tipo de usuario es (Cliente o Empleado)
                                    if (oUsuario.TIPOUSUARIO.Equals("C")) //Si es cliente
                                    {
                                        Cliente oCliente = bd.Cliente.Where(p => p.IIDCLIENTE.Equals(oUsuarioCLS.iid)).First(); //Buscamos el cliente
                                        oCliente.bTieneUsuario = 1; //Indicamos que ya tiene Usuario en la BD
                                    }
                                    else //Si es empleado
                                    {
                                        Empleado oEmpleado = bd.Empleado.Where(p => p.IIDEMPLEADO.Equals(oUsuarioCLS.iid)).First();
                                        oEmpleado.bTieneUsuario = 1;
                                    }

                                    rpta = bd.SaveChanges().ToString(); //Dentro de la transaccion, no funciona el SaveChanges
                                    if (rpta == "0") rpta = ""; //Si es 0, no se afecto filas entonces hubo un error (Al editar se admite 0)
                                    transaccion.Complete(); //Aca recien se guarda en la BD                     
                                }
                            }
                            else //Si titulo es mayor a 1, se quiere editar
                            {
                                //Para saber si el usuario se repite en la base, que tenga distinto Id al del parametro
                                cantidad = bd.Usuario.Where(p => p.NOMBREUSUARIO == oUsuarioCLS.nombreusuario
                                && p.IIDUSUARIO != titulo).Count();

                                if (cantidad >= 1)
                                {
                                    rpta = "-1";
                                }
                                else
                                {
                                    //Editar
                                    Usuario ousuario = bd.Usuario.Where(p => p.IIDUSUARIO == titulo).First();  //Busco por el Id que viene en Titulo
                                    ousuario.IIDROL = oUsuarioCLS.iidrol;
                                    ousuario.NOMBREUSUARIO = oUsuarioCLS.nombreusuario;
                                    rpta = bd.SaveChanges().ToString(); //Devolveria 0 o 1, dependiendo de las filas que afecto
                                    transaccion.Complete();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                rpta = ""; //Sobreescribimos a vacio
            }
            //Se envia la rpta, luego la capturamos con JS
            return rpta;
        }

        //Action para realizar el filtrado de Persona por nombre
        public ActionResult Filtrar(UsuarioCLS oUsuarioCLS)
        {
            string nombrePersona = oUsuarioCLS.nombrePersona;
            listaPersonas();
            listarRol();
            List<UsuarioCLS> listaUsuario = new List<UsuarioCLS>();
            using (var bd = new BDPasajeEntities())
            {
                //Si es null, osea no se ingreso nombre para filtrar, entonces muestro todas las listas de usuarios que sean clientes o empleados
                if (oUsuarioCLS.nombrePersona == null)
                {
                    List<UsuarioCLS> listaUsuarioCliente = (from usuario in bd.Usuario
                                                            join cliente in bd.Cliente
                                                            on usuario.IID equals
                                                            cliente.IIDCLIENTE
                                                            join rol in bd.Rol
                                                            on usuario.IIDROL equals rol.IIDROL
                                                            where usuario.bhabilitado == 1
                                                            && usuario.TIPOUSUARIO == "C"
                                                            select new UsuarioCLS
                                                            {
                                                                iidusuario = usuario.IIDUSUARIO,
                                                                nombrePersona = cliente.NOMBRE + " " + cliente.APPATERNO + " " + cliente.APMATERNO,
                                                                nombreusuario = usuario.NOMBREUSUARIO,
                                                                nombreRol = rol.NOMBRE,
                                                                nombreTipoEmpleado = "Cliente"
                                                            }).ToList();


                    List<UsuarioCLS> listaUsuarioEmpleado = (from usuario in bd.Usuario
                                                             join empleado in bd.Empleado
                                                             on usuario.IID equals
                                                             empleado.IIDEMPLEADO
                                                             join rol in bd.Rol
                                                             on usuario.IIDROL equals rol.IIDROL
                                                             where usuario.bhabilitado == 1
                                                             && usuario.TIPOUSUARIO == "E"
                                                             select new UsuarioCLS
                                                             {
                                                                 iidusuario = usuario.IIDUSUARIO,
                                                                 nombrePersona = empleado.NOMBRE + " " + empleado.APPATERNO + " " + empleado.APMATERNO,
                                                                 nombreusuario = usuario.NOMBREUSUARIO,
                                                                 nombreRol = rol.NOMBRE,
                                                                 nombreTipoEmpleado = "Empleado"
                                                             }).ToList();
                    listaUsuario.AddRange(listaUsuarioCliente);
                    listaUsuario.AddRange(listaUsuarioEmpleado);
                    listaUsuario = listaUsuario.OrderBy(p => p.iidusuario).ToList();
                }
                else //Si se ingreso un nombre, lo busco en ambas tablas y armo una lista
                {
                    List<UsuarioCLS> listaUsuarioCliente = (from usuario in bd.Usuario
                                                            join cliente in bd.Cliente
                                                            on usuario.IID equals
                                                            cliente.IIDCLIENTE
                                                            join rol in bd.Rol
                                                            on usuario.IIDROL equals rol.IIDROL
                                                            where usuario.bhabilitado == 1
                                                            && (
                                                            cliente.NOMBRE.Contains(nombrePersona) //Verifico si lo que ingrese lo contiene algun Cliente
                                                             || cliente.APPATERNO.Contains(nombrePersona)
                                                              || cliente.APMATERNO.Contains(nombrePersona)
                                                            )
                                                            && usuario.TIPOUSUARIO == "C"
                                                            select new UsuarioCLS
                                                            {
                                                                iidusuario = usuario.IIDUSUARIO,
                                                                nombrePersona = cliente.NOMBRE + " " + cliente.APPATERNO + " " + cliente.APMATERNO,
                                                                nombreusuario = usuario.NOMBREUSUARIO,
                                                                nombreRol = rol.NOMBRE,
                                                                nombreTipoEmpleado = "Cliente"
                                                            }).ToList();


                    List<UsuarioCLS> listaUsuarioEmpleado = (from usuario in bd.Usuario
                                                             join empleado in bd.Empleado
                                                             on usuario.IID equals
                                                             empleado.IIDEMPLEADO
                                                             join rol in bd.Rol
                                                             on usuario.IIDROL equals rol.IIDROL
                                                             where usuario.bhabilitado == 1
                                                             && usuario.TIPOUSUARIO == "E" &&
                                                              (
                                                            empleado.NOMBRE.Contains(nombrePersona) //Verifico si lo que ingrese lo contiene algun Empleado
                                                             || empleado.APPATERNO.Contains(nombrePersona)
                                                              || empleado.APMATERNO.Contains(nombrePersona)
                                                            )
                                                             select new UsuarioCLS
                                                             {
                                                                 iidusuario = usuario.IIDUSUARIO,
                                                                 nombrePersona = empleado.NOMBRE + " " + empleado.APPATERNO + " " + empleado.APMATERNO,
                                                                 nombreusuario = usuario.NOMBREUSUARIO,
                                                                 nombreRol = rol.NOMBRE,
                                                                 nombreTipoEmpleado = "Empleado"
                                                             }).ToList();
                    listaUsuario.AddRange(listaUsuarioCliente);
                    listaUsuario.AddRange(listaUsuarioEmpleado);
                    listaUsuario = listaUsuario.OrderBy(p => p.iidusuario).ToList();
                }

            }
            //Envio a la vista parcial la lista de usuarios
            return PartialView("_TablaUsuario", listaUsuario);
        }

        //Funcion para recuperar info por su ID y mostrarla en el Popups
        public JsonResult recuperarInformacion(int iidusuario)
        {
            UsuarioCLS oUsuarioCLS = new UsuarioCLS();

            using (var bd = new BDPasajeEntities())
            {
                Usuario oUsuario = bd.Usuario.Where(p => p.IIDUSUARIO == iidusuario).First(); //Busco la entidad por Id
                oUsuarioCLS.nombreusuario = oUsuario.NOMBREUSUARIO; //Asignamos los valores
                oUsuarioCLS.iidrol = (int)oUsuario.IIDROL;
            }

            //Serializo a JSON, para luego tomarlo de la vista
            return Json(oUsuarioCLS, JsonRequestBehavior.AllowGet);
        }

        //Accion para eliminar Usuarios de manera logica
        public int Eliminar(int idUsuario) //El id viene de la vista parcial
        {
            int rpta = 0; //0 es error

            try
            {
                using (BDPasajeEntities bd = new BDPasajeEntities())
                {
                    Usuario oUsuario = bd.Usuario.Where(p => p.IIDUSUARIO == idUsuario).First(); //Busco el usuario por su Id
                    oUsuario.bhabilitado = 0; //Eliminacion logica
                    rpta = bd.SaveChanges(); //rpta tendra 1 si se pudo guardar
                }
            }
            catch (Exception ex)
            {
                rpta = 0; //Sobrescribo a 0
            }

            //Retorno 0 o 1, dependiedo si se pudo guardar o no (Lo tomo en la vista Index con JS)
            return rpta;
        }


        //FUNCION PARA CREAR LISTAS DE PERSONAS (juntando Clientes y Empleados)
        public void listaPersonas()
        {
            List<SelectListItem> listaPersonas = new List<SelectListItem>();

            using (var bd = new BDPasajeEntities())
            {
                //Obtengo la lista de clientes habilitados que NO tienen usuario
                List<SelectListItem> listaClientes = (from item in bd.Cliente
                                                      where item.BHABILITADO == 1 && item.bTieneUsuario != 1 //1: Indicaria que tiene usuario
                                                      select new SelectListItem
                                                      {
                                                          Text = item.NOMBRE + " " + item.APPATERNO + " " + item.APMATERNO + " (C)",
                                                          Value = item.IIDCLIENTE.ToString()
                                                      }).ToList();

                //Obtengo la lista de empleados habilitados que NO tienen usuario
                List<SelectListItem> listaEmpleados = (from item in bd.Empleado
                                                       where item.BHABILITADO == 1 && item.bTieneUsuario != 1
                                                       select new SelectListItem
                                                       {
                                                           Text = item.NOMBRE + " " + item.APPATERNO + " " + item.APMATERNO + " (E)",
                                                           Value = item.IIDEMPLEADO.ToString()
                                                       }).ToList();
                //Combinamos ambas listas con AddRange
                listaPersonas.AddRange(listaClientes);
                listaPersonas.AddRange(listaEmpleados);
                listaPersonas = listaPersonas.OrderBy(p => p.Text).ToList(); //Ordenamos por texto
                listaPersonas.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });
                ViewBag.listaPersona = listaPersonas;
            }
        }

        //FUNCION PARA CREAR LISTAS DE ROLES
        public void listarRol()
        {
            List<SelectListItem> listaRol;

            using (var bd = new BDPasajeEntities())
            {
                listaRol = (from item in bd.Rol
                            where item.BHABILITADO == 1
                            select new SelectListItem
                            {
                                Text = item.NOMBRE,
                                Value = item.IIDROL.ToString()
                            }).ToList();
            }

            listaRol.Insert(0, new SelectListItem { Text = "--Seleccione--", Value = "" });

            ViewBag.listaRol = listaRol;
        }
    }
}