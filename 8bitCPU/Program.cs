
namespace NexTos.An8bitCPU
{
    static class Program
    {
        [System.STAThread]
        static void Main()
        {
            Computer rk = new Computer_RK86(32);

            System.Console.ReadKey();
        }
    }
}
