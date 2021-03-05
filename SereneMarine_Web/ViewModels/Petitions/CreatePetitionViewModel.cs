using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SereneMarine_Web.ViewModels.Petitions
{
    public class CreatePetitionViewModel
    {
        public string petition_id { get; set; }
        public string User_Id { get; set; }
        [Required]
        [Display(Name = "Petiton Name")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Please enter a Petition name between 10 and 100 characters in length")]
        public string name { get; set; }
        [Required]
        [Display(Name = "Petiton Description")]
        [DataType(DataType.MultilineText)]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description of the petition must be between 10 and 500 characters in length")]
        public string description { get; set; }
        [Required]
        [Display(Name = "Petiton Signature Goal")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a number for this field")]
        public int required_signatures { get; set; }
        [Display(Name = "Completed")]
        public bool completed { get; set; }
    }
}
