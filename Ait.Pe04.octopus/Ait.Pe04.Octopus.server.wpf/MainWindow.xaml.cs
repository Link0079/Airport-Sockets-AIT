using Ait.Pe04.Octopus.Core.Entities;
using Ait.Pe04.Octopus.Core.Helpers;
using Ait.Pe04.Octopus.Core.Services;
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
using System.Windows.Threading;

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
        PlaneService _planeService;
        LaneService _laneService;
        Destinations _destinations;

        long id = 1;
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
            lblSos.Visibility = Visibility.Hidden;
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
            _planeService = new PlaneService();
            _laneService = new LaneService();
            _destinations = new Destinations();
        }

        private void CloseServer()
        {
            _serverOnline = false;
            _planeService = null;
            _laneService = null;
            _destinations = null;
            try
            {
                _socket.Close();
            }
            catch
            { }
            _socket = null;
            InsertMessage(lstInRequest, $"Airspace closed at {DateTime.Now:G}");
        }

        public static void DoEvents()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
        private void StartListening()
        {
            IPAddress ip = IPAddress.Parse(cmbIPs.SelectedItem.ToString()); // Get ip in combobox
            int port = int.Parse(cmbPorts.SelectedItem.ToString()); // Get port in combobox
            IPEndPoint endPoint = new IPEndPoint(ip, port); // Create new IpEndpoint from ip and port
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Bind(endPoint);
                _socket.Listen(_maxConnections);
                InsertMessage(lstInRequest, $"Airspace opened at {DateTime.Now:g} \nHave a good day!");
                InsertMessage(lstInRequest, $"Maximum airplanes allowed : {_maxConnections}");

                while (_serverOnline) // While _serverOnline is true
                {
                    DoEvents();
                    if (_socket != null)
                    {
                        if (_socket.Poll(100000, SelectMode.SelectRead))
                        {
                            Socket clientSocket = _socket.Accept();
                            HandleClientCall(clientSocket);
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

            //string result;

            if (serverResponseInText.Length < 1)
            {
                serverResponseInText = $"{serverResponseInText} is unkown";
            }
            //else
            //{
            //    result = $"{ExecuteCommand(serverResponseInText)}";
            //}

            byte[] clientResponse = Encoding.ASCII.GetBytes(serverResponseInText);
            clientCall.Send(clientResponse);

            clientCall.Shutdown(SocketShutdown.Both);
            clientCall.Close();
        }
        private string HandleInstruction(string instruction)
        {
            InsertMessage(lstInRequest,instruction);
            string trimmedInstruction = instruction.Trim().Replace("##OVER", "");
            string[] data = trimmedInstruction.Split(";");
            string clientResponse = "";
            long existingPlaneId = GetIDFromCommand(data);

            foreach(var command in data)
            {
                //split the command on '='
                var commands = command.Split('=');
                switch (commands[0])
                {
                    #region IDENTIFICATION
                    case "IDENTIFICATION":
                        long currentId = id;
                        InsertMessage(lstOutResponse, $"ID: {currentId} - {commands[0]}");

                        //Create newly connected plane on the server
                        string planeName = commands[1];

                        Plane newPlane = new Plane(id, planeName);

                        var destination = GetRandomDestination();
                        newPlane.SetDestination(destination);

                        _planeService.AddPlane(newPlane);

                        id++; //set id for next plane
                        clientResponse = $"IDNUMBER={currentId};DESTINATIONSET={destination}";
                        break;
                    #endregion

                    #region ADDPASS
                    case "ADDPASS":
                        clientResponse = _planeService.AddPassengerToPlane(existingPlaneId);
                        break;
                    #endregion
                    
                    #region SUBSPASS
                    case "SUBSPASS":
                        clientResponse = _planeService.SubstractPassengerOfPlane(existingPlaneId);
                        break;
                    #endregion

                    #region REQLANE
                    case "REQLANE":
                        var requestingPlane = _planeService.FindPlane(existingPlaneId);
                        clientResponse = _laneService.AddPlaneToLane(requestingPlane);
                        break;
                    #endregion

                    #region GOTOLANE
                    case "GOTOLANE":
                        var planeToLane = _planeService.FindPlane(existingPlaneId);
                        clientResponse = _laneService.GetRequestLaneFromPlane(planeToLane);
                        break;
                    #endregion

                    #region REQLIFT
                    case "REQLIFT":
                        Plane planeToLift = _planeService.FindPlane(existingPlaneId);
                        _laneService.MakeLaneAvailable(planeToLift);
                        clientResponse = _planeService.PlaneWantsToLiftOff(existingPlaneId);
                        break;
                    #endregion

                    #region REQLAND
                    case "REQLAND":
                        var landingPlane = _planeService.FindPlane(existingPlaneId);
                        clientResponse = _laneService.AddPlaneToLane(landingPlane);
                        break;
                    #endregion

                    #region STARTENG
                    case "STARTENG":
                        clientResponse = _planeService.StartPlaneEngine(existingPlaneId);
                        break;
                    #endregion

                    #region
                    case "STOPENG":
                        clientResponse = _planeService.StopPlaneEngine(existingPlaneId);
                        break;
                    #endregion

                    #region
                    case "SOS":
                        clientResponse = _planeService.SendSOS(existingPlaneId);
                        break;
                    #endregion
                    case "BYEBYE":
                        clientResponse = $"Lost Connection to a plane";
                        break;
                }
                //return clientResponse;
            }
            InsertMessage(lstOutResponse, clientResponse);
            return clientResponse;
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
            StartServer(); // Starting server and setting _serverOnline to true
            StartListening();
        }

        private void BtnStopServer_Click(object sender, RoutedEventArgs e)
        {

            InsertMessage( lstInRequest,"Airspace closing");
            btnStopServer.Visibility = Visibility.Hidden;
            btnStartServer.Visibility = Visibility.Visible;
            CloseServer();
        }

        private void InsertMessage(ListBox source, string message)
        {
            source.Items.Insert(0, message);
        }

        private string ExecuteCommand(string[] command)
        {
            string message = $"{command[0]} - {command[1]}";

            foreach(var instruction in command)
            {
                var commands = instruction.Split("=");

                if (commands[0] == "ID")
                {
                    //search plane in service
                    long planeId = Int32.Parse(commands[1]);
                    _planeService.FindPlane(planeId);
                    continue;
                }

                if (commands[0] == "IDENTIFICATION")
                {
                    long currentId = id;
                    InsertMessage(lstOutResponse, $"ID: {currentId} - {commands[1]}");

                    //Create newly connected plane on the server
                    string planeName = commands[1];

                    Plane newPlane = new Plane(id, planeName);

                    var destination = GetRandomDestination();
                    newPlane.SetDestination(destination);

                    _planeService.AddPlane(newPlane);

                    id++; //set id for next plane
                    return $"IDNUMBER={currentId};DESTINATIONSET={destination}";
                } //Add a passenger
                  // Again, we end up with 4 objects in data/command; it looks messy but it works
                else if (commands[0] == "ADDPASS")
                {
                    InsertMessage(lstOutResponse, message);
                    return message;
                } // Substract a passenger 
                else if (commands[0] == "SUBSPASS")
                {
                    InsertMessage(lstOutResponse, message);
                    return message;
                } // Request a lane
                else if (commands[0] == "REQLANE")
                {
                    InsertMessage(lstOutResponse, message);
                    return message;
                } // Move to a lane
                else if (commands[0] == "GOTOLANE")
                {
                    InsertMessage(lstOutResponse, message);
                    return message;
                } // Request liftoff
                else if (commands[0] == "REQLIFT")
                {
                    InsertMessage(lstOutResponse, message);
                    return message;
                } // Request landing
                else if (commands[0] == "REQLAND")
                {
                    InsertMessage(lstOutResponse, message);
                    return message;
                } // Start plane engine
                else if (commands[0] == "STARTENG")
                {
                    InsertMessage(lstOutResponse, message);
                    return message;
                } // Stop plane engine
                else if (commands[0] == "STOPENG")
                {
                    InsertMessage(lstOutResponse, message);
                    return message;
                } // SOS button
                else if (commands[0] == "SOS")
                {
                    lblSos.Visibility = Visibility.Visible;
                    lblSos.Text = message;
                    InsertMessage(lstOutResponse, message);
                    return message;
                }
                else
                {
                    InsertMessage(lstOutResponse, "UNKNOWN INSTRUCTION");
                    return "UNKNOWN INSTRUCTION";
                }
            }
            return "";
            
            // To add as last command
            //else if
            //{
            //    InsertMessage(lstOutResponse, "UNKNOWN INSTRUCTION");
            //    return "UNKNOWN INSTRUCTION";
            //}
        }

        private string GetRandomDestination()
        {
            Random rng = new Random();
            var index = rng.Next(0, 10);
            return _destinations.Airports.ElementAt(index).Key;
        }

        private long GetIDFromCommand(string[] data)
        {
            if (data[0].Contains("IDENTIFICATION")) return id;
            //Command starts with ID=
            string planeIdString = data[0].Replace("ID=", "");
            long planeId = Int32.Parse(planeIdString);
            return planeId;
        }
    }
}
