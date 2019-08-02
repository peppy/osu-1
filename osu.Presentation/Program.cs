// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Platform;
using osu.Game.Tests;

namespace osu.Presentation
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            using (DesktopGameHost host = Host.GetSuitableHost(@"osu", true))
            {
                switch (args.FirstOrDefault() ?? string.Empty)
                {
                    default:
                        host.Run(new PresentationGame());
                        break;

                    case "--tests":
                        host.Run(new OsuTestBrowser());
                        break;
                }

                return 0;
            }
        }
    }
}
