using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;

namespace TestCam
{
    public partial class Form1 : Form
    {
        static SerialPort _serialPort;
        public byte[] Buff = new byte[2];

        private double Px2CmScale;

        private Capture capture;
        private Image<Bgr, Byte> IMG;

        private Image<Gray, Byte> R_frame;
        private Image<Gray, Byte> G_frame;
        private Image<Gray, Byte> B_frame;
        private Image<Gray, Byte> GrayImg;

        private Image<Gray, Byte> R_Img_seg;
        private Image<Gray, Byte> R_Img_cor;
        private Image<Gray, Byte> B_Img_seg;
        private Image<Gray, Byte> B_Img_cor;

        

       


        public Form1()
        {
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM9";
            _serialPort.BaudRate = 9600;
           // _serialPort.Open();

            InitializeComponent();
        }

       
        private void processFrame(object sender, EventArgs e)
        {
            if (capture == null)
            {
                try
                {
                    capture = new Capture(1);
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame();


            R_frame = IMG[2].Copy();
            G_frame = IMG[1].Copy();
            B_frame = IMG[0].Copy();
            
            GrayImg = IMG.Convert<Gray, Byte>();

            R_Img_seg = IMG.Convert<Gray, Byte>();
            R_Img_cor = IMG.Convert<Gray, Byte>();
            B_Img_seg = IMG.Convert<Gray, Byte>();
            B_Img_cor = IMG.Convert<Gray, Byte>();

           

            label14.Text = "Width: " + GrayImg.Width.ToString();
            label15.Text = "Height: " + GrayImg.Height.ToString();


           

            int r_th, b_th, r_cor, b_cor;

            r_th = trackBar1.Value;
            b_th = trackBar2.Value;
            r_cor = trackBar3.Value;
            b_cor = trackBar4.Value;



           

            for (int i = 0; i < GrayImg.Width; i++)
            {
                for (int j = 0; j < GrayImg.Height; j++)
                {
                    if ((R_frame[j, i].Intensity >= r_th) && (B_frame[j, i].Intensity + G_frame[j, i].Intensity) < r_th)

                        R_Img_seg.Data[j, i, 0] = 255;
                    else
                        R_Img_seg.Data[j, i, 0] = 0;

                }
            }

            R_Img_cor = R_Img_seg.Copy();


            for (int count = 0; count < r_cor; count++)
            {
                for (int i = 1; i < GrayImg.Width - 1; i++)
                    for (int j = 1; j < GrayImg.Height - 1; j++)

                        if (R_Img_seg[j, i].Intensity != 0)
                        {
                            if ((R_Img_seg[j, i + 1].Intensity == 0) ||
                                (R_Img_seg[j - 1, i - 1].Intensity == 0) ||
                                (R_Img_seg[j - 1, i].Intensity == 0) ||
                                (R_Img_seg[j - 1, i + 1].Intensity == 0) ||
                                (R_Img_seg[j + 1, i + 1].Intensity == 0) ||
                                (R_Img_seg[j + 1, i].Intensity == 0) ||
                                (R_Img_seg[j + 1, i - 1].Intensity == 0))
                                R_Img_cor.Data[j, i, 0] = 0;
                            else R_Img_cor.Data[j, i, 0] = 255;

                        }
                        else
                            R_Img_cor.Data[j, i, 0] = 0;


                R_Img_cor.CopyTo(R_Img_seg);
            }

            

            for (int i = 0; i < GrayImg.Width; i++)
            {
                for (int j = 0; j < GrayImg.Height; j++)
                {
                    if (((B_frame[j, i].Intensity < b_th) && (R_frame[j, i].Intensity) < b_th) && (G_frame[j, i].Intensity) < b_th)
                        B_Img_seg.Data[j, i, 0] = 255;
                    else
                        B_Img_seg.Data[j, i, 0] = 0;



                }

            }

            B_Img_cor = B_Img_seg.Copy();

            for (int count = 0; count < b_cor; count++)
            {
                for (int i = 1; i < GrayImg.Width - 1; i++)
                    for (int j = 1; j < GrayImg.Height - 1; j++)

                        if (B_Img_seg[j, i].Intensity != 0)
                        {
                            if ((B_Img_seg[j, i + 1].Intensity == 0) ||
                                (B_Img_seg[j - 1, i - 1].Intensity == 0) ||
                                (B_Img_seg[j - 1, i].Intensity == 0) ||
                                (B_Img_seg[j - 1, i + 1].Intensity == 0) ||
                                (B_Img_seg[j + 1, i + 1].Intensity == 0) ||
                                (B_Img_seg[j + 1, i].Intensity == 0) ||
                                (B_Img_seg[j + 1, i - 1].Intensity == 0))
                                B_Img_cor.Data[j, i, 0] = 0;
                            else B_Img_cor.Data[j, i, 0] = 255;

                        }
                        else
                            B_Img_cor.Data[j, i, 0] = 0;


                B_Img_cor.CopyTo(B_Img_seg);
            }
            


            try
            {

                imageBox1.Image = IMG;
                imageBox2.Image = R_Img_cor;
                imageBox3.Image = B_Img_cor;

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {

            Application.Idle += processFrame;
            button1.Enabled = false;
            button2.Enabled = true;
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Idle -= processFrame;
            button1.Enabled = true;
            button2.Enabled = false;
        }
       
        private void button3_Click(object sender, EventArgs e)
        {
            
            Buff[0] =(byte) 75;  
            Buff[1] = (byte) 95; 
            _serialPort.Write(Buff, 0, 2);
        }

        
        private void shootButton_Click(object sender, EventArgs e)
        {

            int Xpx = 0;
            int Ypx = 0;
            int n = 0;

            for (int i = 0; i < GrayImg.Width; i++)
                for (int j = 0; j < GrayImg.Height; j++)
                {
                    if (B_Img_cor[j, i].Intensity > 128)
                    {
                        Xpx += i;
                        Ypx += j;
                        n++;

                    }
                }

            if (n > 0)
            {

                Xpx = Xpx / n;
                Ypx = Ypx / n;

               

                double Py = Xpx - (GrayImg.Width / 2);
                double Pz = -(Ypx - (GrayImg.Height / 2));

                double Xcm = 100;
                double Ycm = Py * Px2CmScale;
                double Zcm = Pz * Px2CmScale + 22;
               
                textBox2.Text = Ycm.ToString("0.00");
                textBox4.Text = Zcm.ToString("0.00");
                textBox3.Text = Xpx.ToString("0.00");
                textBox7.Text = Ypx.ToString("0.00");


               
                double Th1 = Math.Atan(Ycm / Xcm);
                double Th2 = Math.Atan(((Zcm) / Ycm) * Math.Sin(Th1)) * (180 / Math.PI);

                Th1 = Th1 * (180 / Math.PI);

                textBox6.Text = Th1.ToString("0.00");
                textBox5.Text = Th2.ToString("0.00");

                Th1 = (95- (int) (Th1));
                Th2 = (70- (int)(Th2));
                Buff[0] = (byte)Th1;  
                Buff[1] = (byte)Th2; 

                _serialPort.Write(Buff, 0, 2);
            }
            
        }

        
        private void calibrationButton_Click(object sender, EventArgs e)
        {
           
            double[] proj = new double[GrayImg.Width];
            for (int i = 0; i < GrayImg.Width; i++)
            {
                double column = 0;
                for (int j = 0; j < GrayImg.Height; j++)
                {
                    proj[i] = column = column + ((R_Img_cor[j, i].Intensity) / 255);
                }

            }

            

            int k = 0;
            double sum = 0;

            while (k < proj.Length && proj[k] == 0) { k++; }
            k += 5;
            int start = k;
            for (int i = 0; i < 2; i++)
            {
                while (k < proj.Length && proj[k] != 0) k++;
                k += 5;

                while (k < (GrayImg.Width - 5) && proj[k] == 0) k++;
                k += 5;
                int end = k;
                sum = sum + (end - start);
                start = end;
            }

            double Avg = sum / 2.0;
            Px2CmScale = 20.0 / Avg;

            
            label16.Text = "Scale: " + Px2CmScale.ToString("0.00");
            label12.Text = "Average: " + Avg.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Buff[0] = (byte)45;  
            Buff[1] = (byte)45; 
            _serialPort.Write(Buff, 0, 2);
        }

        private void button5_Click(object sender, EventArgs e)
        {

            Buff[0] = (byte)94;  
            Buff[1] = (byte)85; 
            _serialPort.Write(Buff, 0, 2);
        }
    }
}