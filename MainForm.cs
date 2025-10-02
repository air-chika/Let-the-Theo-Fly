using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

namespace Let_the_Theo_Fly
{
    public partial class MainForm : Form
    {
        public Game game = null;
        private List<Keys> legalKeys = new List<Keys>() { Keys.Left, Keys.Down, Keys.Right, Keys.Z, Keys.X };
        SoundPlayer player;

        public MainForm()
        {
            InitializeComponent();
            Focus();
            Icon = Pictures.avcib_mgmhe_001;
        }

        private void FrameClock_Tick(object sender, EventArgs e)
        {
            game.AddCount();
            game.MainLoop();
            TimeLabel.Text = $"Time: {game.RemainTime}s";
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (legalKeys.Contains(e.KeyCode))
                game?.PassKeyDown(e.KeyCode);
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (legalKeys.Contains(e.KeyCode))
                game?.PassKeyUp(e.KeyCode);
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            ScoreLabel.Visible = true;
            TimeLabel.Visible = true;
            FinalLabel.Visible = false;

            StartGameButton.Visible = false;
            player = new SoundPlayer(Pictures.game);
            player.Load();
            player.Play();

            game = new Game(this);
            game.Start();
            FrameClock.Start();
            GameTime.Start();
        }

        private void GameTime_Tick(object sender, EventArgs e)
        {
            if (--game.RemainTime == 0)
            {
                FrameClock.Stop();

                ScoreLabel.Visible = false;
                TimeLabel.Visible = false;
                FinalLabel.Visible = true;
                FinalLabel.Text = $"Your Score: {game.Score}";

                StartGameButton.Visible = true;

                game.Remove();

                if (game.Score < 10)
                   // player = new SoundPlayer("win1.wav");
                    player = new SoundPlayer(Pictures.win1);
                else
                    player = new SoundPlayer(Pictures.win2);
                player.Load();
                player.Play();
            }
            else if (game.RemainTime == 40)
            {
                game.AddSino();
            }
            else if (game.RemainTime == 20)
            {
                game.AddBadeline();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
