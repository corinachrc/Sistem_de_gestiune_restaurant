namespace RestaurantManager.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
