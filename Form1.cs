using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Whiteboard
{
    public partial class Form1 : Form
    {          
        public delegate void ShowMessage(string message);
        public ShowMessage myDelegate;

        UdpClient udpClient;
        Thread thread;
        public Form1()
        {
            InitializeComponent();
            string strHostName = System.Net.Dns.GetHostName();

            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            string ip = "-";
            foreach (IPAddress ipAddress in ipEntry.AddressList)
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    ip = ipAddress.ToString();
                }
            }
            if (ip != "-")
                myIpField.Text = ip;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                thread.Abort();
                udpClient.Close();
                Close();
            }
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                Int32 otherPort = int.Parse(otherPortField.Text);
                IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, otherPort);
                byte[] content = udpClient.Receive(ref remoteIPEndPoint);

                if (content.Length > 0)
                {
                    string message = Encoding.ASCII.GetString(content);
                    this.Invoke(myDelegate, new object[] { message });
                }
            }
        }

        private void ShowMessageMethod(string message)
        {
            toolStripStatusLabel2.Text = message;
            try{
                int xAxis = int.Parse(message.Substring(0, message.IndexOf(",")));
                int yAxis = int.Parse(message.Substring(message.IndexOf(',') + 1));
                /*string colString = message.Substring(message.LastIndexOf(',') + 1);
                ColorConverter converter = new ColorConverter();

                Color col = (Color)converter.ConvertFromString(colString);

                toolStripStatusLabel2.Text = "The coordinates= '" + xAxis + "' ,yaxis= ;"+ yAxis+";"+ col;
                */
                bit = new Bitmap(bit, panel1.Size);
                panel1.BackgroundImage = bit;
                bit.SetPixel(xAxis, yAxis, col);

            }catch{
                toolStripStatusLabel2.Text = message;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private Bitmap bit = new Bitmap("C:/Users/Pelle/Dropbox/Skola/Stationära enheter c#/whiteboard/Whiteboard/Untitled.bmp");
        private Color col = Color.Black;
        private bool mouse_down = false;

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mouse_down = true;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mouse_down = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel1.Text = e.X + "," + e.Y;
            if (e.X < panel1.Size.Width && e.X > 0 && e.Y < panel1.Size.Height && e.Y > 0)
            {
                if (mouse_down == true)
                {
                    bit = new Bitmap(bit, panel1.Size);
                    panel1.BackgroundImage = bit;
                    bit.SetPixel(e.X, e.Y, col);

                    Int32 otherPort = int.Parse(otherPortField.Text);
                    IPAddress ip = IPAddress.Parse(otherIpField.Text.Trim());
                    IPEndPoint ipEndPoint = new IPEndPoint(ip, otherPort);
                    byte[] content = Encoding.ASCII.GetBytes(toolStripStatusLabel1.Text);
                    try
                    {
                        int count = udpClient.Send(content, content.Length, ipEndPoint);
                        if (count > 0)
                        {
                            toolStripStatusLabel2.Text = "Sending cordinates.";
                        }
                    }
                    catch
                    {
                        toolStripStatusLabel2.Text = "Error sending cordinates.";
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            col = colorDialog1.Color;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Int32 otherPort = int.Parse(otherPortField.Text);
            IPAddress ip = IPAddress.Parse(otherIpField.Text.Trim());
            IPEndPoint ipEndPoint = new IPEndPoint(ip, otherPort);
            byte[] content = Encoding.ASCII.GetBytes(messageBox.Text);
            try
            {
                int count = udpClient.Send(content, content.Length, ipEndPoint);
                if (count > 0)
                {
                    toolStripStatusLabel2.Text = "Message has been sent.";
                }
            }
            catch
            {
                toolStripStatusLabel2.Text = "Error sending message.";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            udpClient = new UdpClient(int.Parse(myPortField.Text));
            myDelegate = new ShowMessage(ShowMessageMethod);
            thread = new Thread(new ThreadStart(ReceiveMessage));
            thread.IsBackground = true;
            thread.Start();

        }
    }
}
