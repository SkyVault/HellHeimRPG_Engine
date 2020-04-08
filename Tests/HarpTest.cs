using Microsoft.VisualStudio.TestTools.UnitTesting;
using Harp;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Tests {
    [TestClass]
    public class HarpTest {
        [TestMethod]
        public void WhitespaceTest() {
            Lexer lexer = new Lexer("  \n\t  X ");
            lexer.SkipWhiteSpace();
            Assert.AreEqual('X', lexer.Ch);
        }

        [TestMethod]
        public void NumbersTest() {
            var tests = new List<string> { 
                "3.1415926",
                "-.31",
                "-6",
                ".54",
                "86",
            };

            string code = Enumerable.Aggregate(tests, (acc, x) => acc + " " + x); 

            var lex = new Lexer(code);
            int index = 0;
            while (!lex.Eof) {
                var tok = lex.GetNext();
                Assert.AreEqual(TokTypes.Number, tok.Type);
                Assert.AreEqual(tests[index++], tok.Lexeme);
            }
        }

        [TestMethod]
        public void AtomTest() {
            var atoms = new List<string> {
                "hello",
                "world",
                "TRUE"
            };

            string code = Enumerable.Aggregate(atoms, (acc, x) => acc + " " + x);

            var lex = new Lexer(code);
            var index = 0;
            while (!lex.Eof) {
                var tok = lex.GetNext();
                Assert.AreEqual(TokTypes.Atom, tok.Type);
                Assert.AreEqual(atoms[index++], tok.Lexeme);
            }
        }

        [TestMethod]
        public void PeekTest() {
            Lexer lexer = new Lexer("123 567");
            lexer.GetNext();

            var a = lexer.PeekNext();
            Assert.IsFalse(lexer.Eof);

            var b = lexer.GetNext(); 
            Assert.IsTrue(lexer.Eof);

            Assert.IsTrue(a.Lexeme == b.Lexeme);
        }

        [TestMethod]
        public void AstTest() {
            var harp = new Harp.Harp();
            var node = harp.Parse("(+ 1 (2 3 4) 3)");

            var env = new Dict();
            var atom = new Atom { Name = "Apple" };

            env[atom] = new Num { Value = 42 };
           
            Console.WriteLine();
        }

        [TestMethod]
        public void EvalTest() {
            var harp = new Harp.Harp();
            var env = new Env();

            harp.LoadHarpLibInto(env);
            var t = harp.Eval(env, "(defn add (a b) (+ a b))");
            var a = harp.Eval(env, "(add (add 1 2) 3)");
            Assert.Fail($"{a}");
        }
    }
}
