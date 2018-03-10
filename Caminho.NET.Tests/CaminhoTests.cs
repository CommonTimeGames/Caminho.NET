using NUnit.Framework;
using System;
using Caminho;

namespace Caminho.NET.Tests
{
    [TestFixture()]
    public class CaminhoTests
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
            Engine.Start(name: "Data/basic");
            Assert.AreEqual(Engine.Current.Text, "First");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Second");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Third");
        }

        [Test()]
        public void TestStatus()
        {
            Engine.Start(name: "Data/basic.lua");
            Assert.AreEqual(Engine.Status, CaminhoStatus.Active);

            Engine.Continue();
            Engine.Continue();
            Engine.Continue();

            Assert.AreEqual(Engine.Status, CaminhoStatus.Inactive);
        }

        [Test()]
        public void TestPackage()
        {
            Engine.Start(name: "Data/basic.lua", package: "second");
            Assert.AreEqual(Engine.Current.Text, "Second:First");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Second:Second");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Second:Third");
        }

        [Test()]
        public void TestSequence()
        {
            Engine.Start(name: "Data/basic.lua", package: "third");
            Assert.AreEqual(Engine.Current.DisplayText, "Sequence1");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.DisplayText, "Sequence2");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.DisplayText, "Sequence3");
        }

        [Test()]
        public void TestTextNodes()
        {
            Engine.Start(name: "Data/basic.lua", package: "fourth");
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Text);
            Assert.AreEqual(Engine.Current.Text, "First");
            Assert.AreEqual(Engine.Current.DisplayText, "First");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Text);
            Assert.AreEqual(Engine.Current.TextKey, "fourth.key");
            Assert.AreEqual(Engine.Current.DisplayText, "fourth.key");

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Text);
            Assert.AreEqual(Engine.Current.DisplayText, "FunctionText");
        }

        [Test()]
        public void TestChoiceNodes()
        {
            Engine.Start(name: "Data/choice.lua");
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Choice);
            Assert.AreEqual(Engine.Current.Text, "What do you choose?");

            Engine.Continue(1);
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Text);
            Assert.AreEqual(Engine.Current.Text, "First Selected!");

            Engine.Start(name: "Data/choice.lua");
            Engine.Continue(2);
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Text);
            Assert.AreEqual(Engine.Current.Text, "Second Selected!");

            Engine.Start(name: "Data/choice.lua");
            Engine.Continue();
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Text);
            Assert.AreEqual(Engine.Current.Text, "First Selected!");
        }

        [Test()]
        public void TestFunctionNodes()
        {
            Engine.Start(name: "Data/functions.lua");
            Assert.AreEqual(Engine.Context["test"], 3);

            Engine.Start(name: "Data/functions.lua", package: "second");
            Assert.AreEqual(Engine.Context["test"], 4);

            Engine.Start(name: "Data/functions.lua", package: "third");
            Assert.AreEqual(Engine.Context["test"], 5);
        }

        [Test()]
        public void TestSetNode()
        {
            Engine.Start(name: "Data/context.lua");
            Assert.AreEqual(Engine.Current.Text, "Second");
            Assert.AreEqual(Engine.Context["test"], 5);

            Engine.Start(name: "Data/context.lua", package: "second");
            Assert.AreEqual(Engine.Current.Text, "Second");
            Assert.AreEqual(Engine.Context["test"], "foo");
        }

        [Test()]
        public void TestIncrementNode()
        {
            Engine.Start(name: "Data/context.lua", package: "third");
            Assert.AreEqual(Engine.Current.Text, "Second");
            Assert.AreEqual(Engine.Context["test"], 6);
        }

        [Test()]
        public void TestDecrementNode()
        {
            Engine.Start(name: "Data/context.lua", package: "fourth");
            Assert.AreEqual(Engine.Current.Text, "Second");
            Assert.AreEqual(Engine.Context["test"], 7);
        }

        [Test()]
        public void TestAutoAdvance()
        {
            Engine.Start(name: "Data/autoAdvance.lua");
            Assert.AreEqual(Engine.Current.Text, "Stop here.");
            Assert.AreEqual(Engine.Context["foo"], 7);

            Engine.Start(name: "Data/autoAdvance.lua", package: "second");
            Assert.AreEqual(Engine.Current.Text, "Start here.");
            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Stop here.");
            Assert.AreEqual(Engine.Context["foo"], 7);

            Engine.AutoAdvance = false;

            Engine.Start(name: "Data/autoAdvance.lua", package: "second");
            Assert.AreEqual(Engine.Current.Text, "Start here.");


            Engine.Continue();
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Function);

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Increment);

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Type, CaminhoNodeType.Increment);

            Engine.Continue();
            Assert.AreEqual(Engine.Current.Text, "Stop here.");
            Assert.AreEqual(Engine.Context["foo"], 7);
        }
    }
}
