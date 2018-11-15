using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloha.Model.Entities
{
    public class Worker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        [Required]
        public string Name { get; set; }

        public string Surname { get; set; }

        [Url]
        public string PhotoUrl { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Notes { get; set; }

        public virtual User User { get; set; }

        public virtual Workstation Workstation { get; set; }
    }
}