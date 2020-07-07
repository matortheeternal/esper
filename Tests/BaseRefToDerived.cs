using NUnit.Framework;

namespace Tests {
    public class A {
        protected string message;

        public A(string message) {
            this.message = message;
        }
    }

    public class B : A {
        public B(string message) : base(message) { }

        public string GetMessage() {
            return message;
        }
    }

    public class C : A {
        public C(string message) : base(message) { }
    }

    public class BaseRefToDerived {
        [Test]
        public void TestDowncast() {
            A example = new B("hello");
            B casted = example as B;
            Assert.IsNotNull(casted);
            Assert.AreEqual(casted.GetMessage(), "hello");
        }

        [Test]
        public void TestIsNotDerivedClass() {
            A example = new C("hello");
            Assert.IsFalse(example is B);
        }
    }
}
