using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 哲球棋
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            KeyPreview = true;
            QEnv = new List<Env>();
            this.AutoSize = true;
            SetBall(new Point(7, 9), true);
            Refresh();
            float fontsize = 12;
            Label A_Label = new Label() { Text = "A方", Font = new Font(this.Font.Name, fontsize) };
            Label B_Label = new Label() { Text = "B方", Font = new Font(this.Font.Name, fontsize) };
            B_Label.Location = new Point(this.Width / 2, (int)fontsize);
            A_Label.Location = new Point(this.Width / 2, this.Height - (int)fontsize);
            Controls.Add(A_Label);
            Controls.Add(B_Label);
        }
        bool Switch = true;
        int scale = 50;


        void Refresh()
        {
            
            int W = 14 * scale, H = 18 * scale;
            bmp = new Bitmap(W, H);
            G = Graphics.FromImage(bmp);
            
            for (int i = 0; i < 18; i++)
            {
                G.DrawLine(new Pen(Color.Black, 0.5f),
                    0, i * scale,
                    W, i * scale);
            }
            for (int i = 0; i < 14; i++)
            {
                G.DrawLine(new Pen(Color.Black, 0.5f),
                      i * scale, 0,
                      i * scale, H);
            }

            G.FillEllipse(Brushes.Gray, (MouseNow.X * scale) - scale / 2, (MouseNow.Y * scale) - scale / 2, scale, scale);

            foreach (var item in QEnv)
            {
                if (item.Type == "Member")
                {
                    G.FillEllipse(Brushes.Blue,
                                (item.P.X * scale) - scale / 2,
                                (item.P.Y * scale) - scale / 2,
                                scale, scale);
                }
                else if (item.Type == "Ball")
                {
                    G.FillEllipse(Brushes.Red,
                                (item.P.X * scale) - scale / 2,
                                (item.P.Y * scale) - scale / 2,
                                scale, scale);
                }
            }
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Image = bmp;

            
        }
        Bitmap bmp;
        Graphics G;
        List<Env> QEnv = new List<Env>();
        List<List<Env>> EveryStep = new List<List<Env>>();
        Point Ball = new Point(14 / 2, 18 / 2);
        private void Form1_Load(object sender, EventArgs e)
        {

        }
       
        //EveryStep.Add(new List<Env>(QEnv));
        //int i = QEnv.FindIndex(x => x.P.X == P.X && x.P.Y == P.Y);
        bool SetBall(Point P, bool SetBall)
        {
            if (SetBall)
            {
                bool EatOnlineMemberWin = false;
                int i = QEnv.FindIndex(x => x.P.X == P.X && x.P.Y == P.Y);
                if (i >= 0)
                {
                    if (QEnv[i].P.Y == 0 || QEnv[i].P.Y == 18)
                    {
                        EatOnlineMemberWin = true;
                    }
                    else
                    {
                        return false;
                    }
                }
                
                //now ball
                //先找出現在球在哪裡
                //再判斷是否在直線上
                //現在想放到P點
                //查Qenv中前個Ball到P是否是直線
                Env BeforeBall = QEnv.Find(x => x.Type == "Ball");
                bool checktemp = true;
                List<Point> EatenList = new List<Point>();
                if (BeforeBall != null)
                {
                    double xc = BeforeBall.P.X - P.X;
                    double yc = BeforeBall.P.Y - P.Y;
                    double angletemp = Math.Atan2(yc, xc) * 180 / Math.PI;
                    if (angletemp % 45 == 0)//是直線
                    {
                        //確定直線上皆有Member可連吃
                        if (angletemp == 0)
                            checktemp = CheckEat(BeforeBall.P, P, new Point(-1, 0), false, EatenList, EatOnlineMemberWin);
                        if (angletemp == 45)
                            checktemp = CheckEat(BeforeBall.P, P, new Point(-1, -1), false, EatenList, EatOnlineMemberWin);
                        if (angletemp == 90)
                            checktemp = CheckEat(BeforeBall.P, P, new Point(0, -1), false, EatenList, EatOnlineMemberWin);
                        if (angletemp == 135)
                            checktemp = CheckEat(BeforeBall.P, P, new Point(1, -1), false, EatenList, EatOnlineMemberWin);
                        if (angletemp == 180)
                            checktemp = CheckEat(BeforeBall.P, P, new Point(1, 0), false, EatenList, EatOnlineMemberWin);
                        if (angletemp == -135)
                            checktemp = CheckEat(BeforeBall.P, P, new Point(1, 1), false, EatenList, EatOnlineMemberWin);
                        if (angletemp == -90)
                            checktemp = CheckEat(BeforeBall.P, P, new Point(0, 1), false, EatenList, EatOnlineMemberWin);
                        if (angletemp == -45)
                            checktemp = CheckEat(BeforeBall.P, P, new Point(-1, 1), false, EatenList, EatOnlineMemberWin);
                    }
                    else
                    {
                        checktemp = false;
                    }
                }
                
                if (checktemp)
                {
                    EveryStep.Add(new List<Env>(QEnv));
                    QEnv.RemoveAll(x => (EatenList.FindIndex(y => x.P.X == y.X && x.P.Y == y.Y) >= 0));
                    QEnv.RemoveAll(x => x.Type == "Ball");
                    QEnv.Add(new Env(P, "Ball"));
                }
                else
                {
                    MessageBox.Show("母湯");
                }
            }
            else
            {
                int i = QEnv.FindIndex(x => x.P.X == P.X && x.P.Y == P.Y);
                if (i >= 0) return false;
                EveryStep.Add(new List<Env>(QEnv));
                button1_Click(null, null);
                QEnv.Add(new Env(P, "Member"));
            }
            return true;
        }
        Point MouseNow = new Point(0, 0);

        bool CheckEat(Point Now, Point End, Point Vector,bool HaveEatenAtLeastOneChess,List<Point> EatenList,bool eatonlinememberwin)
        {
            if (Now.X + Vector.X < 0 || Now.X + Vector.X > 14 ||
                Now.Y + Vector.Y < 0 || Now.Y + Vector.Y > 18)
            {
                return false;
            }
            Point NP = new Point(Now.X + Vector.X, Now.Y + Vector.Y);
            int temp = QEnv.FindIndex(x => x.P.X == NP.X && x.P.Y == NP.Y && x.Type == "Member");
            if (temp >= 0)
            {
                EatenList.Add(new Point(NP.X, NP.Y));
                if (NP.X == End.X && NP.Y == End.Y) return true;
                return CheckEat(NP, End, Vector, true, EatenList, eatonlinememberwin);
            }
            if (HaveEatenAtLeastOneChess && NP.X == End.X && NP.Y == End.Y) return true;
            return false;
        }

        int CheckWinner()
        {
            int index = QEnv.FindIndex(x => x.Type == "Ball");
            if (QEnv[index].P.Y == 0)
            {
                return 0;
            }
            if (QEnv[index].P.Y == 18)
            {
                return 1;
            }
            return -1;
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            double x = Math.Round((double)e.X / (double)scale);
            double y = Math.Round((double)e.Y / (double)scale);
            int xi = (int)x;
            int yi = (int)y;
            MouseNow = new Point(xi, yi);
            label1.Text = (Switch ? "A方：" : "B方：") + MouseNow.ToString();
            Refresh();
        }
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (SetBall(MouseNow, (e.Button == MouseButtons.Left ? false : true)))
            {
                int winner = CheckWinner();
                if (winner == 0)
                {
                    MessageBox.Show("A方Win");
                }
                else if (winner == 1)
                {
                    MessageBox.Show("B方Win");
                }
                if (winner >= 0) Application.Restart();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Switch = !Switch;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show(e.KeyData.ToString());
            if (e.KeyData.ToString() == "Z, Control" && EveryStep.Count > 1)
            {
                QEnv = EveryStep.Last();
                EveryStep.RemoveAt(EveryStep.Count - 1);
                Refresh();
            }
        }


    }
    class Env
    {
        public Point P;
        public string Type;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="type">Member,Ball</param>
        public Env(Point p, string type)
        {
            P = new Point(p.X, p.Y);
            Type = type;
        }
    }

 
}
