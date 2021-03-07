using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.PetitionsSigned
{
    public class PetitionsSignedRegisterModel
    {
        [Required]
        public string petition_id { get; set; }
        [Required]
        public string User_Id { get; set; }
    }
}
