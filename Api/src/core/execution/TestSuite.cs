// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution;

using System.Reflection;

using Api;

internal sealed class TestSuite : IDisposable
{
    private readonly Lazy<IEnumerable<TestCase>> testCases;

    public TestSuite(TestSuiteNode suite)
        : this(
            FindTypeOnAssembly(suite.AssemblyPath, suite.ManagedType),
            suite.Tests,
            suite.SourceFile)
    {
    }

    internal TestSuite(Type type, List<TestCaseNode> tests, string sourceFile)
    {
        Instance = Activator.CreateInstance(type)
                   ?? throw new InvalidOperationException($"Cannot create an instance of '{type.FullName}' because it does not have a public parameterless constructor.");

        Name = type.Name;
        ResourcePath = sourceFile;

        // we do lazy loading to only load test case one times
        testCases = new Lazy<IEnumerable<TestCase>>(() => LoadTestCases(type, tests));
    }

    public int TestCaseCount => TestCases.Count();

    public IEnumerable<TestCase> TestCases => testCases.Value;

    public string ResourcePath { get; set; }

    public string Name { get; set; }

    public string? FullName => GetType().FullName;

    public object Instance { get; set; }

    public bool FilterDisabled { get; set; }

    public void Dispose()
    {
        if (Instance is IDisposable disposable)
            disposable.Dispose();
    }

    private static List<TestCase> LoadTestCases(Type type, List<TestCaseNode> includedTests)
        =>
        [
            .. type.GetMethods()
                .Where(m => m.IsDefined(typeof(TestCaseAttribute)))
                .Join(
                    includedTests,
                    m => m.Name,
                    test => test.ManagedMethod,
                    (mi, test) => new TestCase(test.Id, mi, test.LineNumber, test.AttributeIndex))
        ];

    private static Type FindTypeOnAssembly(string assemblyPath, string clazz)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            // if (assembly.Location != assemblyName)
            //    continue;
            var type = assembly.GetType(clazz);
            if (type != null)
                return type;
        }

        try
        {
            Console.WriteLine($"[GdUnit4] Loading assembly from: {assemblyPath}");
            Console.WriteLine($"[GdUnit4] Assembly exists: {File.Exists(assemblyPath)}");
            var assembly = Assembly.LoadFrom(assemblyPath);
            Console.WriteLine($"[GdUnit4] Assembly loaded: {assembly.FullName}");
            var type = assembly.GetType(clazz);
            Console.WriteLine($"[GdUnit4] Type found: {type != null}");
            if (type == null)
                throw new InvalidOperationException($"Type {clazz} not found in assembly {assembly.FullName}");
            return type;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to resolve type '{clazz}': {ex.Message}");
            Console.Error.WriteLine($"Exception: {ex}");
            throw new InvalidOperationException($"Could not find type {clazz} on assembly {assemblyPath}");
        }
    }
}
