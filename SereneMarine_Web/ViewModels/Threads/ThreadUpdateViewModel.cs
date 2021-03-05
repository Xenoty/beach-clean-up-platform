using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SereneMarine_Web.ViewModels.Threads
{
    public class ThreadUpdateViewModel
    {
        public string thread_id { get; set; }
        public string User_Id { get; set; }

        [Required]
        [Display(Name = "Topic")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Please enter an Thread topic between 3 and 50 characters in length")]
        [RegularExpression(@"^[a-zA-Z0-9'-'\s]*$", ErrorMessage = "Please enter a event name made up of letters and numbers only")]
        public string thread_topic { get; set; }
        [Required]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Please enter an Thread dsecription between 3 and 200 characters in length")]
        [RegularExpression(@"^[.,;""'()&%#!`\/=+\-a-zA-Z0-9'-'\s]*$", ErrorMessage = "Please enter a Thread description made up of letters and numbers only")]
        public string thread_descr { get; set; }
        [Display(Name = "Close Thread")]
        [DefaultValue(false)]
        public bool thread_closed { get; set; }
        [Display(Name = "Author")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Please enter an Thread topic between 3 and 50 characters in length")]
        [RegularExpression(@"^[a-zA-Z0-9'-'\s]*$", ErrorMessage = "Please enter a event name made up of letters and numbers only")]
        public string author { get; set; }
    }

}
