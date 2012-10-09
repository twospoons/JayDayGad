using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace DumpPEFile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var bytes = File.ReadAllBytes(@"C:\dev\JayDayGad\JayDayGad\JayDayService\bin\Debug\le\JayDayService.pe");
            var dt = "new byte [] { ";
            foreach(var t in bytes) 
            {
                dt += t.ToString() + ", ";
            }
            dt += " }";
            Debug.Print(dt);
        }
    }
}
