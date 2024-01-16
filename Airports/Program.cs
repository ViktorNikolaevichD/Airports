using AirportsAndFlights.Entities;
using System.Diagnostics;
using System.Text.Json;

namespace AirportsAndFlights
{
    public class Program
    {
        static void Main(string[] args)
        {
            MPI.Environment.Run(ref args, comm =>
            {
                // Локальная база для добавленных объектов
                AddedDb AddedDb = new AddedDb();
                // Локальная база
                LocalDb localDb = new LocalDb();

                // Предзагрузка базы данных
                // Первым загружает базу 0 процесс, чтобы не было проблемы с одновременным созданием базы данных
                if (comm.Rank == 0)
                    // База данных для 0 процесса
                    localDb = Commands.LoadingDb(comm.Rank, comm.Size);
                // После того как 0 процесс загрузит базу, то все процессы выйдут из барьера
                comm.Barrier();
                // Все !0 процессы загрузят базу
                if (comm.Rank != 0)
                    // База данных для остальных(не 0) процессов
                    localDb = Commands.LoadingDb(comm.Rank, comm.Size);

                // Замер времени работы
                Stopwatch stopWatch = new Stopwatch();
                // Команда пользователя
                string? command = null;

                while (command != "quit")
                {
                    // Получение команды от пользователя
                    if (comm.Rank == 0)
                    {
                        Console.Write("Введите команду \ngenAirport - сгенерировать аэропорты;" +
                                                      "\ngenFlight - сгенерировать рейсы;" +
                                                      "\nupdateFlight - обновить статусы рейсова;" +
                                                      "\nvievFlight - посмотреть предстоящие вылеты;" +
                                                      "\nquit - выйти из программы: ");
                        command = Console.ReadLine();
                    }

                    // Рассылка команды по всем процессам
                    comm.Broadcast(ref command, 0);

                    switch (command)
                    {
                        case "genAirport":
                            // Число строк для генерации
                            int count = 0;
                            // Число строк для генерации каждым процессом
                            int countForRank = 0;
                            if (comm.Rank == 0)
                            {
                                Console.Write("Сколько аэропортов сгенерировать: ");
                                count = Convert.ToInt32(Console.ReadLine());
                                stopWatch.Restart();
                                countForRank = count / comm.Size;
                            }

                            // Разослать всем процессам количество строк для генерации
                            comm.Broadcast(ref countForRank, 0);
                            
                            if (comm.Rank == 0)
                            {
                                // Добавить недостающие строчки
                                countForRank = countForRank + (count % comm.Size);
                            }
                            
                            
                            // Генерируем аэропорты
                            Commands.GenerateAirports(countForRank);

                            // Все процессы ждут окончания генерации
                            comm.Barrier();

                            // Обновление локальной базы данных
                            localDb = Commands.LoadingDb(comm.Rank, comm.Size);

                            if (comm.Rank == 0)
                            {
                                stopWatch.Stop();
                                Console.WriteLine("Аэропорты сгенерированы");
                            }
                            break;
                        case "genFlight":
                            // Число строк для генерации
                            count = 0;
                            // Число строк для генерации каждым процессом
                            countForRank = 0;
                            if (comm.Rank == 0)
                            {
                                Console.Write("Сколько аэропортов сгенерировать: ");
                                count = Convert.ToInt32(Console.ReadLine());
                                stopWatch.Restart();
                                countForRank = count / comm.Size;
                            }

                            // Разослать всем процессам количество строк для генерации
                            comm.Broadcast(ref countForRank, 0);

                            if (comm.Rank == 0)
                            {
                                // Добавить недостающие строчки
                                countForRank = countForRank + (count % comm.Size);
                            }


                            // Генерируем аэропорты
                            Commands.GenerateFlights(countForRank);

                            // Все процессы ждут окончания генерации
                            comm.Barrier();

                            // Обновление локальной базы данных
                            localDb = Commands.LoadingDb(comm.Rank, comm.Size);

                            if (comm.Rank == 0)
                            {
                                stopWatch.Stop();
                                Console.WriteLine("Аэропорты сгенерированы");
                            }
                            break;
                        case "updateFlight":
                            if (comm.Rank == 0)
                            {
                                Console.WriteLine("Обновляем статусы рейсов");
                                stopWatch.Restart();
                            }

                            // Обновление статусов рейсов
                            Commands.UpdateFlightStatus(localDb);

                            // Все процессы ждут окончания обновления
                            comm.Barrier();

                            if (comm.Rank == 0)
                            {
                                stopWatch.Stop();
                                Console.WriteLine("Статусы обновлены");
                            }
                            break;
                        case "vievFlight":
                            // Id аэропорта вылета
                            int departureAirportId = 0;
                            // Id аэропорта прилета
                            int arrivalAirportId = 0;
                            // На сколько дней вперед просматривать рейсы
                            int daysAhead = 0;
                            if (comm.Rank == 0)
                            {
                                Console.Write("Id аэропорта вылета: ");
                                departureAirportId = Convert.ToInt32(Console.ReadLine());
                                Console.Write("Id аэропорта прилета: ");
                                arrivalAirportId = Convert.ToInt32(Console.ReadLine());
                                Console.Write("На сколько дней вперед: ");
                                daysAhead = Convert.ToInt32(Console.ReadLine());

                                Console.WriteLine("Выводим рейсы");
                                stopWatch.Restart();
                            }
                            // Разослать всем процессам значения
                            comm.Broadcast(ref departureAirportId, 0);
                            comm.Broadcast(ref arrivalAirportId, 0);
                            comm.Broadcast(ref daysAhead, 0);

                            // Обновление статусов рейсов
                            Commands.UpdateFlightStatus(localDb);

                            // Все процессы ждут окончания обновления
                            comm.Barrier();

                            if (comm.Rank == 0)
                            {
                                // Собрать списки в 0 процессе
                                string[] flights = comm.Gather(JsonSerializer.Serialize(Commands.ViewFlights(
                                                                                        localDb, 
                                                                                        departureAirportId,
                                                                                        arrivalAirportId,
                                                                                        daysAhead)), 0);
                                // Сборка нескольких списков рейсов в один
                                List<Flight> flightList = flights
                                            .Select(x => JsonSerializer.Deserialize<List<Flight>>(x)!)
                                            .Where(p => p != null)
                                            .Aggregate((a, b) => a.Concat(b).ToList());
                                if (flightList.Count() < 0)
                                {
                                    Console.WriteLine("Запланированных рейсов нет");
                                    stopWatch.Stop();
                                    break;
                                }

                                // Вывести список рейсов
                                foreach (Flight flight in flightList)
                                {
                                    Console.WriteLine(string.Format(
                                        "\nId {0:5} | " +
                                        "DepartureAirportId {1:5} {2:30} | " +
                                        "ArrivalAirportId {3:5} {4:30} | " +
                                        "DeparureTime {5} | " +
                                        "ArrivalTime {6} | " +
                                        "Status {7}",
                                        flight.Id,
                                        flight.DepartureAirportId,
                                        flight.DepartureAirport.City,
                                        flight.ArrivalAirportId,
                                        flight.ArrivalAirport.City,
                                        flight.DepartureTime,
                                        flight.ArrivalTime,
                                        flight.Status));
                                }
                                stopWatch.Stop();
                            }
                            else
                            {
                                comm.Gather(JsonSerializer.Serialize(Commands.ViewFlights(
                                                                    localDb,
                                                                    departureAirportId,
                                                                    arrivalAirportId,
                                                                    daysAhead)), 0);
                            }
                            break;
                        default:
                            if (comm.Rank == 0 && command != "quit")
                                Console.WriteLine("Неизвестная команда");
                            break;
                    }
                    if (comm.Rank == 0)
                    {
                        // Вывод времени выполнения
                        TimeSpan ts = stopWatch.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
                        Console.WriteLine($"RunTime {comm.Rank} " + elapsedTime);
                    }
                    // Барьер, чтобы все процессы подождали, пока 0 процесс выведет время работы
                    comm.Barrier();
                }
            });
        }
    }
}