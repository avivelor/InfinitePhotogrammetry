using System;
using System.Diagnostics;
#if PLATFORM_LUMIN && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif // PLATFORM_LUMIN && !UNITY_EDITOR

namespace UnityEngine.XR.MagicLeap
{
    public static class MagicLeapLogger
    {
        public enum LogLevel : uint {
            /*! Output a fatal error which causes program termination. */
            Fatal = 0,
            /*! Output a serious error. The program may continue. */
            Error = 1,
            /*! Output a warning which may be ignorable. */
            Warning = 2,
            /*! Output an informational message. */
            Info = 3,
            /*! Output a message used during debugging. */
            Debug = 4,
            /*! Output a message used for noisier informational messages. */
            Verbose = 5
        }
        static class Native
        {
#if PLATFORM_LUMIN && !UNITY_EDITOR
            const string library = "ml_logging_ext";
            [DllImport(library, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "MLLoggingLog")]
            public static extern void Log(LogLevel level, string tag, string message);
#else
            public static void Log(LogLevel level, string tag, string message)
            {
                switch (level)
                {
                    case LogLevel.Fatal:
                    case LogLevel.Error:
                        UnityEngine.Debug.LogErrorFormat("[{0}]: {1}", tag, message);
                        break;
                    case LogLevel.Warning:
                        UnityEngine.Debug.LogWarningFormat("[{0}]: {1}", tag, message);
                        break;
                    case LogLevel.Info:
                    case LogLevel.Debug:
                    case LogLevel.Verbose:
                    default:
                        UnityEngine.Debug.LogFormat("[{0}]: {1}", tag, message);
                        break;
                }
            }
#endif // PLATFORM_LUMIN && !UNITY_EDITOR
        }

        [Conditional("DEVELOPMENT_BUILD")]
        public static void Assert(bool condition, string tag, string format, params object[] args)
        {
            Assert(condition, LogLevel.Debug, tag, format, args);
        }

        internal static void Assert(bool condition, LogLevel level, string tag, string format, params object[] args)
        {
            if (!condition)
                Native.Log(level, tag, string.Format(format, args));
        }

        public static void AssertError(bool condition, string tag, string format, params object[] args)
        {
            Assert(condition, LogLevel.Error, tag, format, args);
        }

        [Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string tag, string format, params object[] args)
        {
            Native.Log(LogLevel.Debug, tag, string.Format(format, args));
        }

        public static void Warning(string tag, string format, params object[] args)
        {
            Native.Log(LogLevel.Warning, tag, string.Format(format, args));
        }

        public static void Error(string tag, string format, params object[] args)
        {
            Native.Log(LogLevel.Error, tag, string.Format(format, args));
        }
    }
}