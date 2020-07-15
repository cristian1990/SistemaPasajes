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
        // GET: Marca
        public ActionResult Index()
        {
            List<MarcaCLS> listaMarca = null;
            using (var bd = new BDPasajeEntities())
            {
                //Utilizando LINQ
                listaMarca = (from m in bd.Marca
                              where m.BHABILITADO == 1
                              select new MarcaCLS
                              {
                                  iidmarca = m.IIDMARCA,
                                  nombre = m.NOMBRE,
                                  descripcion = m.DESCRIPCION
                              }).ToList();
            }
            return View(listaMarca);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Agregar(MarcaCLS oMarcaCLS)
        {
            if (ModelState.IsValid)
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
                return RedirectToAction("Index");
            }
            return View(oMarcaCLS);
        }

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
            //int nregistradosEncontrados = 0;
            //string nombreMarca = oMarcaCLS.nombre;
            //int iidmarca = oMarcaCLS.iidmarca;
            //using (var bd = new BDPasajeEntities())
            //{

            //    nregistradosEncontrados = bd.Marca.Where(p => p.NOMBRE.Equals(nombreMarca) && !p.IIDMARCA.Equals(iidmarca)).Count();
            //}

            if (!ModelState.IsValid /*|| nregistradosEncontrados >= 1*/)
            {
                //if (nregistradosEncontrados >= 1) oMarcaCLS.mensajeError = "Ya se encuentra registrada la marca";
                return View(oMarcaCLS);
            }

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
    }
}