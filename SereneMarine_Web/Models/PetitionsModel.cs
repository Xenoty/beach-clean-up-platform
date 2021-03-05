using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SereneMarine_Web.Models
{
    public class PetitionsModel
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

        public List<PetitionsModel> PetitionsViewModel { get; set; }
    }
}
