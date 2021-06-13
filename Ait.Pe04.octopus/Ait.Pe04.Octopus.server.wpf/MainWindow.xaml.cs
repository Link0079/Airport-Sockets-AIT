using Ait.Pe04.Octopus.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ait.Pe04.Octopus.server.wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Socket _socket;
        bool _serverOnline;
        int _maxConnections = 10;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartUpConfig();
            grpAirfield.Visibility = Visibility.Hidden;
        }

        private void StartUpConfig()
        {
            cmbIPs.ItemsSource = IP.GetActiveIPs();
            for (int port = 49200; port <= 49500; port++)
            {
                cmbPorts.Items.Add(port);
            }
            ServerConfig.GetConfig(out string savedIP, out int savedPort);
            try
            {
                cmbIPs.SelectedItem = savedIP;
            }
            catch
            {
                cmbIPs.SelectedItem = "127.0.0.1";
            }
            try
            {
                cmbPorts.SelectedItem = savedPort;
            }
            catch
            {
                cmbPorts.SelectedItem = 49200;
            }
            btnStartServer.Visibility = Visibility.Visible;
            btnStopServer.Visibility = Visibility.Hidden;
        }

        private void SaveConfig()
        {
            if (cmbIPs.SelectedItem == null) return;
            if (cmbPorts.SelectedItem == null) return;

            string ip = cmbIPs.SelectedItem.ToString();
            int port = int.Parse(cmbPorts.SelectedItem.ToString());
            ServerConfig.WriteConfig(ip, port);
        }

        private void StartServer()
        {
            _serverOnline = true;
        }

        private void CloseServer()
        {
            _serverOnline = false;

            try
            {
                if (_socket != null) _socket.Close();
            }
            catch
            { 
                
            }
            _socket = null;
        }

        private void StartListening()
        {
            IPAddress ip = IPAddress.Parse(cmbIPs.SelectedItem.ToString());
            int port = int.Parse(cmbPorts.SelectedItem.ToString());
            IPEndPoint endPoint = new IPEndPoint(ip, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Bind(endPoint);
                _socket.Listen(_maxConnections);
                while (_serverOnline)
                {
                    if(_socket != null)
                    {
                        if (_socket.Poll(100000, SelectMode.SelectRead))
                        {
                            HandleClientCall(_socket.Accept());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleClientCall(Socket clientCall)
        {
            byte[] clientRequest = new Byte[1024];
            string instruction = null;

            while (true)
            {

                int numByte = clientCall.Receive(clientRequest);
                instruction += Encoding.ASCII.GetString(clientRequest, 0, numByte);
                if (instruction.IndexOf("##OVER") > -1)
                    break;
            }
            string serverResponseInText = HandleInstruction(instruction);
            if (serverResponseInText != "")
            {
                byte[] serverResponse = Encoding.ASCII.GetBytes(serverResponseInText);
                clientCall.Send(serverResponse);
            }
            clientCall.Shutdown(SocketShutdown.Both);
            clientCall.Close();
        }
        private string HandleInstruction(string instruction)
        {
            return string.Empty;
        }

        private void CmbIPs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveConfig();
        }

        private void CmbPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveConfig();
        }

        private void BtnStartServer_Click(object sender, RoutedEventArgs e)
        {
            grpAirfield.Visibility = Visibility.Visible;
            btnStartServer.Visibility = Visibility.Hidden;
            btnStopServer.Visibility = Visibility.Visible;
        }

        private void BtnStopServer_Click(object sender, RoutedEventArgs e)
        {

            lstInRequest.Items.Insert(0, "\n=============\nAirspace closing \nGoodnight\n=============");
            btnStopServer.Visibility = Visibility.Hidden;
            btnStartServer.Visibility = Visibility.Visible;
        }
    }
}
