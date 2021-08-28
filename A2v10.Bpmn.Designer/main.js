
'use strict';

const { app, BrowserWindow, dialog, Menu, ipcMain } = require('electron')
const path = require('path');
const fs = require('fs');

const { mainMenu } = require('./mainmenu');

let mainWindow = null;

const currentFile = {
	name: 'Untitled.bpmn',
	path: ''
};

function createWindow() {
	mainWindow = new BrowserWindow({
		width: 1300,
		height: 800,
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

	mainWindow.on('close', (ev, bw) => {
		let res = dialog.showMessageBoxSync({
			title: 'Exit application',
			type:'question',
			message: 'Are you sure?',
			buttons: ['Save', 'Close', 'Cancel']
		});
		console.dir(res);
		//ev.preventDefault();
	})
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

ipcMain.on('R.FILE.OPEN', (ev) => {
	var file = dialog.showOpenDialogSync(mainWindow, {
		properties: ['openFile'],
		filters: [
			{ name: 'Bpmn files', extensions: ["bpmn"] },
			{ name: 'All files', extensions: ["*"] }
		]
	});
	if (!file) {
		ev.returnValue = null;
		return;
	}
	currentFile.path = file[0];
	currentFile.name = path.basename(currentFile.path);
	var fileData = fs.readFileSync(currentFile.path, { encoding: 'UTF-8' });
	ev.returnValue = { data: fileData, name: currentFile.name };
});

function ensureSaved(data) {
	let r = dialog.showMessageBoxSync(mainWindow, {
		title: 'Save changes?',
		type: 'question',
		message: `Do you want to save changes to '${currentFile.name}'?`,
		buttons: ['Save', "Don't save", 'Cancel']
	});
	if (r === 0) {
		// save file (data)
		return 'saved';
	} else if (r === 1)
		return 'continue';
	else
		return 'cancel';
}

ipcMain.on('R.FILE.CHECKSAVE', (ev, arg) => {
	ev.returnValue = ensureSaved(arg.data);
});

ipcMain.on('RENDER.TESTSYNC', (ev, arg) => {
	console.dir(arg);
	ev.returnValue = 'return value';
});

