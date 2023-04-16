// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

'use strict';

const { ipcRenderer } = require('electron');

const fs = require('fs');
const pathtool = require('path');

const fileName = new URLSearchParams(window.location.search).get('filename');

/*
var x = fs.readFile('D:/Git/A2v10.WorkflowEngine/A2v10.Workflow.SqlServer.Tests/TestFiles/externalVariables/correlationId.bpmn',
	{ encoding: 'UTF-8' }, (err, data) => {
		debugger;
		alert(data);
	});
*/

const builder = window.BpmnModeler;

const bpmnModeler = builder.create('canvas', 'js-properties-panel');
let pp = bpmnModeler.get("propertiesPanel");
window.PropertiesPanel = pp;
const commandStack = bpmnModeler.get("commandStack");

const TITLE_NAME = ' - A2v10 Bpmn Designer';

function start() {
	function readBpmn(xmlContent) {
		bpmnModeler.importXML(xmlContent);
		bpmnModeler.on('commandStack.changed', (ev) => {
			let modified = commandStack.canUndo();
			ipcRenderer.send('FILE.MODIFIED', { modified });
		});
	}
	if (fileName) {
		document.title = pathtool.basename(fileName) + TITLE_NAME;
		fs.readFile(fileName,
			{ encoding: 'UTF-8' }, (err, data) => {
				readBpmn(data);
			});
	} else {
		document.title = "untitiled.bpmn" + TITLE_NAME;
		readBpmn(builder.defaultXml);
	}
}

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

let scriptEditor = null;

let sc = document.getElementById('script-editor');
sc._open_ = function (text, callback) {
	sc.style.display = 'block';
	sc._callback_ = callback;
	scriptEditor.setValue(text, 1);
	scriptEditor.focus();
};
sc._close_ = function () {
	if (sc._callback_) {
		let txt = scriptEditor.getValue();
		sc._callback_(txt);
		sc._callback_ = null;
	}
	sc.style.display = 'none';
};
document.getElementById('ace-editor-ok').addEventListener('click', () => {
	sc._close_();
});

function aceClose() {
	sc._callback_ = null;
	sc.style.display = 'none';
}

['ace-editor-cancel', 'ace-editor-close'].forEach(x => {
	document.getElementById(x).addEventListener('click', aceClose);
});

let aee = document.getElementById('ace-editor-element');
scriptEditor = ace.edit(aee, {
	useWorker: false,
	fontSize: 16,
	tabSize: 2,
	highlightActiveLine: false,
	theme: 'ace/theme/sqlserver',
	mode: 'ace/mode/javascript'
});

start();

