using Microsoft.Extensions.Logging;

namespace RaceDay.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<Event> Events { get; set; }
            = new List<Event>();
    }
}
