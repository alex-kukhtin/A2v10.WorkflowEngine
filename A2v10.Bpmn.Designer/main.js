
'use strict';

const { app, BrowserWindow, Menu, dialog, MenuItem } = require('electron')
const fs = require('fs');
const path = require('path');

function createWindow() {
	const mainWindow = new BrowserWindow({
		width: 1024,
		height: 768,
		webPreferences: {
			nodeIntegration: true,
			contextIsolation: false,
			enableRemoteModule: true //,
			//preload: path.join(__dirname, 'preload.js')
		}
	});

	// and load the index.html of the app.
	mainWindow.loadFile('index.html');

	// Open the DevTools.
	//mainWindow.webContents.openDevTools()
}

app.whenReady().then(() => {
	createWindow();

	app.on('activate', function () {
		if (BrowserWindow.getAllWindows().length === 0)
			createWindow()
	})
});

app.on('window-all-closed', function () {
	if (process.platform !== 'darwin')
		app.quit()
});

