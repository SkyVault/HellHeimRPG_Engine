using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using Harp;
using Mono.Terminal;

namespace Harp {
    public enum TokTypes {
        Eof,
        Atom,
        Number,
        String,
        Quote,
        Bool,
        DoubleQuote,
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
                Type == TokTypes.String ||
                Type == TokTypes.Bool;
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
            while (!_it.Eof && char.IsWhiteSpace(Ch))
                _it.Next();
        }

        public Token PeekNext() {
            var start = _it.Ref;
            var token = GetNext();
            _it = start.Ref;
            return token;
        }

        public Token GetNext() { return _getNext(); }

        Token _getNext() { 
            SkipWhiteSpace();

            (bool isNeg, bool isDec) flags = (false, false);

            if (Ch == ';')
            {
                while (!_it.Eof && Ch != '\n') {
                    _it.Next();
                }
            }

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

            var ch = Ch.ToString();
            switch (Ch) {
                case '(': { _it.Next(); return new Token(TokTypes.OpenParen, ch); }
                case ')': { _it.Next(); return new Token(TokTypes.CloseParen, ch); }
                case '[': { _it.Next(); return new Token(TokTypes.OpenBracket, ch); }
                case ']': { _it.Next(); return new Token(TokTypes.CloseBracket, ch); }
                case '{': { _it.Next(); return new Token(TokTypes.OpenBrace, ch); }
                case '}': { _it.Next(); return new Token(TokTypes.CloseBrace, ch); }
                case '\'': { _it.Next(); return new Token(TokTypes.Quote, ch); }
            }

            if (Ch == '#') {
                start = _it.Ref;
                _it.Next();

                var _ch = Ch;
                if (!_it.Eof && (Ch == 'f' || Ch == 't')) {
                    _it.Next();
                    return new Token(TokTypes.Bool, _ch.ToString());
                } 
                _it = start.Ref;
            }

            // Handle string literals 
            start = _it.Ref;
            if (Ch == '\"') {
                while (true) {
                    _it.Next();

                    if (_it.Eof) { 
                        Assert.Fail("Unmatched quotes");
                    }

                    if (Ch == '\"') {
                        _it.Next();

                        var ss = "";
                        while (start != _it) {
                            ss += start.Current;
                            start.Next();
                        }

                        if (ss == "") { return new Token(TokTypes.String, ""); }
                        // TODO(Dustin): Strip the quotes here
                        if (ss.StartsWith('\"') && ss.EndsWith('\"')) ss = ss.Substring(1, ss.Length-2);
                        return new Token(TokTypes.String, ss); 
                    }
                }
            }

            while (true) {
                if (_it.Eof
                    || char.IsWhiteSpace(Ch)
                    || "(){}[]',".Contains(_it.Current)) {
                    string lexeme = "";
                    while (start != _it && !start.Eof) {
                        lexeme += start.Current;
                        start.Next();
                    }

                    if (lexeme == "") return _getNext();

                    if (lexeme.StartsWith(':')) {
                        return new Token(TokTypes.String, lexeme);
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
            if (this is Vec vec) { return vec.Items == (obj as Vec).Items; }
            Assert.Fail("Unhandled comparison");
            return false;
        }

        public bool IsValue => this is Num || this is Str || this is Bool;

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string x = "";
            if (this is None) { return "<none>"; }
            else if (this is Num num) { return num.Value.ToString(); }
            else if (this is Str str) { return str.Value; }
            else if (this is Atom atom) { return atom.Name; }
            else if (this is Bool b) { return b.Flag ? "#t" : "#f"; } 
            else if (this is Lambda lambda) { return $"<lambda>"; } 
            else if (this is Seq seq) {
                if (seq.Items.Count == 0) { return "()"; }
                x += "(";
                for (int i = 0; i < seq.Items.Count; i++) {
                    x += $"{seq.Items[i]}";
                    if (i < seq.Items.Count - 1) x += " ";
                }
                x.Substring(0, x.Length - 2);
                x += ")";
            } 
            else if (this is Vec vec) {
                if (vec.Items.Count == 0) { return "[]"; } 
                x += "["; 
                for (int i = 0; i < vec.Items.Count; i++) { x += $"{vec.Items[i]}"; if (i < vec.Items.Count - 1) x += " "; } 
                x.Substring(0, x.Length -  2);
                x += ']';
            }
            else if (this is NativeFunc func) {
                return $"<native-func>";
            }
            else if (this is Dict dict) {
                x += "{";
                foreach (var key in dict.Pairs.Keys) {
                    x += $"{key}: {dict[new Str { Value = key }]} ";
                } 
                return x + "}";
            }
            else {
                Assert.Fail($"Unhandled value in ToString: {this.GetType()}");
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
        public Dictionary<string, Val> Pairs
            = new Dictionary<string, Val>();
 
        public bool Evaluated = false;
        public List<Val> Literal = new List<Val>();

        private string getKey(Val val)
        {
            Assert.IsTrue(val.IsValue);

            return val switch {
                Num num => num.Value.ToString(CultureInfo.InvariantCulture),
                Str str => str.Value,
                _ => ""
            };
        }

        public Val this[Val key] {
            get => Pairs[getKey(key)];
            set => Pairs[getKey(key)] = value;
        }
    }

    public class Seq : Val, IEnumerable { 
        public List<Val> Items { get; set; } = new List<Val>();
        public IEnumerator GetEnumerator() => Items.GetEnumerator();

        public Val this[int index] => Items[index];
    }

    public class Vec : Val, IEnumerable {
        public List<Val> Items { get; set; } = new List<Val>();
        public IEnumerator GetEnumerator() => Items.GetEnumerator(); 
    }

    public class NativeFunc : Val { public Func<Seq, Val> Func = a => a; }

    public class Lambda : Val {
        public List<string> Args = new List<string>();
        public Seq Progn = new Seq();
        public Dictionary<string, Val> Closure = null;
    }

    public class None : Val { }

    public class Env : Val {
        public List<Dictionary<string, Val>> Vars
            = new List<Dictionary<string, Val>> { new Dictionary<string, Val>() };

        public Env Clone()
        {
            var env = new Env();
            foreach(var scope in env.Vars) {
                var d = new Dictionary<string, Val>();
                foreach (var key in scope.Keys) {
                    d[key] = scope[key];
                }
                env.Vars.Add(d);
            }
            return env;
        }

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

    public class Harp
    {
        private bool breakNext = false; 

        public static void StartRepl() { 
            var harp = new Harp();
            var env = new Env();
            harp.LoadHarpLibInto(env);

            LineEditor le = new LineEditor("repl") {
                HeuristicsMode = "csharp"
            };

            le.AutoCompleteEvent += (text, pos) =>
            {
                string prefix = "";
                var completions = new string[] {
                    "defn", "let", "def", "lambda", "#f", "#t", "io/write", "io/writeln", "io/readkey", "io/readline",
                    "loop", "dotimes"
                };
                return new LineEditor.Completion(prefix, completions);
            };

            string code; 
            while ((code = le.Edit("> ", "")) != null) {
                var result = harp.Eval(env, code); 
                Console.WriteLine(result);
            }
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
                return new Str { Value = token.Lexeme };
            }

            if (token.Type == TokTypes.Bool) {
                return new Bool {Flag = (token.Lexeme == "t")};
            }

            Assert.Fail($"Unhandled token type {token.Type}");
            return new None();
        } 

        Val ParseList(Lexer lexer) { 
            var seq = new Seq();

            while (!lexer.Eof) {
                var token = lexer.PeekNext();

                if (token.Type == TokTypes.OpenParen 
                    || token.Type == TokTypes.OpenBracket
                    || token.Type == TokTypes.OpenBrace) {
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

        private Val ParseVec(Lexer lexer) { 
            var seq = new Vec();

            while (!lexer.Eof) {
                var token = lexer.PeekNext();

                if (token.Type == TokTypes.OpenParen 
                    || token.Type == TokTypes.OpenBracket
                    || token.Type == TokTypes.OpenBrace) {
                    seq.Items.Add(ParseSExpr(lexer));
                }

                if (token.IsTerminal) {
                    // NOTE(Dustin): We can improve this by passing the peeked token in
                    // otherwise we are calling PeakNext twice, which inturn calls GetToken
                    seq.Items.Add(ParseSExpr(lexer));
                }

                if (token.Type == TokTypes.CloseBracket) {
                    lexer.GetNext();
                    return seq;
                }
            }

            return seq;
        }

        private Val ParseDict(Lexer lexer) {
            var dict = new Dict();

            while (!lexer.Eof) {
                var peek = lexer.PeekNext();

                if (peek.Type == TokTypes.OpenParen
                    || peek.Type == TokTypes.OpenBracket
                    || peek.Type == TokTypes.OpenBrace) {
                    dict.Literal.Add(ParseSExpr(lexer)); 
                } else if (peek.IsTerminal) { 
                    dict.Literal.Add(ParseSExpr(lexer));
                } else if (peek.Type == TokTypes.CloseBrace) {
                    lexer.GetNext();
                    return dict; 
                }
            }

            return dict;
        }

        Val ParseSExpr(Lexer lexer) {
            var token = lexer.PeekNext();
            bool quoted = false; 

            if (token.Type == TokTypes.Quote) {
                quoted = true;
                lexer.GetNext();
                token = lexer.PeekNext();
            }

            if (token.IsTerminal) {
                var result = ParseObject(lexer);
                result.Quoted = quoted;
                return result;
            }

            if (token.Type == TokTypes.OpenParen) {
                lexer.GetNext();
                var result = ParseList(lexer);
                result.Quoted = quoted;
                return result;
            }

            if (token.Type == TokTypes.OpenBracket) {
                lexer.GetNext();
                var result = ParseVec(lexer);
                result.Quoted = quoted;
                return result; 
            }

            if (token.Type == TokTypes.OpenBrace) {
                lexer.GetNext();
                var result = ParseDict(lexer);
                result.Quoted = quoted;
                return result; 
            }

            Assert.Fail("Unbalanced parenthesis");
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

        public Val EvalObject(Env env, Val ast)
        {
            if (ast.Quoted) return ast;
            if (ast is Lambda lambda) { return lambda; }
            if (ast is Vec vec) { return vec; }
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

                    if (a.Name == "break") { 
                        breakNext = true;
                        return new None();
                    }

                    if (a.Name == "dotimes") {
                        if (args.Items.Count < 2) {
                            Console.WriteLine("dotimes requires at least 1 argument");
                        }

                        if (args[0] is Num times) {
                            int n = (int)Math.Floor(times.Value);
                            var progn = new Seq();
                            for (int i = 1; i < args.Items.Count; i++) { progn.Items.Add(args[i]);}
                            for (int i = 0; i < n; i++) {
                                EvalProgn(env, progn);
                            } 
                        } else { 
                            Console.WriteLine("dotimes requires a number argument");
                        }

                    }

                    if (a.Name == "loop") {
                        while (!breakNext) {
                            EvalProgn(env, args); 
                        }

                        breakNext = false;
                        return new None();
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
                        if (!(args.Items[0] is Vec)) {
                            Console.WriteLine("Lambda definition requires a vector for its arguments");
                            return new None();
                        }

                        var xs = args.Items[0] as Vec;

                        var body = new Seq();
                        for (int i = 1; i < args.Items.Count; i++) {
                            body.Items.Add(args.Items[i]);
                        }

                        var xss = new List<string>();
                        xs.Items.ForEach(x => xss.Add((x as Atom).Name));

                        // Shallow 
                        var theClosure = env.Vars[env.Vars.Count - 1];

                        var lamb = new Lambda {
                            Args = xss,
                            Progn = body,
                            Closure = theClosure
                        };
                        return lamb;
                    }

                    if (a.Name == "defn") {
                        //TODO(Dustin): Handle functions without a body
                        Assert.IsTrue(args.Items.Count >= 2);
                        var name = args.Items[0] as Atom;

                        if (!(args.Items[1] is Vec)) {
                            Console.WriteLine("Lambda definition requires a vector for its arguments");
                            return new None(); 
                        }

                        var arguments = args.Items[1] as Vec;

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
                } else if (first is Lambda _lambda) {
                    env.Push();
                    for (int i = 0; i < args.Items.Count; i++) {
                        env.Put(_lambda.Args[i], 
                            EvalObject(env, args.Items[i])); 
                    }

                    // Put the closure into the environment
                    if (_lambda.Closure != null) {
                        foreach (var key in _lambda.Closure.Keys)
                        {
                            env.Put(key, _lambda.Closure[key]);
                        }
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
            if (ast is Bool bol) { return bol; }

            if (ast is Dict dict) {
                if (!dict.Evaluated) {
                    // Evaluating the dictionary
                    if (dict.Literal.Count % 2 != 0) {
                        Assert.Fail("Dictionary is missing a key or value");
                    }

                    for (int i = 0; i < (int)dict.Literal.Count; i += 2) {
                        var key = EvalObject(env, dict.Literal[i]);
                        var val = EvalObject(env, dict.Literal[i + 1]);

                        if (key.IsValue == false) {
                            Assert.Fail("Dictionaries require all of their keys to be values");
                        }

                        dict[key] = val;
                    }

                    dict.Literal.Clear();
                    dict.Literal = null;

                    dict.Evaluated = true;
                }

                return dict;
            }

            if (ast is Atom _atom) { 
                return env[_atom.Name];
            } 

            Console.WriteLine($"Unhandled val type in EvalObject: {ast}");
            return new None();
        }

        Val EvalProgn(Env env, Seq ast)
        {
            Val result = new None();
            ast.Items.ForEach(obj => result = EvalObject(env, obj));
            return result;
        }

        public Val Eval(Env env, string code) {
            return EvalProgn(env, Parse(code.Trim()));
        }
    } 
}

class Program
{
    static void Main(string[] args)
    {

        //Harp.Harp.StartRepl();

        string code = File.ReadAllText("programs/rpg/main.harp");
        var h = new Harp.Harp();
        var e = new Harp.Env();
        h.LoadHarpLibInto(e);
        var r = h.Eval(e, code); 
    }
}
