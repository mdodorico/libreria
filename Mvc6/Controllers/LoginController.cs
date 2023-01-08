using Mvc6.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using System.Net.Mail;
using System.Net;
using System.Data.Entity.Validation;

namespace Mvc6.Controllers
{
    public class LoginController : Controller
    {
        bool invalid;
        static string key { get; set; } = "A!9HHhi%XjjYY4YP2@Nob009X";
        public string Confimacion { get; private set; }
        public Exception error { get; private set; }


        public ActionResult Ingresar(Boolean success = false, string message = "")
        {
            ViewBag.Message = message;
            ViewBag.success = success;
            return View();
        }

        public ActionResult recuperarPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public ActionResult Ingresar(UsuarioLogin user)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                if (user == null) 
                {
                    return RedirectToAction("Ingresar", "Login");
                }

                if (ModelState.IsValid)
                {
                    var query = (from us in db.Usuarios
                                 select us
                           ).AsEnumerable().Where(us => us.email == user.email && Decrypt(us.clave) == user.clave);

                    if (query.Count() > 0)
                    {
                        Usuarios us = db.Usuarios.ToList().Where(x => x.email == user.email).FirstOrDefault();
                        Session["userId"] = us.id;
                        Session["userEmail"] = us.email;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Ingresar", new { error = true, message = "Datos incorrectos"});
                    }
                }
                else
                {
                    return RedirectToAction("Ingresar", new { error = true, message = "Datos incorrectos" });
                }
            } 
        }

        public ActionResult Registrarse(string error, string message = "")
        {
            ViewBag.Error = error;
            ViewBag.Message = message;
            return View();
        }

        [HttpPost]
        public ActionResult Registrarse(Usuarios usuario, HttpPostedFileBase file)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                var query = (from us in db.Usuarios
                             select us
                           ).AsEnumerable().Where(us => us.email == usuario.email);

                if (query.Count() > 0)
                {
                    ViewBag.EmailRepetido = "ERROR: el email ya está registrado.";
                    return View();
                }

                usuario.id = Convert.ToInt32(Convert.ToString(db.Usuarios.ToList().Last().id + 1));
                usuario.clave = Encrypt(usuario.clave);
                usuario.rol = "USER";

                if (ModelState.IsValid)
                {
                    if (!IsValidEmail(usuario.email))
                    {
                        ViewBag.EmailNoValido = "ERROR: el email ingresado no es válido.";
                    }

                    if (file != null && file.ContentLength > 0)
                    {
                        string ruta = Server.MapPath("~/Imagenes/");
                        ruta += file.FileName;
                        Upload(ruta, file);
                        usuario.imagen = Path.GetFileName(file.FileName);
                    }
                    else if (file == null && (usuario.imagen == "avatar.png" || usuario.imagen == null))
                    {
                        usuario.imagen = "avatar.png";
                    }
                    
                    db.Usuarios.Add(usuario);
                    db.SaveChanges();
                    enviarEmail(usuario, "Bienvenido/a a Mundo Creativo");
                    return RedirectToAction("Ingresar");
                }
                return View(usuario);
            }
        }

        public void recuperarCuenta(String email)
        {
            using (NegocioEntities db = new NegocioEntities())
            {
                Usuarios user = db.Usuarios.Where(x => x.email == email).FirstOrDefault<Usuarios>();
                db.Entry(user).State = System.Data.EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                        {
                            Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                        }
                    }
                }
                enviarEmail(user, "Cambiar contraseña || Mundo Creativo");
            }
        }

        public void enviarEmail(Usuarios user, String asunto)
        {
            var fromAddress = new MailAddress("mundocreativolibreria@gmail.com", "Mundo Creativo");
            var toAddress = new MailAddress(user.email, user.nombre);
            const string fromPassword = "holamundo1234"; 
            string subject = asunto;
            string body = "¡Hola " + user.nombre + "!" + System.Environment.NewLine + "Estos son los datos de tu cuenta:" + System.Environment.NewLine + "► Usuario: " + user.email + System.Environment.NewLine + "► Contraseña: " + Decrypt(user.clave.ToString());

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
            Response.Redirect("Ingresar");
        }

        public void Upload(string ruta, HttpPostedFileBase file)
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

        [Authorize]
        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Ingresar");
        }

        protected bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper, RegexOptions.None);
            }
            catch (Exception)
            {
                return false;
            }

            if (invalid)
                return false;

            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected string DomainMapper(Match match)
        {
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
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
