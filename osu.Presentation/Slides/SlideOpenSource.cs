namespace osu.Presentation.Slides
{
    public class SlideOpenSource : BulletSlide
    {
        public SlideOpenSource()
            : base("open source", new[]
            {
                "involving the community in development",
                "(close to) 100% transparency",
                "visibility",
                "redundancy",
            })
        {
        }
    }

    public class SlideOpenSourceContributors : SlideWithImage
    {
        public SlideOpenSourceContributors()
            : base("contributors / bounty", "bounty")
        {
        }
    }

    public class SlideOpenSourceContributors2 : SlideWithImage
    {
        public SlideOpenSourceContributors2()
            : base("contributors / bounty", "bounty2")
        {
        }
    }
}
