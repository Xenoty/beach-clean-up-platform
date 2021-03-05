using SereneMarine_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SereneMarine_Web.ViewModels.ThreadMessages
{
    public class ThreadMessagesIndexVM
    {
        public ThreadsModel threadsModel { get; set; }
        public ThreadMessagesModel ThreadMessages { get; set; }
        public List<ThreadMessagesModel> threadMsgsList { get; set; }
    }
}
