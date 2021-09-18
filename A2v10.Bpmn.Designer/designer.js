// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

'use strict';

const { ipcRenderer } = require('electron');

const bpmnModeler = window.Modeler;
const commandStack = bpmnModeler.get("commandStack");

async function start() {
	let wf = await fetch('./workflows/default.bpmn');
	bpmnModeler.importXML(await wf.text());
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
	async clearCommandStack() {
		return commandStack.clear();
	}
};

ipcRenderer.on("FILE.SETCONTENT", (ev, arg) => {
	bpmnModeler.importXML(arg.content);
});

