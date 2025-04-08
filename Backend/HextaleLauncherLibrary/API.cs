#define unmanaged
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using HexTaleLauncherLibrary;
using static HexTaleLauncherLibrary.API;

namespace HexTaleLauncherLibrary
{
#if unmanaged
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
        
        private delegate void ErrorCallback(int errorType, string errorMessage);
        static ErrorCallback? errorCallback;
        
        private delegate void LauncherStatusChangedCallback(LauncherStatus status);
        static LauncherStatusChangedCallback? launcherStatusChangedCallback;

        /* For Internal usage */
        private static async Task RunActionAsync(Action action) => await Task.Run(action);
        public static async Task Alert(string message) => await RunActionAsync(() => alertCallback?.Invoke(message));
        public static async Task Info(string message) => await RunActionAsync(() => infoCallback?.Invoke(message));
        public static async Task ProgressBarValue(int value) => await RunActionAsync(() => progressBarValueCallback?.Invoke(value));
        public static async Task ProgressBarVisibility(bool visible) => await RunActionAsync(() => progressBarVisibilityCallback?.Invoke(Convert.ToInt32(visible)));
        public static async Task Status(string status) => await RunActionAsync(() => statusCallback?.Invoke(status));
        public static async Task launcherStatusChanged(LauncherStatus status) => await RunActionAsync(() => launcherStatusChangedCallback.Invoke(status));

        /* For External usage */
        [UnmanagedCallersOnly(EntryPoint = "Initialize")]
        static public void Initialize() { CatchException(() => launcher = new LauncherLogic()); }

        [UnmanagedCallersOnly(EntryPoint = "DeInitialize")]
        static public void DeInitialize() { CatchException(() => launcher.OnClose()); }

        //Callbacks
        [UnmanagedCallersOnly(EntryPoint = "SetAlertCallback")]
        static public unsafe void SetAlertCallback(delegate* unmanaged<string, void> callback)
        {
            alertCallback = Marshal.GetDelegateForFunctionPointer<AlertCallback>((nint)callback);
        }

        [UnmanagedCallersOnly(EntryPoint = "SetStatusCallback")]
        static public unsafe void SetStatusCallback(delegate* unmanaged<string, void> callback) 
        {
            statusCallback = Marshal.GetDelegateForFunctionPointer<StatusCallback>((nint)callback);
        }

        [UnmanagedCallersOnly(EntryPoint = "SetInfoCallback")]
        static public unsafe void SetInfoCallback(delegate* unmanaged<string, void> callback)
        {
            infoCallback = Marshal.GetDelegateForFunctionPointer<InfoCallback>((nint)callback);
        }

        [UnmanagedCallersOnly(EntryPoint = "SetProgressBarValueCallback")]
        static public unsafe void SetProgressBarValueCallback(delegate* unmanaged<int, void> callback)
        {
            progressBarValueCallback = Marshal.GetDelegateForFunctionPointer<ProgressBarValueCallback>((nint)callback);
        }

        [UnmanagedCallersOnly(EntryPoint = "SetProgressBarVisibilityCallback")]
        static public unsafe void SetProgressBarVisibilityCallback(delegate* unmanaged<int, void> callback)
        {
            progressBarVisibilityCallback = Marshal.GetDelegateForFunctionPointer<ProgressBarVisibilityCallback>((nint)callback);
        }
        
        [UnmanagedCallersOnly(EntryPoint = "SetErrorCallback")]
        static public unsafe void SetErrorCallback(delegate* unmanaged<int, string, void> callback)
        {
            errorCallback = Marshal.GetDelegateForFunctionPointer<ErrorCallback>((nint)callback);
        }

        [UnmanagedCallersOnly(EntryPoint = "SetLauncherStatusChangedCallback")]
        static public unsafe void SetLauncherStatusChangedCallback(delegate* unmanaged<int, void> callback)
        {
            launcherStatusChangedCallback = Marshal.GetDelegateForFunctionPointer<LauncherStatusChangedCallback>((nint)callback);
        }

        //Functions
        [UnmanagedCallersOnly(EntryPoint = "CheckForUpdates")]
        static public void CheckForUpdates()
        {
            CatchException(() => launcher.CheckForUpdates().Wait());
        }

        [UnmanagedCallersOnly(EntryPoint = "OnPlayButtonClick")]
        static public void OnPlayButtonClick()
        {
            CatchException(launcher.OnPlayButtonClick);
        }

        [UnmanagedCallersOnly(EntryPoint = "OpenGameDir")]
        static public void OpenGameDir()
        {
            CatchException(() => launcher.OpenGameDirInFileExplorer());
        }
        
        [UnmanagedCallersOnly(EntryPoint = "CheckAndRepair")]
        static public void CheckAndRepair()
        {
            CatchException(() => launcher.CheckAndRepair().Wait());
        }
        
        [UnmanagedCallersOnly(EntryPoint = "Uninstall")]
        static public void Uninstall()
        {
            CatchException(() => launcher.Uninstall().Wait());
        }
        
        [UnmanagedCallersOnly(EntryPoint = "IsGameRunning")]
        static public bool IsGameRunning() => LauncherLogic.IsGameRunning();

        static private void HandleLauncherException(LauncherException e)
        {
            errorCallback?.Invoke((int)e.type, e.ToString());
        }

        static private void CatchException(Action action)
        {
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
            }
        }
    }
#endif
}