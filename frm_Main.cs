using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Game
{
    public partial class FormMain : Form
    {
        private  Socket _clientSocket;
        byte[] receivedBuf = new byte[1024];
        string _Name { get; set; }
        public FormMain()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
        }
        private void ReceiveData(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            int received = socket.EndReceive(result);
            byte[] dataBuf = new byte[received];
            Array.Copy(receivedBuf, dataBuf, received);
            string check = Encoding.ASCII.GetString(dataBuf);
            if(CheckIfDisconnect(check, socket))
            {
                return;
            }
            if (check.Contains(">>") && check.Substring(3, check.Length - 3).Contains(">>") == false)
            {
                AddNames(check);
            }
            else
            {
                mainChat.Text = mainChat.Text + (Encoding.ASCII.GetString(dataBuf));
            }
            _clientSocket.BeginReceive(receivedBuf, 0, receivedBuf.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _clientSocket);
        }
        void AddNames(string check)
        {
            int index = listBox1.Items.Count;
            bool found = false;

            if (!check.EndsWith("\r\n"))
                check = check + "\r\n";

            if (index != 0)
            {
                for (int i = 0; i < index; i++)
                {    
                    if (listBox1.Items[i].ToString().Contains(check.Substring(2, check.Length - 2)) == false)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    //if (_Name.Replace("@", "").Contains(check.Replace(">", "").Replace("\r\n", "")))
                    if(!listBox1.Items.Contains(check))
                    {
                        listBox1.Items.Add(check);
                    }
                }

            }
            else
            {

                listBox1.Items.Add(check);
            }
        }
        bool CheckIfDisconnect(string check, Socket socket)
        {
            if (check == "*****")
            {
                statusLabel.Text = "Disconnected from server.";
                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                _clientSocket.Disconnect(false);
                _clientSocket = null;
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return true;
            }
            return false;
        }
        private  void LoopConnect()
        {
            int attempts = 0;
            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect(IPAddress.Loopback, 10010);
                }
                catch (SocketException)
                {
                    statusLabel.Text = ("Connection attempts: " + attempts.ToString());
                }
            }
            statusLabel.Text = ("Connected!");
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (_clientSocket.Connected)
            {
                
                byte[] buffer = Encoding.ASCII.GetBytes(txt_text.Text);
                _clientSocket.Send(buffer);
                mainChat.AppendText("You: " + txt_text.Text + " \r\n");
                txt_text.Clear();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            LoopConnect();
            _clientSocket.BeginReceive(receivedBuf, 0, receivedBuf.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _clientSocket);
            byte[] buffer = Encoding.ASCII.GetBytes("@@" + txtName.Text);
            string text = Encoding.ASCII.GetString(buffer);
            _Name = text;
            _clientSocket.Send(buffer);
        }

        private void mainChat_TextChanged(object sender, EventArgs e)
        {

        }

        private void disconBtn_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            if (_clientSocket.Connected)
            {

                byte[] buffer = Encoding.ASCII.GetBytes("disconnect");
                _clientSocket.Send(buffer);
                mainChat.AppendText("******************\r\n");
                txt_text.Clear();
            }
        }
    }
}
