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

            List<MenuCLS> roles = (List<MenuCLS>)HttpContext.Current.Session["Rol"];

            string nombreControlador = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string accion = filterContext.ActionDescriptor.ActionName;
            int cantidad = roles.Where(p => p.nombreControlador == nombreControlador).Count();

            //Si session es nulo (No se logueo), entonces redireccionamos al Login
            if (usuario == null || cantidad == 0)
            {
                filterContext.Result = new RedirectResult("~/Login/Index");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}