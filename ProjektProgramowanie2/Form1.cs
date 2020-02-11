using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace ProjektProgramowanie2
{
    public partial class Form1 : Form
    {

        private static readonly HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        /* 
         *  Metoda jest wywoływana w momencie gdy zostanie naciśnięty Button1
         *  Sprawdza czy backgroundWorker1 jest zajęty i
         *  jeżeli jest wolny, uruchamia go i przekazuje jako argument
         *  tekst z searchBoxa.
         */

        private void Button1_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync(this.searchBox.Text);
            }
        }

        /* Metoda odpowiadająca za pracę backgroundWorkera
         * Wywoływana zostaje po tym jak zostanie wywołany kod
         * "backgroundWorker1.RunWorkerAsync(this.searchBox.Text);"
         * Pobiera jako argument nazwę miasta i tworzy przy jego pomocy
         * URI które zostaje użyte do połączenia się z API OpenWeatherMap
         * Następnie w sposób asynchroniczny pobiera z niego dane o pogodzie z danego miasta
         * używając do tego biblioteki HttpClient
         * Na koniec przekazuje wynik do e.Result
         * Dodatkowo po każdej ważniejszej operacji wywoływana jest metoda ReportProgress
         * aby dać ogólną informację na jakim etapie wykonania jest nasz wątek
         */

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            worker.ReportProgress(0);

            string baseUrl = "https://api.openweathermap.org/data/2.5/weather";
            string city = e.Argument.ToString();
            string apiKey = "INSERT YOUR KEY HERE";

            worker.ReportProgress(10);

            string url = baseUrl + "?q=" + city + "&appid=" + apiKey;
            Uri uri = new Uri(url);

            worker.ReportProgress(30);

            var requestCode = client.GetAsync(uri).Result.StatusCode;

            worker.ReportProgress(50);

            if (requestCode.ToString() == "OK")
            {
                e.Result = client.GetStringAsync(new Uri(url)).Result.ToString();
            }
            else
            {
                e.Result = "Wartość pola search jest niepoprawna";
            }
            worker.ReportProgress(100);
        }

        /*
         * Metoda ProgressChanged wykonuje się za każdym razem, gdy 
         * w DoWork wywoływane jest ReportProgress
         * Służy ona do aktualizacji progressBar1
         * */

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        /*RunWorkerCompleted wykonuje się w momencie gdy DoWork skończy pracę.
         * Pobiera ona e.Result i konwertuje jego zawartość do klasy String
         * Następnie sprawdza czy chcemy dane w postaci surowego JSONa
         * czy chcemy je w postaci sformatowanej.
         * W pierwszym przypadku po prostu przekazujemy result do outputBox.Text
         * W drugim używamy biblioteki Json.NET do sparsowania naszego JSONa
         * następnie korzystając z metod tej biblioteki wyciągamy z JSONa kilka 
         * przykładowych informacji i przekazujemy je do outputBox.Text
         * */

        private void backgroundWorker1_RunWorkerCompleted(
           object sender, RunWorkerCompletedEventArgs e)
        {
            string result = e.Result.ToString();

            if(result.Equals("Wartość pola search jest niepoprawna") || rawRadioButton.Checked)
            {
                this.outputBox.Text = e.Result.ToString();
            }
            else
            {
                JObject weather = JObject.Parse(result);

                JObject main = weather.Value<JObject>("main");

                String city = weather.Value<String>("name");

                double temp = main.Value<double>("temp") - 273.15; //API podaje temperaturę w Kelvinach
                String pressure = main.Value<String>("pressure");

                this.outputBox.Text = "City: " + city + "\n"
                    + "Temperature: " + temp + " C \n"
                    + "Pressure: " + pressure + "\n";
            }

        }

        /*
         * Zamyka formatkę po wybraniu opcji Exit z MenuStrip
         * */

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
