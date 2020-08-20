using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
                    Session["lista"] = listaMarca; //Guardamos la listaMarca en una sesion, para el PDF
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
                    Session["lista"] = listaMarca; //Guardamos la listaMarca en una sesion, para el PDF
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


        //Accion para generar un archivo PDF (Antes añadir la referencia a itextsharp.dll)
        //Si quiero devolver un archivo a la vista, mi action debe ser FileResult 
        public FileResult GenerarPDF()
        {
            Document doc = new Document(); //Creo el documento
            byte[] buffer;

            using (MemoryStream ms = new MemoryStream()) //Para guardar el PDF en memoria (MemoryStream)
            {
                PdfWriter.GetInstance(doc, ms); //Para escribir en el documento
                doc.Open(); //Abrimos el documento

                Paragraph title = new Paragraph("Lista Marca"); //Titulo
                title.Alignment = Element.ALIGN_CENTER; //Alineamos al centro
                doc.Add(title); //Agregamos al documento el titulo

                Paragraph espacio = new Paragraph(" ");
                doc.Add(espacio); //Agregamos al documento el espacio

                //Columnas (Tabla)
                PdfPTable table = new PdfPTable(3); //(3) numero de columnas de la tabla
                //Anchos a las columnas
                float[] values = new float[3] { 30, 40, 80 };
                //Asignado esos anchos a la tabla
                table.SetWidths(values);


                //Creando celdas(Poniendo contenido)-color-alineado
                //el contenido al centro
                PdfPCell celda1 = new PdfPCell(new Phrase("Id Marca"));
                celda1.BackgroundColor = new BaseColor(130, 130, 130);
                celda1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                table.AddCell(celda1);

                PdfPCell celda2 = new PdfPCell(new Phrase("Nombre"));
                celda2.BackgroundColor = new BaseColor(130, 130, 130);
                celda2.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                table.AddCell(celda2);

                PdfPCell celda3 = new PdfPCell(new Phrase("Descripcion"));
                celda3.BackgroundColor = new BaseColor(130, 130, 130);
                celda3.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                table.AddCell(celda3);

                //Ingresamos la data a la tabla
                var lista = (List<MarcaCLS>)Session["lista"]; //Obtenemos la lista de la sesion
                int nregistros = lista.Count;
                for (int i = 0; i < nregistros; i++)
                {
                    table.AddCell(lista[i].iidmarca.ToString());
                    table.AddCell(lista[i].nombre);
                    table.AddCell(lista[i].descripcion);
                }
                
                doc.Add(table); //Agregamos la tabla al documento
                doc.Close(); //Cerramos el documento

                buffer = ms.ToArray(); //Guardamos el PDF en la variable buffer (Obtenemos los Byte, para poder retornarlo)
            }
            //Retornamos el archivo con el Content Type
            return File(buffer, "application/pdf");
        }
    }
}