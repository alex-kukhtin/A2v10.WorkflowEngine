
'use strict';

const { app, BrowserWindow, dialog, Menu, ipcMain } = require('electron')
const pathtool = require('path');
const fs = require('fs');

const { mainMenu } = require('./mainmenu');

let mainWindow = null;

const currentFile = {
	name: 'Untitled.bpmn',
	path: '',
	setPath(fullPath) {
		this.path = fullPath;
		this.name = pathtool.basename(fullPath);
	},
	isEmpty() { return !this.path; }
};

function createWindow() {
	mainWindow = new BrowserWindow({
		width: 1300,
		height: 800,
		webPreferences: {
			nodeIntegration: true,
			contextIsolation: false,
			enableRemoteModule: true //,
			//preload: pathtool.join(__dirname, 'preload.js')
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
		ev.returnValue = { cancel: true };
		return;
	}
	currentFile.setPath(file[0])
	var fileData = fs.readFileSync(currentFile.path, { encoding: 'UTF-8' });
	ev.returnValue = { data: fileData, cancel: false, name: currentFile.name };
});

function doSave(data, opts) {
	if (currentFile.isEmpty() || opts?.saveAs) {
		var fullPath = dialog.showSaveDialogSync(mainWindow, {
			properties: ['dontAddToRecent'],
			defaultPath: currentFile.name,
			filters: [
				{ name: 'Bpmn files', extensions: ["bpmn"] },
				{ name: 'All files', extensions: ["*"] }
			]
		});
		if (!fullPath)
			return {cancel: true }
		currentFile.setPath(fullPath);
	}
	fs.writeFileSync(currentFile.path, data);
	return { cancel: false, name: currentFile.name };
}

function ensureSaved(data) {
	let r = dialog.showMessageBoxSync(mainWindow, {
		title: 'Save changes?',
		type: 'question',
		message: `Do you want to save changes to '${currentFile.name}'?`,
		buttons: ['Save', "Don't save", 'Cancel']
	});
	if (r === 0) // save
		return doSave(data, { saveAs: false });
	else if (r === 1) // don't save
		return { cancel: false };
	else // cancel
		return { cancel: true };
}

ipcMain.on('R.FILE.CHECKSAVE', (ev, arg) => {
	ev.returnValue = ensureSaved(arg.data);
});

ipcMain.on('R.FILE.SAVE', (ev, arg) => {
	ev.returnValue = doSave(arg.data, { saveAs: arg.saveAs });
});
