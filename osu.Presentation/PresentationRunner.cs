// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Platform;

namespace osu.Presentation
{
    public static class PresentationRunner
    {
        [STAThread]
        public static int Main(string[] args)
        {
            using (DesktopGameHost host = Host.GetSuitableHost(@"osu", true))
            {
                host.Run(new PresentationGame());
                return 0;
            }
        }
    }
}