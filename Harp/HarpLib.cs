using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Harp {
    public static class Lib {
        public static void LoadHarpLibInto(this Harp harp, Env env){
            void error(string msg) {

            }

            void proto(Seq args, string fn_name, params Type[] types) {
                if (args.Items.Count < types.Length) {
                    Console.WriteLine(
                        $"Function: {fn_name} requires at least {types.Length} arguments, but got {args.Items.Count}");
                }
            }

            Val getTerminal(Val arg) { 
                var value = harp.EvalObject(env, arg); 
                while (!value.IsValue) value = harp.EvalObject(env, value);
                return value;
            } 
            
            env.Vars[0]["+"] = new NativeFunc { Func = (args) => {
                Num result = new Num() {Value = 0}; 
                foreach (Val arg in args) {
                    if (getTerminal(arg) is Num num) { result.Value += num.Value; }
                    else { Assert.Fail("TODO: Handle other types"); }
                } 
                return result;
            }}; 
            
            env.Vars[0]["-"] = new NativeFunc { Func = (args) => {
                Num result = new Num() {Value = (getTerminal(args[0]) as Num).Value}; 
                for(int i = 1; i < args.Items.Count; i++) {
                    if (getTerminal(args[i]) is Num num) { result.Value -= num.Value; }
                    else { Assert.Fail("TODO: Handle other types"); }
                } 
                return result;
            }};

            env.Vars[0]["eq?"] = new NativeFunc {
                Func = (args) => {
                    if (args.Items.Count < 2) {
                        Assert.Fail("eq? requires at least 2 arguments");
                    }

                    var a = harp.EvalObject(env, args.Items[0]);
                    for (int i = 1; i < args.Items.Count; i++)
                        if (!a.Equals(harp.EvalObject(env, args.Items[i])))
                            return Bool.False;

                    return Bool.True;
                }
            };

            env.Vars[0]["<"] = new NativeFunc() { Func = (args) => {
                if (args.Items.Count < 2) { Console.WriteLine("< function requires 2 arguments"); return Bool.False; } 
                if (getTerminal(args[0]) is Num an && getTerminal(args[1]) is Num bn)
                    return new Bool {Flag = an.Value < bn.Value}; 
                Console.WriteLine("< function requires all of its arguments to be numbers");
                return Bool.False;
            }};

            env.Vars[0][">"] = new NativeFunc() { Func = (args) => {
                if (args.Items.Count < 2) { Console.WriteLine("< function requires 2 arguments"); return Bool.False; } 
                if (getTerminal(args[0]) is Num an && getTerminal(args[1]) is Num bn)
                    return new Bool {Flag = an.Value > bn.Value}; 
                Console.WriteLine("> function requires all of its arguments to be numbers");
                return Bool.False;
            }};

            env.Vars[0]["<="] = new NativeFunc() { Func = (args) => {
                if (args.Items.Count < 2) { Console.WriteLine("< function requires 2 arguments"); return Bool.False; } 
                if (getTerminal(args[0]) is Num an && getTerminal(args[1]) is Num bn)
                    return new Bool {Flag = an.Value <= bn.Value}; 
                Console.WriteLine("<= function requires all of its arguments to be numbers");
                return Bool.False;
            }};

            env.Vars[0][">="] = new NativeFunc() { Func = (args) => {
                if (args.Items.Count < 2) { Console.WriteLine("< function requires 2 arguments"); return Bool.False; } 
                if (getTerminal(args[0]) is Num an && getTerminal(args[1]) is Num bn)
                    return new Bool {Flag = an.Value >= bn.Value}; 
                Console.WriteLine(">= function requires all of its arguments to be numbers");
                return Bool.False;
            }};
 
            env.Vars[0]["neq?"] = new NativeFunc { 
                Func = (args) => {
                    if (args.Items.Count < 2) {
                        Assert.Fail("eq? requires at least 2 arguments");
                    }

                    var a = harp.EvalObject(env, args.Items[0]);
                    for (int i = 1; i < args.Items.Count; i++)
                        if (a.Equals(harp.EvalObject(env, args.Items[i])))
                            return Bool.False;

                    return Bool.True;
                }
            };

            env.Vars[0]["exit"] = new NativeFunc {
                Func = (args) => {
                    System.Environment.Exit(69);
                    return Bool.True;
                }
            };

            env.Vars[0]["io/write"] = new NativeFunc {
                Func = (args) => {
                    args.Items.ForEach((i) => {
                        Console.Write($"{harp.EvalObject(env, i)} ");
                    });
                    return args;
                }
            };

            env.Vars[0]["io/set-cursor"] = new NativeFunc {
                Func = (args) =>
                {
                    if (args.Items.Count < 2)
                    {
                        Console.WriteLine("< function requires 2 arguments");
                        return Bool.False;
                    }

                    if (getTerminal(args[0]) is Num an && getTerminal(args[1]) is Num bn)
                    {
                        Console.SetCursorPosition(
                            (int) Math.Floor(an.Value),
                            (int) Math.Floor(bn.Value)
                        );
                        return new Vec() {Items = new List<Val> {an, bn}};
                    }

                    Console.WriteLine("io/set-cursor expects 2 integer arguments");
                    return new None();
                }
            };

            env.Vars[0]["io/writeln"] = new NativeFunc {
                Func = (args) => {
                    args.Items.ForEach((i) => {
                        Console.WriteLine($"{harp.EvalObject(env, i)} ");
                    });
                    return args;
                }
            };

            env.Vars[0]["io/read-line"] = new NativeFunc {
                Func = (args) => {
                    var input = Console.ReadLine();
                    if (double.TryParse(input, out double n)) {
                        return new Num() {Value = n};
                    } else {
                        return new Str() {Value = input};
                    }
                }
            };

            env.Vars[0]["io/clear"] = new NativeFunc() {
                Func = (args) => { Console.Clear(); return new None(); } 
            };

            env.Vars[0]["io/read-key"] = new NativeFunc {
                Func = (args) => {
                    var input = Console.ReadKey(true).KeyChar.ToString();
                    if (double.TryParse(input, out var n)) {
                        return new Num() {Value = n};
                    } else {
                        return new Str() {Value = input};
                    }
                }
            };

            env.Vars[0]["d/get"] = new NativeFunc() {
                Func = (args) => {
                    if (args.Items.Count != 2) {
                        Console.WriteLine("g/get requires 2 arguments (dict, key)");
                        return new None();
                    }

                    if (harp.EvalObject(env, args[0]) is Dict d) {
                        return d[harp.EvalObject(env, args[1])];
                    } else {
                        Console.WriteLine("g/get requires its first argument to be the dictionary");
                        return new None();
                    }
                } 
            };

            env.Vars[0]["d/set"] = new NativeFunc() {
                Func = (args) => {
                    if (args.Items.Count != 3) {
                        Console.WriteLine("g/set requires 3 arguments (dict key value)");
                        return new None();
                    } 
                    if (harp.EvalObject(env, args[0]) is Dict d) {
                        var key = harp.EvalObject(env, args[1]);
                        d[key] = harp.EvalObject(env, args[2]);
                        return d[key];
                    } else {
                        Console.WriteLine("g/get requires its first argument to be the dictionary");
                        return new None();
                    }
                }
            };

            env.Vars[0]["v/push"] = new NativeFunc {
                Func = (args) => {
                    proto(args, "v/push", typeof(Vec), typeof(Val));

                    if (args.Items.Count < 2) { Console.WriteLine($"v-push expects at least 2 arguments"); }
                    if (harp.EvalObject(env, args.Items[0]) is Vec v) {
                        for (int i = 1; i < args.Items.Count; i++) { 
                            var value = harp.EvalObject(env, args.Items[i]); 
                            while (!value.IsValue) value = harp.EvalObject(env, value); 
                            v.Items.Add(value);
                        } 
                        return v;
                    }
                    else {
                        Console.WriteLine($"v-push expects the first argument to be a vec but its: {args.Items[0]}");
                        return new None();
                    }
                }
            };

            env.Vars[0]["v/get"] = new NativeFunc {
                Func = (args) => {
                    if (args.Items.Count < 2) { Console.WriteLine($"v-get expects at least 2 arguments"); }

                    if (harp.EvalObject(env, args.Items[0]) is Vec v) {
                        if (harp.EvalObject(env, args.Items[1]) is Num num) {
                            int index = (int) Math.Floor(num.Value);
                            return v.Items[index];
                        } else {
                            Console.WriteLine($"v-get: second argument should be an integer but is {args.Items[1]} ");
                            return new None();
                        }
                    } else {
                        Console.WriteLine($"v-get expects the first argument to be a vec but its: {args.Items[0]}");
                        return new None();
                    } 
                }
            };

            env.Vars[0]["inc"] = new NativeFunc {
                Func = (args) => {
                    if (args.Items.Count < 1) {
                        Console.WriteLine("Inc requires at least 1 argument.");
                        return new None();
                    }

                    if (harp.EvalObject(env, args.Items[0]) is Num num) {
                        num.Value += 1;
                        return num;
                    } else {
                        Console.WriteLine("Inc requires a number");
                        return new None();
                    }
                }
            };

            env.Vars[0]["dec"] = new NativeFunc {
                Func = (args) => {
                    if (args.Items.Count < 1) {
                        Console.WriteLine("Inc requires at least 1 argument.");
                        return new None();
                    }

                    if (harp.EvalObject(env, args.Items[0]) is Num num) {
                        num.Value -= 1;
                        return num;
                    } else {
                        Console.WriteLine("Inc requires a number");
                        return new None();
                    }
                }
            };
        }
    }
}
