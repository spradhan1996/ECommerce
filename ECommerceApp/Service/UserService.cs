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

      

        public async Task<Users> GetCurrentUserAsync()
        {
            var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
                return null; 

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username); 

            return user;
        }
    }

}
