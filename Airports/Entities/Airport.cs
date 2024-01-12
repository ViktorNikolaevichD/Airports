namespace AirportsAndFlights.Entities
{
    public class Airport // Таблица аэропортов
    {
        public int Id { get; set; } // Id аэропорта
        public string Name { get; set; } // Название аэропорта
        public string City { get; set; } // Город
        public string Status { get; set; } // Статус (активный, неактивный)
    }
}
