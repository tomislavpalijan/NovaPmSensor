using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PmSensor.Communication
{
    public interface IParticleMassSensorParser
    {
        event Action<ParticleMassSensorValues> NewMeasurementEvent;
        void Parse(byte b);
    }

    public class ParticleMassSensorValues
    {
        public float TwoPointFiveMicroMeterValue { get; set; }
        public float TenMicroMeterValue { get; set; }
    }

    public class ParticleMassSensorParser : IParticleMassSensorParser
    {
        private int pm25lb = 2;
        private int pm25hb = 3;
        private int pm10hb = 5;
        private int pm10lb = 4;
        
        private const byte MessageHeader = 0xAA;
        private const byte MeasureCommand = 0xC0;
        private const byte MessageTail = 0xAB;
        private int rxBufferPosition = 0;
        private byte[] rxBuffer = new byte[10];
        private bool isMeasurement = false;
        public event Action<ParticleMassSensorValues> NewMeasurementEvent;

        public void Parse(byte b)
        {
            if (rxBufferPosition == 0)
            {
                if (b == MessageHeader)
                {
                    rxBuffer[rxBufferPosition++] = b;
                }
            }
            else
            {
                if (rxBufferPosition == 1 && b == MeasureCommand)
                {
                    rxBuffer[rxBufferPosition++] = b;
                    isMeasurement = true;
                }
                else
                {
                    if (isMeasurement && rxBufferPosition < 9)
                    {
                        rxBuffer[rxBufferPosition++] = b;
                    }
                    else if (rxBufferPosition == 9 && b == MessageTail)
                    {
                        rxBuffer[rxBufferPosition] = b;
                        if(VerifyCheckSum())
                            Task.Factory.StartNew(() =>
                            {
                                var message = new ParticleMassSensorValues
                                {
                                    TwoPointFiveMicroMeterValue = (float)(rxBuffer[pm25hb] * 256 + rxBuffer[pm25lb]) / 10,
                                    TenMicroMeterValue = (float) (rxBuffer[pm10hb] * 256 + rxBuffer[pm10lb]) / 10
                            };
                                OnNewMeasurementEvent(message);
                            });
                        rxBufferPosition = 0;
                        isMeasurement = false;
                    }
                }
            }
        }

        private bool VerifyCheckSum()
        {
            var isCorrect = true;

            uint expectedCs = rxBuffer[8];
            uint calculatedCs = 0;
            for (int i = 2; i < 8; i++)
            {
                calculatedCs += rxBuffer[i];
            }

            if (expectedCs != (calculatedCs % 256))
                isCorrect = false;

            return isCorrect;
        }


        protected virtual void OnNewMeasurementEvent(ParticleMassSensorValues obj)
        {
            NewMeasurementEvent?.Invoke(obj);
        }
    }
}