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
            List<Pollution> pollutions = new List<Pollution>()
            {
                new Pollution{Location="1",LastUpdate=Convert.ToDateTime("12.04.2018"),Value=0.16},
                new Pollution{Location="2",LastUpdate=Convert.ToDateTime("12.04.2018"),Value=0.14},
                new Pollution{Location="1",LastUpdate=Convert.ToDateTime("13.04.2018"),Value=0.17},
                new Pollution{Location="2",LastUpdate=Convert.ToDateTime("14.04.2018"),Value=0.16},
                new Pollution{Location="1",LastUpdate=Convert.ToDateTime("14.04.2018"),Value=0.13}

            };
            var map = Mappers.Xy<DataModel>().X(p => p.Number).Y(p => p.Value);
            var dates = pollutions.Select(p => p.LastUpdate.ToShortDateString()).ToList();
            dates = dates.Distinct().ToList();
            foreach (var item in pollutions)
            {
                if (!cartesianChart1.Series.Any(p => p.Title == item.Location))
                {
                    cartesianChart1.Series.Add(
                        new LineSeries(map)
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

            cartesianChart1.AxisX.Add(new Axis { Labels = dates });
            //cartesianChart1.AxisY.Add(new Axis
            //{
            //    Title = "Sold Apps",
            //    LabelFormatter = value => value.ToString("N")
            //});
        }

        private void CartesianChart1OnDataClick(object sender, ChartPoint chartPoint)
        {
            MessageBox.Show("You clicked (" + chartPoint.X + "," + chartPoint.Y + ")");
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            //var result = await Get("US");
            //List<string> locations = new List<string>();
            //cartesianChart1.LegendLocation = LegendLocation.Right;
            //foreach (var item in result)
            //{
            //    cartesianChart1.Series.Add(new ColumnSeries
            //    {
            //        Title = item.Location,
            //        Values = new ChartValues<double> { item.Value },
            //    });

            //    locations.Add(item.LastUpdate.ToShortDateString());
            //}
            //cartesianChart1.AxisX.FirstOrDefault().Labels = locations;
            //cartesianChart1.AxisX.FirstOrDefault().ShowLabels = false;
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
    }
}
