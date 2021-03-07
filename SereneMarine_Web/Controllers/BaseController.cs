using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace SereneMarine_Web.Controllers
{
    public class BaseController : Controller
    {
        #region Protected Variables

        protected HttpClient client = new HttpClient();
        protected HttpResponseMessage response = null;

        #endregion
    }
}
