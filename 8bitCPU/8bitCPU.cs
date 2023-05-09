using System;
using System.Collections.Concurrent;
using System.Text;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace NexTos.An8bitCPU
{

    public struct instr
    {
        //public byte opcode;
        //public byte prefix;
        //public byte stages;
        public string mnemo;
        public byte par1;
        public byte par2;
        public byte args;
        public Func<int> action;
        public byte cycles;
    }

    public class An8bitCPU
    {
        protected byte[] REGs;

        //protected ushort addr;
        //protected byte data;
        //protected byte m_state;
        //protected bool data_out;
        protected int ticks;

        protected bool int_ena;
        protected bool st_halt;
        protected bool st_int;

        protected instr[] CMDs;
        protected instr current;

        protected Func<ushort, byte> Mem_Read;
        protected Action<ushort, byte> Mem_Write;

        protected StringBuilder trc;    // trace log

        protected An8bitCPU()
        {
            trc = new StringBuilder();
            init();
        }
        /*
                ~An8bitCPU()
                {

                }
        */

        protected void init()
        {
            Mem_Read = Mem_noRead;
            Mem_Write = Mem_noWrite;
            CreateRegs();
            CreateLookup();
            Reset();
        }

        protected virtual void CreateRegs()
        {
            //
        }

        protected virtual void CreateLookup()
        {
            // fill / ( for test. to be deleted)
            CMDs = new instr[256];
            for (int i = 0; i < 256; i++)
            {
                //CMDs[i].opcode = (byte)i;
                //CMDs[i].prefix = 0;
                //CMDs[i].stages = 0;

                CMDs[i].mnemo = "*UNK";
                CMDs[i].cycles = 0;
                CMDs[i].action = act_error;
            }
        }

        protected int act_error()
        {
            System.Console.WriteLine("Unimplemented instruction!");
            return current.cycles;
        }

        public void Connect()
        {

        }

        public void Connect_mem(Func<ushort, byte> mem_read, Action<ushort, byte> mem_write)
        {
            Mem_Read = mem_read;
            Mem_Write = mem_write;
        }

        public void Reset()
	    {
            int_ena = false;
            st_halt = false;
            st_int = false;
            ticks = 0;
            //data = 0;
            //addr = 0;

        }

        public void Step()
	    {
            //
        }

        protected byte Mem_noRead(ushort addr)
        {
            return 0xFF;
        }

        protected void Mem_noWrite(ushort addr, byte value)
        {
            // Do Nothing;
        }


        protected ushort Mem_ReadW(ushort addr)
        {
            return (ushort)(Mem_Read(addr) + (Mem_Read((ushort)(addr + 1)) << 8));
        }

        protected void Mem_WriteW(ushort addr, ushort value)
        {
            Mem_Write((ushort)(addr), (byte)(value & 0xFF));
            Mem_Write((ushort)(addr + 1), (byte)(value >> 8));
            return;
        }

        protected void Io_Read()
	    {
	
	    }

	    protected void Io_Write()
	    {
	
	    }

	    protected void Int_ack()
	    {
		
	    }
    }

}

