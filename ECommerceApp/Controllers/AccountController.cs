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

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        //public async Task<IActionResult> Index()
        //{
        //    try
        //    {
        //        var products = await _context.Products.ToListAsync();
        //        var currentUser = await _userService.GetCurrentUserAsync();  // Assuming you have a method to fetch the current user

        //        // Create a view model that holds both Products and the current User
        //        var viewModel = new IndexViewModel
        //        {
        //            Products = products,
        //            CurrentUser = currentUser
        //        };

        //        return View(viewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        return View(ex.Message);  // Handle error and display the exception message
        //    }
        //}


        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Assuming you store passwords as hashed values
                    var user = await _context.Users
                        .Where(u => u.Username == model.Username)
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        // For the sake of simplicity, comparing plain-text passwords (not recommended)
                        // In a production environment, always hash and compare the passwords securely
                        HttpContext.Session.SetString("Username", user.Username);
                        if (user.Password == model.Password)
                        {
                            // Create claims based on the user's role
                            var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Role, user.Role) // Assuming the user has a Role field
                        };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = false, // Session-based authentication
                            };

                            // Sign in the user
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                            // Redirect based on the user's role
                            if (user.Role == "Admin")
                            {
                                return RedirectToAction("Index", "Products"); // Redirect to Admin Dashboard
                            }
                            else if (user.Role == "Seller")
                            {
                                return RedirectToAction("Index", "Products"); // Redirect to Seller Dashboard
                            }
                            else
                            {
                                return RedirectToAction("Index", "Products"); // Redirect to Products Page
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
            //HttpContext.Session.Clear(); // Clear session data
            Response.Cookies.Delete(".AspNetCore.Identity.Application"); // Delete authentication cookies if using Identity
            return RedirectToAction("Login"); // Redirect to login page
        }


        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(string newUsername, string newPassword, string confirmPassword, string newRole, string name, string mobileno)
        {
            // Validate that passwords match
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return RedirectToAction("CreateUser"); // Redirect back to Create User page
            }

            // Check if user already exists
            if (_context.Users.Any(u => u.Username == newUsername))
            {
                TempData["ErrorMessage"] = "Username already exists.";
                return RedirectToAction("CreateUser");
            }

            // Create a new user
            var newUser = new Users
            {
                Username = newUsername,
                Password = newPassword,  // In a real app, hash this password
                Role = newRole,
                Name = name,
                MobileNo = mobileno
            };

            // Save the user to the database
            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Display success message and redirect to login page
            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public JsonResult CheckUsername(string username)
        {
            bool exists = _context.Users.Any(u => u.Username == username);
            return Json(new { exists });
        }


        //public IActionResult UpdateUser()
        //{
        //    var username = HttpContext.Session.GetString("Username"); // Get the current username from session
        //    if (string.IsNullOrEmpty(username))
        //    {
        //        return RedirectToAction("Login"); // Redirect to login if not authenticated
        //    }

        //    var user = _context.Users.FirstOrDefault(u => u.Username == username);
        //    if (user == null)
        //    {
        //        TempData["ErrorMessage"] = "User not found.";
        //        return RedirectToAction("Index", "Home");
        //    }

        //    var model = new Users
        //    {
        //       // Username = user.Username,
        //        //Password = user.Password,
        //        Role = user.Role,
        //        Name = user.Name,
        //        MobileNo = user.MobileNo
        //    };

        //    //return View(model);
        //    return PartialView("_UpdateUser", model);
        //}

        [HttpGet]
        public IActionResult UpdateUser()
        {
            var username = HttpContext.Session.GetString("Username"); // Get username from session
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login"); // Redirect to login if session is empty
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

            return PartialView("_UpdateUser", model); // ✅ Load partial view
        }

        [HttpPost]
        public IActionResult UpdateUser(Users model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return PartialView("_UpdateUser", model); // Return view with validation errors
            //}

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

            // ✅ Update user data
            //user.Password = model.Password;
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
            _context.SaveChanges(); // Save changes to DB

            TempData["SuccessMessage"] = "User updated successfully.";
            //return RedirectToAction("Index", "Home"); // Redirect after update
            return Json(new { success = true, message = "User updated successfully." });
        }


        public async Task<IActionResult> GetUpdateUserPartial()
        {
            var currentUser = await _userService.GetCurrentUserAsync();  // Assuming you have a method to get the current user

            return PartialView("_UpdateUser", currentUser);  // Return the _UpdateUser partial view with the current user
        }

    }
}
   
