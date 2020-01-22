using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using UnityEditor;

namespace UnityEditor.XR.MagicLeap.Remote
{
#if UNITY_EDITOR && PLATFORM_LUMIN
    internal static class MagicLeapRemoteAutomation
    {
        // VDCLI -s
        // VDCLI -R Peripheral
        // VDCLI -R Simulator
        // VDCLI -R "Virtual Room"

        // TODO :: replace this with proper logic that goes through ML's path discovery mechanism.
        public static string remoteRoot
        {
            get
            {
                return CombineOrNull(EditorPrefs.GetString("LuminSDKRoot", null), "VirtualDevice");
            }
        }

        public static string vdcli
        {
            get
            {
#if UNITY_EDITOR_WIN
                return CombineOrNull(remoteRoot, "bin/VDCLI.exe");
#else
                return CombineOrNull(remoteRoot, "bin/VDCLI");
#endif
            }
        }

        public static IEnumerable<string> ExecuteCommand(string args)
        {
            if (vdcli == null)
                throw new Exception("Unable to resolve path to VDCLI, have you set your LuminSDK path?");
            var psi = new ProcessStartInfo {
                FileName = vdcli,
                Arguments = args,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using (Process p = Process.Start(psi))
            {
                var output = p.StandardOutput.ReadToEnd();
                var error = p.StandardError.ReadToEnd();
                p.WaitForExit();
                return output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            }
        }

        static string CombineOrNull(string path1, string path2)
        {
            return (string.IsNullOrEmpty(path1.Trim())) ? null : Path.Combine(path1, path2);
        }

        //[MenuItem("Magic Leap/ML Remote/Start MLRemote Server (Test)", true)]
        static bool EnableStartRemoteServer()
        {
            return true;
        }

        //[MenuItem("Magic Leap/ML Remote/Stop MLRemote Server (Test)", true)]
        static bool EnableStopRemoteServer()
        {
            return true;
        }

        //[MenuItem("Magic Leap/ML Remote/Start MLRemote Server (Test)")]
        static void StartRemoteServer()
        {
            ExecuteCommand("-s");
            ExecuteCommand("-R Peripheral");
            ExecuteCommand("-R Simulator");
            ExecuteCommand("-R \"Virtual Room\"");
        }

        //[MenuItem("Magic Leap/ML Remote/Stop MLRemote Server (Test)")]
        static void StopRemoteServer()
        {
            ExecuteCommand("-k");
        }
    }
#endif // UNITY_EDITOR && PLATFORM_LUMIN
}