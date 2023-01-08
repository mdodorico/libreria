using Mvc6.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;

namespace Mvc6.Controllers
{
    public class UsuariosController : Controller
    { 
        static string key { get; set; } = "A!9HHhi%XjjYY4YP2@Nob009X";
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
                List<Usuarios> userList = db.Usuarios.ToList<Usuarios>();
                List<Usuarios> userListFinal = new List<Usuarios>();

                foreach (Usuarios u in userList)
                {
                    Usuarios uFinal = new Usuarios();
                    uFinal.id = u.id;
                    uFinal.imagen = u.imagen;
                    uFinal.nombre = u.nombre;
                    uFinal.apellido = u.apellido;
                    uFinal.email = u.email;
                    uFinal.clave = u.clave;
                    uFinal.rol = u.rol;

                    userListFinal.Add(uFinal);
                }

                log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                logger.Error("Error de sistema");

                return Json(new { data = userListFinal }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new Usuarios());
            else
            {
                using (NegocioEntities db = new NegocioEntities())
                {
                    Usuarios user = db.Usuarios.Where(x => x.id == id).FirstOrDefault<Usuarios>();
                    user.clave = Decrypt(user.clave.ToString());
                    return View(user);
                }
            }
        }

        [HttpPost]
        public ActionResult AddOrEdit(Usuarios user, HttpPostedFileBase file)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                if (user.id == 0)
                {
                    var query = (from us in db.Usuarios
                                 select us
                           ).AsEnumerable().Where(us => us.email == user.email);

                    if (query.Count() > 0)
                    {
                        MessageBox.Show("El email ya existe", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return RedirectToAction("Index");
                    }

                    user.id = Convert.ToInt32(Convert.ToString(db.Usuarios.ToList().Last().id + 1));
                    user.clave = Encrypt(user.clave);

                    if (file != null && file.ContentLength > 0)
                    {
                        string ruta = Server.MapPath("~/Imagenes/");
                        ruta += file.FileName;
                        Subir(ruta, file);
                        user.imagen = Path.GetFileName(file.FileName);

                    }
                    else if (file == null && (user.imagen == "avatar.png" || user.imagen == null))
                    {
                        user.imagen = "avatar.png";
                    }

                    if (user.nombre==null || user.apellido==null || user.email==null || user.clave==null)
                    {
                        MessageBox.Show("Nombre, apellido, email y clave son datos obligatorios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return RedirectToAction("Index");
                    }

                    db.Usuarios.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    user.clave = Encrypt(user.clave);
                    if (file != null && file.ContentLength > 0)
                    {
                        string ruta = Server.MapPath("~/Imagenes/");
                        ruta += file.FileName;
                        Subir(ruta, file);
                        user.imagen = Path.GetFileName(file.FileName);

                    }
                    else if (file == null && (user.imagen == "avatar.png" || user.imagen == null))
                    {
                        user.imagen = "avatar.png";
                    }

                    db.Entry(user).State = System.Data.EntityState.Modified;
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
                Usuarios user = db.Usuarios.Where(x => x.id == id).FirstOrDefault<Usuarios>();
                db.Usuarios.Remove(user);
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

        public static string Encrypt(string text)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateEncryptor())
                    {
                        byte[] textBytes = UTF8Encoding.UTF8.GetBytes(text);
                        byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                        return Convert.ToBase64String(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        public static string Decrypt(string cipher)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateDecryptor())
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipher);
                        byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return UTF8Encoding.UTF8.GetString(bytes);
                    }
                }
            }
        }
    }
}
