
import BpmnModeler from 'bpmn-js/lib/Modeler';

import propertiesPanelModule from 'bpmn-js-properties-panel';
import propertiesProviderModule from './provider/workflow';

import workflowModdleDescriptor from './descriptors/workflow';

const xml = `<?xml version="1.0" encoding="UTF-8"?>
<definitions xmlns="http://www.omg.org/spec/BPMN/20100524/MODEL" 
		xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" 
		xmlns:omgdi="http://www.omg.org/spec/DD/20100524/DI" 
		xmlns:omgdc="http://www.omg.org/spec/DD/20100524/DC" 
		xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
		xmlns:wf="clr-namespace:A2v10.Workflow;assembly=A2v10.Workflow" id="sid-38422fae-e03e-43a3-bef4-bd33b32041b2" targetNamespace="http://bpmn.io/bpmn" exporter="bpmn-js (https://demo.bpmn.io)" exporterVersion="7.5.0">
	<process id="Process_1" isExecutable="true">
		<extensionElements>
			<wf:Variables />
		</extensionElements>
		<startEvent id="StartEvent_1y45yut" name="start">
			<outgoing>SequenceFlow_0h21x7r</outgoing>
		</startEvent>
		<task id="Activity_1q4vvhs">
			<incoming>SequenceFlow_0h21x7r</incoming>
			<outgoing>Flow_1mtt2co</outgoing>
		</task>
		<endEvent id="EndEvent_0sk8619">
			<incoming>Flow_1mtt2co</incoming>
		</endEvent>
		<sequenceFlow id="SequenceFlow_0h21x7r" sourceRef="StartEvent_1y45yut" targetRef="Activity_1q4vvhs" />
		<sequenceFlow id="Flow_1mtt2co" sourceRef="Activity_1q4vvhs" targetRef="EndEvent_0sk8619" />
	</process>
	<bpmndi:BPMNDiagram id="BpmnDiagram_1">
		<bpmndi:BPMNPlane id="BpmnPlane_1" bpmnElement="Process_1">
			<bpmndi:BPMNEdge id="SequenceFlow_0h21x7r_di" bpmnElement="SequenceFlow_0h21x7r">
				<omgdi:waypoint x="188" y="210" />
				<omgdi:waypoint x="290" y="210" />
			</bpmndi:BPMNEdge>
			<bpmndi:BPMNEdge id="Flow_1mtt2co_di" bpmnElement="Flow_1mtt2co">
				<omgdi:waypoint x="390" y="210" />
				<omgdi:waypoint x="482" y="210" />
			</bpmndi:BPMNEdge>
			<bpmndi:BPMNShape id="StartEvent_1y45yut_di" bpmnElement="StartEvent_1y45yut">
				<omgdc:Bounds x="152" y="192" width="36" height="36" />
				<bpmndi:BPMNLabel>
					<omgdc:Bounds x="159" y="235" width="23" height="14" />
				</bpmndi:BPMNLabel>
			</bpmndi:BPMNShape>
			<bpmndi:BPMNShape id="Event_0sk8619_di" bpmnElement="EndEvent_0sk8619">
				<omgdc:Bounds x="482" y="192" width="36" height="36" />
			</bpmndi:BPMNShape>
			<bpmndi:BPMNShape id="Activity_1q4vvhs_di" bpmnElement="Activity_1q4vvhs">
				<omgdc:Bounds x="290" y="170" width="100" height="80" />
			</bpmndi:BPMNShape>
		</bpmndi:BPMNPlane>
	</bpmndi:BPMNDiagram>
</definitions>
`;

window.BpmnModeler = {
	create: function (canvas, propPanel) {
		if (typeof (canvas) === 'string')
			canvas = document.getElementById(canvas);
		if (typeof (propPanel) === 'string')
			propPanel = document.getElementById(propPanel);
		return new BpmnModeler({
			container: canvas,
			keyboard: {
				bindTo: document.getRootNode()
			},
			propertiesPanel: {
				parent: propPanel
			},
			additionalModules: [
				propertiesPanelModule,
				propertiesProviderModule
			],
			moddleExtensions: {
				Workflow: workflowModdleDescriptor,
				workflow: workflowModdleDescriptor,
				wf: workflowModdleDescriptor
			}
		});

	},
	createSimple: function (canvas) {
		if (typeof (canvas) === 'string')
			canvas = document.getElementById(canvas);
		return new BpmnModeler({
			container: canvas,
			keyboard: {
				bindTo: document.getRootNode()
			},
			moddleExtensions: {
				workflow: workflowModdleDescriptor
			}
		});
	},
	defaultXml: xml
};
