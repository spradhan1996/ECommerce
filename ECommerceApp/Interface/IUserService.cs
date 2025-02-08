using ECommerceApp.Models;

namespace ECommerceApp.Interface
{
    public interface IUserService
    {
       
            Task<Users> GetCurrentUserAsync();  
        

    }
}
