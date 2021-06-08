using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PmSensor.Communication
{
    public interface IParticleMassSensorParser
    {
        event Action<byte[]> NewMeasurementEvent;
        void Parse(byte b);
    }

    public class ParticleMassSensorParser : IParticleMassSensorParser
    {
        private const byte MessageHeader = 0xAA;
        private const byte MeasureCommand = 0xC0;
        private const byte MessageTail = 0xAB;
        private int rxBufferPosition = 0;
        private byte[] rxBuffer = new byte[10];
        private bool isMeasurement = false;
        public event Action<byte[]> NewMeasurementEvent;

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
                                var message = new byte[10];
                                Array.Copy(rxBuffer, message, rxBuffer.Length);
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


        protected virtual void OnNewMeasurementEvent(byte[] obj)
        {
            NewMeasurementEvent?.Invoke(obj);
        }
    }
}