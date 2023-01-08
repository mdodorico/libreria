using Mvc6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace Mvc6.Controllers
{
    public class HomeController : Controller
    {
        NegocioEntities db = new NegocioEntities();

        public ActionResult Index()
        {
            return View(db.Productos.ToList());
        }

        public ActionResult Terminar()
        {
            return View("Finalizar");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Remove("userId");
            Session.Remove("userEmail");
            Session.RemoveAll();
            return RedirectToAction("Ingresar", "Login");
        }
    }
}
