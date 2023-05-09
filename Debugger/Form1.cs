using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NexTos.An8bitCPU;

namespace NexTos.Debugger
{


    enum dbg_status : int
    {
        idle = 0,
        busy = 1,
        actv = 2,
        step = 3
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            addr = 0;
            tmr = 0;
            tm0 = 100.0;
            state = 0;
            InitializeComponent();
        }

   //   private Some8bitCPU myCPU = new CPU_i80();

        private int addr;
        private double tmr;
        private double tm0;
        private dbg_status state;

        private void sys_tick()
        {
                addr++;
                textBox14.Text = string.Format("0x{0,4:X4}", addr);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tmr += (double)trackBar1.Value / 2.5 + 0.3;

            if (tmr >= tm0)
            {
                tmr = 0;
                sys_tick();
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (state != dbg_status.actv)
            {
                button1.Text = "Stop";
                state = dbg_status.actv;
                timer1.Enabled = true;
            }
            else
            {
                button1.Text = "Run";
                state = dbg_status.idle;
                timer1.Enabled = false;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            sys_tick();
        }

    }
}
