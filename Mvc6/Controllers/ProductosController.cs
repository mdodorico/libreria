using Mvc6.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Mvc6.Controllers
{
    public class ProductosController : Controller
    {
        public string Confimacion { get; private set; }
        public Exception error { get; private set; }

        public ActionResult Index()
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                var UserEmail = Session["userEmail"] as string;

                var us = db.Usuarios.FirstOrDefault(e => e.email == UserEmail);

                if (us.rol == "ADM")
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        public ActionResult GetData()
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                List<Productos> prodList = db.Productos.ToList<Productos>();
                List<Productos> prodListFinal = new List<Productos>();

                foreach (Productos p in prodList)
                {
                    Productos pFinal = new Productos();
                    pFinal.id = p.id;
                    pFinal.imagen = p.imagen;
                    pFinal.nombre = p.nombre;
                    pFinal.precio = p.precio;
                    pFinal.descripcion = p.descripcion;
                    pFinal.stock = p.stock;

                    prodListFinal.Add(pFinal);
                }

                log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                logger.Error("Error de sistema");

                return Json(new { data = prodListFinal }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new Productos());
            else
            {
                using (NegocioEntities db = new NegocioEntities())
                {
                    return View(db.Productos.Where(x => x.id == id).FirstOrDefault<Productos>());
                }
            }
        }

        [HttpPost]
        public ActionResult AddOrEdit(Productos producto, HttpPostedFileBase file)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                if (producto.id == 0)
                {
                    producto.id = Convert.ToInt32(Convert.ToString(db.Productos.ToList().Last().id + 1));

                    if (file != null && file.ContentLength > 0)
                    {
                        string ruta = Server.MapPath("~/Imagenes/");
                        ruta += file.FileName;
                        Subir(ruta, file);
                        producto.imagen = Path.GetFileName(file.FileName);

                    }
                    else if (file == null && (producto.imagen == "no-image.png" || producto.imagen == null))
                    {
                        producto.imagen = "no-image.png";
                    }

                    db.Productos.Add(producto);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                } else {

                    if (file != null && file.ContentLength > 0)
                    {
                        string ruta = Server.MapPath("~/Imagenes/");
                        ruta += file.FileName;
                        Subir(ruta, file);
                        producto.imagen = Path.GetFileName(file.FileName);

                    }
                    else if (file == null && (producto.imagen == "no-image.png" || producto.imagen == null))
                    {
                        producto.imagen = "no-image.png";
                    }

                    db.Entry(producto).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                Productos prod = db.Productos.Where(x => x.id == id).FirstOrDefault<Productos>();
                db.Productos.Remove(prod);
                db.SaveChanges();
                return Json(new { success = true, message = "Registro eliminado exitosamente" }, JsonRequestBehavior.AllowGet);
            }
        }

        public void Subir(string ruta, HttpPostedFileBase file)
        {
            try
            {
                file.SaveAs(ruta);
                this.Confimacion = "Imagen guardada";
            }
            catch (Exception ex)
            {
                this.error = ex;
            }
        }

        protected override void Dispose(bool disposing)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}
