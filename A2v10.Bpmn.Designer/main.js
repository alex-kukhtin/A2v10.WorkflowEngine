// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

'use strict';

const { app, BrowserWindow, dialog, Menu, shell } = require('electron')

const { mainMenu } = require('./mainmenu');
const document = require('./document');

function createWindow() {
	let mainWindow = new BrowserWindow({
		width: 1300,
		height: 800,
		title: 'A2v10 Bpmn Designer',
		icon: __dirname + '/favicon.ico',
		webPreferences: {
			//preload: '/preload.js',
			sandbox: false,
			nodeIntegration: true,
			contextIsolation: false,
			enableRemoteModule: true
		}
	});
	let fileName = '';
	if (process.argv.length > 1) {
		fileName = process.argv[process.argv.length - 1];
	}

	document.setMainWindow(mainWindow, fileName);


	// and load the index.html of the app.
	mainWindow.loadFile('index.html', {
		query: {
			filename: fileName
		}
	});

	mainWindow.webContents.setWindowOpenHandler(({ url }) => {
		// open url in a browser and prevent default
		shell.openExternal(url);
		return { action: 'deny' };
	});


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

