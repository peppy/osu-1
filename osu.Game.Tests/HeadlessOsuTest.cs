// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Platform;

namespace osu.Game.Tests
{
    public abstract class HeadlessOsuTest
    {
        protected static T LoadGameIntoHost<T>(GameHost host, T game)
            where T : Framework.Game
        {
            Task.Run(() => host.Run(game));
            WaitForOrAssert(() => game.IsLoaded, $"{typeof(T)} failed to start in a reasonable amount of time");
            return game;
        }

        protected static void WaitForOrAssert(Func<bool> result, string failureMessage, int timeout = 60000)
        {
            Task task = Task.Run(() =>
            {
                while (!result()) Thread.Sleep(200);
            });

            Assert.IsTrue(task.Wait(timeout), failureMessage);
        }
    }
}
