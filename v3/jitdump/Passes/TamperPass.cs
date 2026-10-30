// Passes/TamperPass.cs — Neutralize integrity checks and perform cctor line-surgery for hybrid T9
using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace AutoReaktor.Passes {
    public static class TamperPass {
        public static int Run(ModuleDef mod) {
            int killed = 0;
            int surgeryCount = 0;

            foreach (var t in mod.GetTypes()) {
                if (t.Name != "<Module>") continue;
                foreach (var m in t.Methods) {
                    if (!m.IsStaticConstructor) continue;
                    if (!m.HasBody) continue;
                    var instrs = m.Body.Instructions;

                    // 1. Line surgery: keep string encryption table init / calls, but neuter anti-tamper / loader faults
                    for (int i = 0; i < instrs.Count; i++) {
                        // If we spot a throw or a call to a known tampered helper that faults type-load, make it harmless
                        if (instrs[i].OpCode == OpCodes.Throw) {
                            instrs[i].OpCode = OpCodes.Ret;
                            killed++;
                        }
                    }

                    // 2. Specific Reactor 7.x cctor pattern surgery:
                    // If cctor calls a method that checks image integrity and throws, replace branch/call with Nop
                    for (int i = 0; i < instrs.Count - 1; i++) {
                        if (instrs[i].OpCode == OpCodes.Call && instrs[i].Operand is IMethod meth) {
                            // Check for suspicious internal helper names or patterns characteristic of Reactor AT/loader
                            if (meth.Name.Contains("smethod_") || meth.Name.Contains("Class3")) {
                                // Nop out the call and its result test if followed by Brtrue/Brfalse
                                instrs[i].OpCode = OpCodes.Nop;
                                instrs[i].Operand = null;
                                surgeryCount++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"[pass] TamperPass neutralized throws: {killed}, cctor helper calls noped: {surgeryCount}");
            return killed + surgeryCount;
        }
    }
}
