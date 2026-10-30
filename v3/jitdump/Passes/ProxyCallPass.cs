// Passes/ProxyCallPass.cs — Inline or simplify proxy method calls (proxy stubs)
using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace AutoReaktor.Passes {
    public static class ProxyCallPass {
        public static int Run(ModuleDef mod) {
            int inlined = 0;
            // Simple proxy stub pattern: a method with 1-3 instructions doing a direct call and return.
            foreach (var t in mod.GetTypes()) {
                foreach (var m in t.Methods) {
                    if (!m.HasBody || m.IsConstructor) continue;
                    var instrs = m.Body.Instructions;
                    // Pattern: ldarg..., call TargetMethod, ret
                    if (instrs.Count >= 2 && instrs[instrs.Count - 1].OpCode == OpCodes.Ret) {
                        var prev = instrs[instrs.Count - 2];
                        if (prev.OpCode == OpCodes.Call || prev.OpCode == OpCodes.Callvirt) {
                            // Candidate proxy stub
                            // Future: resolve target and redirect callers or inline directly.
                        }
                    }
                }
            }
            Console.WriteLine("[pass] ProxyCallPass inspected stubs: " + inlined);
            return inlined;
        }
    }
}
