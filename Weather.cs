namespace RaceDay.Models
{
    public class Weather
    {
        public int WeatherID { get; set; }
        public int EventID { get; set; }

        public DateTime ForecastDate { get; set; }
        public decimal? Temperature { get; set; }
        public int? Humidity { get; set; }
        public decimal? WindKph { get; set; }
        public int? RainProbability { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Event? Event { get; set; }
    }
}
