// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using NUnit.Framework;
using osu.Game.Skinning;

namespace osu.Game.Tests.Skins
{
    [TestFixture]
    public class SkinDatabaseTest : HeadlessOsuTest
    {
        private SkinManager skinManager;

        private CleanRunHeadlessGameHost host;

        [SetUp]
        public void SetUp()
        {
            host = new CleanRunHeadlessGameHost("SkinTest");
            var osu = LoadGameIntoHost(host, new SkinOsuGameBase());
            skinManager = osu.SkinManager;
        }

        [TearDown]
        public void TearDown()
        {
            host?.Exit();
            host?.Dispose();
        }

        [Test]
        public void TestDefaultState()
        {
            var skins = skinManager.GetAllUsableSkins();

            Assert.AreEqual(1, skins.Count);
            Assert.IsTrue(skins[0].Name == SkinInfo.Default.Name);
        }

        [Test]
        public async Task TestImportSkin()
        {
            SkinInfo test = new SkinInfo
            {
                Name = "test skin",
                Creator = "poop"
            };

            await skinManager.Import(test);

            var skins = skinManager.GetAllUsableSkins();

            Assert.AreEqual(1, skins.Count);
        }

        private class SkinOsuGameBase : OsuGameBase
        {
            public new SkinManager SkinManager => base.SkinManager;
        }
    }
}
