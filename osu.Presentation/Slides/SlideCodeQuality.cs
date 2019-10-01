namespace osu.Presentation.Slides
{
    public class SlideCodeQuality : BulletSlide
    {
        public SlideCodeQuality()
            : base("code quality", new[]
            {
                "review, approve, merge",
                "documentation on all public classes/methods",
                "rewrite, refactor until happy",
                "heavily unit tested",
            })
        {
        }
    }
}
