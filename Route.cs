namespace RaceDay.Models
{
    public class Route
    {
        public int RouteID { get; set; }
        public int EventID { get; set; }

        public string? RouteName { get; set; }
        public decimal DistanceKm { get; set; }

        public string? StartLocation { get; set; }
        public string? FinishLocation { get; set; }
        public string? RouteUrl { get; set; }
        public string? Description { get; set; }

        public Event? Event { get; set; }
    }
}
