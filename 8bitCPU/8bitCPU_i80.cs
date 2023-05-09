using System;
using System.Security.Cryptography;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace NexTos.An8bitCPU
{
    public class CPU_i80 : An8bitCPU
    {
        enum regs : byte
        {
            B, C, D, E, H, L, M, A
        }

        enum regp : byte
        {
            BC, DE, HL, SP, PSW
        }

        enum cmds_alu : byte
        {
            ADD, ADC, SUB, SBB, ANA, XRA, ORA, CMP
        }

        enum cmds_ali : byte
        {
            ADI, ACI, SUI, SBI, ANI, XRI, ORI, CPI
        }

        enum cmds_rot : byte
        {
            RLC, RRC, RAL, RAR, DAA, CMA, STC, CMC
        }

        enum cmds_cond : byte
        {
            NZ, Z, NC, C, PO, PE, P, M
        }

        public CPU_i80()
        {
            //
        }

        protected override void CreateRegs()
        {
            REGs = new byte[16];
            // B, C, D, E, H, L, F, A, SP, PC 
            for (int i = 0; i < REGs.Length; i++)
            {
                REGs[i] = 0;
            }
        }


        
        /*
        protected byte rg_A
        {
            get { return REGs[(byte)regs.A]; }
            set { REGs[(byte)regs.A] = value; }
        }

        protected byte rg_B
        {
            get { return REGs[(byte)regs.B]; }
            set { REGs[(byte)regs.B] = value; }
        }
        protected byte rg_C
        {
            get { return REGs[(byte)regs.C]; }
            set { REGs[(byte)regs.C] = value; }
        }
        protected byte rg_D
        {
            get { return REGs[(byte)regs.D]; }
            set { REGs[(byte)regs.D] = value; }
        }

        protected byte rg_E
        {
            get { return REGs[(byte)regs.E]; }
            set { REGs[(byte)regs.E] = value; }
        }
        protected byte rg_H
        {
            get { return REGs[(byte)regs.H]; }
            set { REGs[(byte)regs.H] = value; }
        }
        protected byte rg_L
        {
            get { return REGs[(byte)regs.L]; }
            set { REGs[(byte)regs.L] = value; }
        }
*/
        protected ushort rg_PSW
        {
            get { return (ushort)((REGs[(byte)regs.A] << 8) + REGs[6]); }
            set
            {
                REGs[(byte)regs.A] = (byte)((value >> 8) & 0xFF);
                REGs[6] = (byte)(value & 0xFF);
            }
        }
        protected ushort rg_BC
        {
            get { return (ushort)((REGs[(byte)regs.B] << 8) + REGs[(byte)regs.C]); }
            set
            {
                REGs[(byte)regs.B] = (byte)((value >> 8) & 0xFF);
                REGs[(byte)regs.C] = (byte)(value & 0xFF);
            }
        }
        protected ushort rg_DE
        {
            get { return (ushort)((REGs[(byte)regs.D] << 8) + REGs[(byte)regs.E]); }
            set
            {
                REGs[(byte)regs.D] = (byte)((value >> 8) & 0xFF);
                REGs[(byte)regs.E] = (byte)(value & 0xFF);
            }
        }
        protected ushort rg_HL
        {
            get { return (ushort)((REGs[(byte)regs.H] << 8) + REGs[(byte)regs.L]); }
            set
            {
                REGs[(byte)regs.H] = (byte)((value >> 8) & 0xFF);
                REGs[(byte)regs.L] = (byte)(value & 0xFF);
            }
        }
        protected ushort rg_SP
        {
            get { return (ushort)((REGs[8] << 8) + REGs[9]); }
            set
            {
                REGs[8] = (byte)((value >> 8) & 0xFF);
                REGs[9] = (byte)(value & 0xFF);
            }
        }
        protected ushort rg_PC
        {
            get { return (ushort)((REGs[10] << 8) + REGs[11]); }
            set
            {
                REGs[10] = (byte)((value >> 8) & 0xFF);
                REGs[11] = (byte)(value & 0xFF);
            }
        }

        protected byte rg_F
        {
            get { return REGs[6]; }
            set { REGs[6] = value; }
        }

        protected byte rg_M
        {
            get { return Mem_Read(rg_HL); }
            set { Mem_Write(rg_HL, value); }
        }

        protected ushort rg_MM
        {
            get { return (ushort)((Mem_Read((ushort)(rg_HL + 1)) << 8) + Mem_Read(rg_HL)); }
            set
            {
                Mem_Write((ushort)(rg_HL + 1), (byte)((value >> 8) & 0xFF));
                Mem_Write(rg_HL, (byte)(value & 0xFF));
            }
        }

        protected void push(ushort value)
        {
            Mem_WriteW((ushort)(rg_SP - 2), value);
            rg_SP -= 2;
        }

        protected ushort pop()
        {
            rg_SP += 2;
            return Mem_ReadW((ushort)(rg_SP - 2));
        }


        public new void Step()
        {
            // Read
            ushort addr = rg_PC;
            byte opcode = Mem_Read(rg_PC++);
            trc.AppendFormat("0x{0,4:X4}\t{1,2:X2} ", addr, opcode);


            // Decode
            current = CMDs[opcode];

            if (current.args == 1)
            {
                trc.AppendFormat("{0,2:X2} ", Mem_Read(rg_PC));
                trc.AppendFormat("\t\t{0}0x{1,2:X2}\t ", current.mnemo, Mem_Read(rg_PC));
            }
            else if (current.args == 2)
            {
                trc.AppendFormat("{0,2:X2} {1,2:X2} ", Mem_Read(rg_PC), Mem_Read((ushort)(rg_PC + 1)));
                trc.AppendFormat("\t{0}0x{1,4:X4}\t ", current.mnemo, Mem_ReadW(rg_PC));
            }
            else
            {
                trc.AppendFormat("\t\t{0}\t\t ", current.mnemo);
            }
            
            // Execute
            int tc = current.action();
            trc.AppendFormat("\n");
            ticks += tc;
        }

        public void State()
        {
            trc.AppendFormat(" A=0x{0,2:X2} B=0x{1,2:X2} C=0x{2,2:X2} D=0x{3,2:X2} E=0x{4,2:X2} H=0x{5,2:X2} L=0x{6,2:X2}  F=0x{7,2:X2}\t SP=0x{8,4:X4}\t PC=0x{9,4:X4}\t {10} cycles\n",
                REGs[(byte)regs.A], REGs[(byte)regs.B], REGs[(byte)regs.C], REGs[(byte)regs.D], REGs[(byte)regs.E], REGs[(byte)regs.H], REGs[(byte)regs.L], rg_F, rg_SP, rg_PC, ticks);
        }

        public String Status()
        {
            return trc.ToString();
        }

        protected override void CreateLookup()
        {
            base.CreateLookup();
            //
            string i_mnemo;
            Func<int> i_action;
            byte i_cycles;
            byte i_args;
            byte i_par1;


            for (int cmd = 0x00; cmd <= 0x3F; cmd++) // codes 0x00 - 0x3F
            {
                i_mnemo = "*UNK";
                i_action = act_error;
                i_cycles = 0;
                i_args = 0;
                i_par1 = 0;
                
                if ((cmd & 0b00000100) == 0) // 0, 1, 2, 3, 8, 9, A, B
                {
                    byte temp = (byte)((cmd & 0b00110000) >> 4);

                    //NOP, LXI, STA, INX, RSV, DAD, LDA, DCX
                    switch (cmd & 0b00000011)
                    {
                        // 0x00 0x08 0x10 0x18 0x20 0x28 0x30 0x38
                        case 0:
                            if (cmd != 0x00)
                            {
                                i_mnemo = "*NOP";   // Undocumented!
                            }
                            else
                            {
                                i_mnemo = "NOP";    // Documented.
                            }
                            i_action = act_nop;
                            i_cycles = 4;
                            break;
                        
                        // 0x01 0x09 0x11 0x19 0x21 0x29 0x31 0x39
                        case 1:
                            i_par1 = temp;
                            if ((cmd & 0b00001000) == 0)
                            {
                                i_mnemo = String.Format("LXI {0}, ", ((regp)i_par1).ToString());
                                i_action = act_lxi;
                                i_cycles = 10;
                                i_args = 2;
                            }
                            else
                            {
                                i_mnemo = String.Format("DAD {0}", ((regp)i_par1).ToString());
                                i_action = act_dad;
                                i_cycles = 10;
                            }
                            break;

                        // 0x02 0x0A 0x12 0x1A 0x22 0x2A 0x32 0x3A
                        case 2:
                            if (temp == 3)    // sta lda
                            {
                                if ((cmd & 0b00001000) == 0)
                                {
                                    i_mnemo = "STA ";
                                    i_action = act_sta;
                                }
                                else
                                {
                                    i_mnemo = "LDA ";
                                    i_action = act_lda;
                                }
                                i_cycles = 13;
                                i_args = 2;
                            }
                            else if (temp == 2)   // shld lhld
                            {
                                if ((cmd & 0b00001000) == 0)
                                {
                                    i_mnemo = "SHLD ";
                                    i_action = act_shld;
                                }
                                else
                                {
                                    i_mnemo = "LHLD ";
                                    i_action = act_lhld;
                                }
                                i_cycles = 16;
                                i_args = 2;
                            }
                            else    // stax ldax
                            {
                                i_par1 = temp;
                                if ((cmd & 0b00001000) == 0)
                                {
                                    i_mnemo = String.Format("STAX {0}", ((regp)i_par1).ToString());
                                    i_action = act_stax;
                                }
                                else
                                {
                                    i_mnemo = String.Format("LDAX {0}", ((regp)i_par1).ToString());
                                    i_action = act_ldax;
                                }
                                i_cycles = 7;
                            }
                            break;

                        // 0x03 0x0B 0x13 0x1B 0x23 0x2B 0x33 0x3B
                        case 3:
                            i_par1 = temp;
                            if ((cmd & 0b00001000) == 0)
                            {
                                i_mnemo = String.Format("INX {0}", ((regp)i_par1).ToString());
                                i_action = act_inx;
                            }
                            else
                            {
                                i_mnemo = String.Format("DCX {0}", ((regp)i_par1).ToString());
                                i_action = act_dcx;
                            }
                            i_cycles = 5;
                            break;
                    }
                }
                else // 4, 5, 6, 7, C, D, E, F
                {
                    i_par1 = (byte)((cmd & 0b00111000) >> 3);
                    switch (cmd & 0b00000011)
                    {
                        // 0x04 0x0C 0x14 0x1C 0x24 0x2C 0x34 0x3C
                        case 0:
                            i_mnemo = String.Format("INR {0}", ((regs)i_par1).ToString());
                            i_action = act_inr;
                            if ((regs)i_par1 != regs.M) i_cycles = 5;
                                else i_cycles = 10; // 5/10
                            break;
                        
                        // 0x05 0x0D 0x15 0x1D 0x25 0x2D 0x35 0x3D 
                        case 1:
                            i_mnemo = String.Format("DCR {0}", ((regs)i_par1).ToString());
                            i_action = act_dcr;
                            if ((regs)i_par1 != regs.M) i_cycles = 5;
                                else i_cycles = 10; // 5/10
                            break;

                        // 0x06 0x0E 0x16 0x1E 0x26 0x2E 0x36 0x3E 
                        case 2:
                            i_mnemo = String.Format("MVI {0}, ", ((regs)i_par1).ToString());
                            i_args = 1;
                            i_action = act_mvi;
                            if ((regs)i_par1 != regs.M) i_cycles = 7;
                                else i_cycles = 10; // 7/10
                            break;

                        // 0x07 0x0F 0x17 0x1F 0x27 0x2F 0x37 0x3F
                        case 3:
                            // RLC, RRC, RAL, RAR, DAA, CMA, STC, CMC
                            i_mnemo = ((cmds_rot)i_par1).ToString();
                            i_action = act_rot;
                            i_cycles = 4;
                            break;
                    }
                }
                CMDs[cmd].mnemo = i_mnemo;
                CMDs[cmd].par1 = i_par1;
                CMDs[cmd].args = i_args;
                CMDs[cmd].action = i_action;
                CMDs[cmd].cycles = i_cycles;
            }

            for (int cmd = 0x40; cmd <= 0x7F; cmd++) // codes 0x40 - 0x7F
            {
                if (cmd == 0x76)
                {
                    CMDs[cmd].mnemo = "HLT";
                    CMDs[cmd].action = act_hlt;
                    CMDs[cmd].cycles = 7;
                }
                else
                {
                    CMDs[cmd].par1 = (byte)((cmd & 0b00111000) >> 3);
                    CMDs[cmd].par2 = (byte)(cmd & 0b00000111);

                    CMDs[cmd].mnemo = String.Format("MOV {0}, {1}", (regs)CMDs[cmd].par1, (regs)CMDs[cmd].par2);
                    CMDs[cmd].action = act_mov;
                    if (((regs)CMDs[cmd].par1 != regs.M) && ((regs)CMDs[cmd].par2 != regs.M)) CMDs[cmd].cycles = 5;
                        else CMDs[cmd].cycles = 7; // 5/7
                };
            }

            for (int cmd = 0x80; cmd <= 0xBF; cmd++) // codes 0x80 - 0xBF
            {
                // ADD, ADC, SUB, SBB, ANA, XRA, ORA, CMP
                CMDs[cmd].par1 = (byte)((cmd & 0b00111000) >> 3);
                CMDs[cmd].par2 = (byte)(cmd & 0b00000111);
                CMDs[cmd].mnemo = String.Format("{0} {1}", (cmds_alu)CMDs[cmd].par1, (regs)CMDs[cmd].par2);
                CMDs[cmd].action = act_alu;
                if ((regs)CMDs[cmd].par1 != regs.M) CMDs[cmd].cycles = 4;
                    else CMDs[cmd].cycles = 7; // 4/7
            }

            for (int cmd = 0xC0; cmd <= 0xFF; cmd++) // codes 0xC0 - 0xFF
            {
                i_mnemo = "*UNK";
                i_action = act_error;
                i_cycles = 0;
                i_args = 0;
                i_par1 = 0;

                if ((cmd & 0b00000001) == 0)
                {
                    i_par1 = (byte)((cmd & 0b00111000) >> 3);
                    switch ((cmd & 0b00000110) >> 1)
                    {
                        // 0, 8 ret cond
                        case 0:
                            i_mnemo = String.Format("R{0}", (cmds_cond)i_par1);
                            i_action = act_rcon;
                            i_cycles = 5; // 5/11
                            break;
                        // 2, A jmp cond
                        case 1:
                            i_mnemo = String.Format("J{0} ", (cmds_cond)i_par1);
                            i_action = act_jcon;
                            i_cycles = 10;
                            i_args = 2;
                            break;
                        // 4, C call cond
                        case 2:
                            i_mnemo = String.Format("C{0} ", (cmds_cond)i_par1);
                            i_action = act_ccon;
                            i_cycles = 11; // 11/17
                            i_args = 2;
                            break;
                        // 6, E ali block
                        case 3:
                            // adi, aci, sui, sbi, ani, xri, ori, cpi
                            i_mnemo = String.Format("{0} ", (cmds_ali)i_par1);
                            i_action = act_ali;
                            i_cycles = 7;
                            i_args = 1;
                            break;
                    }
                }
                else
                {
                    switch ((cmd & 0b00001110) >> 1)
                    {
                        case 0:
                            // 0xX1 pop
                            i_par1 = (byte)((cmd & 0b00110000) >> 4);
                            if (i_par1 == 3) i_par1 = (byte)regp.PSW;
                            i_mnemo = String.Format("POP {0}", (regp)i_par1);
                            i_action = act_pop;
                            i_cycles = 10;
                            break;

                        case 1:
                        case 5:
                            // 0xX3, 0xXB jmp in out
                            switch ((cmd & 0b00110000) >> 4)
                            {
                                case 0:
                                    // 0xC3 0xCB jmp
                                    if (cmd == 0xC3) i_mnemo = String.Format("JMP ");
                                        else i_mnemo = String.Format("*JMP ");
                                    i_action = act_jmp;
                                    i_cycles = 10;
                                    i_args = 2;
                                    break;

                                case 1:
                                    // 0xD3 0xDB out in
                                    if (cmd == 0xD3)
                                    {
                                        i_mnemo = String.Format("OUT ");
                                        i_action = act_out;
                                    }
                                    else if (cmd == 0xDB)
                                    {
                                        i_mnemo = String.Format("IN ");
                                        i_action = act_inp;
                                    }
                                    i_cycles = 10;
                                    i_args = 1;
                                    break;

                                case 2:
                                    // 0xE3 0xEB xthl xchg
                                    if (cmd == 0xE3)
                                    {
                                        i_mnemo = String.Format("XTHL");
                                        i_action = act_xthl;
                                        i_cycles = 10;
                                    }
                                    else if (cmd == 0xEB)
                                    {
                                        i_mnemo = String.Format("XCHG");
                                        i_action = act_xchg;
                                        i_cycles = 4;
                                    }
                                    break;

                                case 3:
                                    // 0xF3 0xFB di ei
                                    if (cmd == 0xF3)
                                    {
                                        i_mnemo = String.Format("DI");
                                        i_action = act_di;
                                    }
                                    else if (cmd == 0xFB)
                                    {
                                        i_mnemo = String.Format("EI");
                                        i_action = act_ei;
                                    }
                                    i_cycles = 4;
                                    break;
                            }
                            break;
                        
                        case 2:
                            // 0xX5 push
                            i_par1 = (byte)((cmd & 0b00110000) >> 4);
                            if (i_par1 == 3) i_par1 = (byte)regp.PSW;
                            i_mnemo = String.Format("PUSH {0}", (regp)i_par1);
                            i_action = act_push;
                            i_cycles = 11;
                            break;
                        
                        case 3:
                        case 7:
                            // 0xX7, 0xXF rst
                            i_par1 = (byte)((cmd & 0b00111000) >> 3);
                            i_mnemo = String.Format("RST {0}", i_par1);
                            i_action = act_rst;
                            i_cycles = 11;
                            break;
                        
                        case 4:
                            // 0xX9 ret pchl sphl
                            if (((cmd & 0b00100000) >> 1) == 0)
                            {
                                if (cmd == 0xC9) i_mnemo = "RET";
                                    else i_mnemo = "*RET";
                                i_action = act_ret;
                                i_cycles = 10;
                            }
                            else
                            {
                                if (cmd == 0xE9)
                                {
                                    i_mnemo = "PCHL";
                                    i_action = act_pchl;
                                }
                                if (cmd == 0xF9)
                                {
                                    i_mnemo = "SPHL";
                                    i_action = act_sphl;
                                }
                                i_cycles = 5;
                            }
                            break;
                        
                        case 6:
                            // 0xXD call
                            if (cmd == 0xCD) i_mnemo = "CALL ";
                                else i_mnemo = "*CALL ";
                            i_action = act_call;
                            i_args = 2;
                            i_cycles = 17;
                            break;
                    }

                }

                //
                CMDs[cmd].mnemo = i_mnemo;
                CMDs[cmd].par1 = i_par1;
                CMDs[cmd].args = i_args;
                CMDs[cmd].action = i_action;
                CMDs[cmd].cycles = i_cycles;
            }
        }

    // Actions:
        private int act_nop()
        {
            // no operation
            System.Console.WriteLine("No Operation");
            return current.cycles;
        }

        private int act_mvi()
        {
            // move immediate byte to register/memory
            byte data = Mem_Read(rg_PC);
            rg_PC += current.args; // 1 byte
            System.Console.WriteLine("Load {0} with 0x{1,2:X2}", (regs)current.par1, data);
            if ((regs)current.par1 != regs.M) REGs[current.par1] = data;
                else Mem_Write(rg_HL, data);
            return current.cycles;
        }

        private int act_lxi()
        {
            // move immediate word to reg. pair
            ushort data = Mem_ReadW(rg_PC);
            rg_PC += current.args; // 2 bytes
            System.Console.WriteLine("Load {0} with 0x{1,4:X4}", (regp)current.par1, data);
            switch (current.par1)
            {
                case 0:
                    rg_BC = data;
                    break;
                case 1:
                    rg_DE = data;
                    break;
                case 2:
                    rg_HL = data;
                    break;
                case 3:
                    rg_SP = data;
                    break;
            }
            return current.cycles;
        }

        private int act_dad()
        {
            // 16-bit add
            ushort data = 0;
            switch (current.par1)
            {
                case 0:
                    data = rg_BC;
                    break;
                case 1:
                    data = rg_DE;
                    break;
                case 2:
                    data = rg_HL;
                    break;
                case 3:
                    data = rg_SP;
                    break;
            }
            System.Console.WriteLine("Add {0} to HL", (regp)current.par1);
            rg_HL += data;
            return current.cycles;
        }

        private int act_sta()
        {
            //
            ushort addr = Mem_ReadW(rg_PC);
            rg_PC += current.args; // 2 bytes
            byte data = REGs[(byte)regs.A];
            System.Console.WriteLine("Store A to 0x{0,4:X4}\t 0x{1,2:X2}", addr, data);
            Mem_Write(addr, data);
            return current.cycles;
        }

        private int act_lda()
        {
            //
            ushort addr = Mem_ReadW(rg_PC);
            rg_PC += current.args; // 2 bytes
            byte data = Mem_Read(addr);
            System.Console.WriteLine("Load A from 0x{0,4:X4}\t 0x{1,2:X2}", addr, data);
            REGs[(byte)regs.A] = data;
            return current.cycles;
        }

        private int act_stax()
        {
            //
            ushort addr;
            if (current.par1 == 0) addr = rg_BC;
                else addr = rg_DE;
            byte data = REGs[(byte)regs.A];
            System.Console.WriteLine("Store A to 0x{0,4:X4}\t 0x{1,2:X2}", addr, data);
            Mem_Write(addr, data);
            return current.cycles;
        }

        private int act_ldax()
        {
            //
            ushort addr;
            if (current.par1 == 0) addr = rg_BC;
                else addr = rg_DE;
            byte data = Mem_Read(addr);
            System.Console.WriteLine("Load A from 0x{0,4:X4}\t 0x{1,2:X2}", addr, data);
            REGs[(byte)regs.A] = data;
            return current.cycles;
        }
        private int act_shld()
        {
            //
            ushort addr = Mem_ReadW(rg_PC);
            rg_PC += current.args; // 2 bytes
            ushort data = rg_HL;
            System.Console.WriteLine("Store HL to 0x{0,4:X4}\t 0x{1,4:X4}", addr, data);
            Mem_WriteW(addr, data);
            return current.cycles;
        }

        private int act_lhld()
        {
            //
            ushort addr = Mem_ReadW(rg_PC);
            rg_PC += current.args; // 2 bytes
            ushort data = Mem_ReadW(addr);
            System.Console.WriteLine("Load HL from 0x{0,4:X4}\t 0x{1,4:X4}", addr, data);
            rg_HL = data;
            return current.cycles;
        }

        private int act_inr()
        {
            // increment register
            System.Console.WriteLine("Increment {0}", (regs)current.par1);
            if ((regs)current.par1 != regs.M) REGs[current.par1]++;
                else Mem_Write(rg_HL, (byte)(Mem_Read(rg_HL) + 1));
            return current.cycles;
        }

        private int act_dcr()
        {
            // decrement register
            System.Console.WriteLine("Decrement {0}", (regs)current.par1);
            if ((regs)current.par1 != regs.M) REGs[current.par1]--;
                else Mem_Write(rg_HL, (byte)(Mem_Read(rg_HL) - 1));
            return current.cycles;
        }

        private int act_inx()
        {
            // increment reg. pair
            System.Console.WriteLine("Increment {0}", (regp)current.par1);
            switch (current.par1)
            {
                case 0:
                    rg_BC++;
                    break;
                case 1:
                    rg_DE++;
                    break;
                case 2:
                    rg_HL++;
                    break;
                case 3:
                    rg_SP++;
                    break;
            }
            return current.cycles;
        }

        private int act_dcx()
        {
            // decrement reg. pair
            System.Console.WriteLine("Decrement {0}", (regp)current.par1);
            switch (current.par1)
            {
                case 0:
                    rg_BC--;
                    break;
                case 1:
                    rg_DE--;
                    break;
                case 2:
                    rg_HL--;
                    break;
                case 3:
                    rg_SP--;
                    break;
            }
            return current.cycles;
        }


        //
        private int act_rot()
        {
            //
            System.Console.WriteLine("Unimplemented instruction: rot block");
            return current.cycles;
        }

        private int act_alu()
        {
            //
            System.Console.WriteLine("Unimplemented instruction: alu block");
            return current.cycles;
        }

        private int act_ali()
        {
            //
            ushort addr = rg_PC;
            byte data = Mem_Read(addr);
            rg_PC += current.args;
            System.Console.WriteLine("Unimplemented instruction: ali block with arg = 0x{0,2:X2}", data);
            return current.cycles;
        }

        private int act_hlt()
        {
            // halt
            System.Console.WriteLine("Halt!");
            st_halt = true;
            return current.cycles;
        }

        private int act_mov()
        {
            // Move reg to reg
            byte data;
            if ((regs)current.par2 != regs.M) data = REGs[current.par2];
                else data = Mem_Read(rg_HL);
            System.Console.WriteLine("Move 0x{0,2:X2} from {1} to {2}", data, (regs)current.par2, (regs)current.par1);
            if ((regs)current.par1 != regs.M) REGs[current.par1] = data;
                else Mem_Write(rg_HL, data);
            return current.cycles;
        }

        private int act_push()
        {
            ushort data = 0;
            switch (current.par1)
            {
                case 0:
                    data = rg_BC;
                    break;
                case 1:
                    data = rg_DE;
                    break;
                case 2:
                    data = rg_HL;
                    break;
                case 4:
                    data = rg_PSW;
                    break;
            }
            System.Console.WriteLine("Push 0x{0,4:X4} from {1}", data, (regp)current.par1);
            push(data);
            return current.cycles;
        }

        private int act_pop()
        {
            ushort data = pop();
            System.Console.WriteLine("Pop  0x{0,4:X4}  to  {1}", data, (regp)current.par1);
            switch (current.par1)
            {
                case 0:
                    rg_BC = data;
                    break;
                case 1:
                    rg_DE = data;
                    break;
                case 2:
                    rg_HL = data;
                    break;
                case 4:
                    rg_PSW = data;
                    break;
            }
            return current.cycles;
        }

        private int act_pchl()
        {
            //
            System.Console.WriteLine("Move HL to PC");
            rg_PC = rg_HL;
            return current.cycles;
        }

        private int act_sphl()
        {
            //
            System.Console.WriteLine("Move HL to SP");
            rg_SP = rg_HL;
            return current.cycles;
        }

        private int act_xthl()
        {
            //
            System.Console.WriteLine("Unimplemented instruction: XTHL");
            return current.cycles;
        }

        private int act_xchg()
        {
            //
            System.Console.WriteLine("Unimplemented instruction: XCHG");
            return current.cycles;
        }

        private int act_inp()
        {
            //
            rg_PC += current.args;
            System.Console.WriteLine("Unimplemented instruction: IN");
            return current.cycles;
        }

        private int act_out()
        {
            //
            rg_PC += current.args;
            System.Console.WriteLine("Unimplemented instruction: OUT");
            return current.cycles;
        }

        private int act_ei()
        {
            //
            System.Console.WriteLine("Unimplemented instruction: EI");
            return current.cycles;
        }

        private int act_di()
        {
            //
            System.Console.WriteLine("Unimplemented instruction: DI");
            return current.cycles;
        }

        private int act_jmp()
        {
            ushort jp_a = Mem_ReadW(rg_PC);
            System.Console.WriteLine("Jump to 0x{0, 4:X4}", jp_a);
            rg_PC = jp_a;
            return current.cycles;
        }

        private int act_call()
        {
            ushort jp_a = Mem_ReadW(rg_PC);
            push((ushort)(rg_PC + 2));
            System.Console.WriteLine("Call of 0x{0, 4:X4}", jp_a);
            rg_PC = jp_a;
            return current.cycles;
        }

        private int act_ret()
        {
            ushort jp_a = pop();
            System.Console.WriteLine("Return to 0x{0, 4:X4}", jp_a);
            rg_PC = jp_a;
            return current.cycles;
        }

        private int act_jcon()
        {
            //
            ushort jp_a = Mem_ReadW(rg_PC);
                rg_PC += current.args;

            System.Console.WriteLine("Unimplemented instruction: jump cond");
            return current.cycles;
        }

        private int act_ccon()
        {
            //
            ushort jp_a = Mem_ReadW(rg_PC);
                rg_PC += current.args;

            System.Console.WriteLine("Unimplemented instruction: call cond");
            return current.cycles;
        }

        private int act_rcon()
        {
            //
            System.Console.WriteLine("Unimplemented instruction: ret cond");
            return current.cycles;
        }

        private int act_rst()
        {
            //
            System.Console.WriteLine("Unimplemented instruction: RST {0} 0x{1,4:X4}", current.par1, 8 * current.par1);
            return current.cycles;
        }
    }
}
