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
            var num_o = new Some<int>(32);
            var worked = false;

            Assert.AreEqual(32, num_o.Map((n) => {
                worked = true;
                return n;
            }));

            Assert.IsTrue(worked);

            var num_none = new None<int>();

            Option<string> test() {
                return new Some<string>("Hello World");
            }

            var myO = test();

            Assert.IsTrue(myO is Some<string> o);
        }
    }
}
