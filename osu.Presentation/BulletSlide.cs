using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;

namespace osu.Presentation
{
    public class BulletSlide : SlideWithTitle
    {
        public BulletSlide(string title, string[] bullets)
            : base(title)
        {
            TextFlowContainer textFlow;

            Content.Add(textFlow = new TextFlowContainer(s => s.Font = OsuFont.Default.With(size: 50))
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            });

            foreach (var b in bullets)
                textFlow.AddParagraph("• " + b);
        }
    }
}