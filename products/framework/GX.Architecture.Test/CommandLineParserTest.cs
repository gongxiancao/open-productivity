using GX.Architecture.Configuration.CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GX.Architecture.Test
{


    /// <summary>
    ///This is a test class for CommandLineParserTest and is intended
    ///to contain all CommandLineParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommandLineParserTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        public void TestEntryPoint(string source, string dest)
        {

        }


        public void TestEntryPoint(
            [DefaultValue("sourceTest")]
            string source, 
            [Switch, DefaultValue(true)]
            bool verbose,
            [Alias("destAlias"), Name("destName")]
            string dest)
        {

        }

        void TestParse(CommandLineParser target, string source, object[] expected)
        {
            object[] actual;
            actual = target.Parse(source);

            Assert.IsTrue(expected.SequenceEqual(actual), "expect:{0}, actual={1}", string.Join(",", expected), string.Join(",", actual));
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest()
        {
            MehtodParamterConfigurationDefinitionGenerator gen = new MehtodParamterConfigurationDefinitionGenerator();

            {
                ConfigurationInfo configuartionInfo = gen.Generate(this.GetType().GetMethod("TestEntryPoint", new Type[] { typeof(string), typeof(string) }));
                CommandLineParser target = new CommandLineParser(configuartionInfo);


                //TestParse(target, "/ test1 / test2", new object[] { "test1", "/123" });
                TestParse(target, "/source test1 /dest \"/123\"", new object[] { "test1", "/123" });
                TestParse(target, "", new object[] { null, null });
                TestParse(target, "/source source /dest dest", new object[] { "source", "dest" });
                TestParse(target, "/source \"source\" /dest dest", new object[] { "source", "dest" });
                TestParse(target, "/source source /dest \"dest\"", new object[] { "source", "dest" });
                TestParse(target, "/source \"source\" /dest \"dest\"", new object[] { "source", "dest" });
                TestParse(target, "/source \"sou rce\" /dest \"dest\"", new object[] { "sou rce", "dest" });
                TestParse(target, "/source \"so \"\"urce\" /dest dest", new object[] { "so \"urce", "dest" });
                TestParse(target, "/source \"s\"\"\"\"ource\" /dest dest", new object[] { "s\"\"ource", "dest" });
                TestParse(target, "/source source dest", new object[] { "source", "dest" });
                TestParse(target, "/dest dest source", new object[] { "source", "dest" });
                TestParse(target, "/source:source /dest dest", new object[] { "source", "dest" });
                TestParse(target, "/source : source /dest dest", new object[] { "source", "dest" });
                TestParse(target, "\\\\rubicon\\test \"abc\"", new object[] { "\\\\rubicon\\test", "abc"});
            }
            {
                ConfigurationInfo configuartionInfo = gen.Generate(this.GetType().GetMethod("TestEntryPoint", new Type[] { typeof(string), typeof(bool), typeof(string) }));
                CommandLineParser target = new CommandLineParser(configuartionInfo);
                TestParse(target, "", new object[] { "sourceTest", true, null });
                TestParse(target, "/source test1", new object[] { "test1", true, null });
                TestParse(target, "/source test1 /destAlias \"/123\"", new object[] { "test1", true, "/123" });
            }
        }
    }
}
