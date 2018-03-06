using NUnit.Framework;
using System;
using Caminho;

namespace Caminho.NET.Tests
{
    [TestFixture()]
    public class BasicTests
    {
        private CaminhoEngine Engine { get; set; }

        [SetUp]
        public void SetUp()
        {
            Engine = new CaminhoEngine();
            Engine.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            Engine = null;
        }

        [Test]
        public void TestBasic()
        {
            Engine.Start("Data/basic");
            Assert.AreEqual(Engine.Current.Text, "First");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Second");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Third");
        }

        [Test()]
        public void TestStatus()
        {
            Engine.Start("Data/basic.lua");
            Assert.AreEqual(Engine.Status, CaminhoStatus.Active);

            Engine.Continue();
            Engine.Continue();
            Engine.Continue();

            Assert.AreEqual(Engine.Status, CaminhoStatus.Inactive);
        }

        [Test()]
        public void TestPackage()
        {
            Engine.Start("Data/basic.lua", "second");
            Assert.AreEqual(Engine.Current.Text, "Second:First");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Second:Second");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Second:Third");
        }
    }
}
