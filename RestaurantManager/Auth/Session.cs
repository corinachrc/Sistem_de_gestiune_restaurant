using RestaurantManager.Models;

namespace RestaurantManager.Auth
{
    public static class Session
    {
        public static User? CurrentUser { get; set; }
        public static bool IsAdmin => CurrentUser?.Role == "Admin";
        public static bool IsWaiter => CurrentUser?.Role == "Waiter";
    }
}
