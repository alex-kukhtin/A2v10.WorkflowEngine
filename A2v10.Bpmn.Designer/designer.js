'use strict';

const TITLE_SUFFIX = ' - A2v10 Bpmn Designer';

const { ipcRenderer } = require('electron');

const bpmnModeler = window.Modeler;

async function start() {
	let wf = await fetch('./workflows/default.bpmn');
	bpmnModeler.importXML(await wf.text());
}

start();

ipcRenderer.on("FILE.OPEN", async (event, arg) => {
	// check if file has changed
	let cs = bpmnModeler.get("commandStack");
	if (cs.canUndo()) {
		let content = await bpmnModeler.saveXML();
		let res = ipcRenderer.sendSync('R.FILE.CHECKSAVE', { data: content });
		console.dir(res);
		if (res === 'cancel')
			return;
	}
	let r = ipcRenderer.sendSync('R.FILE.OPEN');
	if (!r)
		return;
	bpmnModeler.importXML(r.data);
	document.title = r.name + TITLE_SUFFIX;
});

ipcRenderer.on("FILE.TEST", (event, arg) => {
	console.dir(arg);
	arg.x += 5;
	arg.y += 10;
});


