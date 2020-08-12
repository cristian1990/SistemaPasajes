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

            return View();
        }

        //Utilizo transacciones, porque voy a tener que manipular en 2 tablas, para insertar y actualizar
        //la de Usuario y la de cliente o Empleado
        public int Guardar(UsuarioCLS oUsuarioCLS, int titulo) //titulo viene de la vista
        {
            int rpta = 0;

            try 
            {
                using (var bd = new BDPasajeEntities())
                {
                    //Abro un bloque de transaccion
                    using (var transaccion = new TransactionScope())
                    {
                        if (titulo == 1) //1: significa Agregar
                        {
                            Usuario oUsuario = new Usuario();
                            oUsuario.NOMBREUSUARIO = oUsuarioCLS.nombreusuario;
                            SHA256Managed sha = new SHA256Managed(); //Para cifrar la clave
                            byte[] byteContra = Encoding.Default.GetBytes(oUsuarioCLS.contra); //Convierto a array de Byte
                            byte[] byteContraCifrado = sha.ComputeHash(byteContra); //Cifro
                            string cadenaContraCifrada = BitConverter.ToString(byteContraCifrado).Replace("-", ""); //Convierto a cadena
                            oUsuario.CONTRA = cadenaContraCifrada;
                            //oUsuario.TIPOUSUARIO = oUsuarioCLS.nombrePersonaEnviar.Substring(oUsuarioCLS.nombrePersonaEnviar.Length - 2, 1);
                            //oUsuario.IID = oUsuarioCLS.iid;
                            //oUsuario.IIDROL = oUsuarioCLS.iidrol;
                            //oUsuario.bhabilitado = 1;
                            //bd.Usuario.Add(oUsuario);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                rpta = 0;
            }

            return rpta;
        }


        //FUNCION PARA CREAR LISTAS DE PERSONAS
        public void listaPersonas()
        {
            List<SelectListItem> listaPersonas = new List<SelectListItem>();

            using (var bd = new BDPasajeEntities())
            {
                //Obtengo la lista de clientes habilitados que NO tienen usuario
                List<SelectListItem> listaClientes = (from item in bd.Cliente
                                                      where item.BHABILITADO == 1 && item.bTieneUsuario != 1
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