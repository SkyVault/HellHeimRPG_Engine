using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Harp {
    public static class Lib {
        public static void LoadHarpLibInto(this Harp harp, Env env){
            void error(string msg) {

            }

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
                Num result = new Num() {Value = 0}; 
                foreach (Val arg in args) {
                    var value = harp.EvalObject(env, arg); 
                    while (!value.IsValue) value = harp.EvalObject(env, value); 
                    if (value is Num num) { result.Value += num.Value; }
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

            env.Vars[0]["v-push"] = new NativeFunc {
                Func = (args) => {
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

            env.Vars[0]["v-get"] = new NativeFunc {
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
        }
    }
}
