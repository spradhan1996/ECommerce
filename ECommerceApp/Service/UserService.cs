using ECommerceApp.Interface;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceApp.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        //public async Task<Users> GetCurrentUserAsync()
        //{
        //    // Assuming the user is authenticated and you have user information in the claims
        //   // var userId = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var username = HttpContext.Session.GetString("Username");
        //    //if (string.IsNullOrEmpty(userId)) return null;  // Handle if the user is not logged in

        //    var user = await _context.Users.FindAsync(1);
        //    return user;
        //}

        public async Task<Users> GetCurrentUserAsync()
        {
            var username = _httpContextAccessor.HttpContext?.Session.GetString("Username"); // ✅ Get username from session safely

            if (string.IsNullOrEmpty(username))
                return null; // ✅ Return null if session is empty (user not logged in)

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username); // ✅ Get user by username

            return user;
        }
    }

}
