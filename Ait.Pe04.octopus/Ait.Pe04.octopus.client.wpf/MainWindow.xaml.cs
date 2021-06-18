using Ait.Pe04.Octopus.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ait.Pe04.octopus.client.wpf
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
        //string destination;
        int actualPassengers;
        int passengers;
        Socket _socket;
        IPEndPoint _serverEndPoint;
        Destinations _destinations = new Destinations(); //get dict of airports

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartUpConfig();
        }

        private void StartUpConfig()
        {
            cmbIPs.ItemsSource = IP.GetActiveIPs();
            for(int port = 49200; port <= 49500; port++)
            {
                cmbPorts.Items.Add(port);
            }
            ClientConfig.GetConfig(out string planeIP, out string serverIP, out int savedPort, out string planeName);

            try
            {
                cmbIPs.SelectedItem = planeIP;
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
            txtServerIP.Text = serverIP;
            txtActivePlane.Text = planeName;
            btnConnectToServer.Visibility = Visibility.Visible;
            btnDisconnectFromServer.Visibility = Visibility.Hidden;
            grpActivePlane.Visibility = Visibility.Hidden;
            txtDestination.IsEnabled = false;
        }

        private void ContactServer()
        {
            IPAddress serverIP = IPAddress.Parse(txtServerIP.Text);
            //string activeIP = cmbIPs.SelectedItem.ToString();
            int serverPort = int.Parse(cmbPorts.SelectedItem.ToString());
            string activePlane = txtActivePlane.Text;

            _serverEndPoint = new IPEndPoint(serverIP, serverPort);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string message = "IDENTIFICATION=" + activePlane + ";##OVER";
            SendMessageToServerWaitOnResponse(message);
        }

        private void btnConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            ClientConfig.WriteConfig(cmbIPs.SelectedItem.ToString(), txtServerIP.Text, int.Parse(cmbPorts.SelectedItem.ToString()), txtActivePlane.Text);
            btnConnectToServer.Visibility = Visibility.Hidden;
            btnDisconnectFromServer.Visibility = Visibility.Visible;
            grpActivePlane.Visibility = Visibility.Visible;

            cmbIPs.IsEnabled = false;
            cmbPorts.IsEnabled = false;
            txtActivePlane.IsEnabled = false;
            txtServerIP.IsEnabled = false;

            //destination = txtDestination.Text;
            lblOnLane.Content = "0"; // adding .IsEnabled = false; to this Label when RequestLane is pressed ?
            lblPassengerCount.Content = "0"; // adding .IsEnabled = false; to this Label when RequestLane is pressed ?
            txtDestination.Text = ""; // adding .IsEnabled = false; to this TextBox when RequestLane is pressed ?
            //destination = txtDestination.Text;

            // .IsEnabled is used in place of .Visibility because I think that seeing the buttons will look better than seeing a white empty space
            // can't decide until the connection between client and server can be established though
            btnAddPassengers.IsEnabled = true;
            btnSubtractPassengers.IsEnabled = false;
            btnRequestLane.IsEnabled = true;
            btnGoToLane.IsEnabled = false; //will be enabled after RequestLane button is pressed; then RequestLane becomes disabled
            btnRequestLiftOff.IsEnabled = false; // will be enabled after the plane goes on a lane (using btnGoToLane)
            btnRequestLanding.IsEnabled = false; // will be enabled when the plane is in flight ?
            btnStartEngine.IsEnabled = false;
            btnStopEngine.IsEnabled = false; // will be enabled when btnStartEngine is clicked; then btnStartEngine.IsEnabled = false
            btnSOS.IsEnabled = false; // will be enabled when enabled when plane is in flight ?

            ContactServer();
        }

        private void btnDisconnectFromServer_Click(object sender, RoutedEventArgs e)
        {
            string message = "ID=" + lblMyID.Content + ";BYEBYE;##OVER";
            SendMessageToServerDontWaitOnResponse(message);

            btnConnectToServer.Visibility = Visibility.Visible;
            btnDisconnectFromServer.Visibility = Visibility.Hidden;
            grpActivePlane.Visibility = Visibility.Hidden;
            lblMyID.Content = "";

            cmbIPs.IsEnabled = true;
            cmbPorts.IsEnabled = true;
            txtActivePlane.IsEnabled = true;
            txtServerIP.IsEnabled = true;
        }
        private void SendMessageToServerDontWaitOnResponse(string message)
        {
            lstOutRequest.Items.Insert(0, message);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(_serverEndPoint);
                byte[] outMessage = Encoding.ASCII.GetBytes(message);
                _socket.Send(outMessage);
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
            }
            catch
            {
                btnDisconnectFromServer_Click(null, null);
            }
        }
        private void SendMessageToServerWaitOnResponse(string message)
        {
            lstOutRequest.Items.Insert(0, message);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(_serverEndPoint);
                byte[] outMessage = Encoding.ASCII.GetBytes(message);
                byte[] inMessage = new byte[8000];

                _socket.Send(outMessage);
                int responseLength = _socket.Receive(inMessage);
                string response = Encoding.ASCII.GetString(inMessage, 0, responseLength).ToUpper().Trim();
                lstInResponse.Items.Insert(0, response);

                HandleServerResponse(response);
            }
            catch
            {
                btnDisconnectFromServer_Click(null, null);
                //return "ERROR ENCOUNTERED! STANDBY##OVER";
            }
            finally
            {
                if (_socket != null)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    _socket = null;
                }
            } 

        }

        //private void TrimMessageAndSendToList(string message) 
        //{
        //    lstOutRequest.Items.Insert(0, message);
        //    message.Replace("##OVER", "").Trim();
        //}

        private string CreateMessage(string overMessage) 
        {
            return$"ID={lblMyID.Content};{overMessage}";
        }

        private static string GetLaneString(string stringSource, string stringStart, string StringEnd) 
        {
            if(stringSource.Contains(stringStart) && stringSource.Contains(StringEnd))
            {
                int Start, End;
                Start = stringSource.IndexOf(stringStart, 0) + stringStart.Length;
                End = StringEnd.IndexOf(StringEnd, Start);
                return stringSource.Substring(Start, End - Start);
            }
            return"";
        }

        private void cmbIPs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //method when IP select is changed

            // in btnConnectToServer_Click we have this line
            //ClientConfig.WriteConfig(cmbIPs.SelectedItem.ToString(), txtServerIP.Text, int.Parse(cmbPorts.SelectedItem.ToString()), txtActivePlane.Text);
            // it send whatever values we have there to the server
            // this cmb does not need a method
        }

        private void cmbPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //method when Port select is changed

            // idem cmbIPs_SelectionChanged
        }

        private void btnAddPassengers_Click(object sender, RoutedEventArgs e)
        {
            string message = CreateMessage("ADDPASS##OVER");
            SendMessageToServerWaitOnResponse(message);

            #region Old code

            //try
            //{
            //    actualPassengers = Convert.ToInt32(lblPassengerCount.Content); // converting label content to int
            //    passengers = actualPassengers;
            //}
            //catch (Exception error)
            //{
            //    MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            // Checking if lblPassengerCount.Content can be parsed, normally we do not need this because we add passangers with the buttons
            //if(!int.TryParse((string)lblPassengerCount.Content, out actualPassangers)) 
            //{
            //    txtBlockFeedback.Background = Brushes.Red;
            //    txtBlockFeedback.Text = "You have to enter a number for the passengers.";
            //}
            //else 
            //{
            //    passengers = actualPassangers + 10;
            //    lblPassengerCount.Content = passengers.ToString();
            //    txtBlockFeedback.Background = Brushes.Green;
            //    txtBlockFeedback.Text = " 10 passengers have boarded the planne ";
            //}

            //if (passengers> 9)  // check to see if plane there is more place on the plane
            //{
            //    btnSubtractPassengers.IsEnabled = true;
            //    btnAddPassengers.IsEnabled = false;
            //    tbkFeedback.Background = Brushes.Red;
            //    tbkFeedback.Text = "The plane can only hold 10 passengers. We can not squeeze more in.";
            //}
            //else 
            //{
            //    btnSubtractPassengers.IsEnabled = true;
            //    passengers = actualPassengers + 1;
            //    lblPassengerCount.Content = passengers.ToString(); // update lblPassengerCount with the new number of passengers
            //    tbkFeedback.Background = Brushes.Green;
            //    tbkFeedback.Text = " A passenger has boarded the plane. ";
            //}

            //if(passengers >= 1) 
            //{
            //    tbkFeedback.Background = Brushes.Green;
            //    tbkFeedback.Text = tbkFeedback.Text + 
            //        " The plane has enough passengers for lift of.";
            //}

            //string message = "ID=" + lblMyID.Content + lblPassengerCount.Content + "|ADDPASS##OVER";
            #endregion
        }

        private void btnSubtractPassengers_Click(object sender, RoutedEventArgs e)
        {
            #region Old Code
            //try
            //{
            //    actualPassengers = Convert.ToInt32(lblPassengerCount.Content); // converting label content to int
            //    passengers = actualPassengers;

            //}
            //catch (Exception error)
            //{
            //    MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            //if (passengers <= 0)
            //{
            //    btnSubtractPassengers.IsEnabled = false;
            //    btnAddPassengers.IsEnabled = true;
            //    passengers = actualPassengers - 1;
            //}
            //else 
            //{
            //    btnAddPassengers.IsEnabled = true;
            //    passengers = actualPassengers - 1;
            //    lblPassengerCount.Content = passengers.ToString();
            //}
            #endregion

            string message = CreateMessage("SUBSPASS##OVER");
            SendMessageToServerWaitOnResponse(message);
        }

        private void btnRequestLane_Click(object sender, RoutedEventArgs e)
        {
            //method to request lane for the plane
            string message = CreateMessage("REQLANE##OVER");
            SendMessageToServerWaitOnResponse(message);

            #region Old Code
            //try
            //{
            //    actualPassengers = Convert.ToInt32(lblPassengerCount.Content); // converting label content to int
            //    passengers = actualPassengers;

            //}
            //catch (Exception error)
            //{
            //    MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            //if(passengers < 1) 
            //{
            //    //tbkFeedback.Background = Brushes.Red;
            //    //tbkFeedback.Text = " The plane does not have enough passengers for lift off.\n " +
            //    //                   " At least one passenger has to be boarded in order to request a lane .";
            //}
            //else 
            //{
            //    // Getting an available Lane
            //    // awaiting response from server with an available lane ?
            //    // the available lane would need to be put in lblOnLane.Content

            //    btnSubtractPassengers.IsEnabled = false;
            //    btnAddPassengers.IsEnabled = false;
            //    btnGoToLane.IsEnabled = true;
            //}
            #endregion

        }

        private void btnGoToLane_Click(object sender, RoutedEventArgs e)
        {
            //method for the plane to taxi to lane or land on the lane

            //lblOnLane.Content ="";
            string source = lstInResponse.SelectedIndex.ToString();
            // empty strings have to be replaced with the response strings
            string lane = GetLaneString(source, "", "");
            lblOnLane.Content = lane;

            btnGoToLane.IsEnabled = false;
            btnRequestLane.IsEnabled = false;
            btnRequestLiftOff.IsEnabled = true;
            btnStartEngine.IsEnabled = true;
            string message = CreateMessage("GOTOLANE##OVER");
            SendMessageToServerWaitOnResponse(message);
            

        }

        private void btnRequestLiftOff_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to request permission for takeoff

            btnRequestLiftOff.IsEnabled = false;
            btnRequestLanding.IsEnabled = true;
            btnSOS.IsEnabled = true;
            string message = CreateMessage("REQLIFT##OVER");
            SendMessageToServerWaitOnResponse(message);
            

        }

        private void btnRequestLanding_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to put request for landing

            btnRequestLanding.IsEnabled = false;
            btnStopEngine.IsEnabled = true;
            string message = CreateMessage("REQLAND##OVER");
            SendMessageToServerWaitOnResponse(message);
            
        }

        private void btnStartEngine_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to start the engine before takeoff

            btnStartEngine.IsEnabled = false;
            string message = CreateMessage("STARTENG##OVER");
            SendMessageToServerWaitOnResponse(message);
            
        }

        private void btnStopEngine_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to stop the engine after landing

            btnStopEngine.IsEnabled = false;
            //btnStartEngine.IsEnabled = true;
            string message = CreateMessage("STOPENG##OVER");
            SendMessageToServerWaitOnResponse(message);
            
        }

        private void btnSOS_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to request help in case of emergency
            btnSOS.IsEnabled = false;
            btnRequestLanding.IsEnabled = false;
            btnStartEngine.IsEnabled = false;
            btnStopEngine.IsEnabled = false;
            string message = CreateMessage("SOS##OVER");
            SendMessageToServerWaitOnResponse(message);
            
        }

        private void HandleServerResponse(string response)
        {

            var commandFromServer = response.Trim().Split(";");

            foreach(var commandArray in commandFromServer)
            {
                var command = commandArray.Trim().Split("=");

                switch (command[0])
                {
                    #region DestinationSET
                    case "DESTINATIONSET":
                        var destinationShort = command[1];
                        var destination = _destinations.Airports.GetValueOrDefault(destinationShort);
                        txtDestination.Text = destination;
                        break;
                    #endregion

                    #region IDNUMBER
                    case "IDNUMBER":
                        lblMyID.Content = command[1];
                    break;
                    #endregion

                    #region ADDPASS
                    case "ADDPASS":
                        // ADDPASS=$aantal.passengers;FULL;
                        actualPassengers = Convert.ToInt32(command.Last());

                        lblPassengerCount.Content = actualPassengers.ToString(); // update lblPassengerCount with the new number of passengers

                        if (actualPassengers >= 1)
                        {
                            btnSubtractPassengers.IsEnabled = true;
                        }
                    break;
                    #endregion

                    #region FULL
                    case "FULL":
                        btnAddPassengers.IsEnabled = false;
                        break;
                    #endregion

                    #region SUBSPASS
                    case "SUBSPASS":
                        // SUBSPASS=$aantal.passengers; EMPTY
                        actualPassengers = Convert.ToInt32(command.Last());
                        lblPassengerCount.Content = actualPassengers.ToString();
                        break;
                    #endregion

                    #region EMPTY
                    case "EMPTY":
                        btnSubtractPassengers.IsEnabled = false;
                        break;
                    #endregion

                    #region REQLANE
                    case "REQLANE":
                        // REQLANE=$laneName OF REQLANE=NONEAVAILABLE
                        btnSubtractPassengers.IsEnabled = false;
                        btnAddPassengers.IsEnabled = false;
                        btnGoToLane.IsEnabled = true;
                        break;
                    #endregion

                    #region GOTOLANE
                    case "GOTOLANE":
                        break;
                    #endregion

                    #region REQLIFT
                    case "REQLIFT":
                        break;
                    #endregion

                    #region REQLAND
                    case "REQLAND":
                        break;
                    #endregion

                    #region STARTENG
                    case "STARTENG":
                        break;
                    #endregion

                    #region STOPENG
                    case "STOPENG":
                        break;
                    #endregion

                    #region SOS
                    case "SOS":
                        break;
                    #endregion

                    default:
                        break;
                }
            }
        }
    }
}
