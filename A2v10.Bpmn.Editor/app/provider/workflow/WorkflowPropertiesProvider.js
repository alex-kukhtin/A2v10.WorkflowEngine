import inherits from 'inherits';

import PropertiesActivator from 'bpmn-js-properties-panel/lib/PropertiesActivator';

import documentationProps from '../workflow/parts/bpmn/documentationProps';
import conditionalProps from '../workflow/parts/bpmn/conditionalProps';
import idProps from '../workflow/parts/bpmn/idProps';
import processProps from '../workflow/parts/bpmn/processProps';
import nameProps from '../workflow/parts/bpmn/nameProps';
import linkProps from '../workflow/parts/bpmn/linkProps';
import eventProps from '../workflow/parts/bpmn/eventProps';

import scriptProps from './parts/scriptProps';
import variablesProps from './parts/variablesProps';
import variablesDetailProps from './parts/variablesDetailProps';
import globalScriptGroup from './parts/globalScriptProps';

import extensionElementsImpl from './parts/impl/extensionElements';

// The general tab contains all bpmn relevant properties.
// The properties are organized in groups.
function createGeneralTabGroups(element, bpmnFactory, canvas, elementRegistry, translate) {

	var generalGroup = {
		id: 'general',
		label: 'General',
		entries: []
	};
	idProps(generalGroup, element, translate);
	nameProps(generalGroup, element, bpmnFactory, canvas, translate);
	processProps(generalGroup, element, translate);

	var detailsGroup = {
		id: 'details',
		label: 'Details',
		entries: []
	};
	linkProps(detailsGroup, element, translate);
	eventProps(detailsGroup, element, bpmnFactory, elementRegistry, translate);
	scriptProps(detailsGroup, element, bpmnFactory, translate);
	conditionalProps(detailsGroup, element, bpmnFactory, translate);

	var documentationGroup = {
		id: 'documentation',
		label: 'Documentation',
		entries: []
	};

	documentationProps(documentationGroup, element, bpmnFactory, translate);

	return [
		generalGroup,
		detailsGroup,
		documentationGroup
	];
}

function createGlobalTabGroup(element, bpmnFactory, translate) {
	let scriptGroup = {
		id: 'script',
		label: 'Scripts',
		entries: []
	};

	globalScriptGroup(scriptGroup, element, bpmnFactory, translate);

	return [
		scriptGroup
	];
}

function createVariablesTabGroups(element, bpmnFactory, translate) {

	let variablesGroup = {
		id: 'vars',
		label: 'Variables',
		entries: []
	};

	let variablesDetailGroup = {
		id: 'variables-detail',
		label: 'Variable details',
		entries: [],
		enabled(elem, node) {
			let sel = extensionElementsImpl.getSelectedVariable(node);
			return sel && sel.idx >= 0;
		}
	};

	variablesProps(variablesGroup, element, bpmnFactory, translate);
	variablesDetailProps(variablesDetailGroup, element, translate);

	return [
		variablesGroup,
		variablesDetailGroup
	];
}

export default function WorkflowPropertiesProvider(
	eventBus, bpmnFactory, canvas,
	elementRegistry, translate) {

	PropertiesActivator.call(this, eventBus);

	this.getTabs = function (element) {

		var generalTab = {
			id: 'general',
			label: 'General',
			groups: createGeneralTabGroups(element, bpmnFactory, canvas, elementRegistry, translate)
		};

		var variablesTab = {
			id: 'variables',
			label: 'Variables',
			groups: createVariablesTabGroups(element, bpmnFactory, translate)
		};


		var globalTab = {
			id: 'global',
			label: translate('Global'),
			groups: createGlobalTabGroup(element, bpmnFactory, translate)
		};

		// Show "general" + "variables" tab
		return [
			generalTab,
			variablesTab,
			globalTab
		];
	};
}

inherits(WorkflowPropertiesProvider, PropertiesActivator);