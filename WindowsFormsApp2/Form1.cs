using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            


        }
        public class ComparerDate : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (Convert.ToDateTime(x) == Convert.ToDateTime(y))
                    return 0;
                if (Convert.ToDateTime(x) < Convert.ToDateTime(y))
                    return -1;
                return 1;
            }
        }
        private void CartesianChart1OnDataClick(object sender, ChartPoint chartPoint)
        {
            MessageBox.Show("You clicked (" + chartPoint.X + "," + chartPoint.Y + ")");
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            
            var result = await Get("US");
            Init(result.ToList());
        }

        private static string url = $"https://api.openaq.org/v1/latest?limit=10&city={"St. Louis"}&parameter=o3";
        private static readonly HttpClient client = new HttpClient();
        public async static Task<IEnumerable<Pollution>> Get(string country)
        {
            List<Pollution> pollutions = new List<Pollution>();
            var response = await client.GetAsync(url + "&country=" + country);
            var responseString = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseString);
            var token = json["results"];
            foreach (var item in token.Children())
            {
                var pollution = JsonConvert.DeserializeObject<Pollution>(item["measurements"].First.ToString());
                pollution.City = item["city"].ToString();
                pollution.Location = item["location"].ToString();
                pollutions.Add(pollution);
            }
            return pollutions;
        }

        public void Init(List<Pollution> pollutions)
        {
            //List<Pollution> pollutions = new List<Pollution>()
            //{
            //    new Pollution{Location="ALTON",LastUpdate=Convert.ToDateTime("11.10.2018"),Value=0.017},
            //    new Pollution{Location="ARNOLD",LastUpdate=Convert.ToDateTime("11.10.2018"),Value=0.011},
            //    new Pollution{Location="Alhambra",LastUpdate=Convert.ToDateTime("11.10.2018"),Value=0.014},
            //    new Pollution{Location="BLAIR STREET",LastUpdate=Convert.ToDateTime("11.10.2018"),Value=0.015},
            //    new Pollution{Location="East St.Louis",LastUpdate=Convert.ToDateTime("11.10.2018"),Value=0.016},
            //    new Pollution{Location="Foley",LastUpdate=Convert.ToDateTime("07.11.2016"),Value=0.046},
            //    new Pollution{Location="Jorey",LastUpdate=Convert.ToDateTime("20.07.2017"),Value=0.05},
            //    new Pollution{Location="MARVYL",LastUpdate=Convert.ToDateTime("11.10.2018"),Value=0.013},
            //    new Pollution{Location="Maryland",LastUpdate=Convert.ToDateTime("11.10.2018"),Value=0.017},
            //    new Pollution{Location="NYLWOOD",LastUpdate=Convert.ToDateTime("11.10.2018"),Value=0.018}

            //};
            var map = Mappers.Xy<DataModel>().X(p => p.Number).Y(p => p.Value);
            var dates = pollutions.Select(p => p.LastUpdate.ToShortDateString()).ToList();
            dates = dates.Distinct().ToList();
            dates.Sort(new ComparerDate());
            foreach (var item in pollutions)
            {
                if (!cartesianChart1.Series.Any(p => p.Title == item.Location))
                {
                    cartesianChart1.Series.Add(
                        new ColumnSeries(map)
                        {
                            Title = item.Location,
                            Values = new ChartValues<DataModel>
                            {
                                new DataModel
                                {
                                    Value = item.Value,
                                    Number = dates.FindIndex(p => p == item.LastUpdate.ToShortDateString())
                                }
                            }
                        }
                    );
                }
                else
                {
                    cartesianChart1.Series.FirstOrDefault(p => p.Title == item.Location).Values.Add(

                         new DataModel { Value = item.Value, Number = dates.FindIndex(p => p == item.LastUpdate.ToShortDateString()) });
                }
            }
            cartesianChart1.AxisX.Clear();
            cartesianChart1.AxisX.Add(new Axis { Labels = dates });
            cartesianChart1.LegendLocation = LegendLocation.Right;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
