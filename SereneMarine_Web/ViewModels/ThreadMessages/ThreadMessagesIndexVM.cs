using SereneMarine_Web.Models;
using System.Collections.Generic;

namespace SereneMarine_Web.ViewModels.ThreadMessages
{
    public class ThreadMessagesIndexVM
    {
        public ThreadsModel threadsModel { get; set; }
        public ThreadMessagesModel ThreadMessages { get; set; }
        public List<ThreadMessagesModel> threadMsgsList { get; set; }
    }
}
