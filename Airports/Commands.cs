using AirportsAndFlights.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace AirportsAndFlights
{
    public class Commands
    {
        // Функция загрузки базы данных
        public static LocalDb LoadingDb(int rank, int size)
        {
            using (var db = new AppDbContext())
            {
                // Количество строк в каждой таблице
                //int countAirport = db.Airports.Count();
                int countFlight = db.Flights.Count();
                // Размер части для каждой таблицы
                //int partAirpot = (countAirport / size + 1);
                int partFlight = (countFlight / size + 1);
                // Смещение по каждой таблице
                //int offsetAirport = rank * partAirpot;
                int offsetFlight = rank * partFlight;

                // Вернуть локальную базу данных
                return new LocalDb
                {
                    // Аэропорты (у каждой локальной базы есть список полный список аэропортов)
                    Airports = db.Airports
                            .ToList(),
                    // Рейсы
                    Flights = db.Flights
                            .Include(u => u.DepartureAirport)
                            .Include(u => u.ArrivalAirport)
                            .Skip(offsetFlight)
                            .Take(partFlight)
                            .ToList()
                };
            }
        }
        // Генерация аэропортов
        public static void GenerateAirports(int count)
        {
            using (var db = new AppDbContext())
            {
                for (int i = 0; i < count; i++)
                {
                    db.Airports.Add(new Airport
                    {
                        Name = Faker.Internet.DomainWord(),
                        City = Faker.Address.City(),
                        Status = "Открыт"
                    });
                }
                // Сохранить
                db.SaveChanges();
            }
        }

        // Функция для определения статуса рейса
        public static string CheckFlightStatus(DateTime depTime, DateTime arrivTime, int depAirportId, int arrivAirportId)
        {
            string status = "Неопределен";

            if (depTime == arrivTime)
            {
                status = "Отменен";
                return status;
            }
            if (depAirportId == arrivAirportId)
            {
                status = "Возвращен";
                return status;
            }    
            if (depTime < DateTime.Now &&
                arrivTime <= DateTime.Now)
            {
                status = "Завершен";
                return status;
            }
            if (depTime <= DateTime.Now &&
                arrivTime > DateTime.Now)
            {
                status = "Выполняется";
                return status;
            }
            if (depTime > DateTime.Now &&
                arrivTime > DateTime.Now )
            {
                status = "Запланирован";
                return status;
            }

            return status;
        }
        // Генерация рейсов
        public static void GenerateFlights(int count)
        {
            using (var db = new AppDbContext())
            {
                // Список аэропортов
                List<Airport> airports = db.Airports.Where(u => u.Status == "Открыт").ToList();
                for (int i = 0; i < count;i++)
                {
                    // Аэропорт вылета
                    int departuerAirportId = airports[Faker.RandomNumber.Next(0, airports.Count() - 1)].Id;
                    // Аэропорт прилета
                    int arrivalAirportId = airports[Faker.RandomNumber.Next(0, airports.Count() - 1)].Id;
                    // Время вылета самолета
                    DateTime departureTime = new DateTime(
                        DateTime.Now.Year,
                        Faker.RandomNumber.Next(1, 12),
                        Faker.RandomNumber.Next(1, 28),
                        Faker.RandomNumber.Next(0, 23),
                        Faker.RandomNumber.Next(0, 3) * 15,
                        0);
                    // Время прилета самолета
                    DateTime arrivalTime = departureTime
                        .AddHours(Faker.RandomNumber.Next(0, 9))
                        .AddMinutes(Faker.RandomNumber.Next(0, 3) * 15);

                    // Если время вылета и прилета совпадает, то изменить аэропорт прилета
                    if (departureTime == arrivalTime)
                    {
                        arrivalAirportId = departuerAirportId;
                    }

                    // Статус рейса
                    string status = CheckFlightStatus(departureTime, arrivalTime, departuerAirportId, arrivalAirportId);
                        
                    db.Flights.Add(new Flight
                    {
                        DepartureAirportId = departuerAirportId,
                        ArrivalAirportId = arrivalAirportId,
                        DepartureTime = departureTime,
                        ArrivalTime = arrivalTime,
                        Status = status
                        
                    });
                }
                // Сохранить
                db.SaveChanges();
            }
        }
        
        // 
    }
}
