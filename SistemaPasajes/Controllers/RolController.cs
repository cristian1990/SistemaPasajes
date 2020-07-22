using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Controllers
{
    public class RolController : Controller
    {
        // GET: Rol
        public ActionResult Index()
        {
            List<RolCLS> listaRol = new List<RolCLS>();
            using (var bd = new BDPasajeEntities())
            {
                //Utilizando LINQ
                listaRol = (from rol in bd.Rol
                            where rol.BHABILITADO == 1
                            select new RolCLS
                            {
                                //Datos a mostrar
                                iidRol = rol.IIDROL,
                                nombre = rol.NOMBRE,
                                descripcion = rol.DESCRIPCION
                            }).ToList();
            }

            return View(listaRol);
        }

        public ActionResult Filtro(string nombreRol) //Debe llamarse igual que el texbo de la vista
        {
            List<RolCLS> listaRol = new List<RolCLS>();
            using (var bd = new BDPasajeEntities())
            {
                if (nombreRol == null) //Si no se ingreso nada para filtrar
                { 
                    //Listo todos los roles
                    listaRol = (from rol in bd.Rol
                                where rol.BHABILITADO == 1
                                select new RolCLS
                                {
                                    iidRol = rol.IIDROL,
                                    nombre = rol.NOMBRE,
                                    descripcion = rol.DESCRIPCION
                                }).ToList();
                }
                else //Si se ingreso un rol en el filtro
                {
                    listaRol = (from rol in bd.Rol
                                where rol.BHABILITADO == 1
                                && rol.NOMBRE.Contains(nombreRol) //Busco el Rol, que contenga lo ingresado
                                select new RolCLS
                                {
                                    iidRol = rol.IIDROL,
                                    nombre = rol.NOMBRE,
                                    descripcion = rol.DESCRIPCION
                                }).ToList();
                }
                    
            }
            //Retorno la Vista Parcial, y envio la lista de Roles
            return PartialView("_TablaRol", listaRol);
        }

        public int Guardar(RolCLS oRolCLS, int titulo)
        {
            int rpta = 0;
            using (var bd = new BDPasajeEntities())
            {
                //Si titulo = 1, es que intento agregar
                if (titulo.Equals(1)) //Este 1, lo recibo del Html.Hidden("titulo") del Index
                {
                    Rol oRol = new Rol();
                    oRol.NOMBRE = oRolCLS.nombre;
                    oRol.DESCRIPCION = oRolCLS.descripcion;
                    oRol.BHABILITADO = 1;
                    bd.Rol.Add(oRol);
                    rpta = bd.SaveChanges(); //Almaceno el numero de filas afectadas
                }
            }
           return rpta;
        }
    }
}