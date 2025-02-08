using ECommerceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Linq;
using ECommerceApp.Service;
using ECommerceApp.Interface;

namespace ECommerceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public AccountController(ApplicationDbContext context, IUserService UserService)
        {
            _context = context;
            _userService = UserService;
        }

       
        public IActionResult Login()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                    var user = await _context.Users
                        .Where(u => u.Username == model.Username)
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        
                        HttpContext.Session.SetString("Username", user.Username);
                        if (user.Password == model.Password)
                        {
                            
                            var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Role, user.Role) 
                        };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = false, 
                            };

                           
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                            
                            if (user.Role == "Admin")
                            {
                                return RedirectToAction("Index", "Products");
                            }
                            else if (user.Role == "Seller")
                            {
                                return RedirectToAction("Index", "Products"); 
                            }
                            else
                            {
                                return RedirectToAction("Index", "Products"); 
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt. Incorrect password.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt. User not found.");
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            
            Response.Cookies.Delete(".AspNetCore.Identity.Application"); 
            return RedirectToAction("Login"); 
        }


        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(string newUsername, string newPassword, string confirmPassword, string newRole, string name, string mobileno)
        {
            
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return RedirectToAction("CreateUser"); 
            }

            
            if (_context.Users.Any(u => u.Username == newUsername))
            {
                TempData["ErrorMessage"] = "Username already exists.";
                return RedirectToAction("CreateUser");
            }

           
            var newUser = new Users
            {
                Username = newUsername,
                Password = newPassword,  
                Role = newRole,
                Name = name,
                MobileNo = mobileno
            };

            
            _context.Users.Add(newUser);
            _context.SaveChanges();

           
            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public JsonResult CheckUsername(string username)
        {
            bool exists = _context.Users.Any(u => u.Username == username);
            return Json(new { exists });
        }


       

        [HttpGet]
        public IActionResult UpdateUser()
        {
            var username = HttpContext.Session.GetString("Username"); 
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login"); 
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            var model = new Users
            {
                Username = user.Username,
                Password = user.Password,
                Role = user.Role,
                Name = user.Name,
                MobileNo = user.MobileNo
            };

            return PartialView("_UpdateUser", model); 
        }

        [HttpPost]
        public IActionResult UpdateUser(Users model)
        {
           
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

           
            if (user.Role == "Admin")
            {
                user.Name = model.Name;
                user.MobileNo = model.MobileNo;
            }
            else
            {
                user.Role = model.Role;
                user.Name = model.Name;
                user.MobileNo = model.MobileNo;
            }

            _context.Users.Update(user);
            _context.SaveChanges(); 

            TempData["SuccessMessage"] = "User updated successfully.";
            
            return Json(new { success = true, message = "User updated successfully." });
        }


        public async Task<IActionResult> GetUpdateUserPartial()
        {
            var currentUser = await _userService.GetCurrentUserAsync(); 

            return PartialView("_UpdateUser", currentUser);  
        }

    }
}
   
