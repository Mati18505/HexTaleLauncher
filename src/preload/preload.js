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
    }
}

contextBridge.exposeInMainWorld("app", API);