using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace sanduhr_test
{


    public partial class Form1 : Form
    {
        Bitmap image;
        Bitmap imageLeds;
        
        Random rand = new Random((int)DateTime.Now.Ticks);
        
        Boolean[,] sand = new bool[64, 64];
        Boolean[,] mask = new bool[64, 64];

        int[,] leds = new int[8, 8];

        float angle = 0;
        int sand_count = 1000;

        private static System.Timers.Timer aTimer;
        private static System.Timers.Timer bTimer;
       
        public Form1()
        {
            InitializeComponent();
            image = new Bitmap(pictureBox1.ClientRectangle.Width, pictureBox1.ClientRectangle.Height);
            imageLeds = new Bitmap(pictureBox1.ClientRectangle.Width, pictureBox1.ClientRectangle.Height);
            int putted_sand = 0;
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (x < 8 && y < 8)
                    {
                        leds[y, x] = 0;
                    }

                    mask[y, x] = Properties.Resources.mask2.GetPixel(x, y) != Color.FromArgb(0, 0, 0);
                    if (mask[y, x] && (putted_sand < sand_count)) 
                    {
                        putted_sand++;
                        sand[y, x] = true;
                    }
                    else
                    {
                        sand[y, x] = false;
                    }
                }
            }

           
            aTimer = new System.Timers.Timer(10);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEventA;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            bTimer = new System.Timers.Timer(100);
            // Hook up the Elapsed event for the timer. 
            bTimer.Elapsed += OnTimedEventB;
            bTimer.AutoReset = true;
            bTimer.Enabled = true;
   

        }

        private void OnTimedEventA(Object source, ElapsedEventArgs e)
        {

            gravity2();

        }
        private void OnTimedEventB(Object source, ElapsedEventArgs e)
        {

            redraw();

        }

        private void redraw(){
            Brush brush;
            Graphics g;


            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    leds[y, x] = 0;
                    
                }
            }

            g = Graphics.FromImage(image);
            brush = new SolidBrush(Color.FromArgb(240, 240, 240));
            g.FillRectangle(brush, 0, 0, 256, 256);
            g.TranslateTransform(128.0F, 128.0F);
            g.RotateTransform((float)(angle - 45));
            g.ScaleTransform(2.0F, 2.0F);
            
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {                   
                    if(mask[y, x])
                    {
                        if(sand[y, x])
                        {
                            brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                            leds[y / 8, x / 8]++;
                        }
                        else
                        {
                            brush = new SolidBrush(Color.FromArgb(80, 80, 80));                                                  
                        }                        
                    }
                    else
                    {
                        brush = new SolidBrush(Color.FromArgb(240, 240, 240));
                        leds[y / 8, x / 8] = -1;
                    }
                    g.FillRectangle(brush, x - 32, y - 32, 1, 1);
                }
            }



            g = Graphics.FromImage(imageLeds);
            brush = new SolidBrush(Color.FromArgb(240, 240, 240));
            g.FillRectangle(brush, 0, 0, 256, 256);
            g.TranslateTransform(128.0F, 128.0F);
            g.RotateTransform((float)(angle - 45));
            g.ScaleTransform(2.0F, 2.0F);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (leds[y, x] == -1)
                    {
                        brush = new SolidBrush(Color.FromArgb(240, 240, 240));
                    }
                    else
                    {
                        //int value = ((leds[y, x] * leds[y, x]) - 1) / 16;
                        int value = (int)((float)leds[y, x] * 3.99);
                        brush = new SolidBrush(Color.FromArgb(value, value, 0));
                    
                    }
                                        
                    g.FillRectangle(brush, (x * 8) - 32, (y * 8) - 32, 8, 8);
                    

                }
            }

            pictureBox1.Image = image;
            pictureBox2.Image = imageLeds;

        }

        

        void gravity2()
        {
            int[] probabilities = { 0, 0, 0, 0, 0, 0, 0, 0 };
            int rand_max = 0;
            for (int i = 0; i < 8; i++)
            {
                probabilities[i] = probability(angle, i);
                rand_max += probabilities[i];
            }

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (mask[y, x] && sand[y, x])
                    {
                        int choosen_dir = -1;
                        int rand_num = rand.Next(rand_max);

                        for (int i = 0; i < 8; i++)
                        {
                            rand_num -= probabilities[i];
                            if (rand_num < 0)
                            {
                                choosen_dir = i;
                                break;
                            }
                        }

                        int offs_x = (int)Math.Round(Math.Cos((choosen_dir - 3) * Math.PI / 4));
                        int offs_y = (int)Math.Round(Math.Cos((choosen_dir - 1) * Math.PI / 4));

                        for (int i = probabilities[choosen_dir] / 15; i > 1; i--)
                        {
                            int check_x = (offs_x * i) + x;
                            int check_y = (offs_y * i) + y;

                            if ((check_x >= 0) && (check_x < 64) &&
                                (check_y >= 0) && (check_y < 64))
                            {
                                if (mask[check_y, check_x] && !sand[check_y, check_x])
                                {
                                    sand[y, x] = false;
                                    sand[check_y, check_x] = true;
                                    break;
                                }
                            }
                        }

                    }
                }
            }


        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
            angle = trackBar1.Value;
        }


        private static int probability(float gravity_angle, float neighbor_angle)
        {
            //each val in 0- 360
            while (gravity_angle < 0) { gravity_angle += 360; }
            while (gravity_angle > 360) { gravity_angle -= 360; }
            while (neighbor_angle < 0) { neighbor_angle += 360; }
            while (neighbor_angle > 360) { neighbor_angle -= 360; }
         

            float ret;

            ret = Math.Abs(gravity_angle - neighbor_angle);
            if (ret > 180)
            {
                ret = 360 - ret;
            }

            ret = 10 * (180 - ret) / 180; // ret in 0-10

            return (int)(ret*10); //0-100
        }

        private static int probability(float gravity_angle, int dir)
        {
            return probability(gravity_angle, (float)(dir * 45));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int added = 0;
            int count = 0;
            while ((added < 100) && (count < 1000))
            {
                count++;
                int x = rand.Next(45, 64); 
                int y = rand.Next(20);
                if (mask[y, x] && !sand[y, x])
                {
                    added++;
                    sand[y, x] = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int removed = 0;
            int count = 0;
            while ((removed < 100) && (count < 1000))
            {
                count++;
                int x = rand.Next(20);
                int y = rand.Next(45, 64);
                if (sand[y, x])
                {
                    removed++;
                    sand[y, x] = false;
                }
            }
        }
    }
}
