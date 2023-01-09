using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            var model = new UserLoginModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] UserLoginModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.UserName == "John" && model.Password == "Williams")
                {
                    var identity = new ClaimsIdentity(new List<Claim> {
                        new Claim(ClaimTypes.Name, model.UserName),
                    }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(principal);
                    return Redirect("/");
                }
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
    }
}
