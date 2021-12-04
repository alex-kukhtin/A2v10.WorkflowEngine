'use strict';

import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';

import entryFactory from '../../lib/factory/EntryFactory';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';

import extensionElementsImpl from './impl/extensionElements';


module.exports = function (group, element, bpmnFactory, translate) {

	if (!is(element, 'bpmn:CallActivity'))
		return;
	let bo = getBusinessObject(element);
	if (!bo)
		return;

	var ceEntry = entryFactory.validationAwareTextField(translate, {
		id: 'called-element',
		label: translate('Called Element'),
		modelProperty: 'calledElement'
	});
	ceEntry.get = function (element) {
		return { 'calledElement': bo.get('calledElement') };
	};
	ceEntry.set = function (element, values) {
		return cmdHelper.updateBusinessObject(element, bo, values);
	}
	ceEntry.validate = function (element, values) {
		return [];
	};
	group.entries.push(ceEntry);

	var prmEntry = entryFactory.textBox(translate, {
		id: 'parameters',
		label: translate('Parameters'),
		isScript: true,
		modelProperty: 'text',
		get(elem, node) {
			let ee = extensionElementsImpl.getExtensionElement(elem, "wf:Parameters");
			return ee ? { text: ee.text } : {};
		},
		set(elem, values, node) {
			let ee = extensionElementsImpl.getOrCreateExtensionElement(elem, 'wf:Parameters', bpmnFactory);
			ee.commands.push(cmdHelper.updateBusinessObject(elem, ee.elem, { text: values.text }));
			return ee.commands;
		}
	});
	group.entries.push(prmEntry);
};
