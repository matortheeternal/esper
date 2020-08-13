using NUnit.Framework;

namespace Tests.language {
    public class Aaa {
        public virtual string GetMessage() {
            return "A";
        }
    }

    public class Bbb : Aaa {
        public override string GetMessage() {
            return "B";
        }
    }

    public class TestMethodOverriding {
        [Test]
        public void Test() {
            var b = new Bbb();
            Assert.AreEqual(b.GetMessage(), "B");
            var BbbAsAaa = b as Aaa;
            Assert.AreEqual(BbbAsAaa.GetMessage(), "B");
        }
    }
}
