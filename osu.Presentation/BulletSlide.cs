using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Input;

namespace osu.Presentation
{
    public class BulletSlide : SlideWithTitle
    {
        private readonly string[] bullets;
        private readonly TextFlowContainer textFlow;

        public BulletSlide(string title, string[] bullets)
            : base(title)
        {
            this.bullets = bullets;

            Content.Add(textFlow = new TextFlowContainer(s => s.Font = OsuFont.Default.With(size: 50))
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                LineSpacing = 0.8f,
            });

            showBullets(0);
        }

        private bool showBullets(int count)
        {
            bulletsDisplayed = MathHelper.Clamp(count, 0, bullets.Length);
            textFlow.Clear();
            foreach (var b in bullets.Take(bulletsDisplayed))
                textFlow.AddParagraph("• " + b);

            return bulletsDisplayed == count;
        }

        private int bulletsDisplayed = 1;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            switch (e.Key)
            {
                case Key.Left:
                    return showBullets(bulletsDisplayed - 1);

                case Key.Right:
                    return showBullets(bulletsDisplayed + 1);
            }

            return base.OnKeyDown(e);
        }
    }
}
