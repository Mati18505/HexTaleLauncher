//#define managed

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HexTaleLauncherLibrary
{
#if managed
    public class API
    {
        static LauncherLogic? launcher;

        public delegate void AlertCallback(string message);
        static AlertCallback? alertCallback;

        public delegate void InfoCallback(string message);
        static InfoCallback? infoCallback;

        public delegate void ProgressBarValueCallback(int value);
        static ProgressBarValueCallback? progressBarValueCallback;

        public delegate void ProgressBarVisibilityCallback(int visible);
        static ProgressBarVisibilityCallback? progressBarVisibilityCallback;

        public delegate void StatusCallback(string status);
        static StatusCallback? statusCallback;

        public delegate void ErrorCallback(int errorType, string errorMessage);
        static ErrorCallback? errorCallback;

        public static string version = "";

        /* For Internal usage */
        private static async Task RunActionAsync(Action action) => await Task.Run(action);
        public static async Task Alert(string message) => await RunActionAsync(() => alertCallback?.Invoke(message));
        public static async Task Info(string message) => await RunActionAsync(() => infoCallback?.Invoke(message));
        public static async Task ProgressBarValue(int value) => await RunActionAsync(() => progressBarValueCallback?.Invoke(value));
        public static async Task ProgressBarVisibility(bool visible) => await RunActionAsync(() => progressBarVisibilityCallback?.Invoke(Convert.ToInt32(visible)));
        public static async Task Status(string status) => await RunActionAsync(() => statusCallback?.Invoke(status));

        /* For External usage */
        static public void Initialize() { launcher = new LauncherLogic(); }

        static public void DeInitialize() { launcher.OnClose(); }

        //Callbacks
        static public void SetAlertCallback(AlertCallback callback)
        {
            alertCallback = callback;
        }

        static public void SetStatusCallback(StatusCallback callback)
        {
            statusCallback = callback;
        }

        static public void SetInfoCallback(InfoCallback callback)
        {
            infoCallback = callback;
        }

        static public void SetProgressBarValueCallback(ProgressBarValueCallback callback)
        {
            progressBarValueCallback = callback;
        }

        static public void SetProgressBarVisibilityCallback(ProgressBarVisibilityCallback callback)
        {
            progressBarVisibilityCallback = callback;
        }

        static public void SetErrorCallback(ErrorCallback callback)
        {
            errorCallback = callback;
        }

        //Functions
        static public void CheckForUpdates()
        {
            CatchException(() => launcher.CheckForUpdates().Wait());
        }

        static public void OnPlayButtonClick()
        {
            CatchException(launcher.OnPlayButtonClick);
        }

        static public void OpenGameDir()
        {
            CatchException(() => launcher.OpenGameDirInFileExplorer());
        }

        static public void CheckAndRepair()
        {
            CatchException(() => launcher.CheckAndRepair().Wait());
        }

        static public void Uninstall()
        {
            CatchException(() => launcher.Uninstall().Wait());
        }

        static private void HandleLauncherException(LauncherException e)
        {
            errorCallback?.Invoke((int)e.type, e.ToString());
        }

        static private void CatchException(Action action)
        {
            action();
            /*
            try { action(); }
            catch (AggregateException e) when (e.InnerException is LauncherException) // For async methods in which the task is waited
            {
                HandleLauncherException(e.InnerException as LauncherException);
            }
            catch (LauncherException e)
            {
                HandleLauncherException(e);
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(-1, e.ToString());
            }*/
        }
    }
#endif
}
