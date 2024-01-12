namespace Airports
{
    public class Program
    {
        static void Main(string[] args)
        {
            MPI.Environment.Run(ref args, comm =>
            {
            });
        }
    }
}