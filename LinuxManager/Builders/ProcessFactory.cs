using System.Diagnostics;

namespace LinuxManager.Helpers;

/// <summary>
/// Centralises <see cref="Process"/> creation so that each process behaviour
/// (capturing output, running silently in the background, opening a visible
/// window, elevating, ...) is configured in exactly one place instead of being
/// re-assembled at every call site. The behaviour is selected via
/// <see cref="ProcessType"/>; the underlying flags are still applied through
/// <see cref="ProcessBuilder"/>.
/// </summary>
public static class ProcessFactory
{
    /// <summary>
    /// Creates a (non-started) <see cref="Process"/> configured for the given
    /// <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The behaviour the process should adopt.</param>
    /// <param name="fileName">The executable to run (e.g. "cmd.exe").</param>
    /// <param name="arguments">Optional command-line arguments.</param>
    public static Process Create(ProcessType type, string fileName, string? arguments = null)
    {
        var builder = new ProcessBuilder(fileName);

        if (!string.IsNullOrEmpty(arguments))
        {
            builder.SetArguments(arguments);
        }

        switch (type)
        {
            case ProcessType.ReadOutput:
                builder.SetUseShellExecute(false)
                       .SetRedirectStandardOutput(true)
                       .SetCreateNoWindow(true);
                break;

            case ProcessType.ReadOutputAndError:
                builder.SetUseShellExecute(false)
                       .SetRedirectStandardOutput(true)
                       .SetRedirectStandardError(true)
                       .SetCreateNoWindow(true);
                break;

            case ProcessType.Background:
                builder.SetUseShellExecute(false)
                       .SetCreateNoWindow(true);
                break;

            case ProcessType.Interactive:
                builder.SetUseShellExecute(true)
                       .SetCreateNoWindow(true);
                break;

            case ProcessType.Elevated:
                builder.SetUseShellExecute(true)
                       .SetVerb("runas");
                break;

            case ProcessType.Default:
                // Framework defaults: let the OS shell open the resource as-is.
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported process type");
        }

        return builder.Build();
    }
}
