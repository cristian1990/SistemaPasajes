using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaPasajes.Models; //Colocar

namespace SistemaPasajes.Controllers
{
    public class MarcaController : Controller
    {
        #region Accion para Listar Datos
        // GET: Marca
        public ActionResult Index(MarcaCLS omarcaCLS)
        {
            string nombreMarca = omarcaCLS.nombre; //Obtengo y almaceno lo ingresado
            List<MarcaCLS> listaMarca = null;

            using (var bd = new BDPasajeEntities())
            {
                //Verifico si se ingreso un filtro de busqueda 
                if (omarcaCLS.nombre == null) //si es null, es que no se ingreso nada
                {
                    //Si no se ingreso filtro, muestro todo
                    //Utilizando LINQ
                    listaMarca = (from marca in bd.Marca
                                  where marca.BHABILITADO == 1
                                  select new MarcaCLS
                                  {
                                      iidmarca = marca.IIDMARCA,
                                      nombre = marca.NOMBRE,
                                      descripcion = marca.DESCRIPCION
                                  }).ToList();
                    //Session["lista"] = listaMarca;
                }
                else //Si se ingreso una marca para filtrar
                {
                    listaMarca = (from marca in bd.Marca
                                  where marca.BHABILITADO == 1
                                  && marca.NOMBRE.Contains(nombreMarca) //Busco la marca que contenga lo ingresado
                                  select new MarcaCLS
                                  {
                                      iidmarca = marca.IIDMARCA,
                                      nombre = marca.NOMBRE,
                                      descripcion = marca.DESCRIPCION
                                  }).ToList();
                    //Session["lista"] = listaMarca;
                }
            }
            return View(listaMarca);
        }

        #endregion

        #region Acciones para Agregar
        public ActionResult Agregar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Agregar(MarcaCLS oMarcaCLS)
        {
            //PARA VALIDAR LA MARCA
            int nregistrosEncontrados = 0;
            string nombreMarca = oMarcaCLS.nombre; 
            using (var bd = new BDPasajeEntities())
            {
                //Vefifico cuantas veces se repite la marca en la base de datos
                nregistrosEncontrados = bd.Marca.Where(p => p.NOMBRE.Equals(nombreMarca)).Count();
            }
            //FIN

            //Si el ModelState no es valido o ya existe la marca 
            if (!ModelState.IsValid || nregistrosEncontrados >= 1)
            {
                if (nregistrosEncontrados >= 1) 
                    oMarcaCLS.mensajeError = "El nombre marca ya existe"; //Se debe agregar un if en la vista.

                return View(oMarcaCLS); //Retorno la vista, para que quede lo que ingreso hasta el momento 
            }
            else //Si la marca no existe, cargo los datos en la base
            {
                using (var bd = new BDPasajeEntities())
                {
                    Marca oMarca = new Marca();
                    oMarca.NOMBRE = oMarcaCLS.nombre;
                    oMarca.DESCRIPCION = oMarcaCLS.descripcion;
                    oMarca.BHABILITADO = 1;
                    bd.Marca.Add(oMarca);
                    bd.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Acciones para Editar

        //El id, lo pasamos desde la vista
        //Para recuperar los datos y mostrarlos en pantalla
        public ActionResult Editar(int id)
        {
            MarcaCLS oMarcaCLS = new MarcaCLS();
            using (var bd = new BDPasajeEntities())
            {
                //Busca y almacena la marca con el ID pasado como parametro
                Marca oMarca = bd.Marca.Where(p => p.IIDMARCA.Equals(id)).First();
                //Llenamos el modelo
                oMarcaCLS.iidmarca = oMarca.IIDMARCA;
                oMarcaCLS.nombre = oMarca.NOMBRE;
                oMarcaCLS.descripcion = oMarca.DESCRIPCION;
            }

            return View(oMarcaCLS);
        }

        //Para realizar la edicion en la base de datos
        [HttpPost]
        public ActionResult Editar(MarcaCLS oMarcaCLS)
        {
            //PARA VALIDAR LA MARCA
            int nregistradosEncontrados = 0;
            string nombreMarca = oMarcaCLS.nombre;
            int iidmarca = oMarcaCLS.iidmarca;

            using (var bd = new BDPasajeEntities())
            {
                //Busco la cantidad de registro encontrados en la base de datos, donde el nombre sea igual y el ID sea distinto
                nregistradosEncontrados = bd.Marca.Where(p => p.NOMBRE.Equals(nombreMarca) && !p.IIDMARCA.Equals(iidmarca)).Count();
            }
            //FIN

            //Si el ModelState no es valido o ya existe la marca 
            if (!ModelState.IsValid || nregistradosEncontrados >= 1)
            {
                if (nregistradosEncontrados >= 1) 
                    oMarcaCLS.mensajeError = "Ya se encuentra registrada la marca"; //Se debe agregar un if en la vista.

                return View(oMarcaCLS); //Retorno la vista, para que quede lo que ingreso hasta el momento 
            }

            //Si la marca no existe, cargo los datos en la base
            int idMarca = oMarcaCLS.iidmarca;
            using (var bd = new BDPasajeEntities())
            {
                Marca oMarca = bd.Marca.Where(p => p.IIDMARCA.Equals(idMarca)).First();
                oMarca.NOMBRE = oMarcaCLS.nombre;
                oMarca.DESCRIPCION = oMarcaCLS.descripcion;
                //bd.Entry(oMarca).State = EntityState.Modified; //Va?
                bd.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Acciones para Eliminar
        //El ID, lo recibo de la vista Index, hacemos una Eliminacion Logica
        public ActionResult Eliminar(int id)
        {
            using (var bd = new BDPasajeEntities())
            {
                //Busco la Marca en la base, mediante el Id pasado
                Marca oMarca = bd.Marca.Where(p => p.IIDMARCA.Equals(id)).First();
                oMarca.BHABILITADO = 0;
                bd.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        #endregion
    }
}