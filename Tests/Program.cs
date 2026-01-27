// SPDX-License-Identifier: MIT
using Xunit.Abstractions;
using Xunit.Runners;

namespace Mbemc.DataExchange.Tests;

/// <summary>Provides a console test runner if no nunit testadapter is installed</summary>
class Program
{
    #region Private Fields

    /// <summary>to guarantee consistent console output</summary>
    static readonly object consoleLock = new();

    /// <summary>An event that is triggered as soon as the runner finishes.</summary>
    static readonly ManualResetEvent finished = new(false);

    /// <summary>A counter for all errors.</summary>
    static int errors;

    /// <summary>contains all testnames (identified by the method name of the test) that should be executed. is set to null if all tests should be executed.</summary>
    static HashSet<string>? testsToExecute;

    #endregion Private Fields

    #region Private Methods

    static int Main(string[] args)
    {
        Console.WriteLine($"Running tests with framework {Environment.Version}");
        Console.WriteLine("---");

        var assemblyPath = typeof(Program).Assembly.Location;
        testsToExecute = args.Length > 0 ? new(args, StringComparer.Ordinal) : null;

        using var runner = AssemblyRunner.WithoutAppDomain(assemblyPath);
        runner.OnTestStarting = OnTestStarting;
        runner.OnTestPassed = OnTestPassed;
        runner.OnTestFailed = OnTestFailed;
        runner.OnExecutionComplete = OnExecutionComplete;
        runner.TestCaseFilter = TestCaseFilter;
        runner.Start();
        finished.WaitOne();
        finished.Dispose();

        return errors;
    }

    static void OnExecutionComplete(ExecutionCompleteInfo info)
    {
        lock (consoleLock)
        {
            if (errors == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("---: info TI9999: All tests successfully completed.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"---: error TE9999: {errors} tests failed!");
            }
            Console.ResetColor();
        }
        finished.Set();
    }

    static void OnTestFailed(TestFailedInfo info)
    {
        lock (consoleLock)
        {
            errors++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{info.TypeName}.cs: error TE0001: {info.TypeName}.{info.MethodName}");
            Console.WriteLine(info.ExceptionMessage);
            Console.ResetColor();
        }
    }

    static void OnTestPassed(TestPassedInfo info)
    {
        lock (consoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{info.TypeName}.cs: info TI0002: Success {info.TypeName}.{info.MethodName}");
            Console.ResetColor();
        }
    }

    static void OnTestStarting(TestStartingInfo info)
    {
        lock (consoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{info.TypeName}.cs: info TI0001: Start {info.TypeName}.{info.MethodName}");
            Console.ResetColor();
        }
    }

    static bool TestCaseFilter(ITestCase testCase) => testsToExecute is null || testsToExecute.Contains(testCase.TestMethod.Method.Name);

    #endregion Private Methods
}
