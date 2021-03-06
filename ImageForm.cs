﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace Aont
{
    public partial class ImageForm : Form
    {
        static readonly Random rand = new Random();

        public ImageForm()
        {
            InitializeComponent();
        }


        Bitmap Original;
        private void LoadClick(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
        }

        Bitmap Converted;
        private void ConvertClick(object sender, EventArgs e)
        {
            if (this.Original == null)
                return;
            Converted = new Bitmap(Original.Width, Original.Height, PixelFormat.Format1bppIndexed);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            int Width = Original.Width, Height = Original.Height;
            using (var proc = new BmpProc1(Converted))
            {
                double alpha, beta;

                {
                    double b;
                    using (var proc_original = new BmpProc32(Original))
                        for (int y = 0; y < Height; ++y)
                        {
                            for (int x = 0; x < Width; ++x)
                            {
                                b = proc_original[x, y].GetBrightness();
                                alpha = rand.NextDouble();
                                beta = rand.NextDouble();
                                b += this.b_sigma * Math.Sqrt(-2 * Math.Log(alpha)) * Math.Cos(2 * Math.PI * beta);
                                if (b > this.b_average)
                                    proc[x, y] = true;
                                else
                                    proc[x, y] = false;
                            }
                        }
                }
            }
            sw.Stop();
            this.pictureBox1.Image = Converted;
            MessageBox.Show(string.Format("{0}秒", sw.Elapsed.TotalSeconds));


        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Original = new Bitmap(this.openFileDialog1.FileName);

            this.pictureBox1.Image = Original;
            this.Converted = null;
        }

        private void SaveClick(object sender, EventArgs e)
        {
            if (this.Converted == null)
                return;
            this.saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Converted.Save(this.saveFileDialog1.FileName);
        }


        private void originalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Image = this.Original;
        }

        private void convertedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Converted == null)
            {
                ConvertClick(this, null);
            }
            else
            {
                this.pictureBox1.Image = this.Converted;
            }
        }

        double b_average = 0.5, b_sigma = 0.1;
        private unsafe void feedbackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double b;
            using (var proc_original = new BmpProc32(Original))
            {
                for (int y = 0; y < Height; ++y)
                {
                    for (int x = 0; x < Width; ++x)
                    {
                        b = proc_original.GetPixel(x, y).GetBrightness();
                        b_average += b;
                        b_sigma += b * b;
                    }
                }
                b_average /= Width * Height;
                b_sigma = Math.Sqrt(b_sigma / (Width * Height) - b_average * b_average);
            }

        }

        private void brightnessToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {

            this.toolStripTextBox1.Text = b_average.ToString();
            this.toolStripTextBox2.Text = b_sigma.ToString();
        }


        private void brightnessToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                this.b_sigma = double.Parse(this.toolStripTextBox2.Text);
                //this.averageToolStripMenuItem.HideDropDown();
            }
            catch { }
            try
            {
                this.b_average = double.Parse(this.toolStripTextBox1.Text);
                //this.averageToolStripMenuItem.HideDropDown();
            }

            catch { }
        }


    }



}


