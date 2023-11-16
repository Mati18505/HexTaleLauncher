// See the Electron documentation for details on how to use preload scripts:
// https://www.electronjs.org/docs/latest/tutorial/process-model#preload-scripts

const { ipcRenderer, contextBridge } = require("electron");

const API = {
    window: {
      close: () => ipcRenderer.send("app/close"),
      minimize: () => ipcRenderer.send("app/minimize"),
    },

    misc: {
      openSite: (site) => {ipcRenderer.send("misc/openSite", site)},
    },

    launcher: {
      playButtonClick: () => ipcRenderer.send("launcher/playButtonClick"),
      progressBarVisible: (callback) => ipcRenderer.on('launcher/progressBarVisible', callback),
      progressBarValue: (callback) => ipcRenderer.on('launcher/progressBarValue', callback),
      status: (callback) => ipcRenderer.on('launcher/status', callback),
      info: (callback) => ipcRenderer.on('launcher/info', callback),

      saveSettings: (settings) => ipcRenderer.send("launcher/saveSettings", settings),
      getSettings: () => ipcRenderer.invoke("launcher/getSettings"),
    },
}

contextBridge.exposeInMainWorld("app", API);