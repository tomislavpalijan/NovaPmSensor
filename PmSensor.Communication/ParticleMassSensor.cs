using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace PmSensor.Communication
{
    /// <summary>
    /// The SDS011 using principle of laser scattering,can get the particle 
    /// concentration between 0.25 to 10μm in the air.
    /// The unit of the measurement results is μg/m³ <see cref="ParticleMassSensorValues"/>  
    /// </summary>
    public class ParticleMassSensor
    {
        private readonly byte[] _queryMode = { 0xAA, 0xB4, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x02, 0xAB };
        private readonly byte[] _activeMode = { 0xAA, 0xB4, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x01, 0xAB };
        private readonly byte[] _queryPmData = { 0xAA, 0xB4, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x02, 0xAB };
        private readonly byte[] _deviceIdQuery =  { 0xAA, 0xB4, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x03, 0xAB };
        private readonly byte[] _sleep = { 0xAA, 0xB4, 0x06, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x05, 0xAB };
        private readonly byte[] _wakeUp = { 0xAA, 0xB4, 0x06, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x06, 0xAB };
        private readonly byte[] _firmware = { 0xAA, 0xB4, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x05, 0xAB };

        public IParticleMassSensorParser Parser { get; set; }   
        public Parity Parity { get; set; }
        public SerialPort Port { get; private set; }
        public string PortName { get; set; }
        public int BaudRate { get; }
        public StopBits StopBit { get; }
        public int DataBits { get; }

        public event Action<bool> PortOpenChangedEvent;

        /// <summary>
        /// The unit of the measurement results <see cref="ParticleMassSensorValues"/> 2.5μm and 10μm is μg/m³
        /// </summary>
        public event Action<ParticleMassSensorValues> NewMeasurementEvent;

        public event Action<string> ErrorMessageEvent; 

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
        /// <see cref="ParticleMassSensor"/>
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
            catch (Exception exp)
            {
                OnErrorMessageEvent(exp.Message);
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

        private void ParserOnNewMeasurementEvent(ParticleMassSensorValues obj)
        {
            NewMeasurementEvent?.Invoke(obj);
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

        /// <summary>
        /// sets active mode or query mode
        /// in active mode sensor will send every second a measurement (1Hz)
        /// </summary>
        /// <param name="on">true = active mode, false = query mode</param>
        public void SetActiveMode(bool on)
        {
            SendCommand(@on ? _activeMode : _queryMode);
        }

        /// <summary>
        /// sets sleep or wake-up 
        /// </summary>
        /// <param name="on">true = sleep , false = wake-up</param>
        public void SetSleep(bool on)
        {
            SendCommand(@on ? _sleep : _wakeUp);
        }

        /// <summary>
        /// gets device id
        /// </summary>
        public void GetDeviceId()
        {
            SendCommand(_deviceIdQuery);
        }

        /// <summary>
        /// gets firmware
        /// </summary>
        public void GetFirmware()
        {
            SendCommand(_firmware);
        }

        /// <summary>
        /// gets measurement only in query mode available.
        /// </summary>
        public void GetMeasurement()
        {
            SendCommand(_queryPmData);
        }

        private void SendCommand(byte[] commandBytes, int offset = 0 )
        {
            if(Port == null || !IsPortOpen)
                return;

            Port.Write(commandBytes, offset, commandBytes.Length);
        }

        protected virtual void OnPortOpenChangedEvent(bool isPortOpen)
        {
            PortOpenChangedEvent?.Invoke(isPortOpen);
        }

        protected virtual void OnErrorMessageEvent(string obj)
        {
            ErrorMessageEvent?.Invoke(obj);
        }
    }
}