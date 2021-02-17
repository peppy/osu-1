// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Encapsulates gameplay timing logic and provides a <see cref="Play.GameplayClock"/> for children.
    /// </summary>
    public class GameplayClockContainer : GameplayOffsetClockContainer
    {
        private readonly WorkingBeatmap beatmap;

        [NotNull]
        private ITrack track;

        public readonly BindableBool IsPaused = new BindableBool();

        private readonly double gameplayStartTime;
        private readonly bool startAtGameplayStart;

        private readonly double firstHitObjectTime;

        public readonly BindableNumber<double> UserPlaybackRate = new BindableDouble(1)
        {
            Default = 1,
            MinValue = 0.5,
            MaxValue = 2,
            Precision = 0.1,
        };

        /// <summary>
        /// The final clock which is exposed to underlying components.
        /// </summary>
        public GameplayClock GameplayClock => localGameplayClock;

        [Cached(typeof(GameplayClock))]
        private readonly LocalGameplayClock localGameplayClock;

        /// <summary>
        /// Creates a new <see cref="GameplayClockContainer"/>.
        /// </summary>
        /// <param name="beatmap">The beatmap being played.</param>
        /// <param name="gameplayStartTime">The suggested time to start gameplay at.</param>
        /// <param name="startAtGameplayStart">
        /// Whether <paramref name="gameplayStartTime"/> should be used regardless of when storyboard events and hitobjects are supposed to start.
        /// </param>
        public GameplayClockContainer(WorkingBeatmap beatmap, double gameplayStartTime, bool startAtGameplayStart = false)
        {
            this.beatmap = beatmap;
            this.gameplayStartTime = gameplayStartTime;
            this.startAtGameplayStart = startAtGameplayStart;
            track = beatmap.Track;

            firstHitObjectTime = beatmap.Beatmap.HitObjects.First().StartTime;

            RelativeSizeAxes = Axes.Both;

            // the clock to be exposed via DI to children.
            localGameplayClock = new LocalGameplayClock(FinalConsumableOffsetClock);

            AdjustableClock.IsCoupled = false;

            GameplayClock.IsPaused.BindTo(IsPaused);

            IsPaused.BindValueChanged(onPauseChanged);
        }

        private void onPauseChanged(ValueChangedEvent<bool> isPaused)
        {
            if (isPaused.NewValue)
                this.TransformBindableTo(pauseFreqAdjust, 0, 200, Easing.Out).OnComplete(_ => AdjustableClock.Stop());
            else
                this.TransformBindableTo(pauseFreqAdjust, 1, 200, Easing.In);
        }

        /// <summary>
        /// Duration before gameplay start time required before skip button displays.
        /// </summary>
        public const double MINIMUM_SKIP_TIME = 1000;

        private readonly BindableDouble pauseFreqAdjust = new BindableDouble(1);

        [BackgroundDependencyLoader]
        private void load()
        {
            // sane default provided by ruleset.
            double startTime = gameplayStartTime;

            if (!startAtGameplayStart)
            {
                startTime = Math.Min(0, startTime);

                // if a storyboard is present, it may dictate the appropriate start time by having events in negative time space.
                // this is commonly used to display an intro before the audio track start.
                double? firstStoryboardEvent = beatmap.Storyboard.EarliestEventTime;
                if (firstStoryboardEvent != null)
                    startTime = Math.Min(startTime, firstStoryboardEvent.Value);

                // some beatmaps specify a current lead-in time which should be used instead of the ruleset-provided value when available.
                // this is not available as an option in the live editor but can still be applied via .osu editing.
                if (beatmap.BeatmapInfo.AudioLeadIn > 0)
                    startTime = Math.Min(startTime, firstHitObjectTime - beatmap.BeatmapInfo.AudioLeadIn);
            }

            Seek(startTime);

            AdjustableClock.ProcessFrame();
        }

        public void Restart()
        {
            Task.Run(() =>
            {
                track.Seek(0);
                track.Stop();

                Schedule(() =>
                {
                    AdjustableClock.ChangeSource(track);
                    updateRate();

                    if (!IsPaused.Value)
                        Start();
                });
            });
        }

        public void Start()
        {
            if (!AdjustableClock.IsRunning)
            {
                // Seeking the decoupled clock to its current time ensures that its source clock will be seeked to the same time
                // This accounts for the audio clock source potentially taking time to enter a completely stopped state
                Seek(GameplayClock.CurrentTime);

                AdjustableClock.Start();
            }

            IsPaused.Value = false;
        }

        /// <summary>
        /// Skip forward to the next valid skip point.
        /// </summary>
        public void Skip()
        {
            if (GameplayClock.CurrentTime > gameplayStartTime - MINIMUM_SKIP_TIME)
                return;

            double skipTarget = gameplayStartTime - MINIMUM_SKIP_TIME;

            if (GameplayClock.CurrentTime < 0 && skipTarget > 6000)
                // double skip exception for storyboards with very long intros
                skipTarget = 0;

            Seek(skipTarget);
        }

        public void Stop()
        {
            IsPaused.Value = true;
        }

        /// <summary>
        /// Changes the backing clock to avoid using the originally provided track.
        /// </summary>
        public void StopUsingBeatmapClock()
        {
            removeSourceClockAdjustments();

            track = new TrackVirtual(track.Length);
            AdjustableClock.ChangeSource(track);
        }

        protected override void Update()
        {
            if (IsPaused.Value)
                return;

            base.Update();
        }

        private bool speedAdjustmentsApplied;

        private void updateRate()
        {
            if (speedAdjustmentsApplied)
                return;

            track.AddAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            localGameplayClock.MutableNonGameplayAdjustments.Add(pauseFreqAdjust);
            localGameplayClock.MutableNonGameplayAdjustments.Add(UserPlaybackRate);

            speedAdjustmentsApplied = true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            removeSourceClockAdjustments();
        }

        private void removeSourceClockAdjustments()
        {
            if (!speedAdjustmentsApplied) return;

            track.RemoveAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            track.RemoveAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            localGameplayClock.MutableNonGameplayAdjustments.Remove(pauseFreqAdjust);
            localGameplayClock.MutableNonGameplayAdjustments.Remove(UserPlaybackRate);

            speedAdjustmentsApplied = false;
        }

        private class LocalGameplayClock : GameplayClock
        {
            public readonly List<Bindable<double>> MutableNonGameplayAdjustments = new List<Bindable<double>>();

            public override IEnumerable<Bindable<double>> NonGameplayAdjustments => MutableNonGameplayAdjustments;

            public LocalGameplayClock(FramedOffsetClock underlyingClock)
                : base(underlyingClock)
            {
            }
        }
    }
}
