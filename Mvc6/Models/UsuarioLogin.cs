using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Mvc6.Models
{
    public class UsuarioLogin
    {
        [Required(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string email { get; set; }

        [Required(ErrorMessage = "Clave inválida")]
        [DataType(DataType.Password)]
        [Display(Name = "Clave")]
        public string clave { get; set; }

        [Display(Name = "¿Recordar cuenta?")]
        public bool RememberMe { get; set; }
    }

    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
