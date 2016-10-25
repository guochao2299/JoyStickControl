using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JoyStickControl
{
    public partial class frmMain : Form
    {
        private SerialPort m_port = null;
        private UserSettings m_settings = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void toolSettings_Click(object sender, EventArgs e)
        {
            frmSettings fs = new frmSettings(m_settings);
            if (fs.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                m_settings.IsAutoSaveWhenAppClosed = fs.IsSendCmdWhenReceiveKeyCode;
                m_settings.SerialPort = fs.PortName;
            }
        }

        private void toolSerialControl_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (m_port != null && m_port.IsOpen)
                {
                    m_port.Close();
                    m_port.Dispose();
                    m_port = null;

                    this.toolSerialControl.Text = "开启串口监控";
                    this.toolSerialControl.Checked = false;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(m_settings.SerialPort))
                    {
                        MessageBox.Show("当前串口为空，请在设置窗口中选中可用端口");
                        return;
                    }

                    string[] ports = SerialPort.GetPortNames();

                    if (ports == null || ports.Length <= 0)
                    {
                        MessageBox.Show("本机没有可用串口!");
                        return;
                    }

                    if (!ports.Any(r => string.Compare(r, m_settings.SerialPort, true) == 0))
                    {
                        MessageBox.Show(string.Format("端口{0}不存在，请在设置窗口中选中可用端口"));
                        return;
                    }

                    m_player.ResetPosition(this.flpPanel.Width / 2, this.flpPanel.Height / 2);
                    this.flpPanel.Invalidate(this.flpPanel.ClientRectangle);

                    m_port = new SerialPort(m_settings.SerialPort, 9600);
                    m_port.Parity = Parity.None;
                    m_port.StopBits = StopBits.One;
                    m_port.DataBits = 8;
                    m_port.Handshake = Handshake.None;
                    m_port.RtsEnable = true;

                    m_port.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);

                    m_port.Open();

                    this.lblPath.Text = "/";

                    this.toolSerialControl.Text = "关闭串口监控";
                    this.toolSerialControl.Checked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("串口操作失败，错误消息为：" + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private delegate void UpdateSerialPortDataHandler(string cnt);
        private const int X_INDEX = 1;
        private const int Y_INDEX = 3;
        private const int Z_INDEX = 5;
        private const int PRESSED_SIGN = 1;

        private void UpdateSerialPortData(string cnt)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new UpdateSerialPortDataHandler(UpdateSerialPortData), cnt);
            }
            else
            {
                Regex r = new Regex(LIST_CMD);
                if (r.IsMatch(cnt))
                {
                    string[] subStrs = cnt.Split(SPLIT_CHAR);
                    float speedX = (STABLE_X - Convert.ToInt32(subStrs[X_INDEX])) * 1.0f / MAX_RANGE * MAX_SPEED;
                    float speedY = (STABLE_Y - Convert.ToInt32(subStrs[Y_INDEX])) * 1.0f / MAX_RANGE * MAX_SPEED;
                    m_player.IsPressed = Convert.ToInt32(subStrs[Z_INDEX]) == PRESSED_SIGN;
                    m_player.UpdateXPosition(speedX, 0, this.flpPanel.Width - PIC_WIDTH);
                    m_player.UpdateYPosition(speedY, 0, this.flpPanel.Height - PIC_WIDTH);
                    this.flpPanel.Invalidate(this.flpPanel.ClientRectangle);
                    Console.WriteLine(string.Format("X Speed = {0},Y Speed = {1}", speedX, speedY));
                }
            }
        }
        
        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine().TrimEnd('\r');
            Console.WriteLine(indata);
            UpdateSerialPortData(indata);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                m_settings = UserSettings.LoadFromFile();
                m_settings.UserSettingsChanged += delegate(object obj, EventArgs ea)
                {
                    this.lblStatus.Text = m_settings.ToString();
                };
                this.lblStatus.Text = m_settings.ToString();

                m_player = new Player(this.flpPanel.Width / 2, this.flpPanel.Height / 2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载配置文件失败，错误消息为:" + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private const string LIST_CMD = @"^X:\d+:Y:\d+:B:[01]{1}$";
        private const char SPLIT_CHAR = ':';

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (m_port != null && m_port.IsOpen)
                {
                    m_port.Close();
                    m_port.Dispose();
                    m_port = null;
                }

                if (m_settings.IsAutoSaveWhenAppClosed)
                {
                    m_settings.Serialize2File();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }


        private const int STABLE_X = 501;
        private const int STABLE_Y = 503;
        private const int MAX_RANGE = 512;
        private const int MAX_SPEED = 10;
        private const int PIC_WIDTH = 40;
        private volatile Player m_player = null;

        private void flpPanel_Paint(object sender, PaintEventArgs e)
        {
            Image i = JoyStickControl.Properties.Resources.闭伞;
            if (m_player.IsPressed)
            {
                i = JoyStickControl.Properties.Resources.开伞;
            }

            e.Graphics.DrawImage(i, m_player.XPosition, m_player.YPosition, PIC_WIDTH, PIC_WIDTH);
        }
    }
}
