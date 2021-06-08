using System;
using PmSensor.Communication;

namespace PmSensor.ConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {
            var sensor = new ParticleMassSensor("COM9");

            sensor.PortOpenChangedEvent += delegate(bool b) { Console.WriteLine($"port is open {b}"); };

            sensor.Open();

            Console.WriteLine("start reading ...!");

            Console.ReadKey(true);
        }
    }
}
