using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Petitions
{
    public class PetitionRegisterModel
    {
        [Required]
        public string name { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public int required_signatures { get; set; }
    }
}
