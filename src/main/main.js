const { app, BrowserWindow, ipcMain, shell, dialog } = require('electron');
const path = require('path');
const ffi = require('@breush/ffi-napi');

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (require('electron-squirrel-startup')) {
  app.quit();
}

var mainWindow;

const createWindow = () => {
  // Create the browser window.
  mainWindow = new BrowserWindow({
    width: 1366,
    height: 768,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
    },
    titleBarStyle: 'hidden',
    title: "HexTale Launcher",
  });

  // and load the index.html of the app.
  if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
    mainWindow.loadURL(MAIN_WINDOW_VITE_DEV_SERVER_URL);
  } else {
    mainWindow.loadFile(path.join(__dirname, `${MAIN_WINDOW_VITE_NAME}/index.html`));
  }
};

var HexTaleLauncherLib = new ffi.Library(path.join(__dirname, "../../HexTaleLauncherLibrary"), {
  "OnPlayButtonClick": ["bool", []],
  "CheckForUpdates": ["void", []],
  "Initialize": ["void", []],
  "DeInitialize": ["void", []],
  "SetInfoCallback": ["void", ['pointer']],
  "SetAlertCallback": ["void", ['pointer']],
  "SetProgressBarValueCallback": ["void", ['pointer']],
  "SetStatusCallback": ["void", ['pointer']],
  "SetProgressBarVisibilityCallback": ["void", ['pointer']]
});

var infoCallback;
var alertCallback;
var statusCallback;
var progressBarValueCallback;
var progressBarVisibilityCallback;

function InitializeLibrary(){
  infoCallback = ffi.Callback('void', ['string'], (message) => mainWindow.webContents.send("launcher/info", message));
  HexTaleLauncherLib.SetInfoCallback(infoCallback);
  
  alertCallback = ffi.Callback('void', ['string'], (message) => dialog.showErrorBox("Warning", message));
  HexTaleLauncherLib.SetAlertCallback(alertCallback);

  statusCallback = ffi.Callback('void', ['string'], (message) => mainWindow.webContents.send("launcher/status", message));
  HexTaleLauncherLib.SetStatusCallback(statusCallback);
  
  progressBarValueCallback = ffi.Callback('void', ['int'], (value) => mainWindow.webContents.send("launcher/progressBarValue", value));
  HexTaleLauncherLib.SetProgressBarValueCallback(progressBarValueCallback);
  
  progressBarVisibilityCallback = ffi.Callback('void', ['int'], (visible) => mainWindow.webContents.send("launcher/progressBarVisible", Boolean(visible)));
  HexTaleLauncherLib.SetProgressBarVisibilityCallback(progressBarVisibilityCallback);
}

function InitializeLauncher() {
  console.log("Initializing");
  HexTaleLauncherLib.Initialize();
  console.log("Checking for updates");

  HexTaleLauncherLib.CheckForUpdates.async((err, res) => {
    if (err) throw err;
    console.log("Checking for updates done");
  });
}
app.on('ready', () => {
  InitializeLibrary();
  createWindow();
  setTimeout(()=>{InitializeLauncher(); },1000); // we wait until the window opens to receive all callbacks
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

ipcMain.on("app/close", () => {
  app.quit();
  console.log("Deinitializing");
  HexTaleLauncherLib.DeInitialize();
  
  //Avoids garbage collection?
  progressBarVisibilityCallback;
  progressBarValueCallback;
  statusCallback;
  alertCallback;
  infoCallback;
});

ipcMain.on("app/minimize", () => {
  mainWindow.minimize();
});

ipcMain.on("misc/openSite", (event, site) => {
  shell.openExternal(site);
});

ipcMain.on("launcher/playButtonClick", () => {
  HexTaleLauncherLib.OnPlayButtonClick.async((err, res) => {
    if (err) throw err;
    console.log("OnPlay done with result: " + res);
    if(res == true)
      if(settings.exitLauncherWhenGameStarts)
        mainWindow.close();
  });
});

var settings = {
  exitLauncherWhenGameStarts: true
};

ipcMain.on("launcher/saveSettings", (event, newSettings) => settings = newSettings);

ipcMain.handle("launcher/getSettings", async (event) => {
  return JSON.stringify(settings);
});