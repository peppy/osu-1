// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Configuration;

namespace osu.Game.Screens.Play
{
    public abstract class GameplayOffsetClockContainer : Container
    {
        /// <summary>
        /// The decoupled clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        protected readonly DecoupleableInterpolatingFramedClock AdjustableClock;

        /// <summary>
        /// The clock which should be exposed and used for display purposes.
        /// </summary>
        protected readonly FramedOffsetClock FinalConsumableOffsetClock;

        private readonly FramedOffsetClock platformOffsetClock;

        private double totalOffset => FinalConsumableOffsetClock.Offset + platformOffsetClock.Offset;

        private Bindable<double> userAudioOffset;

        protected GameplayOffsetClockContainer()
        {
            AdjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            platformOffsetClock = new HardwareCorrectionOffsetClock(AdjustableClock) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            FinalConsumableOffsetClock = new HardwareCorrectionOffsetClock(platformOffsetClock);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.BindValueChanged(offset => FinalConsumableOffsetClock.Offset = offset.NewValue, true);
        }

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// <remarks>
        /// Adjusts for any offsets which have been applied (so the seek may not be the expected point in time on the underlying audio track).
        /// </remarks>
        /// </summary>
        /// <param name="time">The destination time to seek to.</param>
        public virtual void Seek(double time)
        {
            // remove the offset component here because most of the time we want the seek to be aligned to gameplay, not the audio track.
            // we may want to consider reversing the application of offsets in the future as it may feel more correct.
            AdjustableClock.Seek(time - totalOffset);

            // manually process frame to ensure GameplayClock is correctly updated after a seek.
            FinalConsumableOffsetClock.ProcessFrame();
        }

        protected override void Update()
        {
            FinalConsumableOffsetClock.ProcessFrame();
            base.Update();
        }

        private class HardwareCorrectionOffsetClock : FramedOffsetClock
        {
            // we always want to apply the same real-time offset, so it should be adjusted by the difference in playback rate (from realtime) to achieve this.
            // base implementation already adds offset at 1.0 rate, so we only add the difference from that here.
            public override double CurrentTime => base.CurrentTime + Offset * (Rate - 1);

            public HardwareCorrectionOffsetClock(IClock source, bool processSource = true)
                : base(source, processSource)
            {
            }
        }
    }
}
