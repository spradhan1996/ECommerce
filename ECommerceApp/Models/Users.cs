namespace ECommerceApp.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Ideally should be hashed, not plaintext
        public string Role { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
    }
}
