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

                        if (ousuario.TIPOUSUARIO == "C")
                        {
                            Cliente oCliente = bd.Cliente.Where(p => p.IIDCLIENTE == ousuario.IID).First();
                            Session["nombreCompleto"] = oCliente.NOMBRE + " " + oCliente.APPATERNO + " " + oCliente.APMATERNO;

                        }
                        else
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
    }
}