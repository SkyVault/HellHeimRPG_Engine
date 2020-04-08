using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Harp {
    public enum TokTypes {
        Eof,
        Atom,
        Number,
        String,
        OpenParen,
        CloseParen,
        OpenBracket,
        CloseBracket,
        OpenBrace,
        CloseBrace,
    }

    public class Token {
        public Token(TokTypes type, string lexeme) { Type = type; Lexeme = lexeme; }
        public Token() { }
        public TokTypes Type { get; set; } = TokTypes.Eof;
        public string Lexeme { get; set; } = "";

        public bool IsTerminal {
            get =>
                Type == TokTypes.Atom ||
                Type == TokTypes.Number ||
                Type == TokTypes.String;
        }
    }

    public class Lexer {
        private It<char> _it = null;
        private string _source = "";

        public char Ch { get => _it.Current; }
        public bool Eof { get => _it.Eof; }

        public Lexer(string code) {
            Load(code);
        }

        public Lexer() { }

        public void Load(string code) {
            // TODO(Dustin): Clean this up
            char[] chars = code.ToCharArray();
            var list = new List<char>();
            list.AddRange(chars);
            _it = new It<char>(list);
            _source = code;
        }

        public void SkipWhiteSpace() {
            while (!_it.Eof && char.IsWhiteSpace(_it.Current))
                _it.Next();
        }

        public Token PeekNext() {
            var start = _it.Ref;
            var token = GetNext();
            _it = start.Ref;
            return token;
        }

        public Token GetNext()
        {
            return _getNext();
        }

        Token _getNext() { 
            SkipWhiteSpace();

            (bool isNeg, bool isDec) flags = (false, false);

            var start = _it.Ref;
            if (Ch == '-') { flags.isNeg = true; _it.Next(); }
            if (_it.Eof) return new Token(TokTypes.Atom, Ch.ToString());
            if (Ch == '.') { flags.isDec = true; _it.Next(); }
            if (_it.Eof) return new Token(TokTypes.Atom, Ch.ToString());

            if (char.IsDigit(Ch)) {
                while (!_it.Eof) {
                    if (!char.IsDigit(_it.Peek(1))) {

                        if (_it.Peek() == '.') {
                            if (!flags.isDec) {
                                flags.isDec = true;
                                _it.Next();
                                continue;
                            }
                        }

                        string lexeme = "";
                        while (start != _it) {
                            lexeme += start.Current;
                            start.Next();
                        }
                        _it.Next();
                        lexeme += start.Current;
                        return new Token(TokTypes.Number, lexeme);
                    }
                    _it.Next();
                }
            } else {
                _it = start.Ref;
            }

            if (Ch == '(') { var ch = Ch.ToString(); _it.Next(); return new Token(TokTypes.OpenParen, ch); }
            if (Ch == ')') { var ch = Ch.ToString(); _it.Next(); return new Token(TokTypes.CloseParen, ch); }
            if (Ch == '[') { var ch = Ch.ToString(); _it.Next(); return new Token(TokTypes.OpenBracket, ch); }
            if (Ch == ']') { var ch = Ch.ToString(); _it.Next(); return new Token(TokTypes.CloseBracket, ch); }
            if (Ch == '{') { var ch = Ch.ToString(); _it.Next(); return new Token(TokTypes.OpenBrace, ch); }
            if (Ch == '}') { var ch = Ch.ToString(); _it.Next(); return new Token(TokTypes.CloseBrace, ch); }

            start = _it.Ref;
            while (true) {
                if (_it.Eof
                    || char.IsWhiteSpace(Ch)
                    || "(){}[]',".Contains(_it.Current)) {
                    string lexeme = "";
                    while (start != _it && !start.Eof) {
                        lexeme += start.Current;
                        start.Next();
                    }

                    return new Token(TokTypes.Atom, lexeme);
                }

                _it.Next();
            }
        }
    }

    public abstract class Val {
        public bool Quoted { get; set; } = false;

        public override bool Equals(object obj) {
            if (obj.GetType() != GetType()) return false;
            if (this is Num num) { return num.Value == (obj as Num).Value; }
            if (this is Str str) { return str.Value == (obj as Str).Value; }
            if (this is Atom atom) { return atom.Name == (obj as Atom).Name; }
            if (this is Bool b) { return b.Flag; }
            if (this is Seq seq) { return seq.Items == (obj as Seq).Items; }
            Assert.Fail("Unhandled comparison");
            return false;
        }

        public bool IsValue { 
            get => this is Num || this is Str || this is Bool;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            if (this is Num num) { return num.Value.ToString(); }
            if (this is Str str) { return str.Value; }
            if (this is Atom atom) { return atom.Name; }
            if (this is Bool b) { return b.Flag ? "true" : "false"; }

            if (this is Lambda lambda) { return $"<lambda>"; }

            string x = "";
            if (this is Seq seq) {
                x += "(";
                foreach (var val in seq.Items) {
                    x += $"{val} ";
                }
                x.Substring(0, x.Length - 2);
                x += ")";
            }
            return x;
        }
    }

    public class Num : Val { public double Value { get; set; } = 0; }
    public class Atom : Val { public string Name { get; set; } = ""; }
    public class Str : Val { public string Value { get; set; } = ""; }
    public class Bool : Val {
        public static Bool True { get => new Bool { Flag = true }; }
        public static Bool False { get => new Bool { Flag = false }; }
        public bool Flag = false; 
    }
    public class Dict : Val {
        public Dictionary<Val, Val> Pairs
            = new Dictionary<Val, Val>();
        public Val this[Val key] {
            get => Pairs[key];
            set => Pairs[key] = value;
        }
    }
    public class Seq : Val { public List<Val> Items { get; set; } = new List<Val>(); }
    public class NativeFunc : Val { public Func<Seq, Val> Func = a => a; }

    public class Lambda : Val {
        public List<string> Args = new List<string>();
        public Seq Progn = new Seq();
    }

    public class None : Val { }

    public class Env : Val {
        public List<Dictionary<string, Val>> Vars
            = new List<Dictionary<string, Val>> { new Dictionary<string, Val>() };

        public void Push() {
            Vars.Add(new Dictionary<string, Val>());
        }

        public void Pop() {
            Vars.RemoveAt(Vars.Count - 1);
        }

        public void Put(string key, Val val) {
            Vars[Vars.Count - 1][key] = val;
        }

        public Val this[string key] {
            get {
                for (int i = Vars.Count - 1; i >= 0; i--) {
                    if (Vars[i].ContainsKey(key))
                        return Vars[i][key];
                }
                Console.WriteLine($"Cannot find atom '{key}' in env.");
                return new None();
            }
            set {
                for (int i = Vars.Count - 1; i >= 0; i--) {
                    if (Vars[i].ContainsKey(key)) {
                        Vars[i][key] = value;
                        return;
                    }
                }
                Assert.Fail($"Cannot find atom '{key}' in env.");
            }
        } 
    }

    /*# EBNF
      <program> ::= <s-exper-list>
      <s-expr> ::= <terminal> | '(' <s-expr>* ')'
     */

    public class Harp {
        public static Val StartRepl() {
            var harp = new Harp();
            var env = new Env();
            harp.LoadHarpLibInto(env);

            while (true) {
                Console.Write("> ");
                string input = Console.ReadLine();

                var result = harp.Eval(env, input);

                Console.WriteLine($"{result}");
            }

            return new None();
        }

        Val ParseObject(Lexer lexer) {
            var token = lexer.GetNext();

            if (token.Type == TokTypes.Atom) {
                return new Atom { Name = token.Lexeme };
            }

            if (token.Type == TokTypes.Number) {
                Assert.IsTrue(double.TryParse(token.Lexeme, out double val));
                return new Num { Value = val };
            }

            if (token.Type == TokTypes.String) {
                return new Str { Value = token.Lexeme.Substring(1, token.Lexeme.Length - 2) };
            }

            Assert.Fail($"Unhandled token type {token.Type}");
            return new None();
        } 

        Val ParseList(Lexer lexer) { 
            var seq = new Seq();

            while (!lexer.Eof) {
                var token = lexer.PeekNext();

                if (token.Type == TokTypes.OpenParen) {
                    seq.Items.Add(ParseSExpr(lexer));
                }

                if (token.IsTerminal) {
                    // NOTE(Dustin): We can improve this by passing the peeked token in
                    // otherwise we are calling PeakNext twice, which inturn calls GetToken
                    seq.Items.Add(ParseSExpr(lexer));
                }

                if (token.Type == TokTypes.CloseParen) {
                    lexer.GetNext();
                    return seq;
                }
            }

            return seq;
        }

        Val ParseSExpr(Lexer lexer) {
            var token = lexer.PeekNext(); 

            if (token.IsTerminal) { return ParseObject(lexer); }

            if (token.Type == TokTypes.OpenParen) {
                lexer.GetNext();
                return ParseList(lexer);
            }

            Assert.Fail("Unbalanced parenthices");
            return new None();
        }

        public Seq Parse(string code) {
            Lexer lexer = new Lexer(code);

            var result = new Seq();

            while (!lexer.Eof) {
                result.Items.Add(ParseSExpr(lexer));
            }

            return result;
        }

        public Val EvalObject(Env env, Val ast) {
            if (ast is Lambda lambda) { return lambda; }
            if (ast is Seq seq) {
                var count = seq.Items.Count; 

                // NOTE(Dustin): Empty list evaluates to itself, may want to change that later
                if (count == 0) { return seq; } 

                var args = new Seq();

                // Should we evaluate each argument? or keep it lazy?
                args.Items.AddRange(seq.Items.GetRange(1, seq.Items.Count - 1));

                if (seq.Items[0] is Atom a) {
                    if (a.Name == "progn") {
                        return EvalProgn(env, args);
                    }

                    if (a.Name == "if") {
                        Assert.IsTrue(
                            args.Items.Count <= 3 && args.Items.Count > 1,
                            "Error, if statement requires a conditional and one branch.");
                        var conditional = EvalObject(env, args.Items[0]);

                        if (conditional is Bool b) {
                            if (b.Flag) {
                                return EvalObject(env, args.Items[1]);
                            } else {
                                if (args.Items.Count == 3) {
                                    return EvalObject(env, args.Items[2]);
                                }
                            }
                        } else {
                            Assert.Fail("Error, conditional is not a boolean.");
                        }
                        return new None();
                    }

                    if (a.Name == "def") {
                        Assert.IsTrue(args.Items.Count > 0);

                        var variable = args.Items[0];

                        if (variable is Atom atom) {
                            if (args.Items.Count > 1) {
                                env.Put(atom.Name, EvalObject(env, args.Items[1]));
                            } else {
                                env.Put(atom.Name, new None());
                            }
                        } else {
                            Assert.Fail($"def expexts and atom but got {variable.GetType()}");
                        }

                        return variable;
                    }

                    if (a.Name == "lambda") {
                        var xs = args.Items[0] as Seq;

                        var body = new Seq();
                        for (int i = 1; i < args.Items.Count; i++) {
                            body.Items.Add(args.Items[i]);
                        }

                        var xss = new List<string>();
                        xs.Items.ForEach(x => xss.Add((x as Atom).Name));
                        return new Lambda {
                            Args = xss,
                            Progn = body
                        };
                    }

                    if (a.Name == "defn") {
                        Assert.IsTrue(args.Items.Count == 3);
                        var name = args.Items[0] as Atom;
                        var arguments = args.Items[1] as Seq;

                        var body = new Seq();
                        for (int i = 2; i < args.Items.Count; i++)
                            body.Items.Add(args.Items[i]);

                        var xs = new List<string>();
                        arguments.Items.ForEach(x => xs.Add((x as Atom).Name));
                        var theLambda = new Lambda {
                            Args = xs,
                            Progn = body
                        };

                        env.Put(name.Name, theLambda);
                        return theLambda;
                    }
                }

                Val first = EvalObject(env, seq.Items[0]);

                // Eval
                if (first is NativeFunc nativeFunc) {
                    env.Push();
                    var result = nativeFunc.Func(args);
                    env.Pop();
                    return result;
                    // ReSharper disable once InconsistentNaming
                } else if (first is Lambda _lambda) {
                    env.Push();
                    for (int i = 0; i < args.Items.Count; i++) {
                        env.Put(_lambda.Args[i], args.Items[i]);
                    }
                    var result = EvalProgn(env, _lambda.Progn);
                    env.Pop();
                    return result;
                } else {
                    Console.WriteLine($"Cannot apply non function type {first.GetType()}");

                    return new None();
                }
            }

            if (ast is Num num) { return num; }
            if (ast is Str str) { return str; }

            if (ast is Atom _atom) { 
                return env[_atom.Name];
            }

            return new None();
        }

        Val EvalProgn(Env env, Seq ast) {
            Val result = new None();
            ast.Items.ForEach(obj => result = EvalObject(env, obj));
            return result;
        }

        public Val Eval(Env env, string code) {
            return EvalProgn(env, Parse(code));
        }
    } 
} 
