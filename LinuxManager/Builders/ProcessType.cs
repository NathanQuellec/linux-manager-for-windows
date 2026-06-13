namespace LinuxManager.Helpers;

/// <summary>
/// Describes how a process should behave once started. Each value maps to a
/// single, centralised <see cref="System.Diagnostics.ProcessStartInfo"/>
/// configuration in <see cref="ProcessFactory"/>, so call sites only have to
/// pick an intent instead of re-assembling the same flags every time.
/// </summary>
public enum ProcessType
{
    /// <summary>
    /// Runs hidden (no window) and captures the standard output stream.
    /// Use when the caller needs to read the command's stdout.
    /// </summary>
    ReadOutput,

    /// <summary>
    /// Runs hidden (no window) and captures both the standard output and
    /// standard error streams.
    /// </summary>
    ReadOutputAndError,

    /// <summary>
    /// Runs hidden (no window) without capturing any stream. Fire-and-forget
    /// work such as terminating, shutting down or silently launching a distro.
    /// </summary>
    Background,

    /// <summary>
    /// Launched through the OS shell with a visible window so the user can
    /// interact with it (e.g. opening a distribution in a terminal).
    /// </summary>
    Interactive,

    /// <summary>
    /// Launched through the OS shell with the "runas" verb to request
    /// elevation (UAC prompt).
    /// </summary>
    Elevated,

    /// <summary>
    /// Framework defaults: lets the OS shell open the given resource
    /// (e.g. opening a folder in Explorer).
    /// </summary>
    Default,
}
