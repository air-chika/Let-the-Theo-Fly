using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Forms;

namespace Let_the_Theo_Fly
{
    public class Game
    {
        public long Count = 0;
        public int Score = 0;

        public MainForm form;
        public Size ClientSize;
        public int Unit;

        public int RemainTime = 60;

        private bool NewFrame = false;

        public Point RebornPoint;

        public Player player;
        public Theo theo;
        public Eye eye;
        public Floor floor;
        public Sino sino;
        public Badeline badeline;

        public Game(MainForm form)
        {
            this.form = form;
            ClientSize = form.ClientSize;
            Unit = ClientSize.Width / 240;

            RebornPoint = new Point(ClientSize.Width / 5, ClientSize.Height - 10 * Unit);

            player = new Player(
                RebornPoint,
                12 * Unit, 12 * Unit, 9 * Unit, 9 * Unit,
                form, this);
            theo = new Theo(
                new Point(ClientSize.Width / 3, ClientSize.Height - 10 * Unit),
                16 * Unit, 16 * Unit, 12 * Unit, 12 * Unit, 18 * Unit, 18 * Unit,
                form, this);
            eye = new Eye(
                new Point(ClientSize.Width, ClientSize.Height / 2),
                36 * Unit, 72 * Unit, 24 * Unit, 24 * Unit,
                form, this);
            floor = new Floor(
                new Point(ClientSize.Width / 2, ClientSize.Height + 20 * Unit),
                3 * ClientSize.Width, 30 * Unit, 3 * ClientSize.Width, 30 * Unit,
                form, this);
        }

        public void AddSino()
        {
            sino = new Sino(
                new Point(ClientSize.Width / 4, ClientSize.Height / 2),
                16 * Unit, 16 * Unit, 12 * Unit, 12 * Unit,
                form, this);
        }

        public void AddBadeline()
        {
            int startX = player.Position.X - 20 * Unit < player.HitBox.Width / 2 ?
                player.Position.X + 20 * Unit : player.Position.X - 20 * Unit;

            badeline = new Badeline(
                new Point(startX, player.Position.Y - 20 * Unit),
                12 * Unit, 12 * Unit, 9 * Unit, 9 * Unit,
                form, this);
        }

        public void Remove()
        {
            player.Remove(form);
            theo.Remove(form);
            eye.Remove(form);
            floor.Remove(form);
            if (sino != null)
                sino.Remove(form);
            if (badeline != null)
                badeline.Remove(form);
        }

        public void AddCount()
        {
            Count++;
            NewFrame = true;
        }

        public void MainLoop()
        {
            if (NewFrame)
            {
                player.Submit();
                theo.Submit();

                if (sino != null)
                    sino.Submit();
                if (badeline != null)
                    badeline.Submit();
            }
        }

        public void PassKeyDown(Keys key)
        {
            player.KeyDown(key);
        }

        public void PassKeyUp(Keys key)
        {
            player.KeyUp(key);
        }

        public void Start()
        {
            form.ScoreLabel.Visible = true;
            form.ScoreLabel.Text = "Score: 0";
            form.TimeLabel.Visible = true;
        }
    }
}
