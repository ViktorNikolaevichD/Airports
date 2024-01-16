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
                int countFlight = db.Flights.Count();
                // Размер части для каждой таблицы
                int partFlight = (countFlight / size + 1);
                // Смещение по каждой таблице
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
        
        // Обновить данные в БД на сервере
        private static void UpdateServerDb(LocalDb localDb)
        {
            using (var db = new AppDbContext())
            {
                // Обновить данные в таблицах
                db.Airports.UpdateRange(localDb.Airports);
                db.Flights.UpdateRange(localDb.Flights);

                db.SaveChanges();
            }
        }

        // Обновить статусы вылетов
        public static void UpdateFlightStatus(LocalDb localDb)
        {
            // Обновить статусы рейсов, которые выполняются или у них запланирован статус
            foreach (var flight in localDb.Flights.Where(u => u.Status == "Запланирован" || u.Status == "Выполняется"))
            {
                // Аэропорт вылета
                int departureAirport = flight.DepartureAirportId;
                // Аэропорт прилета
                int arrivalAirport = flight.ArrivalAirportId;

                // Время вылета
                DateTime departureTime = flight.DepartureTime;
                // Время прилета
                DateTime arriavalime = flight.ArrivalTime;

                // Обновить статус
                flight.Status = CheckFlightStatus(departureTime, arriavalime, departureAirport, arrivalAirport);
            }

            // Обновить данные на сервере
            UpdateServerDb(localDb);
        }

        // Посмотреть предстоящие рейсы на N дней вперед
        public static List<Flight> ViewFlights(LocalDb localDb, int departureAirport, int arrivalAiport, int daysAhead)
        {
            // Последнее время вылета не познее
            DateTime lastDepartureTime = new DateTime(
                        DateTime.Now.Year,
                        DateTime.Now.Month,
                        DateTime.Now.Day,
                        23, 59, 59).AddDays(daysAhead);


            return localDb.Flights.Where(p => p.Status == "Запланирован" &&
                                            p.DepartureAirportId == departureAirport &&
                                            p.ArrivalAirportId == arrivalAiport &&
                                            p.DepartureTime <= lastDepartureTime).ToList();
        }
    }
}
