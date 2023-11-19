const fs = require("fs");
const { join } = require('path');

function isFile(target) {
  return fs.statSync(target).isFile();
}

function copyFile(source, target) {
  fs.copyFileSync(source, target);
}

function copyFiles(source, target) {
  const dirs = fs.readdirSync(source);
  for (const d of dirs) {
    if (isFile(join(source, d))) {
      copyFile(join(source, d), join(target, d));
    } else {
      fs.mkdirSync(join(target, d));
      copyFiles(join(source, d), join(target, d));
    }
  }
}



module.exports = {
  hooks: {
    postPackage: async (forgeConfig, options) => {
      copyFiles("./src/dll", options.outputPaths[0]+"/");
    }
  },
  packagerConfig: {
    asar: true,
    ignore: ["config", "HexTale"]
  },

  rebuildConfig: {},
  makers: [
    {
      name: '@electron-forge/maker-squirrel',
      config: {},
    },
    {
      name: '@electron-forge/maker-zip',
      platforms: ['darwin'],
    },
    {
      name: '@electron-forge/maker-deb',
      config: {},
    },
    {
      name: '@electron-forge/maker-rpm',
      config: {},
    },
  ],
  plugins: [
    {
      name: '@electron-forge/plugin-vite',
      config: {
        // `build` can specify multiple entry builds, which can be Main process, Preload scripts, Worker process, etc.
        // If you are familiar with Vite configuration, it will look really familiar.
        build: [
          {
            // `entry` is just an alias for `build.lib.entry` in the corresponding file of `config`.
            entry: 'src/main/main.js',
            config: 'vite.main.config.mjs',
          },
          {
            entry: 'src/preload/preload.js',
            config: 'vite.preload.config.mjs',
          }
        ],
        renderer: [
          {
            name: 'main_window',
            config: 'vite.renderer.config.mjs',
          },
        ],
      },
    },
  ]
};
