using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class ErrorPagesController : Controller
    {
        public IActionResult Page401()
        {
            return View();
        }
        public IActionResult Page403()
        {
            return View();
        }
        public IActionResult Page404()
        {
            return View();
        }


        public IActionResult HandleError(int id)
        {
            if(id == 401)
            {
                return View("Page401");
            }
            if(id == 403)
            {
                return View("Page403");
            }
            if(id == 404)
            {
                return View("Page404");
            }

            return View();
        }
    }
}
