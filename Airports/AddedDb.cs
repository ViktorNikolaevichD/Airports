using AirportsAndFlights.Entities;

namespace AirportsAndFlights
{
    public class AddedDb
    {
        public List<Airport> AddedAirports { get; set; } = new List<Airport> { }; // Добавленные аэропорты
        public List<Flight> AddedFlights { get; set; } = new List<Flight> { }; // Добавленные рейсы
    }
}
