
import BpmnModeler from 'bpmn-js/lib/Modeler';
//import BpmnViewer from "bpmn-js/lib/Viewer";

import propertiesPanelModule from 'bpmn-js-properties-panel';
import propertiesProviderModule from './provider/workflow';

import workflowModdleDescriptor from './descriptors/workflow';

let canvas = document.getElementById('canvas');

let bpmnModeler = new BpmnModeler({
	container: canvas,
	keyboard: {
		bindTo: window
	},
	propertiesPanel: {
		parent: '#js-properties-panel'
	},
	additionalModules: [
		propertiesPanelModule,
		propertiesProviderModule
	],
	moddleExtensions: {
		workflow: workflowModdleDescriptor
	}
});

/*
const bpmnViewer = new BpmnViewer(
	{
		container: canvas,
		keyboard: {
			bindTo: window
		}
	}
);
*/

window.Modeler = bpmnModeler;
// window.Viewer = bpmnViewer;
