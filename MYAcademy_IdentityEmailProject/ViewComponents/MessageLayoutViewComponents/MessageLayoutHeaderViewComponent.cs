using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponents
{
    public class MessageLayoutHeaderViewComponent : ViewComponent //Navbar yani burası
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
