using System;
using PmSensor.Communication;

namespace PmSensor.ConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {
            var sensor = new ParticleMassSensor("COM6");

            Console.WriteLine("Hello World!");
        }
    }
}
