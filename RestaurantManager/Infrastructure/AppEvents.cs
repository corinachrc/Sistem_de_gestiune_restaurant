using System;

namespace RestaurantManager
{
    public static class AppEvents
    {
        public static event Action? MenuChanged;

        public static void RaiseMenuChanged() => MenuChanged?.Invoke();
    }
}