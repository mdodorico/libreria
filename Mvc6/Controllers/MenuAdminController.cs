using Mvc6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mvc6.Controllers
{
    public class MenuAdminController : Controller
    {
        public Exception error { get; private set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MenuAdmin()
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                var UserEmail = Session["userEmail"] as string;

                var us = db.Usuarios.FirstOrDefault(e => e.email == UserEmail);

                if (us.rol == "ADM")
                {
                    return PartialView();
                }
                else 
                {
                    return null;
                }
            }
        }
    }
}
