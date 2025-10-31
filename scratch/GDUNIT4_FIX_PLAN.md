# GdUnit4Net Test Adapter Fix - Separate Test Project Support

## Problem Summary

**Issue:** GdUnit4Net test adapter fails when tests requiring Godot runtime are placed in a separate C# project (following standard .NET testing practices).

**GitHub Issue:** [GD-298: Allow Godot runtime dependent unit tests to be placed in separate C# project](https://github.com/MikeSchulze/gdUnit4Net/issues/298)

**Status:** Open, assigned to maintainer MikeSchulze for milestone 5.1.0

## Current Behavior

When using a sibling project structure like:
```
/home/tom/src/l3x/
├── L3x/                    # Main Godot project
│   ├── project.godot
│   ├── L3x.csproj
│   └── [source files]
└── L3x.Tests/              # Separate test project
    ├── L3x.Tests.csproj
    ├── test.runsettings
    └── [test files]
```

Tests marked with `[RequireGodotRuntime]` fail with:
```
ERROR: Cannot instantiate C# script because the associated class could not be found.
Script: 'res://gdunit4_testadapter_v5/GdUnit4TestRunnerScene.cs'.
```

## Root Cause

1. Test adapter sets working directory to `L3x.Tests/` (where tests are)
2. Generates `gdunit4_testadapter_v5/GdUnit4TestRunnerScene.cs` in test project directory
3. Launches Godot with `--path .` (current directory = test project)
4. Test project has no `project.godot` file
5. Test project uses `Microsoft.NET.Sdk`, not `Godot.NET.Sdk`
6. Godot cannot compile the C# script because it's not in a valid Godot project
7. IPC communication fails → timeout

## What We Tried

### Attempted Workaround #1: ProjectPath in test.runsettings
```xml
<GdUnit4>
  <ProjectPath>/home/tom/src/l3x/L3x/</ProjectPath>
</GdUnit4>
```
**Result:** Setting is ignored by test adapter when launching Godot

### Attempted Workaround #2: Manual file placement
- Manually moved `gdunit4_testadapter_v5/` folder to main Godot project
- Added `gdUnit4.api` package to main project so script compiles
**Result:** Test adapter regenerates files in test project location

## Proposed Solution (from GD-298)

Add `GodotProjectDir` configuration parameter to `.runsettings` that the test adapter actually respects when:
1. Determining where to generate test runner files
2. Launching Godot with `--path` argument
3. Setting working directory for test execution

Example configuration:
```xml
<GdUnit4>
  <GodotProjectDir>/home/tom/src/l3x/L3x/</GodotProjectDir>
</GdUnit4>
```

## Expected Behavior After Fix

1. Test adapter reads `GodotProjectDir` from runsettings
2. Generates `gdunit4_testadapter_v5/` in the Godot project directory
3. Launches Godot with `--path /home/tom/src/l3x/L3x/`
4. Godot compiles test runner as part of main project
5. IPC communication succeeds
6. Tests execute successfully

## Our Test Case

File: `L3x.Tests/Integration/GameSceneIntegrationTests.cs`
Test: `InstrumentationNodeCanBeCreated`

```csharp
[TestCase]
[RequireGodotRuntime]
public void InstrumentationNodeCanBeCreated()
{
    var instrumentation = new InstrumentationNode();
    AssertThat(instrumentation).IsNotNull();

    instrumentation
        .Wait(0.1)
        .Call(() => L3xLogger.Print("Test action"))
        .SetTimeout(5.0);

    AssertThat(instrumentation).IsNotNull();
}
```

## Fork & Fix Plan

1. **Fork gdUnit4Net repository**
2. **Locate test adapter code** that:
   - Reads runsettings configuration
   - Determines working directory
   - Generates test runner files
   - Launches Godot process
3. **Implement GodotProjectDir parameter**:
   - Add runsettings schema/parsing for `<GodotProjectDir>`
   - Modify file generation to use GodotProjectDir when specified
   - Update Godot launch to use `--path <GodotProjectDir>`
4. **Test the fix** with our l3x project structure
5. **Create pull request** with:
   - Implementation
   - Documentation updates
   - Example runsettings configuration
6. **Become contributor #10!** 🎉

## References

- **Repository:** https://github.com/MikeSchulze/gdUnit4Net
- **Issue:** https://github.com/MikeSchulze/gdUnit4Net/issues/298
- **Package:** https://www.nuget.org/packages/gdUnit4.test.adapter/
- **Current Version:** 3.0.0

## Environment Details

- **OS:** Linux (Arch)
- **Godot:** 4.5.1.stable.mono
- **.NET:** 8.0
- **gdUnit4.api:** 5.0.0
- **gdUnit4.test.adapter:** 3.0.0
- **Test Framework:** xUnit 2.4.2
- **IDE:** Claude Code / VS Code

## Success Criteria

✅ Test adapter respects GodotProjectDir configuration
✅ Test runner files generated in Godot project directory
✅ Godot launches with correct --path argument
✅ Tests marked with [RequireGodotRuntime] execute successfully
✅ Our InstrumentationNodeCanBeCreated test passes
✅ Godot window appears during test execution
✅ Pull request accepted by maintainer
