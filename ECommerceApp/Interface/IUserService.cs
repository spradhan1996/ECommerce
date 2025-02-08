using ECommerceApp.Models;

namespace ECommerceApp.Interface
{
    public interface IUserService
    {
       
            Task<Users> GetCurrentUserAsync();  // Method to fetch the current logged-in user
        

    }
}
