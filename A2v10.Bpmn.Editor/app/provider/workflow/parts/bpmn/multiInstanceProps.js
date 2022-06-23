'use strict';

import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import { isAny } from 'bpmn-js/lib/features/modeling/util/ModelingUtil';

import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';

import entryFactory from '../../../lib/factory/entryFactory';


/**
	<MultiInstanceLoopCharacteristics isSequential="true" wf:collection="", wf:variable="">
*/

function getLoopCharacteristics(element) {
	var bo = getBusinessObject(element);
	var lc = bo.loopCharacteristics;
	if (is(lc, "bpmn:StandardLoopCharacteristics"))
		return null;
	return lc;
}

function getProperty(element, propertyName) {
	var lc = getLoopCharacteristics(element);
	return lc && lc.get(propertyName);
}

const MI_SOURCES = [
	'bpmn:Task',
	'bpmn:UserTask',
	'bpmn:ScriptTask',
	'bpmn:ServiceTask',
	'bpmn:CallActivity',
	'bpmn:SubProcess'
];

module.exports = function (group, element, bpmnFactory, translate) {
	if (!isAny(element, MI_SOURCES))
		return;

	let loopCh = getLoopCharacteristics(element);
	if (!loopCh) return;

	let collTextBox = entryFactory.validationAwareTextField(translate, {
		id: 'mi-collection',
		label: translate('Collection'),
		modelProperty: 'collection'
	});

	collTextBox.get = function (elem) {
		return {
			collection: getProperty(elem, 'collection')
		};
	};

	collTextBox.set = function (elem, values) {
		var lc = getLoopCharacteristics(elem);
		return cmdHelper.updateBusinessObject(elem, lc, {
			collection: values.collection || undefined
		});
	};

	collTextBox.validate = function (elem, values) {
		return {};
	}

	group.entries.push(collTextBox);

	let varTextBox = entryFactory.validationAwareTextField(translate, {
		id: 'mi-variable',
		label: translate('Variable'),
		modelProperty: 'variable'
	});
	varTextBox.get = function (elem, node) {
		return {
			variable: getProperty(elem, 'variable')
		};
	};
	varTextBox.set = function (elem, values, node) {
		var lc = getLoopCharacteristics(elem);
		return cmdHelper.updateBusinessObject(elem, lc, {
			variable: values.variable || undefined
		});
	};
	varTextBox.validate = function (elem, values) {
		var lc = getLoopCharacteristics(elem);
		return {};
	};

	group.entries.push(varTextBox);
};
