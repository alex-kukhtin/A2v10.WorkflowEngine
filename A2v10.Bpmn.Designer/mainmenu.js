// Copyright © 2020-2023 Alex Kukhtin. All rights reserved.

'use strict';

const version = '10.1.8097'

const { dialog } = require('electron')
const fs = require('fs');

const document = require('./document');

const FILE_FILTERS = [
	{ name: 'Bpmn files', extensions: ["bpmn"] },
	{ name: 'All files', extensions: ["*"] }
];

const THUMB_FILTERS = [
	{ name: 'Svg files', extensions: ["svg"] },
	{ name: 'All files', extensions: ["*"] }
];

const mainMenu = [
	{
		label: 'File',
		submenu: [
			{
				label: 'New',
				accelerator: 'CmdOrCtrl+N',
				click: fileNew
			},
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
				accelerator: 'CmdOrCtrl+Shift+S',
				click: fileSaveAs
			},
			{
				label: 'Export...',
				accelerator: 'CmdOrCtrl+E',
				click: fileExport
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
			{
				label: 'About...',
				accelerator: 'F1',
				click: showAbout
			}
		]
	}
];

module.exports = {
	mainMenu
};

async function setCurrentName() {
	const res = await dialog.showSaveDialog(document.mainWindow, {
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

async function fileNew(mi, bw) {
	if (document.isModified()) {
		// try to save current document
		let cont = await tryToSaveCurrentFile();
		if (!cont) return;
	}
	bw.webContents.send('FILE.SETNEW');
	document.setNewName();
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

async function fileExport() {
	const fileName = document.file.name.replace('.bpmn', '.svg');
	const res = await dialog.showSaveDialog(document.mainWindow, {
		properties: ['dontAddToRecent'],
		defaultPath: fileName,
		filters: THUMB_FILTERS
	});
	if (res.canceled)
		return;
	const content = await document.getThumb();
	fs.writeFileSync(res.filePath, content.svg);
	return true;
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
			'\n\nCopyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.' +
			'\n\nPowered by https://bpmn.io' + 
			'\nuses ACE script editor (https://ace.c9.io)',
		title: 'A2v10.Bpmn.Designer',
		icon: __dirname + '/favicon.ico'
	});
}