using Mvc6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mvc6.Controllers
{
    public class ProveedoresController : Controller
    {
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
                List<Proveedores> provList = db.Proveedores.ToList<Proveedores>();
                List<Proveedores> provListFinal = new List<Proveedores>();

                foreach (Proveedores p in provList)
                {
                    Proveedores pFinal = new Proveedores();
                    pFinal.id = p.id;
                    pFinal.nombre = p.nombre;
                    pFinal.email = p.email;
                    pFinal.telefono = p.telefono;
                    pFinal.direccion = p.direccion;

                    provListFinal.Add(pFinal);
                }

                log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                logger.Error("Error de sistema");

                return Json(new { data = provListFinal }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new Proveedores());
            else
            {
                using (NegocioEntities db = new NegocioEntities())
                {
                    return View(db.Proveedores.Where(x => x.id == id).FirstOrDefault<Proveedores>());
                }
            }
        }

        [HttpPost]
        public ActionResult AddOrEdit(Proveedores prov)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                if (prov.id == 0)
                {
                    prov.id = Convert.ToInt32(Convert.ToString(db.Proveedores.ToList().Last().id + 1));
                    db.Proveedores.Add(prov);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Nuevo registro guardado exitosamente" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    db.Entry(prov).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Registro editado exitosamente" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                Proveedores prov = db.Proveedores.Where(x => x.id == id).FirstOrDefault<Proveedores>();
                db.Proveedores.Remove(prov);
                db.SaveChanges();
                return Json(new { success = true, message = "Registro eliminado exitosamente" }, JsonRequestBehavior.AllowGet);
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
