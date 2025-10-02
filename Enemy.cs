using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Let_the_Theo_Fly
{
    public class Badeline : Enemy
    {
        private Image Mirrored;

        private Queue<Memory> memory = new Queue<Memory>();

        private static int MaxWaitTime = 60;

        public Badeline(
            Point position,
            int picWidth,
            int picHeight,
            int width,
            int height,
            MainForm form,
            Game game) : base(position, picWidth, picHeight, width, height, form, game)
        {
            Alive = true;

            Pic = Pictures.Badeline;
            Mirrored = Pictures.Badeline;
            Mirrored.RotateFlip(RotateFlipType.RotateNoneFlipX);

            Frame.Image = Pic;

            // 初始路径
            Vector SubPath = Vector.GetDirection(Position, game.player.Position);
            SubPath.X /= MaxWaitTime;
            SubPath.Y /= MaxWaitTime;
            bool FacingRight = game.player.Position.X - Position.X >= 0 ? true : false;
            for (int i = 0; i < MaxWaitTime; i++)
                memory.Enqueue(
                    new Memory(
                        new Point(
                            (int)(Position.X + SubPath.X * i),
                            (int)(Position.Y + SubPath.Y * i)),
                        FacingRight));
        }

        public override void Submit()
        {
            // 玩家存活记录位置
            if (game.player.Alive)
            {
                Alive = true;
                memory.Enqueue(new Memory(game.player.Position, game.player.FacingRight));
            }
            // 否则死亡时按复活时间逼近死亡点
            else if (Alive)
            {
                Alive = false;

                Vector SubPath = Vector.GetDirection(Position, game.RebornPoint);
                SubPath.X /= Player.MaxRebornTime;
                SubPath.Y /= Player.MaxRebornTime;
                bool FacingRight = game.RebornPoint.X - Position.X >= 0 ? true : false;
                for (int i = 0; i < Player.MaxRebornTime; i++)
                    memory.Enqueue(
                        new Memory(
                            new Point(
                                (int)(Position.X + SubPath.X * i),
                                (int)(Position.Y + SubPath.Y * i)),
                            FacingRight));
            }

            Memory next = memory.Dequeue();

            Position = next.Position;
            Frame.Image = next.FacingRight ? Pic : Mirrored;

            AdaptLocation();

            TryKill();

            Frame.Invalidate();
        }

        public override bool TryKill()
        {
            if (base.TryKill())
            {
                game.player.Kill();
                return true;
            }

            return false;
        }

        public struct Memory
        {
            public Point Position;
            public bool FacingRight;

            public Memory(Point position, bool facingRight)
            {
                Position = position;
                FacingRight = facingRight;
            }
        }
    }

    public class Sino : Enemy
    {
        private Image Attack;
        private Image Death;
        private Image MirroredAttack;
        private Image MirroredPic;

        private static int MaxWaitTime = 180;
        private static int MaxDashTime = 60;
        private static int MaxRebornTime = 180;

        private int WaitTime = 240;
        private int DashTime = 0;
        private int RebornTime = 0;

        private double DashSpeed;
        private double WaitSpeed;

        private bool Dashing = false;
        private bool Killed = false;
        private bool FacingRight = true;

        public Sino(
            Point position,
            int picWidth,
            int picHeight,
            int width,
            int height,
            MainForm form,
            Game game) : base(position, picWidth, picHeight, width, height, form, game)
        {
            Alive = true;

            DashSpeed = 3 * game.Unit;
            WaitSpeed = 0.5 * game.Unit;

            Attack = Pictures.Sino_Attack;
            Death = Pictures.Sino_Death;

            Pic = Pictures.Sino_Wait;
            MirroredPic = Pictures.Sino_Wait;
            MirroredPic.RotateFlip(RotateFlipType.RotateNoneFlipX);
            MirroredAttack = Pictures.Sino_Attack;
            MirroredAttack.RotateFlip(RotateFlipType.RotateNoneFlipY);

            FacingRight = Position.X < game.player.Position.X;

            Frame.Image = FacingRight ? Pic : MirroredPic;
        }

        public override void Submit()
        {
            // 刚刚被击杀 记录击杀并移动
            if (Killed)
            {
                // 重置状态与计时
                Killed = false;
                Alive = false;
                Dashing = false;

                WaitTime = MaxWaitTime;
                DashTime = 0;
                RebornTime = MaxRebornTime;

                // 让玩家跳跃
                game.player.Speed.Y = -6 * game.Unit;

                Frame.Image = Death;
            }
            // 正在复活
            else if (!Alive)
            {
                PrePosition = Position = Speed.Move(Position);

                // 复活时间到
                if (--RebornTime == 0)
                {
                    Alive = true;
                    Frame.Image = FacingRight ? Pic : MirroredPic;
                }
            }
            // 正在冲刺
            else if (Dashing)
            {
                PrePosition = Position;
                Position = Speed.Move(Position);

                // 冲刺结束
                if (--DashTime == 0)
                {
                    Dashing = false;
                    WaitTime = MaxWaitTime;
                    Frame.Image = FacingRight ? Pic : MirroredPic;
                }

                TryKill();
            }
            // 正在等待
            else
            {
                PrePosition = Position;
                Position = Speed.Move(Position);

                // 等待结束
                if (--WaitTime == 0)
                {
                    Dashing = true;
                    DashTime = MaxDashTime;
                    Frame.Image = FacingRight ? Attack : MirroredAttack;

                    Speed = Vector.GetDirection(Position, game.player.Position).FixedVector(DashSpeed);
                }
                else
                {
                    Speed = Vector.GetDirection(Position, game.player.Position).FixedVector(WaitSpeed);
                    FacingRight = game.player.Position.X > Position.X;
                    Frame.Image = FacingRight ? Pic : MirroredPic;
                }

                TryKill();
            }

            // 边框检测
            if (HitBox.Top < 0)
            {
                Position.Y = HitBox.Height - HitBox.Top;
                Speed.Y = - Speed.Y;
            }
            if (HitBox.Right > game.ClientSize.Width)
            {
                Position.X = 2 * game.ClientSize.Width - HitBox.Right - HitBox.Width / 2;
                Speed.X = -Speed.X;
            }
            if (HitBox.Bottom > game.floor.HitBox.Top)
            {
                Position.Y = 2 * game.floor.HitBox.Top - HitBox.Bottom;
                Speed.Y = -Speed.Y;
            }
            if (HitBox.Left < 0)
            {
                Position.X = HitBox.Width / 2 - HitBox.Left;
                Speed.X = -Speed.X;
            }

            AdaptLocation();
            Frame.Invalidate();
        }

        public override bool TryKill()
        {
            // 尝试击杀玩家
            if (base.TryKill())
            {
                // 被击杀
                if (BeKilled())
                {
                    Killed = true;
                    Dashing = false;
                    Frame.Image = Death;

                    return false;
                }
                // 击杀玩家
                else
                {
                    game.player.Kill();
                    return true;
                }
            }
            else
                return false;
        }

        private bool BeKilled()
        {
            return PreHitBox.Top > game.player.PreHitBox.Bottom;
        }
    }

    public class Enemy : Entity
    {
        public Enemy(
            Point position,
            int picWidth,
            int picHeight,
            int width,
            int height,
            MainForm form,
            Game game) : base(position, picWidth, picHeight, width, height, true, false, form, game)
        {
        }

        public virtual bool TryKill()
        {
            if (!Alive || !game.player.Alive)
                return false;

            if (CheckHit(game.player))
                return true;

            return false;
        }
    }
}
