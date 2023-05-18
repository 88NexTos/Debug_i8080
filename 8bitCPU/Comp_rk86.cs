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
        //protected byte[] vmem;

        protected Computer()
        {
            //
        }
    }

    public class Computer_RK86 : Computer
    {


        // debug needs
        protected StringBuilder prt;    // ports usage
        protected StringBuilder lst;    // listing
        protected StringBuilder log;    // tr log

        public Computer_RK86(int ram_size = 32)
        {
            const String rom_file_name = "..//..//..//radiorom.bin";

            prt = new StringBuilder();
            lst = new StringBuilder();
            log = new StringBuilder();

            cpu = new CPU_i80();
            ram = new byte[ram_size * 1024];
            rom = new byte[2 * 1024];
            //vmem = new byte[2*1024];

            ram[0] = 0xC3; ram[1] = 0x36; ram[2] = 0xF8; // JMP 0xF836

            // "monitor" code (some kind of BIOS)
            FileStream rom_file = File.OpenRead(rom_file_name);
            for (int i = 0; i < rom_file.Length; i++)
            {
                rom[i] = (byte)rom_file.ReadByte();
            }
            rom_file.Close();
/*
            // test codes - debug
            FileStream prg_file = File.OpenRead("..//..//..//test.bin");
            for (int i = 0; i < prg_file.Length; i++)
            {
                ram[i] = (byte)prg_file.ReadByte();
            }
            prg_file.Close();
            // end test
*/
            cpu.Connect_mem(Mem_Read, Mem_Write);

            cpu.State();

            for (int i = 0; i < 100; i++)
            {
                cpu.Step();
                cpu.State();
            }

            Console.WriteLine("log: ");
            Console.WriteLine(log.ToString());

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
                // RAM
                return ram[addr];
            }
            else if ((addr >= 0x8000) && (addr < 0xDFFF))
            {
                // ports (0x8xxx 0xAxxx 0xCxxx)
                return Prt_Read(addr);
            }

            else if (addr >= 0xF800)
            {
                // ROM
                return rom[addr - 0xF800];
            }
            else
            {
                // not present
                log.Append(String.Format(" Read from unpresent mem at addr 0x{0,4:X4}\n", addr));
                return 0xFF;
            }
        }

        protected void Mem_Write(ushort addr, byte value)
        {
            if ((addr >= 0) && (addr < ram.Length))
            {
                // RAM
                ram[addr] = value;
            }
            else if((addr >= 0x8000) && (addr < 0xFFFF))
            {
                // ports (all - 0x8 0xA 0xC 0xE)
                Prt_Write(addr, value);
            }
            else
            {
                // not present
                log.Append(String.Format(" Write to unpresent mem at addr 0x{0,4:X4} with 0x{1,2:X2}\n", addr, value));
            }
        }

        protected byte Prt_Read(ushort addr)
        {
            //
            prt.Append(String.Format(" Read port at addr 0x{0,4:X4}\n", addr));
            return 0xFF;
        }

        protected void Prt_Write(ushort addr, byte value)
        {
            //
            prt.Append(String.Format(" Write port at addr 0x{0,4:X4} with 0x{1,2:X2}\n", addr, value));
        }

    }
}
