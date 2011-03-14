﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChainingAssertion
{
    [TestClass]
    public class UnitTest
    {
        public TestContext TestContext { get; set; }

        // samples

        [TestMethod]
        public void IsTest()
        {
            // "Is" extend on all object and has three overloads.

            // This same as Assert.AreEqual(25, Math.Pow(5, 2))
            Math.Pow(5, 2).Is(25);

            // lambda predicate assertion.
            // This same as Assert.IsTrue("foobar".StartsWith("foo") && "foobar".EndWith("bar"))
            "foobar".Is(s => s.StartsWith("foo") && s.EndsWith("bar"));

            // has collection assert
            // This same as CollectionAssert.AreEqual(Enumerable.Range(1,5), new[]{1, 2, 3, 4, 5})
            Enumerable.Range(1, 5).Is(1, 2, 3, 4, 5);
        }

        [TestMethod]
        public void CollectionTest()
        {
            // if you want to use CollectionAssert Methods then use Linq to Objects and Is
            new[] { 1, 3, 7, 8 }.Contains(8).Is(true);
            new[] { 1, 3, 7, 8 }.Count(i => i % 2 != 0).Is(3);
            new[] { 1, 3, 7, 8 }.Any().Is(true);
            new[] { 1, 3, 7, 8 }.All(i => i < 5).Is(false);

            // IsOrdered
            var array = new[] { 1, 5, 10, 100 };
            array.Is(array.OrderBy(x => x));
        }

        [TestMethod]
        public void OthersTest()
        {
            // Null Assertions
            Object obj = null;
            obj.IsNull();             // Assert.IsNull(obj)
            new Object().IsNotNull(); // Assert.IsNotNull(obj)

            // Not Assertion
            "foobar".IsNot("fooooooo"); // Assert.AreNotEqual
            new[] { "a", "z", "x" }.IsNot("a", "x", "z"); /// CollectionAssert.AreNotEqual

            // ReferenceEqual Assertion
            var tuple = Tuple.Create("foo");
            tuple.IsSameReferenceAs(tuple); // Assert.AreSame
            tuple.IsNotSameReferenceAs(Tuple.Create("foo")); // Assert.AreNotSame

            // Type Assertion
            "foobar".IsInstanceOf<string>(); // Assert.IsInstanceOfType
            (999).IsNotInstanceOf<double>(); // Assert.IsNotInstanceOfType
        }

        [TestMethod]
        public void AdvancedCollectionTest()
        {
            var lower = new[] { "a", "b", "c" };
            var upper = new[] { "A", "B", "C" };

            // Comparer CollectionAssert, use IEqualityComparer<T> or Func<T,T,bool> delegate
            lower.Is(upper, StringComparer.InvariantCultureIgnoreCase);
            lower.Is(upper, (x, y) => x.ToUpper() == y.ToUpper());

            // or you can use Linq to Objects - SequenceEqual
            lower.SequenceEqual(upper, StringComparer.InvariantCultureIgnoreCase).Is(true);
        }

        [TestMethod]
        public void ExceptionTest()
        {
            // Exception Test(alternative of ExpectedExceptionAttribute)
            // Throws does not allow derived type
            // Catch allows derived type
            AssertEx.Throws<ArgumentNullException>(() => "foo".StartsWith(null));
            AssertEx.Catch<Exception>(() => "foo".StartsWith(null));

            // return value is occured exception
            var ex = AssertEx.Throws<InvalidOperationException>(() =>
            {
                throw new InvalidOperationException("foobar operation");
            });
            ex.Message.Is(s => s.Contains("foobar")); // additional exception assertion

            // must not throw any exceptions
            AssertEx.DoesNotThrow(() =>
            {
                // code

            });
        }

        [TestMethod]
        [TestCase(1, 2, 3)]
        [TestCase(10, 20, 30)]
        [TestCase(100, 200, 300)]
        public void TestCaseTest()
        {
            TestContext.Run((int x, int y, int z) =>
            {
                (x + y).Is(z);
                (x + y + z).Is(i => i < 1000);
            });
        }

        [TestMethod]
        [TestCaseSource("toaruSource")]
        public void TestTestCaseSource()
        {
            TestContext.Run((int x, int y, string z) =>
            {
                string.Concat(x, y).Is(z);
            });
        }

        public static object[] toaruSource = new[]
        {
            new object[] {1, 1, "11"},
            new object[] {5, 3, "53"},
            new object[] {9, 4, "94"}
        };


        public class TestPrivate
        {
            private string privateString = "homu";
            private string PrivateProperty
            {
                get { return privateString + privateString; }
                set { privateString = value; }
            }
            private string PrivateMethod()
            {
                return privateString + privateString;
            }
            private int PrivateMethod2(int x)
            {
                return x * x;
            }
        }

        [TestMethod]
        public void DynamicTest()
        {
            var p = new TestPrivate();

            // AsDynamicでdynamic化して、その後はprivateのメソッドもふつーに呼べる
            (p.AsDynamic().PrivateMethod() as string).Is("homuhomu");
            var actual = p.AsDynamic().PrivateMethod2(100);

            // 拡張メソッドは型確定させないと使えないからねえ
            ((int)actual).Is(10000);
            Assert.AreEqual(10000, actual); // こっちなら型変換しなくても大丈夫ではあるんですが
        }



        // exceptions

        [TestMethod]
        public void Throws()
        {
            try
            {
                AssertEx.Throws<Exception>(() => "foo".StartsWith(null));
            }
            catch (AssertFailedException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void Catch()
        {
            try
            {
                AssertEx.Catch<Exception>(() => "foo".StartsWith(null));
            }
            catch (AssertFailedException)
            {
                Assert.Fail();
            }
            return;
        }

        [TestMethod]
        public void Throws2()
        {
            try
            {
                AssertEx.Throws<Exception>(() => { });
            }
            catch (AssertFailedException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void Catch2()
        {
            try
            {
                AssertEx.Catch<Exception>(() => { });
            }
            catch (AssertFailedException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void Exception()
        {
            var ex = AssertEx.Throws<ArgumentNullException>(() =>
            {
                throw new ArgumentNullException("nullnull");
            });
            ex.ParamName.Is("nullnull");
        }
    }
}