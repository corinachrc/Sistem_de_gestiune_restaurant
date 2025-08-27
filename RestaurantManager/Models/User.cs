namespace RestaurantManager.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = ""; // DEMO: plain text (în proiect real: hash/salt)
        public string Role { get; set; } = "Waiter"; // "Admin" sau "Waiter"
        public bool IsActive { get; set; } = true;
    }
}
