using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using BuildAzure.IoT.Adafruit.BME280;

using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace IoTWeatherHubDevice
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BME280Sensor _sensor;

        static DeviceClient deviceClient;
        static string iotHubUri = "CHANGE";
        static string deviceKey = "CHANGE";
        static string deviceId = "CHANGE";
        public MainPage()
        {
            //this.InitializeComponent();
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Mqtt);
            _sensor = new BME280Sensor();
            DeviceToCloudMessage();
        }

        private async void DeviceToCloudMessage()
        {
            await _sensor.Initialize();
            float temperature = 0.00f;
            float humidity = 0.00f;
            while (true)
            {
                temperature = await _sensor.ReadTemperature();
                humidity = await _sensor.ReadHumidity();
                var sensorData = new
                {
                    date = String.Format("{0}, {1}, {2}",
                                         DateTime.Now.ToLocalTime().TimeOfDay.Hours,
                                         DateTime.Now.ToLocalTime().TimeOfDay.Minutes,
                                         DateTime.Now.ToLocalTime().TimeOfDay.Seconds),
                    temperature = Math.Round(temperature, 2),
                    humidity = Math.Round(humidity, 2)
                };
                var messageString = JsonConvert.SerializeObject(sensorData);
                var message = new Message(byteArray: Encoding.ASCII.GetBytes(messageString));
                await deviceClient.SendEventAsync(message);
                Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                Task.Delay(10000).Wait(); // 11hours running not to exceed the limit of 8000 messages 
            }
        }
    }
}
