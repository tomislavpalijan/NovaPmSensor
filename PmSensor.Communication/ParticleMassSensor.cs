using System;
using System.IO.Ports;
using System.Threading;

namespace PmSensor.Communication
{
    public class ParticleMassSensor
    {
        public Parity Parity { get; set; }
        public SerialPort Port { get; private set; }
        public string PortName { get; set; }
        public int BaudRate { get; }
        public StopBits StopBit { get; }
        public int DataBits { get; }

        public event Action<bool> PortOpenChangedEvent;

        private bool _isPortOpen;

        public bool IsPortOpen
        {
            get => _isPortOpen;
            set
            {
                if(_isPortOpen == value)
                    return;

                _isPortOpen = value;
                OnPortOpenChangedEvent(_isPortOpen);
            }
        }
            

        /// <summary>
        /// Default values 
        /// baud = 9600,
        /// stop bits = one,
        /// data bits = 8,
        /// parity = none
        /// </summary>
        /// <param name="portName">serial port name</param>
        public ParticleMassSensor(string portName)
        : this(portName, 9600, StopBits.One, 8, Parity.None) { }

        public ParticleMassSensor(string portName, int baudRate, StopBits stopBits, int dataBits, Parity parity)
        {
            Parity = parity;
            PortName = portName;
            BaudRate = baudRate;
            StopBit = stopBits;
            DataBits = dataBits;

            Port = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBit);
        }

        public void Open()
        {
            if (Port == null)
                throw new NullReferenceException(nameof(Port));

            if (Port.IsOpen)
                return;

            Port.Open();

            Thread.Sleep(100);

            IsPortOpen = Port.IsOpen;
        }

        public void Close()
        {
            if (Port == null)
                throw new NullReferenceException(nameof(Port));

            Port.Close();

            Thread.Sleep(100);

            IsPortOpen = Port.IsOpen;
        }

        public void ReadData()
        {
            if (!IsPortOpen)
                return;


        }

        protected virtual void OnPortOpenChangedEvent(bool isPortOpen)
        {
            PortOpenChangedEvent?.Invoke(isPortOpen);
        }
    }
}