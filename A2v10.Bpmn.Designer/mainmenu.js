
const { dialog } = require('electron')

const fs = require('fs');
const path = require('path');

const currentFile = {
	name: '',
	path: ''
};

const mainMenu = [
	{
		label: 'File',
		submenu: [
			{
				label: 'Open...',
				accelerator: 'CmdOrCtrl+O',
				click: fileOpen
			},
			{
				label: 'Save',
				accelerator: 'CmdOrCtrl+S',
				click: fileSave
			},
			{
				label: 'Save As...',
				click: fileSaveAs
			},
			{ type: 'separator' },
			{ role: 'quit' }
		]
	},
	{
		label: 'View',
		submenu: [
			{ role: 'reload' },
			{ role: 'forceReload' },
			{ type: 'separator' },
			{ role: 'toggleDevTools' }
		]
	}
];

async function fileOpen(mi, bw, ev) {
	var file = await dialog.showOpenDialog({
		properties: ['openFile'],
		filters: [
			{ name:'Bpmn files', extensions: ["bpmn"] },
			{ name: 'All files', extensions: ["*"] }
		]
	});
	if (file.canceled)
		return;
	currentFile.path = file.filePaths[0];
	currentFile.name = path.basename(currentFile.path);
	fs.readFile(currentFile.path, 'UTF-8', (err, data) => {
		bw.webContents.send("FILE.OPEN", { data, name: currentFile.name });
	});
}

function fileSave(mi, bw, ev) {
	let elem = { x: 1, y: 2 };
	bw.webContents.sendSync("FILE.TEST", elem);
	console.dir(elem);
}

function fileSaveAs() {
	console.dir('file.save.as');
}

module.exports = {
	mainMenu,
	currentFile
};
