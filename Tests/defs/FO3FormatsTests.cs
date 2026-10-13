using esper;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using Tests;

namespace Tests.defs {
    [TestFixture]
    public class FO3FormatsTests {
        private static Session FO3() => Synthetic.GetSession(Games.FO3);

        private static JObject Src(string name) {
            return new JObject { { "name", name } };
        }

        [Test]
        public void TestIdleAnamTable() {
            var def = new esper.defs.FO3.IdleAnamFormat(
                FO3().definitionManager, Src("Animation Group Section"));
            Assert.AreEqual(
                "Idle, Must return a file", def.DataToValue(null, 0));
            Assert.AreEqual("Idle", def.DataToValue(null, 0x80));
            Assert.AreEqual(
                "Whole Body, Must return a file",
                def.DataToValue(null, 20));
            Assert.AreEqual(
                "Weapon Up, Must return a file", def.DataToValue(null, 5));
            Assert.AreEqual(
                "Idle, Must return a file", def.DataToValue(null, 0x40));
            Assert.AreEqual(
                "<Unknown: 8>, Must return a file",
                def.DataToValue(null, 8));
            Assert.Throws<NotImplementedException>(
                () => def.ValueToData(null, "Idle"));
        }

        [Test]
        public void TestWthrClassification() {
            var def = new esper.defs.FO3.WthrDataClassificationFormat(
                FO3().definitionManager, Src("Weather Classification"));
            Assert.AreEqual(
                "<Unknown: 3>", def.DataToValue(null, 3));
            Assert.AreEqual("Snow", def.DataToValue(null, 8));
            Assert.AreEqual("01", def.GetSortKey(null, 1));
            Assert.Throws<NotImplementedException>(
                () => def.ValueToData(null, "Snow"));
        }
    }
}
