using AirportsAndFlights.Entities;

namespace AirportsAndFlights
{
    public class Commands
    {
        // Генерация аэропортов
        public static void GenerateAirports(int count)
        {
            using (var db = new AppDbContext())
            {
                for (int i = 0; i < count; i++)
                {
                    db.Add(new Airport
                    {

                    })
                }
            }
        }
    }
}
