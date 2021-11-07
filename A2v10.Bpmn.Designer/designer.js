// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

'use strict';

const { ipcRenderer } = require('electron');

const builder = window.BpmnModeler;

const bpmnModeler = builder.create('canvas', 'js-properties-panel');
const commandStack = bpmnModeler.get("commandStack");

function start() {
	bpmnModeler.importXML(builder.defaultXml);
	bpmnModeler.on('commandStack.changed', (ev) => {
		let modified = commandStack.canUndo();
		ipcRenderer.send('FILE.MODIFIED', { modified });
	});
}

start();

window.__electronInterop = {
	async getCurrentXml() {
		return bpmnModeler.saveXML();
	},
	async getCurrentSvg() {
		return bpmnModeler.saveSVG();
	},
	async clearCommandStack() {
		return commandStack.clear();
	}
};

ipcRenderer.on("FILE.SETCONTENT", (ev, arg) => {
	bpmnModeler.importXML(arg.content);
});

ipcRenderer.on("FILE.SETNEW", (ev, arg) => {
	bpmnModeler.importXML(builder.defaultXml);
});
