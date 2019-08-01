namespace osu.Presentation.Slides
{
    public class SlideHighPerformance : BulletSlide
    {
        public SlideHighPerformance()
            : base("high performance", new[]
            {
                "multithreaded",
                "minimal draw frames",
                "zero allocations",
                "modern optimisations",
                "eager loading / delayed loading",
            })
        {
        }
    }
}