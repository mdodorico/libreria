using Mvc6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mvc6.Controllers
{
    public class MenuUsuarioController : Controller
    {
        public Exception error { get; private set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MenuUsuario()
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                var UserEmail = Session["userEmail"] as string;

                var us = db.Usuarios.FirstOrDefault(e => e.email == UserEmail);

                if (us != null)
                {
                    try
                    {
                        if (us.imagen == null)
                        {
                            ViewBag.Imagen = "avatar.jpg";
                        }
                        else
                        {
                            ViewBag.Imagen = us.imagen;
                        }
                        ViewBag.Usuario = us.nombre + " " + us.apellido;
                    }
                    catch (Exception e)
                    {
                        this.error = e;
                    }
                }
                return PartialView();
            }
        }
    }
}
