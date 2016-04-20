using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;

namespace ArduinoGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int ARDUINO_GUI_PORT = 2003;
        private IPEndPoint ep;
        private UdpClient client;
        private List<ArduinoControls.LED> leds;

        public MainWindow()
        {
            leds = new List<ArduinoControls.LED>();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            leds.Add(LED0); leds.Add(LED1); leds.Add(LED2); leds.Add(LED3); leds.Add(LED4);
            leds.Add(LED5); leds.Add(LED6); leds.Add(LED7); leds.Add(LED8); leds.Add(LED9);
            leds.Add(LED10); leds.Add(LED11); leds.Add(LED12); leds.Add(LED13);

            ep = new IPEndPoint(IPAddress.Any, ARDUINO_GUI_PORT);
            client = new UdpClient(ep);
            client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            logBox.AppendText(String.Format("Listening on {0}:{1}...\n", ep.Address, ARDUINO_GUI_PORT));
        }

        private void logBoxChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            logBox.ScrollToEnd();
        }

        private void LEDCmd(string [] tokens, string cmd)
        {
            int n;
            if (tokens.Length < 2)
            {
                logBox.AppendText(String.Format("Message '{0}' with no led number\n", cmd));
                return;
            }
            try
            {
                n = int.Parse(tokens[1]);
            }
            catch (FormatException e)
            {
                logBox.AppendText(String.Format("Message '{0}' with invalid led number: {1} [{2}]\n", cmd, tokens[1], e.Message));
                return;
            }
            if (n < 0 || n >= leds.Count)
            {
                logBox.AppendText(String.Format("Message '{0}' with invalid led number: {1}\n", cmd, n));
                return;
            }
            leds[n].IsActive = (cmd == "on");
            logBox.AppendText(String.Format("Turn {0} LED #{1}\n", cmd, n));
        }

        private void StatusUpdate(string msg)
        {
            string [] tokens = msg.Split(new Char[] { ' ', '\t', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 1)
            {
                logBox.AppendText(String.Format("Invalid message received: {0}\n", msg));
                return;
            }

            int n;
            switch (tokens[0])
            {
                case "on":
                    LEDCmd(tokens, "on");
                    break;

                case "off":
                    LEDCmd(tokens, "off");
                    break;

                case "tri":
                    LEDCmd(tokens, "off"); // no current
                    break;

                default:
                    logBox.AppendText(String.Format("Unknown command: {0}\n", msg));
                    break;
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Byte[] receiveBytes = client.EndReceive(ar, ref ep);
            string receiveString = Encoding.ASCII.GetString(receiveBytes).Trim();

            Application.Current.Dispatcher.BeginInvoke(new Action(() => this.StatusUpdate(receiveString)));

            client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

    }
}
