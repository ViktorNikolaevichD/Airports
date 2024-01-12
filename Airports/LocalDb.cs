using AirportsAndFlights.Entities;
    
namespace AirportsAndFlights
{

    public class LocalDb // Локальная база данных
    {
        public List<Airport> Airports { get; set; } = new List<Airport> { }; // Аэропорты
        public List<Flight> Flights { get; set; } = new List<Flight> { }; // Рейсы
    }
}
