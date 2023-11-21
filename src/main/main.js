const { app, BrowserWindow, ipcMain, shell, dialog, autoUpdater } = require('electron');
const path = require('path');
const ffi = require('@breush/ffi-napi');
const fs = require('fs');

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
    mainWindow.loadFile(path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`));
  }
};

var HexTaleLauncherLib = new ffi.Library('HexTaleLauncherLibrary', {
  "OnPlayButtonClick": ["void", []],
  "CheckForUpdates": ["void", []],
  "Initialize": ["void", []],
  "DeInitialize": ["void", []],
  "OpenGameDir": ["void", []],
  "CheckAndRepair": ["void", []],
  "Uninstall": ["void", []],
  "SetInfoCallback": ["void", ['pointer']],
  "SetAlertCallback": ["void", ['pointer']],
  "SetProgressBarValueCallback": ["void", ['pointer']],
  "SetStatusCallback": ["void", ['pointer']],
  "SetProgressBarVisibilityCallback": ["void", ['pointer']],
  "SetErrorCallback": ["void", ['pointer']]
});

var infoCallback;
var alertCallback;
var statusCallback;
var progressBarValueCallback;
var progressBarVisibilityCallback;
var errorCallback;

function InitializeLibrary(){
  infoCallback = ffi.Callback('void', ['string'], (message) => mainWindow.webContents.send("launcher/info", message));
  HexTaleLauncherLib.SetInfoCallback(infoCallback);
  
  alertCallback = ffi.Callback('void', ['string'], (message) => dialog.showMessageBoxSync(null, {title:"Warning", message:message, type:'warning'}));
  HexTaleLauncherLib.SetAlertCallback(alertCallback);

  statusCallback = ffi.Callback('void', ['string'], (message) => mainWindow.webContents.send("launcher/status", message));
  HexTaleLauncherLib.SetStatusCallback(statusCallback);
  
  progressBarValueCallback = ffi.Callback('void', ['int'], (value) => mainWindow.webContents.send("launcher/progressBarValue", value));
  HexTaleLauncherLib.SetProgressBarValueCallback(progressBarValueCallback);
  
  progressBarVisibilityCallback = ffi.Callback('void', ['int'], (visible) => mainWindow.webContents.send("launcher/progressBarVisible", Boolean(visible)));
  HexTaleLauncherLib.SetProgressBarVisibilityCallback(progressBarVisibilityCallback);
  
  errorCallback = ffi.Callback('void', ['int', 'string'], (errorType, errorMessage) => HandleException(errorType, errorMessage));
  HexTaleLauncherLib.SetErrorCallback(errorCallback);
}

function InitializeLauncher() {
  console.log("Initializing");
  HexTaleLauncherLib.Initialize();
}

function LoadConfig() {
  if(fs.existsSync('./config/settings.json'))
  {
    fs.readFile('./config/settings.json', 'utf8', (err, data) => {
      if (err)
      {
        dialog.showErrorBox("Could not load settings", "Cannot read settings file.");
      } 
      else 
      {
        settings = JSON.parse(data);
      }
    });
  }
}

app.on('ready', () => {
  InitializeLibrary();
  createWindow();
  LoadConfig();
  setTimeout(()=>{InitializeLauncher(); },1000); // we wait until the window opens to receive all callbacks
  if(app.isPackaged)
    InitializeAutoUpdater();
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
  errorCallback;
});

ipcMain.on("app/minimize", () => {
  mainWindow.minimize();
});

ipcMain.on("misc/openSite", (event, site) => {
  shell.openExternal(site);
});

ipcMain.on("launcher/playButtonClick", () => {
  HexTaleLauncherLib.OnPlayButtonClick.async((err) => {
    if(err) throw err;
    if(settings.exitLauncherWhenGameStarts)
      mainWindow.close();
  });
});

function ShowError(errorMessage) {
  dialog.showErrorBox("Launcher error", errorMessage);
}

var settings = {
  exitLauncherWhenGameStarts: true
};

ipcMain.on("launcher/saveSettings", (event, newSettings) => {
  settings = newSettings;

  try { 
    fs.writeFileSync('./config/settings.json', JSON.stringify(newSettings), 'utf-8');
   }
  catch(err) { 
    dialog.showErrorBox("Could not save settings", err);
   }
});

ipcMain.handle("launcher/getSettings", async (event) => {
  return JSON.stringify(settings);
});

ipcMain.on("launcher/openGamePathInExplorer", (event) => HexTaleLauncherLib.OpenGameDir());
ipcMain.on("launcher/repair", (event) => HexTaleLauncherLib.CheckAndRepair.async((err, res) => {if (err) throw err;}));
ipcMain.on("launcher/uninstall", (event) => HexTaleLauncherLib.Uninstall.async((err, res) => {if (err) throw err;}));

function InitializeAutoUpdater() {
  autoUpdater.setFeedURL({url: "https://hextale.xyz/files/launcher"});
  autoUpdater.checkForUpdates();
}

autoUpdater.on("error", (error) => {
  ShowError("Update error: " + error);
});

ipcMain.handle("misc/getAppVersion", async (event) => {
  return app.getVersion();
});

function HandleException(errorType, errorMessage)
{
  const ErrorType = {
    General: 0,
    InvalidOperation: 1,
    BadConfig: 2
  };

  const errorTypesMessages = [
    "Launcher general error",
    "You cannot do this now!",
    "Bad config error"
  ];

  const ShowMinorError = (title, message) => dialog.showMessageBoxSync(null, {title:title, message:message, type:'warning'});

  var title;
  if(errorType === -1)
    title = "Unknown error";
  else
    title = errorTypesMessages[errorType];

  if(errorType === ErrorType.InvalidOperation)
    ShowMinorError(title, title);
  else if(errorType === ErrorType.BadConfig)
    ShowMinorError(title, errorMessage + "\n\nCheck your config file (launcherDir/config/config.json)");
  else
    dialog.showErrorBox(title, errorMessage);
}