using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;

namespace OIDCpilot.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IDistributedCache _cache;
        public LogoutModel(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async  Task<IActionResult> OnGetAsync()
        {
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return Page();
        }
    }
}
