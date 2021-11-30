using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.ThreadMessages
{
    public class ThreadMessagesRegisterModel
    {
        [Required]
        public string thread_id { get; set; }
        [Required]
        public string thread_message { get; set; }
    }
}