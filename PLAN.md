# autoreaktor — Development Plan

This document tracks the recovery roadmap, completed architectural fixes, and modular refactoring phases for .NET Reactor 7.x unpacking.

---

## Completed Phases

1. **Behaviour Oracle & Verdict Cleanliness**
   - Replaced fragile string/exit-code heuristics with a true behaviour oracle: execution of the protected binary sets the stdout/stderr baseline.
   - GUI/blocking targets correctly recognized via alive grace windows (`ALIVE_GRACE`).
   - Separation of `run-recovered` vs `run-recovered-base-only` (slayer/de4dot shell vs real body recovery).

2. **x64 Hook & Dump Stability**
   - Corrected attach-time JIT framework discrimination (`mscoree.dll` vs `clr.dll`).
   - Integrated the `--force` `forcedump` companion (`PrepareMethod` walk), achieving stable body acquisition (**270 bodies** on `con48x64`).
   - Quantified the PE anti-tamper scope via `pe_tamper_scope.py`: proved that any in-place mapped image mutation is caught by Reactor's cctor check, making offset-preserving rewrites impossible without anti-tamper neutralization.

3. **Artifact Survivability & Process Tree Safety**
   - Base64 inert snapshots (`NB_B64_OUT`) written *before* PE serialization to survive Windows Defender quarantine.
   - Robust process tree termination (`kill_tree` with `taskkill /F /T` on Windows) to prevent orphaned GUI processes.

4. **String Literal Re-binding (`NB_LDSTRMAP`)**
   - Added string map ingestion (`gen_ldstrmap.cs` + `nbilmerge.cs` US heap rewriter) to recover encrypted literals.

---

## Active Phase: Modular Passes (`Passes/`)

Refactoring `nbilmerge.cs` from a monolithic script into structured domain-specific passes:
- **`Passes/AntiDebugPass.cs`**: Automated removal/neutralization of anti-debugging checks (`Debugger.IsAttached`, timing checks).
- **`Passes/ProxyCallPass.cs`**: Inlining and resolution of proxy method calls (proxy delegate stubs).
- **`Passes/TamperPass.cs`**: Neutralization of integrity/tamper checks in module cctors.

---

## Future Phases

- **Hybrid T9 Stabilization**: Full integration of real-token bodies into the cleaned/unpacked Slayer base with automated generics/type-load validation.
- **Full Matrix CI Enforcement**: Complete green run across all 10 target cells (`matrix.json`).
