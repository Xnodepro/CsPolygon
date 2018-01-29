using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSMONEY
{
    public partial class Form2 : Form
    {
        int maxCountItems = 10000;
        string SiteName = "";
        public Form2()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new System.Threading.Thread(delegate () {
                try
                {
                    AutoRefresh AR = new AutoRefresh(Convert.ToDouble(textBox2.Text.Replace(".", ",")), false, Convert.ToDouble(textBox3.Text.Replace(".", ",")), SiteName, maxCountItems);
                    AR.Login();
                }
                catch (Exception ex) { Program.Mess.Enqueue($"Ошибка 8{ex.Message}"); }
            }).Start();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            new System.Threading.Thread(delegate () {
                try
                {
                    AutoRefresh AR = new AutoRefresh(Convert.ToDouble(textBox2.Text.Replace(".", ",")), true, Convert.ToDouble(textBox3.Text.Replace(".", ",")), SiteName, maxCountItems);
                    AR.Login();
                }
                catch (Exception ex) { Program.Mess.Enqueue($"Ошибка 7{ex.Message}"); }
            }).Start();

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                maxCountItems = Convert.ToInt32(textBox4.Text);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SiteName = comboBox1.Text;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Program.MessAutoRefresh.Count != 0)
            {
                for (int i = 0; i < Program.MessAutoRefresh.Count; i++)
                {
                    textBox1.Text = Program.MessAutoRefresh.Dequeue() + Environment.NewLine + textBox1.Text;
                }
            }
        }
    }
}
