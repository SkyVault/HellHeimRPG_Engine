using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Harp;
using HellHeimRPG;

namespace Tests {
    [TestClass]
    class FunctionalTest { 
        [TestMethod]
        public void OptionTest() {
            var numO = new Some<int>(32);
            var worked = false;

            Assert.AreEqual(32, numO.Map((n) => {
                worked = true;
                return n;
            }));

            Assert.IsTrue(worked);

            var numNone = new None<int>();

            Option<string> Test() {
                return new Some<string>("Hello World");
            }

            var myO = Test();

            Assert.IsTrue(myO is Some<string> o);
        }
    }
}
