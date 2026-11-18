using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Context;
using Microsoft.EntityFrameworkCore;

namespace MYAcademy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponents
{
    public class MessageLayoutSidebarViewComponent : ViewComponent
    {
        private readonly EmailContext _context;

        public MessageLayoutSidebarViewComponent(EmailContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.CategoryName)
                .ToListAsync();

            return View(categories);
        }

    }
}
