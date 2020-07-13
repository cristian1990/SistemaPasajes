using System;
using System.Collections.Generic;
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
    }
}