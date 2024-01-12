namespace Airports.Entities
{
    public class Flight // Таблица рейсов
    {
        public int Id { get; set; } // Номер рейса
        public int DepartureAirportId {  get; set; } // Аэропорт вылета
        public Airport DepartureAirport { get; set; } // Навигационное свойство на Airport
        public int ArrivalAirportId {  get; set; } // Аэропорт прилета
        public Airport ArrivalAirport { get; set; } // Навигационное свойство на Airport
        public DateTime DepartureTime { get; set; } // Время вылета
        public DateTime ArrivalTime {  get; set; } // Время прилета
        public string Status { get; set; } // Статус рейса (запланирован, вылетел, отменен, задержан, прибыл)

    }
}
