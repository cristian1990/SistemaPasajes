using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace SistemaPasajes.ClasesAuxiliares
{
    public class Correo
    {
        public static int enviarCorreo(string nombreCorreo, string asunto, string contenido, string rutaError)
        {
            int rpta = 0; //0: No se pudo enviar el correo
            try
            {
                //Agarro y almaceno los valores de la configuracion en el Web.config
                string correo = ConfigurationManager.AppSettings["correo"];
                string clave = ConfigurationManager.AppSettings["clave"];
                string servidor = ConfigurationManager.AppSettings["servidor"];
                int puerto = int.Parse(ConfigurationManager.AppSettings["puerto"]);
                //Data del correo (definimos)
                MailMessage mail = new MailMessage();
                mail.Subject = asunto;
                mail.IsBodyHtml = true; //Permite ingresar contenido HTML
                mail.Body = contenido;
                mail.From = new MailAddress(correo); //El que envia el correo (yo)
                mail.To.Add(new MailAddress(nombreCorreo)); //A quien se envia (usuario)
                //Envio de correo
                SmtpClient smtp = new SmtpClient();
                smtp.Host = servidor;
                smtp.EnableSsl = true; //Habilitamos para poder enviar
                smtp.Port = puerto;
                smtp.UseDefaultCredentials = false; //No use las credenciales por defecto
                smtp.Credentials = new NetworkCredential(correo, clave);
                smtp.Send(mail);
                rpta = 1; //1: Se pudo enviar el correo correctamente

            }
            catch (Exception ex)
            {
                rpta = 0;

                //Para Log de error
                File.AppendAllText(rutaError, nombreCorreo);
            }

            return rpta;

        }
    }
}