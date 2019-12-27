using ProjectCarsListener.Packets;
using System;
using System.IO.Ports;
using System.Timers;
using PCarsListener = ProjectCarsListener.ProjectCarsListener;

namespace PCarsWind
{
    public class Program
    {
        private static PCarsListener _pcars;
        private static SerialPort _serial;
        private static Timer _serialTimer;
        private static byte _throttle = 0;

        static void Main(string[] args)
        {
            _pcars = new PCarsListener();
            _pcars.Telemetry += Listener_Telemetry;
            _pcars.Start();

            _serial = new SerialPort("COM4", 9600);
            try
            {
                _serial.Open();

                _serialTimer = new Timer(25);
                _serialTimer.Elapsed += SerialTimer_Elapsed;
                _serialTimer.Start();
            }
            catch
            {
                Console.WriteLine("Could not open serial port.");
            }

            Console.ReadKey();

            if (_serial.IsOpen)
                _serial.Close();

            _pcars.Stop();
        }

        private static void Listener_Telemetry(sTelemetryData telemetry)
        {
            var mph = (int)Math.Round(telemetry.sSpeed / 0.44704);
            _throttle = (byte)Map(mph, 0, 100, 0, 255);
            Console.Write($"gear: {telemetry.Gear}, speed: {mph} mph, {_throttle}                \r");
        }

        private static void SerialTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _serial.Write(new[] { _throttle }, 0, 1);
        }

        private static int Map(int x, int in_min, int in_max, int out_min, int out_max)
        {
            return Math.Min(out_max, Math.Max(out_min, (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min));
        }
    }
}