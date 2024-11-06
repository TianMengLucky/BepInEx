using System;

namespace NextBepLoader.Core.Logging;

/// <summary>
///     The level, or severity of a log entry.
/// </summary>
[Flags]
public enum LogLevel
{
    /// <summary>
    ///     No level selected.
    /// </summary>
    None = 0,

    /// <summary>
    ///     A fatal error has occurred, which cannot be recovered from.
    /// </summary>
    Fatal = 1,

    /// <summary>
    ///     An error has occured, but can be recovered from.
    /// </summary>
    Error = 2,

    /// <summary>
    ///     A warning has been produced, but does not necessarily mean that something wrong has happened.
    /// </summary>
    Warning = 4,

    /// <summary>
    ///     An important message that should be displayed to the user.
    /// </summary>
    Message = 8,

    /// <summary>
    ///     A message of low importance.
    /// </summary>
    Info = 16,

    /// <summary>
    ///     A message that would likely only interest a developer.
    /// </summary>
    Debug = 32,

    /// <summary>
    ///     All log levels.
    /// </summary>
    All = Fatal | Error | Warning | Message | Info | Debug
}
