using SistemaPasajes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaPasajes.Filtros
{
    //Luego debemos agregar el filtro Acceder, a donde queremos que permita acceder
    public class Acceder : ActionFilterAttribute //Heredamos
    {
        //Sobrescribimos un metodo OnActionExecuting (Antes de ir a cualquier URL pasa por este metodo)
        //Esto impide que podamos ingresar a una pagina mediante la ruta, si no esta logueado el usuario
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Busco el objeto almacenado en la sesion
            var usuario = HttpContext.Current.Session["Usuario"];

            string nombreControlador = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string nombreAccion = filterContext.ActionDescriptor.ActionName;

            //Para no permitir que por la URL se pueda ingresar sin los roles
            List<MenuCLS> roles = (List<MenuCLS>)HttpContext.Current.Session["Rol"];
            int cantidad = roles.Where(p => p.nombreControlador == nombreControlador).Count();

            //Si session es nulo (No se logueo), entonces redireccionamos al Login
            if (usuario == null || cantidad == 0) //Si no tiene en la lista el controlador asignado o la sesion es null
            {
                filterContext.Result = new RedirectResult("~/Login/Index");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}