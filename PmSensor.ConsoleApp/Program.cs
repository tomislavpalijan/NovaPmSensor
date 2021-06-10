using System;
using System.Threading.Channels;
using PmSensor.Communication;

namespace PmSensor.ConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {
            var sensor = new ParticleMassSensor("COM9");

            sensor.PortOpenChangedEvent += delegate(bool b) { Console.WriteLine($"port is open {b}"); };

            sensor.NewMeasurementEvent += values =>
                Console.WriteLine(
                    $"2.5 pm = {values.TwoPointFiveMicroMeterValue} μg/m³\t 10pm = {values.TenMicroMeterValue} μg/m³");

            sensor.Open();

            Console.WriteLine("start reading ...!");

            Console.ReadKey(true);
        }
    }
}
