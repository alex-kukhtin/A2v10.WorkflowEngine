// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

'use strict';

const version = '10.1.8036'

const { dialog } = require('electron')
const fs = require('fs');

const document = require('./document');

const FILE_FILTERS = [
	{ name: 'Bpmn files', extensions: ["bpmn"] },
	{ name: 'All files', extensions: ["*"] }
];

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
	},
	{
		label: 'Help',
		submenu: [
			{label: 'About...', click: showAbout}
		]
	}
];

module.exports = {
	mainMenu
};

async function setCurrentName() {
	var res = await dialog.showSaveDialog(document.mainWindow, {
		properties: ['dontAddToRecent'],
		defaultPath: document.file.name,
		filters: FILE_FILTERS
	});
	if (res.canceled)
		return false;
	document.setFileName(res.filePath);
	return true;
}

async function saveCurrentFile() {
	if (document.isUntitled()) {
		// try to get fileName
		let cont = await setCurrentName()
		if (!cont)
			return false;
	}
	let content = await document.getContent();
	fs.writeFileSync(document.file.path, content.xml);
	document.setFileSaved();
	await document.clearCommandStack();
	return true;
}

async function tryToSaveCurrentFile() {
	if (!document.isModified())
		return true;
	let r = await dialog.showMessageBox(document.mainWindow, {
		title: 'Save changes?',
		type: 'question',
		message: `Do you want to save changes to '${document.file.name}'?`,
		buttons: ['Save', "Don't save", 'Cancel']
	});
	switch (r.response) {
		case 0: // save
			return await saveCurrentFile();
			break;
		case 1: // don't save
			return true; // continue
		case 2: // cancel
			return false; // cancel
	}
}

async function fileOpen(mi, bw) {
	if (document.isModified()) {
		// try to save current document
		let cont = await tryToSaveCurrentFile();
		if (!cont) return;
	}
	let files = await dialog.showOpenDialog(bw, {
		properties: ['openFile'],
		filters: FILE_FILTERS
	});
	if (files.canceled)
		return;
	fs.readFile(files.filePaths[0], { encoding: 'UTF-8' }, (err, data) => {
		bw.webContents.send('FILE.SETCONTENT', { content: data });
		document.setFileName(files.filePaths[0]);
	});
}

async function fileSave() {
	await saveCurrentFile();
}

async function fileSaveAs() {
	let cont = await setCurrentName();
	if (!cont)
		return;
	await saveCurrentFile();
}

function showAbout(mi, bw) {
	dialog.showMessageBox(bw, {
		message: 'About A2v10 BPMN Designer',
		type: 'info',
		detail: 'version: ' + version +
			'\n\nCopyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.' +
			'\n\npowered by https://bpmn.io',
		title: 'A2v10.Bpmn.Designer',
		icon: __dirname + '/favicon.ico'
	});
}