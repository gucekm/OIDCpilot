using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OIDCpilot.Pages;

// <snippet_Class>
[Authorize]
public class UserDataModel : PageModel
{
    public void OnGet()
    {
        ViewData["Session ID"] = Request.HttpContext.User.Claims.First(claim => claim.Type == "sid").Value;
        ViewData["Username"] = Request.HttpContext.User.Claims.First(claim => claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
        ViewData["First_name"] = Request.HttpContext.User.Claims.First(claim => claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").Value;
        ViewData["Last_name"] = Request.HttpContext.User.Claims.First(claim => claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").Value;
    }
}
// </snippet_Class>

