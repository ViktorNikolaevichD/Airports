using Airports.Entities;
    
namespace Airports
{
    
    public class LocalDb // Локальная база данных
    {
        public List<Airport> Airports { get; set; } // Аэропорты
        public List<Flight> Flights { get; set; } // Рейсы
    }
}
