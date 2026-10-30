// Passes/TamperPass.cs — Neutralize integrity / tamper checks in module cctors
using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace AutoReaktor.Passes {
    public static class TamperPass {
        public static int Run(ModuleDef mod) {
            int killed = 0;
            foreach (var t in mod.GetTypes()) {
                if (t.Name != "<Module>") continue;
                foreach (var m in t.Methods) {
                    if (!m.IsStaticConstructor) continue;
                    if (!m.HasBody) continue;
                    var instrs = m.Body.Instructions;
                    // Look for throw instructions or known tamper check patterns in cctor
                    for (int i = 0; i < instrs.Count; i++) {
                        if (instrs[i].OpCode == OpCodes.Throw) {
                            // Turn the check into Nop / Ret or bypass condition
                            instrs[i].OpCode = OpCodes.Ret;
                            // Nop out preceding load/branch instructions if needed
                            killed++;
                        }
                    }
                }
            }
            Console.WriteLine("[pass] TamperPass neutralized cctor throws: " + killed);
            return killed;
        }
    }
}
