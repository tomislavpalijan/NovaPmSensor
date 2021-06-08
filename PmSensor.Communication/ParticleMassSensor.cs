using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace PmSensor.Communication
{
    public class ParticleMassSensor
    {
        public IParticleMassSensorParser Parser { get; set; }   
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


        private Stream _portStream;
        private const int MESSAGE_SIZE = 20;
        private byte[] _rxBuffer;

        /// <summary>
        /// Default values 
        /// baud = 9600,
        /// stop bits = one,
        /// data bits = 8,
        /// parity = none
        /// parser = <see cref="ParticleMassSensorParser"/>
        /// </summary>
        /// <param name="portName">serial port name</param>
        public ParticleMassSensor(string portName)
        : this(portName, 9600, StopBits.One, 8, Parity.None, new ParticleMassSensorParser()) { }

        public ParticleMassSensor(string portName, IParticleMassSensorParser parser)
            : this(portName, 9600, StopBits.One, 8, Parity.None, parser) { }

        public ParticleMassSensor(string portName, int baudRate, StopBits stopBits, int dataBits, Parity parity, IParticleMassSensorParser parser)
        {
            Parity = parity;
            PortName = portName;
            BaudRate = baudRate;
            StopBit = stopBits;
            DataBits = dataBits;

            Port = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBit);
            Parser = parser;
        }

        public void Open()
        {
            if (Port == null)
                throw new NullReferenceException(nameof(Port));

            if (Port.IsOpen)
                return;

            Parser.NewMeasurementEvent -= ParserOnNewMeasurementEvent;
            Parser.NewMeasurementEvent += ParserOnNewMeasurementEvent;

            _rxBuffer = new byte[MESSAGE_SIZE];

            Port.DataReceived -= PortOnDataReceived;
            Port.DataReceived += PortOnDataReceived;

            Port.Open();

            _portStream = Port.BaseStream;
                        
            Thread.Sleep(100);

            IsPortOpen = Port.IsOpen;


        }

        private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                ParseReceivedData();
            }
            catch (Exception )
            {

            }
        }


        private void ParseReceivedData()
        {
            var bytesToRead = Math.Min(MESSAGE_SIZE, Port.BytesToRead);
            _portStream.Read(_rxBuffer, 0, bytesToRead);

            for (var i = 0; i < bytesToRead; i++)
            {
                Parser.Parse(_rxBuffer[i]);
            }
        }

        private void ParserOnNewMeasurementEvent(byte[] obj)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            if (Port == null)
                throw new NullReferenceException(nameof(Port));

            Port.Close();

            Thread.Sleep(100);

            IsPortOpen = Port.IsOpen;
            _portStream = null;
            _rxBuffer = null;
        }

        public void ReadData()
        {
            if (!IsPortOpen)
                return;

            throw new NotImplementedException();

        }

        protected virtual void OnPortOpenChangedEvent(bool isPortOpen)
        {
            PortOpenChangedEvent?.Invoke(isPortOpen);
        }
    }
}