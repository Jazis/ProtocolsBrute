using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace ProtocolsBrute
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try { File.ReadAllLines("settings.ini"); } 
            catch 
            { 
                MessageBox.Show("Settings file not found! Don't worry, We make a new file");
                FileInfo fileFE = new FileInfo("settings.ini");
                FileStream file = fileFE.Create();
                file.Close();
            }
            try { textBox1.Text = File.ReadAllLines("settings.ini")[0].Split('=')[1]; } catch { }
            try { textBox2.Text = File.ReadAllLines("settings.ini")[1].Split('=')[1]; } catch { }
            try { textBox3.Text = File.ReadAllLines("settings.ini")[2].Split('=')[1]; } catch { }
            try { textBox4.Text = File.ReadAllLines("settings.ini")[3].Split('=')[1]; } catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            File.WriteAllText("Settings.ini", $"[SSH]={textBox1.Text}\n");
            File.AppendAllText("Settings.ini", $"[MYSQL]={textBox2.Text}\n");
            File.AppendAllText("Settings.ini", $"[HTTP-auth]={textBox3.Text}\n");
            File.AppendAllText("Settings.ini", $"[FTP]={textBox4.Text}\n");
            MessageBox.Show("Saved into settings.ini");
        }
    }
}
