using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ForecastCondition
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<String> places = new ObservableCollection<String>();        
        public MainPage()
        {
            this.InitializeComponent();
            PlacesCB();
            
            
        }
        public void PlacesCB()
        {
            places.Add("Porto");
            places.Add("Lisboa");
            places.Add("Coimbra");
            places.Add("Itajuba");
            places = new ObservableCollection<string>(places.OrderBy(i =>i));
        }

        DispatcherTimer timer= new DispatcherTimer();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            timer.Interval = new TimeSpan(0,0,1);
            timer.Tick += timer_Tick;
            timer.Start();            
        }

        private int TempFahtoCels(int Fah)
        {
            return (Convert.ToInt32((Fah - 32) * 0.5556));
        }

        private int TempCelstoFah(int Cels)
        {
            return (Convert.ToInt32((Cels*1.8)+32));
        }

        private void timer_Tick(object sender, object e)
        {
            DateTime time;
            time =DateTime.Now;

            int hours = time.Hour;
            int minutes = time.Minute;
            int seconds = time.Second;

            TextBlockHour.Text = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        public async void TempConverterCities()
        {
            if (this.Cities.SelectedItem != null)
            {
                string src = "https://api.previmeteo.com/4c217b99cd7343a71eebc29596429364/ig/api?weather=" + this.Cities.SelectedItem;
                //Service Access

                WebRequest request = WebRequest.Create(src);
                WebResponse response = await request.GetResponseAsync();
                XDocument doc = XDocument.Load(response.GetResponseStream());

                //Obtain current temperature
            
                foreach (XElement element in doc.Elements().Descendants("temp_c"))
                {
                    
                    if ((bool)this.Fahrenheit.IsChecked)
                    {
                        var fah = TempCelstoFah(int.Parse(element.Attribute("data").Value));
                        TextBlockWeather.Text = "Now: " + fah + "°F";
                    }
                    else
                        TextBlockWeather.Text = "Now: " + element.Attribute("data").Value + "°C";
                }

                //Obtain forecast conditions
                int idx = 0;
                foreach (XElement element in doc.Elements().Descendants("forecast_conditions"))
                {
                    var value = "";
                    int fahMax = int.Parse(element.Descendants("high").First().Attribute("data").Value);
                    int fahMin = int.Parse(element.Descendants("low").First().Attribute("data").Value);
                    var celsMax = TempFahtoCels(fahMax);
                    var celsMin = TempFahtoCels(fahMin);
                    if ((bool)this.Fahrenheit.IsChecked)
                        value = element.Descendants("day_of_week").First().Attribute("data").Value + " - Max " + fahMax + "°F - Min " + fahMin + "°F - " + element.Descendants("condition").First().Attribute("data").Value;
                    else
                        value = element.Descendants("day_of_week").First().Attribute("data").Value + " - Max " + celsMax + "°C - Min " + celsMin + "°C - " + element.Descendants("condition").First().Attribute("data").Value;
                    if (idx == 0)
                        TextBlockWeather1.Text = value;
                    if (idx == 1)
                        TextBlockWeather2.Text = value;
                    if (idx == 2)
                        TextBlockWeather3.Text = value;
                    if (idx == 3)
                        TextBlockWeather4.Text = value;

                    idx++;
                }
            }                        
        }

        private async void Cities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TempConverterCities();
        }

        private void Celsius_Checked(object sender, RoutedEventArgs e)
        {
            TempConverterCities();
        }

        private void Fahrenheit_Checked(object sender, RoutedEventArgs e)
        {
            TempConverterCities();
        }
    }
}
