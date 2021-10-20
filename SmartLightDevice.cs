using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLightSymulator
{
    class SmartLightDevice
    {
        public bool SimulateMalfuncion { get; set; }

        #region Twin

        public DeviceMode Mode { get; private set; }

        public DateTime DateTimeLastModeChange { get; private set; }

        public double LightTopLimit { get; private set; }

        public double LightBottomLimit { get; private set; }

        //
        public bool TepeWork { get; private set; }

       

        #endregion
        #region Telemetry

        public bool LightOn { get; private set; }

        public double PowerConsumption { get; private set; }

        public double LightSensorReading { get; private set; }
        #endregion

        public SmartLightDevice(Twin twin)
        {
           
            UpdateDesiredTwin(twin.Properties.Desired);
        }

        internal void UpdateDesiredTwin(TwinCollection desiredProperties)
        {
           /* if(desiredProperties.Contains(nameof(Mode)))
            {
                Console.WriteLine("Updating Mode");
                var newMode = desiredProperties[nameof(Mode)];
                if (newMode != Mode)
                    DateTimeLastModeChange = DateTime.Now;
                Mode = newMode;
            }
           */

            if(desiredProperties.Contains(nameof(DateTimeLastModeChange)))
            {
                Console.WriteLine("Update DateTimeLastModeChange");
                DateTimeLastModeChange = desiredProperties[nameof(DateTimeLastModeChange)];
            }

            if(desiredProperties.Contains(nameof(LightBottomLimit)))
            {
                Console.WriteLine("Update LightBottomLimit");
                LightBottomLimit = desiredProperties[nameof(LightBottomLimit)];

            }

            if(desiredProperties.Contains(nameof(LightTopLimit)))
            {
                Console.WriteLine("Update LightTopLimit");
                LightTopLimit = desiredProperties[nameof(LightTopLimit)];
            }

            if (desiredProperties.Contains(nameof(TepeWork)))
            {
                
                var nowy = desiredProperties[nameof(TepeWork)];
                if(nowy!=TepeWork)
                {
                    DateTimeLastModeChange = DateTime.Now;
                    Console.WriteLine("Update TepeWork");
                }
                TepeWork = nowy;
               // TepeWork = desiredProperties[nameof(TepeWork)];
            }
        }

       

        
        internal async Task<MethodResponse> LightOffMethodHandler(MethodRequest methodRequest, object userContext)
        {
            
                Console.WriteLine("Received LightOff command");
                LightOn = false;

                 return new MethodResponse(0);

        }

        internal async Task<MethodResponse> LightOnMethodHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("Received LightOn command");
            LightOn = true;

            return new MethodResponse(0);
        }

        internal void Simulate(int id)
        {
            // 10-30 ; 30-50 ; 50-70 ; 70-90



            var rnd = new Random();
            int mod20 = id % 20;
            int mod5 = id % 5;



            if (mod20 < 5)
            {
                // first 5 messages in a sequence
                LightSensorReading = rnd.Next(10 + 4 * mod5, 10 + 4 * (mod5 + 1)) + rnd.NextDouble();
            }
            else if (mod20 < 10)
            {
                // first 6-10 message in a sequence
                LightSensorReading = rnd.Next(30 + 4 * mod5, 30 + 4 * (mod5 + 1)) + rnd.NextDouble();
            }
            else if (mod20 < 15)
            {
                // first 11-15 message in a sequence
                LightSensorReading = rnd.Next(50 + 4 * mod5, 50 + 4 * (mod5 + 1)) + rnd.NextDouble();
            }
            else if (mod20 < 20)
            {
                // first 16-20 message in a sequence
                LightSensorReading = rnd.Next(70 + 4 * mod5, 70 + 4 * (mod5 + 1)) + rnd.NextDouble();
            }

            if (TepeWork) {

               // if (Mode == DeviceMode.AUTOMATIC)
               // {
                    if (LightSensorReading < LightBottomLimit)
                    {
                        LightOn = true;
                    }
                    else if (LightSensorReading > LightTopLimit)
                    {
                        LightOn = false;
                    }
              // }


            }
            else
            {
                Console.Write("Manual");

            }






            bool accident = false;
            if (LightOn)
            {
                // PowerConsumption = rnd.Next(5000, 6000) / 100.0;
                PowerConsumption = rnd.Next(50, 60);
                
              //  SimulateMalfuncion = true; 
              //   if (SimulateMalfuncion)
               //    PowerConsumption += 10;

                if (accident)
                {

                    if (PowerConsumption < 52)
                    {
                        PowerConsumption += 10;
                        Console.WriteLine("Damage!");
                    }



                }
                
                
               
            }
            else
            {
                PowerConsumption = rnd.NextDouble();
            }
        }
    }
}
