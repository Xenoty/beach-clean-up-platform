using System.ComponentModel.DataAnnotations;

namespace SereneMarine_Web.ViewModels.Threads
{
    public class ThreadCreateViewModel
    {
        [Required]
        [Display(Name = "Topic")]
        public string thread_topic { get; set; }

        [Required]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string thread_descr { get; set; }
    }
}
