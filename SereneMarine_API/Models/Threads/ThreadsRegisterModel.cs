using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Threads
{
    public class ThreadsRegisterModel
    {
        [Required]
        public string thread_topic { get; set; }
        [Required]
        public string thread_descr { get; set; }
    }
}