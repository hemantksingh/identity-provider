using Microsoft.AspNetCore.Mvc;

namespace client
{
    public class AuthorizationController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}