using SistemaPasajes.ClasesAuxiliares;
using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CerrarSession()
        {
            Session["Usuario"] = null; //Elimino la sesion
            Session["Rol"] = null;
            return RedirectToAction("Index"); //Aca esta la interfaz de Login
        }

        public string Login(UsuarioCLS oUsuario)
        {
            string rpta = "";

            //Error
            if (!ModelState.IsValid)
            {
                var query = (from state in ModelState.Values //Capturamos el estado y mje de error
                             from error in state.Errors
                             select error.ErrorMessage).ToList();
                rpta += "<ul class='list-group'>";
                foreach (var item in query)
                {
                    rpta += "<li class='list-group-item'>" + item + "</li>";
                }
                rpta += "</ul>";
            }
            else //Si es valido, entramos a la base de datos
            {
                string nombreUsuario = oUsuario.nombreusuario;
                string password = oUsuario.contra;
                //Cifrar, para poder comparar con la clave en la BD
                SHA256Managed sha = new SHA256Managed();
                byte[] byteContra = Encoding.Default.GetBytes(password);
                byte[] byteContraCifrado = sha.ComputeHash(byteContra);
                string cadenaContraCifrada = BitConverter.ToString(byteContraCifrado).Replace("-", ""); //Esto es lo que compararia

                using (var bd = new BDPasajeEntities())
                {
                    //Verificamos en la base que exista el usuario y la contraseña, 1 seria que existe y 0 que no existe
                    int numeroVeces = bd.Usuario.Where(p => p.NOMBREUSUARIO == nombreUsuario
                     && p.CONTRA == cadenaContraCifrada).Count();
                    rpta = numeroVeces.ToString(); //Tendra 1 o 0 

                    if (rpta == "0") rpta = "Usuario o contraseña incorrecta";

                    else //Si el usuario existe
                    {
                        Usuario ousuario = bd.Usuario.Where(p => p.NOMBREUSUARIO == nombreUsuario
                        && p.CONTRA == cadenaContraCifrada).First();
                        //Almaceno todo el objeto Usuario, se podra usar desde cualquier lado
                        Session["Usuario"] = ousuario;

                        if (ousuario.TIPOUSUARIO == "C") //Si es un cliente
                        {
                            //Buscamos el cliente logueado
                            Cliente oCliente = bd.Cliente.Where(p => p.IIDCLIENTE == ousuario.IID).First();
                            //Creamos una sesion para permanecer los datos del cliente y mostrarlos al ingresar (se podria utilizar un ViewBag)
                            Session["nombreCompleto"] = oCliente.NOMBRE + " " + oCliente.APPATERNO + " " + oCliente.APMATERNO;
                        }
                        else //Si es un Empleado
                        {
                            Empleado oEmpleado = bd.Empleado.Where(p => p.IIDEMPLEADO == ousuario.IID).First();
                            Session["nombreCompleto"] = oEmpleado.NOMBRE + " " + oEmpleado.APPATERNO + " " + oEmpleado.APMATERNO;

                        }
                        //Armo una lista con la union de informacion de 3 tablas
                        List<MenuCLS> listaMenu = (from usuario in bd.Usuario
                                                   join rol in bd.Rol
                                                   on usuario.IIDROL equals rol.IIDROL
                                                   join rolpagina in bd.RolPagina
                                                   on rol.IIDROL equals rolpagina.IIDROL
                                                   join pagina in bd.Pagina
                                                   on rolpagina.IIDPAGINA equals pagina.IIDPAGINA
                                                   where rol.IIDROL == ousuario.IIDROL && rolpagina.IIDROL == usuario.IIDROL //Obtenemos bien la info del usuario de la sesion
                                                   && usuario.IIDUSUARIO == ousuario.IIDUSUARIO
                                                   select new MenuCLS //Instancio un nuevo objeto
                                                   {
                                                       //Añado la informacion que me importa al nuevo objeto
                                                       nombreAccion = pagina.ACCION,
                                                       nombreControlador = pagina.CONTROLADOR,
                                                       mensaje = pagina.MENSAJE
                                                   }).ToList();

                        //Creamos una sesion para almacenar la lista y poder utilizarla en cualquier parte
                        Session["Rol"] = listaMenu;
                    }
                }
            }
            return rpta;
        }

        public string RecuperarContra(string IIDTIPO, string correo, string telefonoCelular)
        {
            string rpta = ""; //Vacio es error

            using (var bd = new BDPasajeEntities())
            {
                int cantidad = 0;
                int iidcliente;

                if (IIDTIPO == "C") //Si es un cliente
                {
                    //Para verificar si existe un cliente con esa informacion
                    cantidad = bd.Cliente.Where(p => p.EMAIL == correo && p.TELEFONOCELULAR == telefonoCelular).Count();
                }
                if (cantidad == 0)
                {
                    rpta = "No existe una persona registrada con esa informacion";
                }
                else //Si encontramos el usuario en la BD
                {
                    //Obtenemos el Id del cliente
                    iidcliente = bd.Cliente.Where(p => p.EMAIL == correo && p.TELEFONOCELULAR == telefonoCelular).First().IIDCLIENTE;               
                    //Verificar si el cliente tiene Usuario
                    int nveces = bd.Usuario.Where(p => p.IID == iidcliente && p.TIPOUSUARIO == "C").Count();
                    
                    if (nveces == 0) 
                    {
                        rpta = "No tiene usuario";
                    }
                    else //Si el cliente tiene usuario
                    {
                        //Obtener el usuario por su Id
                        Usuario ousuario = bd.Usuario.Where(p => p.IID == iidcliente && p.TIPOUSUARIO == "C").First();
                       
                        //Modificar su clave
                        Random ra = new Random();
                        int n1 = ra.Next(0, 9); //Next: Devuelve un numero aleatorio entre 0 y 9, no negativo
                        int n2 = ra.Next(0, 9);
                        int n3 = ra.Next(0, 9);
                        int n4 = ra.Next(0, 9);
                        string nuevaContra = n1.ToString() + n2 + n3 + n4;

                        //Cifrar clave
                        SHA256Managed sha = new SHA256Managed();
                        byte[] byteContra = Encoding.Default.GetBytes(nuevaContra);
                        byte[] byteContraCifrado = sha.ComputeHash(byteContra);
                        string cadenaContraCifrada = BitConverter.ToString(byteContraCifrado).Replace("-", "");
                        
                        ousuario.CONTRA = cadenaContraCifrada; //Almaceno la nueva clave cifrada (la que se envia al usuario)
                        rpta = bd.SaveChanges().ToString(); //rpta, tendra un "" o "1" dependiendo si pudo guardar 
                        Correo.enviarCorreo(correo, "Recuperar Clave", "Se reseteo su clave , ahora su clave es :" + nuevaContra, "");
                    }
                }
            }
            return rpta;
        }
    }
}