using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexTos.An8bitCPU
{
    public class Computer
    {
        protected An8bitCPU cpu;
        protected byte[] ram;
        protected byte[] rom;
        protected byte[] vmem;

        protected Computer()
        {
            // ports?
        }
    }

    public class Computer_RK86 : Computer
    {


        // debug needs
        protected StringBuilder prt;    // ports usage
        protected StringBuilder lst;    // listing
        protected StringBuilder lbl;    // symbolic names table

        public Computer_RK86(int ram_size = 32)
        {
            const String rom_file_name = "..//..//..//radiorom.bin";
            FileStream rom_file;

            prt = new StringBuilder();
            lst = new StringBuilder();
            lbl = new StringBuilder();

            cpu = new CPU_i80();
            ram = new byte[ram_size * 1024];
            rom = new byte[2 * 1024];
            //vmem = new byte[2*1024];

            ram[0] = 0xC3; ram[1] = 0x36; ram[2] = 0xF8; // JMP 0xF836

            rom_file = File.OpenRead(rom_file_name);
            for (int i = 0; i < rom_file.Length; i++)
            {
                rom[i] = (byte)rom_file.ReadByte();
            }
            rom_file.Close();

            // test programm
            FileStream prg_file = File.OpenRead("..//..//..//test.bin");
            for (int i = 0; i < prg_file.Length; i++)
            {
                ram[i] = (byte)prg_file.ReadByte();
            }
            prg_file.Close();
            // end test

            cpu.Connect_mem(Mem_Read, Mem_Write);

            cpu.State();

            for (int i = 0; i < 100; i++)
            {
                cpu.Step();
                cpu.State();
            }

            Console.WriteLine("nametable: ");
            Console.WriteLine(lbl.ToString());

            Console.WriteLine("listing: ");
            Console.WriteLine(lst.ToString());

            Console.WriteLine("ports operations: ");
            Console.WriteLine(prt.ToString());

            Console.WriteLine("log: ");
            Console.WriteLine(cpu.Status());

            //
            int a = 0;
        }

        protected byte Mem_Read(ushort addr)
        {
            if ((addr >= 0) && (addr < ram.Length))
            {
                return ram[addr];   // RAM
            }
            else if (addr >= 0xF800)
            {
                return rom[addr - 0xF800];
            }
            else
            {
                prt.Append(String.Format(" Read at addr 0x{0,4:X4}\n", addr));
                return 0xFF;
            }
        }

        protected void Mem_Write(ushort addr, byte value)
        {
            if ((addr >= 0) && (addr < ram.Length))
            {
                ram[addr] = value;  // RAM
            }
            else
            {
                prt.Append(String.Format(" Write at addr 0x{0,4:X4} to 0x{1,2:X2}\n", addr, value));
            }
        }
    }
}
