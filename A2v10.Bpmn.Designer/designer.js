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
		let res = ipcRenderer.sendSync('R.FILE.CHECKSAVE', { data: content.xml });
		if (res.cancel)
			return;
	}
	let r = ipcRenderer.sendSync('R.FILE.OPEN');
	if (r.cancel)
		return;
	bpmnModeler.importXML(r.data);
	document.title = r.name + TITLE_SUFFIX;
});

ipcRenderer.on("FILE.SAVE", async (event, arg) => {
	let content = await bpmnModeler.saveXML();
	let r = ipcRenderer.sendSync('R.FILE.SAVE', { data: content.xml, saveAs: arg?.saveAs || false });
	if (r.cancel)
		return;
	document.title = r.name + TITLE_SUFFIX;
	let cs = bpmnModeler.get("commandStack");
	cs.clear();
});



