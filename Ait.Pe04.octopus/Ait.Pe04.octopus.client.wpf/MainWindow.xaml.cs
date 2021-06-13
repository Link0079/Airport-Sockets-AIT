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

        int passengers;
        Socket _socket;
        IPEndPoint _serverEndPoint;

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
        }

        private void ContactServer()
        {
            IPAddress serverIP = IPAddress.Parse(txtServerIP.Text);
            //string activeIP = cmbIPs.SelectedItem.ToString();
            int serverPort = int.Parse(cmbPorts.SelectedItem.ToString());
            string activePlane = txtActivePlane.Text;

            _serverEndPoint = new IPEndPoint(serverIP, serverPort);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string message = "IDENTIFICATION=" + activePlane + "##OVER";
            lblMyID.Content = SendMessageToServerWaitOnResponse(message);
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

            lblOnLane.Content = ""; // adding .IsEnabled = false; to this Label when RequestLane is pressed ?
            lblPassengerCount.Content = ""; // adding .IsEnabled = false; to this Label when RequestLane is pressed ?
            txtDestination.Text = ""; // adding .IsEnabled = false; to this TextBox when RequestLane is pressed ?

            // .IsEnabled is used in place of .Visibility because I think that seeing the buttons will look better than seeing a white empty space
            // can't decide until the connection between client and server can be established though
            btnAddPassengers.IsEnabled = true;
            btnSubtractPassengers.IsEnabled = true;
            btnRequestLane.IsEnabled = true;
            btnGoToLane.IsEnabled = false; //will be enabled after RequestLane button is pressed; then RequestLane becomes disabled
            btnRequestLiftOff.IsEnabled = false; // will be enabled after the plane goes on a lane (using btnGoToLane)
            btnRequestLanding.IsEnabled = false; // will be enabled when the plane is in flight ?
            btnStartEngine.IsEnabled = true;
            btnStopEngine.IsEnabled = false; // will be enabled when btnStartEngine is clicked; then btnStartEngine.IsEnabled = false
            btnSOS.IsEnabled = false; // will be enabled when enabled when plane is in flight ?
            txtBlockFeedback.IsEnabled = false; // Only to show feedback

            ContactServer();
        }

        private void btnDisconnectFromServer_Click(object sender, RoutedEventArgs e)
        {
            string message = "ID=" + lblMyID.Content + "|BYEBYE##OVER";
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
        private string SendMessageToServerWaitOnResponse(string message)
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
                return response;

            }
            catch
            {
                btnDisconnectFromServer_Click(null, null);
                return "ERROR ENCOUNTERED! STANDBY##OVER";
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
            var actualPassangers = Convert.ToInt32(lblPassengerCount.Content); // converting label content to int
            passengers = actualPassangers;

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

            if (passengers> 150)  // check to see if plane there is more place on the plane
            {
                txtBlockFeedback.Background = Brushes.Red;
                txtBlockFeedback.Text = "The plane can only hold 150 passengers. We can not squeeze more in.";
            }
            else 
            {
                passengers = actualPassangers + 10;
                lblPassengerCount.Content = passengers.ToString();
                txtBlockFeedback.Background = Brushes.Green;
                txtBlockFeedback.Text = " 10 passengers have boarded the plane ";
            }

            string message = "ID=" + lblMyID.Content + lblPassengerCount.Content +"|+PASSENGERS##OVER";
            SendMessageToServerDontWaitOnResponse(message);
        }

        private void btnSubtractPassengers_Click(object sender, RoutedEventArgs e)
        {
            //method to substract passenger from active plane
        }

        private void btnRequestLane_Click(object sender, RoutedEventArgs e)
        {
            //method to request lane for the plane
        }

        private void btnGoToLane_Click(object sender, RoutedEventArgs e)
        {
            //method for the plane to taxi to lane or land on the lane
        }

        private void btnRequestLiftOff_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to request permission for takeoff
        }

        private void btnRequestLanding_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to put request for landing
        }

        private void btnStartEngine_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to start the engine before takeoff
        }

        private void btnStopEngine_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to stop the engine after landing
        }

        private void btnSOS_Click(object sender, RoutedEventArgs e)
        {
            //method for plane to request help in case of emergency
        }
    }
}
