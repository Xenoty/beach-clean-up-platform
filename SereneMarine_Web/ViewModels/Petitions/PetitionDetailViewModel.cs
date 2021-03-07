using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SereneMarine_Web.ViewModels.Petitions
{
    public class PetitionDetailViewModel
    {
        public string id { get; set; }
        public string petition_id { get; set; }
        public string User_Id { get; set; }

        [Display(Name = "Name")]
        public string name { get; set; }

        [Display(Name = "Description")]
        public string description { get; set; }
        [Display(Name = "Signature Goal")]
        public int required_signatures { get; set; }
        [Display(Name = "Total Signatures")]
        public int current_signatures { get; set; }
        [Display(Name = "Completed")]
        public bool completed { get; set; }
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd, MMM, yyyy}")]
        public DateTime created_date { get; set; }

        public List<PetitionsSignedViewModel> petitionsSigned { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }

    public class PetitionsSignedViewModel
    {
        public string petition_id { get; set; }
        public string User_Id { get; set; }

        [Display(Name = "Date Signed")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd, MMM, yyyy}")]
        public string signed_date { get; set; }
        public string Name { get; set; }
    }
}
