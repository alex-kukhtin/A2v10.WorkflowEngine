﻿// Copyright © 2021 Alex Kukhtin. All rights reserved.

'use strict';

const { ipcMain } = require('electron')
const pathtool = require('path');

const DEFAULT_NAME = 'Untitiled.bpmn';

const document = {
	mainWindow: null,
	file: {
		name: DEFAULT_NAME,
		path: '',
		modified: false,
		setFileName(fullPath) {
			this.path = fullPath;
			this.name = pathtool.basename(fullPath);
			this.modified = false;
		},
		setDefaultName() {
			this.path = '';
			this.name = DEFAULT_NAME;
			this.modified = false;
		}
	},
	isUntitled() {
		return !this.file.path;
	},
	isModified() {
		return this.file.modified;
	},
	setMainWindow(window, filename) {
		this.mainWindow = window;
		if (filename)
			this.file.setFileName(filename);
		ipcMain.on('FILE.MODIFIED', (ev, arg) => {
			this.file.modified = arg.modified;
			this.setTitle();
		});
	},
	setFileName(fullPath) {
		this.file.setFileName(fullPath);
		this.setTitle();
	},
	setNewName() {
		this.file.setDefaultName();
		this.setTitle();
	},
	setFileSaved() {
		this.file.modified = false;
		this.setTitle();
	},
	setTitle() {
		this.mainWindow.setTitle(`${this.file.name} - A2v10 Bpmn Designer${this.file.modified ? ' *' : ''}`)
	},
	async getContent() {
		return await this.mainWindow.webContents.executeJavaScript('window.__electronInterop.getCurrentXml();');
	},
	async getThumb() {
		return await this.mainWindow.webContents.executeJavaScript('window.__electronInterop.getCurrentSvg();');
	},
	async clearCommandStack() {
		return await this.mainWindow.webContents.executeJavaScript('window.__electronInterop.clearCommandStack();');
	}
};

module.exports = document;

