using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;


namespace NexTos.Disassembler
{
    struct instr
    {
        public String Opcode { get; set; }
        public string Mnemo { get; set; }
        public byte Params { get; set; }
        public String Par1 { get; set; }
        public String Par2 { get; set; }
        public String Comm { get; set; }
        public byte cycl { get; set; }

    }

    enum regs1 : byte
    {
        B, C, D, E, H, L, M, A
    }

    enum regs2 : byte
    {
        BC, DE, HL, SP
    }

    enum regs3 : byte
    {
        BC, DE, HL, PSW
    }

    enum cond1 : byte
    {
        NZ, Z, NC, C, PO, PE, P, M
    }

    enum cmds1 : byte
    {
        NOP, LXI, STA, INX, RSV, DAD, LDA, DCX
    }

    enum cmds2 : byte
    {
        RLC, RRC, RAL, RAR, DAA, CMA, STC, CMC
    }

    enum cmds3 : byte
    {
        INR, DCR, MVI
    }

    enum cmds4 : byte
    {
        ADD, ADC, SUB, SBB, ANA, XRA, ORA, CMP
    }

    enum cmds5 : byte
    {
        ADI, ACI, SUI, SBI, ANI, XRI, ORI, CPI
    }

    enum cmds6 : byte
    {
        R, J, C
    }

    struct mem_byte
    {
        public byte entry { get; set; }
        public mem_state state { get; set; }
    }

    enum mem_state : byte
    {
        None = 0,
        Loaded = 1,
        Changed = 2,
        Labled = 4,
        Unused = 8,
        Code = 16,
        Data = 32,
        Stack = 64,
        Used = 128
    }

    struct lbl_string
    {
        public lbl_string(String entry, lbl_state state)
        {
            this.entry = entry;
            this.state = state;
        }
        public String entry { get; set; }
        public lbl_state state { get; set; }
    }

    enum lbl_state : byte
    {
        None = 0,
        Loaded = 1,
        Cond = 2,
        Call = 4,
        Jump = 8,
        Code = 16,
        Data = 32,
        Stack = 64,
        Port = 128
    }

    public partial class Form1 : Form
    {
        private int file_length;
        private ushort ld_addr, st_addr, pt_addr;
        private const int mem_space = (64 * 1024);

        private FileStream inp_file;
        private FileStream oup_file;

        private mem_byte[] memory;

        private Dictionary<ushort, lbl_string> labels;

        public Form1()
        {
            InitializeComponent();
            ld_addr = st_addr = pt_addr = 0;
            file_length = 0;
            labels = new Dictionary<ushort, lbl_string>();
            memory = new mem_byte[mem_space];
            for (int i = 0; i < mem_space; i++)
            {
                memory[i].entry = 0xFF;
                memory[i].state = mem_state.None;
            }
        }

        private String param_d8(ref instr curr_instr)
        {
            memory[pt_addr].state = mem_state.Code;
            byte par = memory[pt_addr++].entry;

            curr_instr.Opcode += String.Format(" {0,2:X2}", par);
            curr_instr.Comm = String.Format("\t; d8  = {0,-6}\'{1}\'", par, (char)par);

            return String.Format("0x{0,2:X2}", par);

        }

        private String param_d16(ref instr curr_instr)
        {
            memory[pt_addr].state = mem_state.Code;
            byte par_l = memory[pt_addr++].entry;
            memory[pt_addr].state = mem_state.Code;
            byte par_h = memory[pt_addr++].entry;
            ushort d16 = (ushort)(par_h * 256 + par_l);

            curr_instr.Opcode += String.Format(" {0,2:X2} {1,2:X2}", par_l, par_h);
            curr_instr.Comm = String.Format("; d16 = {0}", d16);

            return String.Format("0x{0,2:X2}{1,2:X2}", par_h, par_l);
        }

        private String param_a16(ref instr curr_instr, lbl_state label_type = lbl_state.None)
        {
            memory[pt_addr].state = mem_state.Code;
            byte par_l = memory[pt_addr++].entry;
            memory[pt_addr].state = mem_state.Code;
            byte par_h = memory[pt_addr++].entry;
            ushort a16 = (ushort)(par_h * 256 + par_l);

            String tmp_str;
            if (!labels.ContainsKey(a16))
            {
                switch (label_type)
                {
                    case (lbl_state.Call):
                        tmp_str = String.Format("call_{0,4:X4}", a16);
                        break;
                    case (lbl_state.Jump):
                        tmp_str = String.Format("jump_{0,4:X4}", a16);
                        break;
                    case (lbl_state.Cond):
                        tmp_str = String.Format("cond_{0,4:X4}", a16);
                        break;
                    case (lbl_state.Data):
                        tmp_str = String.Format("loc_{0,4:X4}", a16);
                        break;
                    case (lbl_state.Stack):
                        tmp_str = String.Format("stck_{0,4:X4}", a16);
                        break;
                    default:
                        tmp_str = "";
                        break;
                }

                labels.Add(a16, new lbl_string(tmp_str, label_type));
                listBox3.Items.Add(String.Format("0x{0,4:X4}\t{1}", a16, tmp_str));
            }
            else
            {
                tmp_str = labels[a16].entry;
            }

            curr_instr.Opcode += String.Format(" {0,2:X2} {1,2:X2}", par_l, par_h);
            curr_instr.Comm = String.Format("; a16 = 0x{0,4:x4} ({0})", a16);

            return tmp_str;
        }

        private void add_labels()
        {
            labels.Add(st_addr, new lbl_string("Start", lbl_state.Code));

            labels.Add(0x8000, new lbl_string("dev_ppi_kbd_1", lbl_state.Port));
            labels.Add(0x8001, new lbl_string("dev_ppi_kbd_2", lbl_state.Port));
            labels.Add(0x8002, new lbl_string("dev_ppi_kbd_3", lbl_state.Port));
            labels.Add(0x8003, new lbl_string("dev_ppi_kbd_4", lbl_state.Port));

            labels.Add(0xA000, new lbl_string("dev_ppi_int_1", lbl_state.Port));
            labels.Add(0xA001, new lbl_string("dev_ppi_int_2", lbl_state.Port));
            labels.Add(0xA002, new lbl_string("dev_ppi_int_3", lbl_state.Port));
            labels.Add(0xA003, new lbl_string("dev_ppi_int_4", lbl_state.Port));

            labels.Add(0xC000, new lbl_string("dev_crt_1", lbl_state.Port));
            labels.Add(0xC001, new lbl_string("dev_crt_2", lbl_state.Port));
            labels.Add(0xC002, new lbl_string("dev_crt_3", lbl_state.Port));
            labels.Add(0xC003, new lbl_string("dev_crt_4", lbl_state.Port));

            labels.Add(0xE000, new lbl_string("dev_dma_1", lbl_state.Port));
            labels.Add(0xE001, new lbl_string("dev_dma_2", lbl_state.Port));
            labels.Add(0xE002, new lbl_string("dev_dma_3", lbl_state.Port));
            labels.Add(0xE003, new lbl_string("dev_dma_4", lbl_state.Port));

            labels.Add(0x7600, new lbl_string("var_scrn_chr_addr", lbl_state.Data));
            labels.Add(0x7602, new lbl_string("var_curs_pos", lbl_state.Data));

            labels.Add(0x7631, new lbl_string("usr_mem_top", lbl_state.Data));

            labels.Add(0xFF5A, new lbl_string("str_head", lbl_state.Data));


            labels.Add(0xF803, new lbl_string("tbl_01_inp_kbrd_chr", lbl_state.Code));
            labels.Add(0xF806, new lbl_string("tbl_02_get_tape_chr", lbl_state.Code));
            labels.Add(0xF809, new lbl_string("tbl_03_out_disp_chr", lbl_state.Code));
            labels.Add(0xF80C, new lbl_string("tbl_04_put_tape_chr", lbl_state.Code));
            labels.Add(0xF80F, new lbl_string("tbl_05_put_prnt_chr", lbl_state.Code));
            labels.Add(0xF812, new lbl_string("tbl_06_chk_kbrd", lbl_state.Code));
            labels.Add(0xF815, new lbl_string("tbl_07_out_disp_hex", lbl_state.Code));
            labels.Add(0xF818, new lbl_string("tbl_08_out_disp_str", lbl_state.Code));
            labels.Add(0xF81B, new lbl_string("tbl_09_chk_scancode", lbl_state.Code));
            labels.Add(0xF81E, new lbl_string("tbl_10_get_curs_pos", lbl_state.Code));
            labels.Add(0xF821, new lbl_string("tbl_11_get_scrn_chr", lbl_state.Code));
            labels.Add(0xF824, new lbl_string("tbl_12_get_tape_blk", lbl_state.Code));
            labels.Add(0xF827, new lbl_string("tbl_13_put_tape_blk", lbl_state.Code));
            labels.Add(0xF82A, new lbl_string("tbl_14_get_chck_sum", lbl_state.Code));
            labels.Add(0xF82D, new lbl_string("tbl_15_ena_scrn", lbl_state.Code));
            labels.Add(0xF830, new lbl_string("tbl_16_get_top", lbl_state.Code));
            labels.Add(0xF833, new lbl_string("tbl_17_set_top", lbl_state.Code));

            labels.Add(0xF836, new lbl_string("cmd_00_Boot", lbl_state.Code));
            labels.Add(0xFE63, new lbl_string("cmd_01_inp_kbrd_chr", lbl_state.Code));
            labels.Add(0xFB98, new lbl_string("cmd_02_get_tape_chr", lbl_state.Code));
            labels.Add(0xFCBA, new lbl_string("cmd_03_out_disp_chr", lbl_state.Code));
            labels.Add(0xFC46, new lbl_string("cmd_04_put_tape_chr", lbl_state.Code));
//          labels.Add(0xFCBA, new lbl_string("cmd_05_put_prnt_chr", lbl_state.Code));
            labels.Add(0xFE01, new lbl_string("cmd_06_chk_kbrd", lbl_state.Code));
            labels.Add(0xFCA5, new lbl_string("cmd_07_out_disp_hex", lbl_state.Code));
            labels.Add(0xF922, new lbl_string("cmd_08_out_disp_str", lbl_state.Code));
            labels.Add(0xFE72, new lbl_string("cmd_09_chk_scancode", lbl_state.Code));
            labels.Add(0xFA7B, new lbl_string("cmd_10_get_curs_pos", lbl_state.Code));
            labels.Add(0xFA7F, new lbl_string("cmd_11_get_scrn_chr", lbl_state.Code));
            labels.Add(0xFAB6, new lbl_string("cmd_12_get_tape_blk", lbl_state.Code));
            labels.Add(0xFB49, new lbl_string("cmd_13_put_tape_blk", lbl_state.Code));
            labels.Add(0xFB16, new lbl_string("cmd_14_get_chck_sum", lbl_state.Code));
            labels.Add(0xFACE, new lbl_string("cmd_15_ena_scrn", lbl_state.Code));
            labels.Add(0xFF52, new lbl_string("cmd_16_get_top", lbl_state.Code));
            labels.Add(0xFF56, new lbl_string("cmd_17_set_top", lbl_state.Code));

            labels.Add(0xFF73, new lbl_string("cmd_U_FF73", lbl_state.Code));
            labels.Add(0xFFD3, new lbl_string("cmd_X_FFD3", lbl_state.Code));
            labels.Add(0xF9C5, new lbl_string("cmd_D_F9C5", lbl_state.Code));
            labels.Add(0xF9D7, new lbl_string("cmd_C_F9D7", lbl_state.Code));
            labels.Add(0xF9ED, new lbl_string("cmd_F_F9ED", lbl_state.Code));
            labels.Add(0xF9F4, new lbl_string("cmd_S_F9F4", lbl_state.Code));
            labels.Add(0xF9FF, new lbl_string("cmd_T_F9FF", lbl_state.Code));
            labels.Add(0xFA26, new lbl_string("cmd_M_FA26", lbl_state.Code));
            labels.Add(0xFA3F, new lbl_string("cmd_G_FA3F", lbl_state.Code));
            labels.Add(0xFA86, new lbl_string("cmd_I_FA86", lbl_state.Code));
            labels.Add(0xFB2D, new lbl_string("cmd_O_FB2D", lbl_state.Code));
            labels.Add(0xFA08, new lbl_string("cmd_L_FA08", lbl_state.Code));
            labels.Add(0xFA68, new lbl_string("cmd_R_FA68", lbl_state.Code));
        }

        private void dasm(bool act = false)
        {
            while ((pt_addr <= (mem_space - 1)) && (memory[pt_addr].state > mem_state.None))
            {
                ushort cmd_addr = pt_addr;
                StringBuilder dsm_line = new StringBuilder(36);
                dsm_line.AppendFormat("0x{0,4:X4}:\t ", pt_addr);
                memory[pt_addr].state = mem_state.Code;
                byte cmd = memory[pt_addr++].entry;

                instr curr_instr = new instr();
                curr_instr.Opcode = String.Format("{0,2:X2}", cmd);
                curr_instr.Mnemo = "UNK";
                curr_instr.Par1 = "";
                curr_instr.Par2 = "";

                byte cmd_tmp1 = (byte)(cmd & 0b00000011);
                byte cmd_tmp2 = (byte)(cmd & 0b00000111);
                byte cmd_tmp3 = (byte)(cmd & 0b00001111);
                byte cmd_tmp4 = (byte)((cmd & 0b00110000) >> 4);
                byte cmd_tmp8 = (byte)((cmd & 0b00111000) >> 3);

                switch ((cmd & 0b11000000) >> 6)
                {
                    case 0:     // codes 0x00 - 0x3F
                        if ((cmd & 0b00000100) == 0) // 0, 1, 2, 3, 8, 9, A, B
                        {
                            curr_instr.Mnemo = ((cmds1)(cmd_tmp1 + ((cmd & 0b00001000) >> 1))).ToString();
                            switch (cmd_tmp1)
                            {
                                case 0:
                                    if (cmd != 0)
                                    {
                                        curr_instr.Mnemo = "*NOP";
                                        curr_instr.Comm = "\t; Undocumented!";
                                    }
                                    break;

                                case 1:
                                    curr_instr.Par1 = ((regs2)cmd_tmp4).ToString(); //lxi
                                    if (cmd_tmp4 == 3) curr_instr.Par2 = param_a16(ref curr_instr, lbl_state.Stack);  //lxi sp, a16
                                    else curr_instr.Par2 = param_d16(ref curr_instr);  //par2 = d16
                                    break;

                                case 2:
                                    if ((cmd_tmp4 & 0b00000010) == 0)
                                    {
                                        curr_instr.Mnemo += "X"; //stax
                                        curr_instr.Par1 = ((regs2)cmd_tmp4).ToString();
                                    }
                                    else
                                    {
                                        if (cmd == 0x22) curr_instr.Mnemo = "SHLD";
                                        if (cmd == 0x2A) curr_instr.Mnemo = "LHLD";
                                        curr_instr.Par1 = param_a16(ref curr_instr, lbl_state.Data);
                                    }
                                    break;

                                case 3:
                                    curr_instr.Par1 = ((regs2)cmd_tmp4).ToString();
                                    break;
                            }
                        }
                        else
                        {
                            if ((cmd_tmp1) == 3)
                            {
                                curr_instr.Mnemo = ((cmds2)cmd_tmp8).ToString(); //rlc..
                            }
                            else
                            {
                                curr_instr.Mnemo = ((cmds3)(cmd_tmp1)).ToString(); //inr-dcr
                                curr_instr.Par1 = ((regs1)cmd_tmp8).ToString();
                                if ((cmd_tmp1) == 2) curr_instr.Par2 = param_d8(ref curr_instr); //mvi
                            }

                        }
                        break;

                    case 1:     // codes 0x40 - 0x7F
                        if (cmd == 0x76) curr_instr.Mnemo = "HLT";
                        else
                        {
                            curr_instr.Mnemo = "MOV";
                            curr_instr.Par1 = ((regs1)cmd_tmp8).ToString();
                            curr_instr.Par2 = ((regs1)cmd_tmp2).ToString();
                        }
                        break;

                    case 2:     // codes 0x80 - 0xBF
                        curr_instr.Par1 = ((regs1)cmd_tmp2).ToString();
                        curr_instr.Mnemo = ((cmds4)cmd_tmp8).ToString();
                        break;

                    case 3:     // codes 0xC0 - 0xFF
                        if ((cmd & 0b00000001) == 0)
                        {
                            if (cmd_tmp2 == 6)      //adi, aci, sui, sbi, ani, xri, ori, cpi
                            {
                                curr_instr.Mnemo = ((cmds5)cmd_tmp8).ToString();
                                curr_instr.Par1 = param_d8(ref curr_instr);
                            }
                            else       // r, j, c with conditions
                            {
                                curr_instr.Mnemo = ((cmds6)(cmd_tmp2 >> 1)).ToString() + ((cond1)cmd_tmp8).ToString();
                                if (cmd_tmp2 != 0)      // j, c with conditions
                                {
                                    curr_instr.Par1 = param_a16(ref curr_instr, lbl_state.Cond);
                                }
                            }
                        }
                        else
                        {
                            if (cmd_tmp3 == 0x01)       // 01 pop
                            {
                                curr_instr.Mnemo = "POP";
                                curr_instr.Par1 = ((regs3)cmd_tmp4).ToString();
                            }

                            if (cmd_tmp2 == 0x03)       // 03, 0B
                            {
                                if (cmd == 0xC3)
                                {
                                    curr_instr.Mnemo = "JMP";
                                    curr_instr.Par1 = param_a16(ref curr_instr, lbl_state.Jump);
                                }
                                if (cmd == 0xCB)
                                {
                                    curr_instr.Mnemo = "*JMP";
                                    curr_instr.Comm = "\t; Undocumented!";
                                    curr_instr.Par1 = param_a16(ref curr_instr, lbl_state.Jump);
                                }

                                if (cmd == 0xD3)
                                {
                                    curr_instr.Mnemo = "OUT";
                                    curr_instr.Par1 = param_d8(ref curr_instr);
                                }
                                if (cmd == 0xDB)
                                {
                                    curr_instr.Mnemo = "IN";
                                    curr_instr.Par1 = param_d8(ref curr_instr);
                                }

                                if (cmd == 0xE3)
                                {
                                    curr_instr.Mnemo = "XTHL";
                                }

                                if (cmd == 0xEB)
                                {
                                    curr_instr.Mnemo = "XCHG";
                                }

                                if (cmd == 0xF3)
                                {
                                    curr_instr.Mnemo = "DI";
                                }

                                if (cmd == 0xFB)
                                {
                                    curr_instr.Mnemo = "EI";
                                }
                            }

                            if (cmd_tmp3 == 0x05)       // 05 push
                            {
                                curr_instr.Mnemo = "PUSH";
                                curr_instr.Par1 = ((regs3)cmd_tmp4).ToString();
                            }


                            if (cmd_tmp2 == 0x07)       //07, 0F rst
                            {
                                curr_instr.Mnemo = "RST";
                                curr_instr.Par1 = cmd_tmp8.ToString();
                            }

                            if (cmd_tmp3 == 0x09)       // 09 ret
                            {
                                if (cmd == 0xC9) curr_instr.Mnemo = "RET";
                                if (cmd == 0xD9)
                                {
                                    curr_instr.Mnemo = "*RET";
                                    curr_instr.Comm = "\t; Undocumented!";
                                }
                                if (cmd == 0xE9) curr_instr.Mnemo = "PCHL";
                                if (cmd == 0xF9) curr_instr.Mnemo = "SPHL";
                            }


                            if (cmd_tmp3 == 0x0D)       // 0D call
                            {
                                if (cmd == 0xCD) curr_instr.Mnemo = "CALL";
                                else
                                {
                                    curr_instr.Mnemo = "*CALL";
                                    curr_instr.Comm = "\t; Undocumented!";
                                }
                                curr_instr.Par1 = param_a16(ref curr_instr, lbl_state.Call);
                            }

                        }
                        break;
                }
                dsm_line.AppendFormat("{0,-8}   {1}", curr_instr.Opcode, curr_instr.Mnemo);
                if (curr_instr.Par1 != "") dsm_line.AppendFormat(" {0}", curr_instr.Par1);
                if (curr_instr.Par2 != "") dsm_line.AppendFormat(", {0}", curr_instr.Par2);
                if (curr_instr.Comm != "") dsm_line.AppendFormat("\t{0}", curr_instr.Comm);

                if (act)
                {
                    if (labels.ContainsKey(cmd_addr)) listBox2.Items.Add(String.Format("{0}:", labels[cmd_addr].entry));
                    listBox2.Items.Add(dsm_line.ToString());
                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if ((0 != saveFileDialog1.ShowDialog()) && (saveFileDialog1.FileName != ""))
            {
                oup_file = File.OpenWrite(saveFileDialog1.FileName.ToString());
                StreamWriter sr = new StreamWriter(oup_file, Encoding.Default, 1024);
                sr.WriteLine("= i8080 disassembler =");
                sr.WriteLine(String.Format(" - Processed file {0}", inp_file.Name.ToString()));
                sr.WriteLine(String.Format("   File size = {0} bytes.", file_length));

                for (int i = 0; i < file_length; i += 16)
                {
                    if (i%256 == 0) sr.WriteLine();
                    StringBuilder line = new StringBuilder(36);
                    line.AppendFormat("0x{0,4:X4}:  ", (ld_addr + i));
                    for (int y = 0; y < 16; y++)
                    {
                        line.AppendFormat("{0,2:X2} ", memory[(ld_addr + i + y)].entry);
                        if (y == 7) line.Append(' ');
                    }
                    sr.WriteLine(line);
                }
                sr.WriteLine();
                foreach (KeyValuePair<ushort, lbl_string> curr_lbl in labels)
                {
                    ushort lbl_adr = curr_lbl.Key;
                    if (lbl_adr < st_addr) sr.WriteLine(String.Format("{0}: 0x{1,4:X4}", curr_lbl.Value.entry, lbl_adr));
                }

                for (int i = 0; i < listBox2.Items.Count; i++)
                {
                    sr.WriteLine(listBox2.Items[i]);
                }
                sr.Flush();
                oup_file.Flush();
                oup_file.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int CountFields = 9;
            int CountRows = 6;

            String[, ] arr = { { "0x0000", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0" },
                               { "0x0000", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0" },
                               { "0x0000", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0" },
                               { "0x0000", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0" },
                               { "0x0000", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0" },
                               { "0x0000", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0", "0x0" },
                             };

            DataTable dt = new DataTable();
            for (int i = 0; i < CountFields; i++)
            {
                dt.Columns.Add(i.ToString());
            }

            for (int i = 0; i < CountRows; i++)
            {
                DataRow row = dt.NewRow();

                for (int j = 0; j < CountFields; j++)
                {
                    row[j] = arr[i, j];
                }

                dt.Rows.Add(row);
            }
            dataGridView1.DataSource = dt;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Bitmap mem_map = new Bitmap(800, 20);
            Graphics flagGraphics = Graphics.FromImage(mem_map);

                flagGraphics.FillRectangle(Brushes.Red, 0, 0, 512, 10);
                flagGraphics.FillRectangle(Brushes.White, 0, 11, 512, 10);

            pictureBox1.Image = mem_map;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            labels.Clear();
            st_addr = UInt16.Parse(textBox2.Text, System.Globalization.NumberStyles.HexNumber);

            add_labels();

            pt_addr = st_addr;
            dasm(false);
            pt_addr = st_addr;
            dasm(true);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            ld_addr = UInt16.Parse(textBox1.Text, System.Globalization.NumberStyles.HexNumber);

            if ((0 != openFileDialog1.ShowDialog()) && (openFileDialog1.FileName != ""))
            {
                inp_file = File.OpenRead(openFileDialog1.FileName.ToString());
                file_length = (int)inp_file.Length;
                for (int i = 0; i<file_length; i++)
                {
                    memory[ld_addr + i].entry = (byte)inp_file.ReadByte();
                    memory[ld_addr + i].state = mem_state.Loaded;
                }
                inp_file.Close();

                label4.Text = file_length.ToString();

                for (int i = 0; i < file_length; i+=8)
                {
                    StringBuilder line = new StringBuilder(36);
                    line.AppendFormat("0x{0,4:X4}: ", (ld_addr + i));
                    for (int y = 0; y < 8; y++)
                    {
                        if (memory[(ld_addr + i + y)].state > mem_state.None)
                        {
                            line.AppendFormat("{0,2:X2} ", memory[(ld_addr + i + y)].entry); 
                        }
                    }
                    listBox1.Items.Add(line.ToString());
                }
            }

        }
    }
}
