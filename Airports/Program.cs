using System.Diagnostics;

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
                                                      "\ngenFlight - сгенерировать рейсы; " +

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