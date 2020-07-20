using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaPasajes.Models;

namespace SistemaPasajes.Controllers
{
    //ESTE CONTROLADOR SE AGREGO PARA REALIZAR VARIOS FILTROS
    public class TipoUsuarioController : Controller
    {
        //Creo una variable global, para almacenar el modelo recibido de la vista Index
        //en oTipoVal, se almacena la informacion ingresada mediante los textbox
        private TipoUsuarioCLS oTipoVal; 
        private bool BuscarTipoUsuario(TipoUsuarioCLS oTipoUsuarioCls)
        {
            bool busquedaId = true;
            bool busquedaNombre = true;
            bool busquedaDescripcion = true;

            //Por cada elemento de la lista, evaluo lo siguiente
            if (oTipoVal.iidtipousuario > 0) //Si no se ingreso nungun tipo a filtrar
                //Si se ingreso algo y cumple con la busqueda, devuelve True, si no False
                busquedaId = oTipoUsuarioCls.iidtipousuario.ToString().Contains(oTipoVal.iidtipousuario.ToString());

            if (oTipoVal.nombre != null) //Si no se ingreso nungun nombre a filtrar
                //Si se ingreso algo y cumple con la busqueda, devuelve True, si no False
                busquedaNombre = oTipoUsuarioCls.nombre.ToString().Contains(oTipoVal.nombre);

            if (oTipoVal.descripcion != null) //Si no se ingreso nunguna descripcion a filtrar
                //Si se ingreso algo y cumple con la busqueda, devuelve True, si no False
                busquedaDescripcion = oTipoUsuarioCls.descripcion.ToString().Contains(oTipoVal.descripcion);

            return (busquedaId && busquedaNombre && busquedaDescripcion);
        }

        // GET: TipoUsuario
        public ActionResult Index(TipoUsuarioCLS oTipoUsuario)
        {
            oTipoVal = oTipoUsuario; //Asigno el modelo a la variable
            List<TipoUsuarioCLS> listaTipoUsuario = null;
            //Pongo una variablepara almacenar la lista filtrada de usuarios
            List<TipoUsuarioCLS> listaFiltrado;

            using (var bd = new BDPasajeEntities())
            {
                //Utilizando LINQ
                listaTipoUsuario = (from tipoUsuario in bd.TipoUsuario
                                    where tipoUsuario.BHABILITADO == 1
                                    select new TipoUsuarioCLS
                                    {
                                        //Informacion que quiero mostrar
                                        iidtipousuario = tipoUsuario.IIDTIPOUSUARIO,
                                        nombre = tipoUsuario.NOMBRE,
                                        descripcion = tipoUsuario.DESCRIPCION
                                    }).ToList();

                //Si no se ingreso nada en los filtros
                //Cuando no se ingresa nada y el campo es Int, el valor por defecto sera 0, como la prop iidtipousuario
                if (oTipoUsuario.iidtipousuario == 0 && oTipoUsuario.nombre == null
                    && oTipoUsuario.descripcion == null)
                {
                    //La listaFiltrado va a ser igual a la listaTipoUsuario
                    //osea se muestran todos los usuarios sin ningun filtro
                    listaFiltrado = listaTipoUsuario;
                }
                else
                {
                    //Predicate: Representa un método que contiene un conjunto de criterios y verifica si el parámetro pasado cumple con esos criterios o no.
                    //unpredicate tambien es un delegado
                    Predicate<TipoUsuarioCLS> pred = new Predicate<TipoUsuarioCLS>(BuscarTipoUsuario); //Funcion asociada buscarTipoUsuario
                    listaFiltrado = listaTipoUsuario.FindAll(pred); //Realizo una busqueda, por cada elemento con FindAll
                }
            }

            return View(listaFiltrado);
        }
    }
}