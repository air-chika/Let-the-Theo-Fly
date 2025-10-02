using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.IO;

namespace Let_the_Theo_Fly
{
    public class Floor : Entity
    {
        public Floor(
            Point position,
            int picWidth,
            int picHeight,
            int width,
            int height,
            MainForm form,
            Game game) : base(position, picWidth, picHeight, width, height, false, false, form, game)
        {
            Pic = Pictures.Floor;
            Frame.Image = Pic;
        }
    }

    public class Theo : Entity
    {
        private bool Catched = false;
        private bool Throwing = false;

        private Rectangle CatchBox;

        private Random random = new Random();

        private static double ThrowSpeedX;
        private static double ThrowSpeedY;
        private static double MaxYSpeed;

        public Theo(
            Point position,
            int picWidth,
            int picHeight,
            int width,
            int height,
            int catchWidth,
            int catchHeight,
            MainForm form,
            Game game) : base(position, picWidth, picHeight, width, height, true, true, form, game)
        {
            CatchBox.Size = new Size(catchWidth, catchHeight);
            CatchBox.Location = GetFixedPoint(CatchBox.Size);

            OnFloor = true;

            ThrowSpeedX = 9 * game.Unit * 0.5;
            ThrowSpeedY = 8 * game.Unit * 0.5;
            MaxYSpeed = 8 * game.Unit * 0.5;

            Pic = Pictures.Theo;
            Frame.Image = Pic;
        }

        public override void Submit()
        {
            // 在地上 跳过
            if (OnFloor)
                return;

            // 被抓住 跟随角色移动
            if (Catched)
            {
                PrePosition = Position;
                Position = new Point(game.player.Position.X, game.player.Position.Y - game.player.HitBox.Size.Height);
                AdaptLocation();
                return;
            }

            // 移动中 结算移动并检测碰撞
            PrePosition = Position;
            Position = Speed.Move(Position);
            Speed.Y = Math.Min(Speed.Y + Gravity, MaxYSpeed);

            // 撞到边框
            if (HitBox.Left < 0)
            {
                Position.X = -HitBox.Left + HitBox.Width / 2;
                Speed.X = -Speed.X;
            }
            else if (HitBox.Right > game.ClientSize.Width)
            {
                Position.X = 2 * game.ClientSize.Width - HitBox.Right - HitBox.Width / 2;
                Speed.X = -Speed.X;
            }

            // 撞到地板
            if (CheckHit(game.floor))
            {
                // 如果速度较小直接停下
                if (Speed.Y < MaxYSpeed)
                {
                    OnFloor = true;
                    Throwing = false;

                    Position.Y = game.floor.HitBox.Top;
                    Speed.X = 0;
                    Speed.Y = 0;
                }
                // 否则速度减半并反弹
                else
                {
                    Position.Y = 2 * game.floor.HitBox.Top - Position.Y;
                    Speed.X /= 2;
                    Speed.Y /= -5;
                }
            }

            // 撞到眼球反弹并计分
            if (Throwing && CheckHit(game.eye))
            {
                // 如果从左边撞上
                if (PreHitBox.Right < game.eye.HitBox.Left)
                {
                    Position = PrePosition;

                    //Position.X = 2 * game.eye.HitBox.Left - PreHitBox.Right - HitBox.Width / 2;
                    // 速度向左反向并随机加值
                    Speed.X = -Math.Abs(Speed.X) - random.NextDouble() * 4 - 1;
                    Speed.Y -= random.NextDouble() * 4;
                }
                // 如果从上边撞上
                else if (PreHitBox.Bottom < game.eye.HitBox.Top)
                {
                    Position = PrePosition;

                    //Position.X = PrePosition.X;
                    //Position.Y = 2 * game.eye.HitBox.Top - PreHitBox.Bottom;
                    // 速度向左上反向并随机加值
                    Speed.X = -Math.Abs(Speed.X) - random.NextDouble() * 4 - 1;
                    Speed.Y = -Speed.Y - random.NextDouble() * 8 - 2;
                }
                // 如果从下边撞上
                if (PreHitBox.Top < game.eye.HitBox.Bottom)
                {
                    Position = PrePosition;

                    //Position.X = PrePosition.X;
                    //Position.Y = 2 * game.eye.HitBox.Bottom - PreHitBox.Top + HitBox.Height;
                    // 速度向左下反向并随机加值
                    Speed.X = -Math.Abs(Speed.X) - random.NextDouble() * 4 - 1;
                    Speed.Y = Math.Min(-Speed.Y + random.NextDouble() * 4 + 1, MaxYSpeed);
                }

                // 计分 重置投掷防止多次碰撞
                game.Score++;
                game.form.ScoreLabel.Text = $"Score: {game.Score}";
                Throwing = false;
            }

            // 适应新位置
            AdaptLocation();

            // 重绘
            Frame.Invalidate();
        }

        public void Throw(bool throwToRight)
        {
            Catched = false;
            Throwing = true;
            Speed.X = throwToRight ? ThrowSpeedX : -ThrowSpeedX;
            Speed.Y = -ThrowSpeedY;
        }

        public void PutDown()
        {
            Catched = false;
        }

        public bool TryCatch()
        {
            if (game.player.HitBox.IntersectsWith(CatchBox))
            {
                Catched = true;
                OnFloor = false;
                Throwing = false;
                return true;
            }

            return false;
        }

        public override void AdaptLocation()
        {
            base.AdaptLocation();
            CatchBox.Location = GetFixedPoint(CatchBox.Size);
        }
    }

    public class Eye : Entity
    {
        public Eye(
            Point position,
            int picWidth,
            int picHeight,
            int width,
            int height,
            MainForm form,
            Game game) : base(position, picWidth, picHeight, width, height, false, false, form, game)
        {
            Pic = Pictures.Eye;
            Frame.Image = Pic;
        }

        #region Calculators
        /// <summary>
        /// 令实体位置为碰撞箱右侧中心，获取组件应在的位置
        /// </summary>
        /// <param name="controlSize">组件的大小</param>
        /// <returns>返回计算后的位置</returns>
        protected override Point GetFixedPoint(Size controlSize)
        {
            return new Point(Position.X - controlSize.Width, Position.Y - controlSize.Height / 2);
        }
        #endregion
    }

    public abstract class Entity
    {
        public Point Position;
        protected PictureBox Frame = new PictureBox();
        protected Image Pic;
        public Rectangle HitBox;

        public Point PrePosition;
        public Rectangle PreHitBox;

        public Vector Speed;

        public static double Gravity;

        protected Game game;

        protected bool Movable;
        protected bool CanAffectedByGravity;
        protected bool OnFloor;
        public bool Alive;

        protected Entity(Point position, int picWidth, int picHeight, int width, int height, bool movable, bool canAffectedByGravity, MainForm form, Game game)
        {
            Gravity = game.Unit * 0.25;

            Position = position;
            PrePosition = position;

            form.Controls.Add(Frame);

            Frame.Size = new Size(picWidth, picHeight);
            Frame.Location = GetFixedPoint(Frame.Size);
            Frame.SizeMode = PictureBoxSizeMode.StretchImage;
            HitBox.Size = new Size(width, height);
            HitBox.Location = GetFixedPoint(HitBox.Size);
            PreHitBox.Size = HitBox.Size;
            PreHitBox.Location = HitBox.Location;


            Movable = movable;
            CanAffectedByGravity = canAffectedByGravity;

            Frame.BackColor = Color.Transparent;
            Frame.Parent = form.BackgroundPanel;
            Frame.Visible = true;

            this.game = game;

        }

        public void Remove(Form form)
        {
            Frame.Visible = false;
            form.Controls.Remove(Frame);
        }

        public virtual void Submit()
        {

        }

        #region Calculators
        /// <summary>
        /// 令实体位置为碰撞箱底部中心，获取组件应在的位置
        /// </summary>
        /// <param name="controlSize">组件的大小</param>
        /// <returns>返回计算后的位置</returns>
        protected virtual Point GetFixedPoint(Size controlSize)
        {
            return new Point(Position.X - controlSize.Width / 2, Position.Y - controlSize.Height);
        }
        protected int Clamp(double value, double min, double max)
        {
            return (int)Math.Min(Math.Max(min, value), max);
        }
        protected int Clamp(double value, int min, int max)
        {
            return (int)Math.Min(Math.Max(min, value), max);
        }
        #endregion

        #region Operators
        public bool CheckHit(Entity other)
        {
            return HitBox.IntersectsWith(other.HitBox);
        }
        /// <summary>
        /// 将材质和碰撞箱移动到新的位置上
        /// </summary>
        public virtual void AdaptLocation()
        {
            Frame.Location = GetFixedPoint(Frame.Size);
            PreHitBox.Location = HitBox.Location;
            HitBox.Location = GetFixedPoint(HitBox.Size);
        }
        #endregion
    }

    public struct Vector
    {
        public double X, Y;

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point Move(Point point)
        {
            return new Point((int)(X + point.X), (int)(Y + point.Y));
        }

        public Vector FixedVector(double length)
        {
            if (X == 0 && Y == 0)
                return new Vector(0, 0);

            if (X == 0 && Y != 0)
                return new Vector(0, Y > 0 ? length : -length);

            if (X != 0 && Y == 0)
                return new Vector(X > 0 ? length : -length, 0);

            double ratio = length / Math.Sqrt(X * X + Y * Y);

            return new Vector(ratio * X, ratio * Y);
        }

        public static Vector GetDirection(Point source, Point aim)
        {
            return new Vector(aim.X - source.X, aim.Y - source.Y);
        }
    }
}
