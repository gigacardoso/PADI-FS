using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PuppetMaster
{
    public partial class Form1 : Form
    {
        TextReader tr;
        ScriptInterpreter SI;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SI = new ScriptInterpreter();
            String location = file_location.Text;
            tr = new StreamReader(@location);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (tr.Peek() != -1)
            {
                String command = tr.ReadLine();
                SI.execute(command);
            }
            Console.WriteLine("Script Ended!");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (tr.Peek() == -1) Console.WriteLine("There are no more steps in the script!");
            String command = tr.ReadLine();
            SI.execute(command);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void okaymethod_Click(object sender, EventArgs e)
        {

        }
    }
}
