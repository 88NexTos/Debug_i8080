/*
 * example demonstrating i8080 CPU emulation
 * on base of simple microcomputer "Radio-86"
 * (only partial emulation of computer!)
 * by Sergey NexTos (c) 2023
 * 
 * CPU emulation code based on project
 * "Intel 8080 (KR580VM80A) microprocessor core model"
 * by 2012 Alexander Demin <alexander@demin.ws>
 */

// project is in active development state. is NOT 100% functional.
// !! NOT FOR USE YET !!


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
