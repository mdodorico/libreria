using Mvc6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;

namespace Mvc6.Controllers
{
    public class CarritoController : Controller
    {
        int userId;
        double valorTotal;

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetData()
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                valorTotal = 0;
                List<Carrito> car = db.Carrito.ToList();
                List<Carrito> shop = new List<Carrito>();
                userId = Convert.ToInt32(Session["userId"].ToString());

                foreach (Carrito c in car)
                {
                    if (c.Usuario == userId)
                    {
                        Carrito carrito = new Carrito();

                        carrito.Id = c.Id;
                        carrito.Imagen = c.Imagen;
                        carrito.Nombre = c.Nombre;
                        carrito.Precio = (float)c.Precio;
                        carrito.Descripcion = c.Descripcion;
                        carrito.Cantidad = c.Cantidad;

                        shop.Add(carrito);
                    }
                }

                foreach (Carrito carro in shop)
                {
                    if (carro.Cantidad == 1)
                    {
                        valorTotal = valorTotal + carro.Precio;
                    }
                    else 
                    {
                        valorTotal = valorTotal + (carro.Precio * carro.Cantidad);
                    }
                }

                log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                logger.Error("Error de sistema");

                return Json(new { data = shop, total = valorTotal}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(int id)
        {
            using (NegocioEntities db = new NegocioEntities()) 
            {
                userId = Convert.ToInt32(Session["userId"].ToString());
                Productos producto = db.Productos.ToList().Where(x => x.id == id).FirstOrDefault();
                Carrito car = new Carrito();

                List<Carrito> carro = db.Carrito.ToList();
                List<Carrito> shop = new List<Carrito>();

                foreach (Carrito c in carro)
                {
                    if (c.Usuario == userId)
                    {
                        Carrito carrito = new Carrito();
                        carrito.Precio = (float)c.Precio;
                        valorTotal = valorTotal + carrito.Precio;
                    }
                }

                if (yaExiste(producto.id))
                {
                    if (hayStock(producto.id))
                    {
                        Carrito c = db.Carrito.Where(x => x.Id == id).FirstOrDefault();
                        c.Cantidad = c.Cantidad + 1;
                        Productos pr = db.Productos.Where(x => x.id == id).FirstOrDefault();
                        pr.stock = pr.stock - 1;

                        valorTotal = valorTotal + c.Precio;

                        db.SaveChanges();
                        return Json(new { total = valorTotal, redirectToUrl = Url.Action("Index", "Carrito") });
                    }
                    else 
                    {
                        MessageBox.Show("No hay stock disponible", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return Json(new { total = valorTotal, redirectToUrl = Url.Action("Index", "Home") });
                    }
                }
                else
                {
                    car.Id = producto.id;
                    car.Imagen = producto.imagen;
                    car.Nombre = producto.nombre;
                    car.Precio = producto.precio;
                    car.Descripcion = producto.descripcion;
                    car.Cantidad = 1;
                    car.Usuario = userId;

                    db.Carrito.Add(car);
                    db.SaveChanges();
                    Productos pr = db.Productos.Where(x => x.id == car.Id).FirstOrDefault();
                    pr.stock = pr.stock - 1;

                    valorTotal = valorTotal + car.Precio;

                    db.SaveChanges();
                    return Json(new { total = valorTotal, redirectToUrl = Url.Action("Index", "Carrito") });
                }
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                Carrito car = db.Carrito.Where(x => x.Id == id).FirstOrDefault<Carrito>();
                Productos pr = db.Productos.Where(x => x.id == id).FirstOrDefault();

                if (car.Cantidad > 1)
                {
                    car.Cantidad = car.Cantidad - 1;
                    pr.stock = pr.stock + 1;
                }
                else if (car.Cantidad == 1)
                {
                    db.Carrito.Remove(car);
                    pr.stock = pr.stock + 1;
                }
                db.SaveChanges();
                valorTotal = valorTotal - car.Precio;
                return Json(new { total = valorTotal, success = true }, JsonRequestBehavior.AllowGet);
            }
        }

        public Boolean yaExiste(int id)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                var query = (from car in db.Carrito
                             select car
                          ).AsEnumerable().Where(car => car.Id == id);

                if (query.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }  
        }

        public Boolean hayStock(int id)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                Productos pr = db.Productos.ToList().Where(x => x.id == id).FirstOrDefault();

                if (pr.stock > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
