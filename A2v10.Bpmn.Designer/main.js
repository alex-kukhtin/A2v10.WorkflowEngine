
'use strict';

const { app, BrowserWindow, dialog, Menu} = require('electron')

const { mainMenu } = require('./mainmenu');

function createWindow() {
	const mainWindow = new BrowserWindow({
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

	mainWindow.on('close', (ev) => {
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

