using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLightSymulator
{
    class Program
    {
        //Ostateczna wersja!!!
        private static string deviceConnectionString = "HostName=iot-hub-projekt.azure-devices.net;DeviceId=latarnia-ostateczny;SharedAccessKey=N1TXflG5fK0nt2FN/MbQyIKufAWRSzc84DkcH4gl3Dk=";
        private readonly static string s_myDeviceId = "latarnia-ostateczny";
        private readonly static string s_iotHubUri = "iot-hub-projekt.azure-devices.net";
        private readonly static string s_deviceKey = "OndUPEgDOI/VXYkzHx0oJQ3CRkaUi21JQDXc/HxS2t4=";
        private static DeviceClient deviceClient = null;

        private static SmartLightDevice device;

        static async Task Main(string[] args)
        {
            try
            {
                 deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);
                //  deviceClient = DeviceClient.Create(s_iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(s_myDeviceId, s_deviceKey), TransportType.Mqtt);
                await deviceClient.OpenAsync();

                await SetupDevice();


                await SendMessages();

               


            }
            catch (AggregateException ex)
            {
                foreach(Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error in sample: {0}", exception);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }

            Console.WriteLine("All messages sent, press enter to exit...");
            Console.WriteLine();
            Console.WriteLine("Closing, please wait...");
            await deviceClient.CloseAsync();
        }

        private static async Task SendMessages()
        {
            Console.WriteLine("Device sending messages to IoThub...\n");
           // device.SimulateMalfuncion = true;
            for(int i=0;0<5;i++)
            {
                for(int id = 0;id<20;id++)
                {
                    await SendSingleMessage(id);
                }
                for(int id = 19;id >=0;id--)
                {
                    await SendSingleMessage(id);
                }
            }
        }

        private static async Task SendSingleMessage(int id)
        {

           // string levelValue;
            device.Simulate(id);
/*
           if (device.PowerConsumption >= 60)
            {
                levelValue = "critical";
                Console.WriteLine("critical");
                
            }
           else
            {
                levelValue = "normal";
                
            }
            */
            var data = new
            {
                
                state = device.LightOn,
                power = device.PowerConsumption,
                light = device.LightSensorReading
            };
           
            var dataString = JsonConvert.SerializeObject(data);
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataString));
            eventMessage.MessageId = Guid.NewGuid().ToString();
           // eventMessage.Properties.Add("level", levelValue);
            Console.WriteLine($"\t{DateTime.Now}>Message:{id}, Data:[{dataString}]");
            await deviceClient.SendEventAsync(eventMessage);
            await Task.Delay(5000);
        }

        private static async Task SetupDevice()
        {
            var twin = await deviceClient.GetTwinAsync();
            device = new SmartLightDevice(twin);
            Console.WriteLine("\nInitial twin value received:");
            Console.WriteLine(JsonConvert.SerializeObject(twin, Formatting.Indented));
            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredTwinPropertyChanged, null);
            await deviceClient.SetMethodHandlerAsync("LightOn", device.LightOnMethodHandler, null);
            await deviceClient.SetMethodHandlerAsync("LightOff", device.LightOffMethodHandler, null);
        }

        private static async Task OnDesiredTwinPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            device.UpdateDesiredTwin(desiredProperties);
            await UpdateReportedTwin();
        }

        private static async Task UpdateReportedTwin()
        {
            Console.WriteLine("Sending current reported properties");
            TwinCollection reportedProperties = new TwinCollection();
            //reportedProperties[nameof(device.Mode)] = device.Mode.ToString("false");
           
            
                reportedProperties[nameof(device.DateTimeLastModeChange)] = device.DateTimeLastModeChange;
                
            
           
            reportedProperties[nameof(device.LightBottomLimit)] = device.LightBottomLimit;
            reportedProperties[nameof(device.LightTopLimit)] = device.LightTopLimit;
            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
        }
    }
}
