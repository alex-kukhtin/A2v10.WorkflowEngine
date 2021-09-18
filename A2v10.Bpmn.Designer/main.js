// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

'use strict';

const { app, BrowserWindow, dialog, Menu} = require('electron')

const { mainMenu } = require('./mainmenu');
const document = require('./document');

function createWindow() {
	let mainWindow = new BrowserWindow({
		width: 1300,
		height: 800,
		icon: __dirname + '/favicon.ico',
		webPreferences: {
			preload: 'preload.js',
			sandbox: false,
			nodeIntegration: true,
			contextIsolation: false,
			enableRemoteModule: true
		}
	});
	document.setMainWindow(mainWindow);
	// and load the index.html of the app.
	mainWindow.loadFile('index.html');

	// Open the DevTools.
	//mainWindow.webContents.openDevTools()

	mainWindow.on('close', (ev, bw) => {
		if (!document.isModified())
			return;
		let res = dialog.showMessageBoxSync({
			title: 'Exit application',
			type: 'question',
			message: `The document '${document.file.name}' has unsaved changes.\nDo you want to quit without saving?`,
			buttons: ['Disard changes', 'Cancel']
		});
		switch (res) {
			case 0: // Discard changes
				break;
			case 1: // cancel
				ev.preventDefault();
				break;
		}
	});
}

const menu = Menu.buildFromTemplate(mainMenu);
Menu.setApplicationMenu(menu);


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

