using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Harp {
    public static class Lib {
        public static void LoadHarpLibInto(this Harp harp, Env env){ 
            //monoid
            Val Reduce(Func<Num, Num, Num> op, Seq args, double initial = 0.0f) {
                var result = new Num { Value = initial };

                for (int i = 0; i < args.Items.Count; i++) {
                    var seq = harp.EvalObject(env, args.Items[i]);

                    if (seq.IsValue) {
                        if (seq is Num n) {
                            result = op(result, n);
                        } else {
                            Console.WriteLine($"Binary operator requires numbers not {seq.GetType()}");
                            return new None();
                        }
                    } else {
                        var seq2 = harp.EvalObject(env, seq);
                        if (seq2.IsValue) {
                            if (seq2 is Num n) {
                                result = op(result, n);
                            } else {
                                Console.WriteLine($"Binary operator requires numbers not {seq2.GetType()}");
                                return new None();
                            }
                        } else {
                            Assert.Fail("Never should've made it here");
                        }
                    }


                }

                return new None();
            }
            
            env.Vars[0]["+"] = new NativeFunc { Func = (args) => {
                return Reduce((a, b) => new Num { Value = a.Value + b.Value }, args);
            }};

            env.Vars[0]["-"] = new NativeFunc { Func = (args) => {
                var first = args.Items[0] as Num;
                args.Items.RemoveAt(0);
                return Reduce(
                    (a, b) => new Num { Value = a.Value - b.Value },
                    args, first.Value);
            }}; 
            
            env.Vars[0]["*"] = new NativeFunc { Func = (args) => {
                return Reduce((a, b) => new Num { Value = a.Value * b.Value }, args, 1.0f);
            }};

            env.Vars[0]["/"] = new NativeFunc { Func = (args) => {
                return new Num { Value = 3 };
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

            env.Vars[0]["show"] = new NativeFunc {
                Func = (args) => {
                    args.Items.ForEach((i) => {
                        Console.WriteLine($"{harp.EvalObject(env, i)} ");
                    });
                    return args;
                }
            };
        }
    }
}
