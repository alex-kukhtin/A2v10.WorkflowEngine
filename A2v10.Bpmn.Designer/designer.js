'use strict';

const TITLE_SUFFIX = ' - A2v10 Bpmn Designer';

const { ipcRenderer } = require('electron');

const bpmnModeler = window.Modeler;

async function start() {
	let wf = await fetch('./workflows/default.bpmn');
	bpmnModeler.importXML(await wf.text());
}

start();

ipcRenderer.on("FILE.OPEN", (event, arg) => {
	console.dir(arg);
	bpmnModeler.importXML(arg.data);
	document.title = arg.name + TITLE_SUFFIX;
	return '1234';
});

ipcRenderer.on("FILE.TEST", (event, arg) => {
	console.dir(arg);
	arg.x += 5;
	arg.y += 10;
});
