using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Let_the_Theo_Fly
{
    public class Player : Entity
    {
        #region States
        public bool Reborning = false;
        private bool Invincible = false;

        private bool TryJump = false;
        private bool TryCatch = false;
        private bool TryThrow = false;
        private bool TryPutDown = false;
        private bool Catching = false;

        public bool FacingRight = true;

        private int RebornTime = 0;
        private int InvincibleTime = 0;
        #endregion

        #region Configs
        private double JumpYSpeed;
        private double MaxXSpeed;
        private double MaxYSpeed;

        public static int MaxRebornTime = 60;
        private static int MaxInvincibleTime = 60;
        #endregion

        private Image Mirrored;
        private Image Death;

        public Player(
            Point position,
            int picWidth,
            int picHeight,
            int width,
            int height,
            MainForm form,
            Game game) : base(position, picWidth, picHeight, width, height, true, true, form, game)
        {
            OnFloor = true;
            Alive = true;

            JumpYSpeed = 4 * game.Unit;
            MaxXSpeed = 2 * game.Unit;
            MaxYSpeed = 4 * game.Unit;

            Pic = Pictures.Madeline_Normal;
            Frame.Image = Pic;

            Mirrored = Pictures.Madeline_Normal;
            Mirrored.RotateFlip(RotateFlipType.RotateNoneFlipX);
            Death = Pictures.Madeline_Death;
        }

        public override void Submit()
        {
            // 正在复活
            if (!Alive)
            {
                // 复活完成
                if (--RebornTime == 0)
                {
                    Alive = true;
                    Invincible = true;

                    InvincibleTime = MaxInvincibleTime;

                    PrePosition = Position = new Point(game.ClientSize.Width / 5, game.ClientSize.Height - 10 * game.Unit);

                    Frame.Image = Pic;
                }
                // 否则空过
                else
                    return;
            }

            // 检测无敌
            if (Invincible && --InvincibleTime == 0)
                Invincible = false;

            if (Speed.X < 0)
            {
                FacingRight = false;
                Frame.Image = Mirrored;
            }
            else if (Speed.X > 0)
            {
                FacingRight = true;
                Frame.Image = Pic;
            }

            Position.X = Clamp(Position.X + Speed.X, HitBox.Width / 2, game.ClientSize.Width - HitBox.Width / 2);
            Position.Y = Convert.ToInt32(Position.Y + Speed.Y);

            // 检测落地
            if (Position.Y > game.floor.HitBox.Top)
            {
                OnFloor = true;
                Position.Y = game.floor.HitBox.Top;
                Speed.Y = 0;
            }
            // 重力
            if (!OnFloor)
                Speed.Y = Convert.ToInt32(Math.Min(Speed.Y + Gravity, MaxYSpeed));

            // 尝试跳跃
            if (TryJump)
            {
                TryJump = false;
                OnFloor = false;
                Speed.Y = -JumpYSpeed;
            }
            // 尝试抓取
            if (TryCatch && game.theo.TryCatch())
            {
                TryCatch = false;
                Catching = true;
            }
            // 尝试扔出
            if (Catching && TryThrow)
            {
                TryThrow = false;
                TryPutDown = false;
                Catching = false;
                game.theo.Throw(FacingRight);
            }
            // 尝试放下
            if (Catching && TryPutDown)
            {
                TryThrow = false;
                TryPutDown = false;
                Catching = false;
                game.theo.PutDown();
            }

            // 适应新位置
            AdaptLocation();

            // 重绘
            Frame.Invalidate();
        }

        public void Kill()
        {
            // 不是无敌状态
            if (!Invincible)
            {
                // 重置状态
                Alive = false;

                TryJump = false;
                TryCatch = false;
                TryThrow = false;
                TryPutDown = false;
                FacingRight = true;
                OnFloor = true;
                if (Catching)
                {
                    Catching = false;
                    game.theo.PutDown();
                }

                Speed.X = Speed.Y = 0;

                Frame.Image = Death;

                // 记录重生
                Reborning = true;
                RebornTime = MaxRebornTime;

                game.form.ScoreLabel.Text = $"Score: {--game.Score}";
            }
        }

        public void KeyDown(Keys key)
        {
            if (Alive)
            {
                switch (key)
                {
                    case Keys.Left:
                        if (Speed.X >= 0)
                            Speed.X = -MaxXSpeed;
                        break;
                    case Keys.Down:
                        if (Catching)
                            TryPutDown = true;
                        break;
                    case Keys.Right:
                        if (Speed.X <= 0)
                            Speed.X = MaxXSpeed;
                        break;
                    case Keys.Z:
                        if (OnFloor)
                            TryJump = true;
                        break;
                    case Keys.X:
                        if (!Catching)
                            TryCatch = true;
                        break;
                }
            }
        }

        public void KeyUp(Keys key)
        {
            if (Alive)
            {
                switch (key)
                {
                    case Keys.Left:
                        if (Speed.X < 0)
                            Speed.X = 0;
                        break;
                    case Keys.Down:
                        if (TryPutDown)
                            TryPutDown = false;
                        break;
                    case Keys.Right:
                        if (Speed.X > 0)
                            Speed.X = 0;
                        break;
                    case Keys.Z:
                        break;
                    case Keys.X:
                        if (Catching)
                            TryThrow = true;
                        if (TryCatch)
                            TryCatch = false;
                        break;
                }
            }
        }
    }
}
