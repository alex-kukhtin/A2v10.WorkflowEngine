

import { getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import elementHelper from 'bpmn-js-properties-panel/lib/helper/ElementHelper';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';

function getSelectedVariable(node) {
	if (!node) return null;
	let tab = node.closest('div.bpp-properties-tab');
	if (!tab) return null;
	let combo = tab.querySelector('select.bpp-variables-list');
	return combo ? { value: combo.value, idx: combo.selectedIndex } : null;
}

function getExtensionElement(elem, type) {
	if (!elem) return null;
	let bo = getBusinessObject(elem);
	if (!bo) return null;
	let ee = bo.get('extensionElements');
	if (!ee || !ee.values) return null;
	for (let v of ee.values) {
		if (v.$type === type) {
			return v;
		}
	}
	return null;
}

function getExtensionElements(elem) {
	if (!elem) return null;
	let bo = getBusinessObject(elem);
	if (!bo) return null;
	return bo.get('extensionElements');
}

function getSelectedVariableObject(node, elem) {
	let sel = getSelectedVariable(node);
	if (!sel || sel.idx === -1)
		return {};
	let vars = getExtensionElement(elem, 'wf:Variables');
	if (!vars || vars.values.length <= sel.idx) return {};
	return vars.values[sel.idx];
}

function getOrCreateExtensionElement(elem, type, bpmnFactory) {
	let commands = [];
	let extelem = getExtensionElement(elem, type);
	if (extelem == null) {
		let ee = getExtensionElements(elem);
		if (ee == null) {
			ee = elementHelper.createElement("bpmn:ExtensionElements", null, elem, bpmnFactory);
			let bo = getBusinessObject(elem);
			commands.push(cmdHelper.updateBusinessObject(elem, bo, { extensionElements: ee }));
		}
		extelem = elementHelper.createElement(type, null, ee, bpmnFactory);
		commands.push(cmdHelper.addElementsTolist(elem, ee, 'values', [extelem]));
	}
	return {
		elem: extelem,
		commands
	};
}

export default {
	getSelectedVariable,
	getSelectedVariableObject,
	getExtensionElement,
	getExtensionElements,
	getOrCreateExtensionElement
};
