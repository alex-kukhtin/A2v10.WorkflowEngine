'use strict';

import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import { isAny } from 'bpmn-js/lib/features/modeling/util/ModelingUtil';

import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';
import elementHelper from 'bpmn-js-properties-panel/lib/helper/ElementHelper';


import entryFactory from '../../../lib/factory/entryFactory';

// todo: loopMaximum, multiInstance in another file

/**
	<StandardLoopCharacteristics testBefore = "false", loopMaximum="5">
		<bpmn:loopCondition  xsi:type="bpmn:tFormalExpression" />
	<MultiInstanceLoopCharacteristics isSequential="true" wf:collection="", wf:elementVariable="">
		<bpmn:loopCardinality xsi:type="bpmn:tFormalExpression"/>
		<bpmn:completionCondition xsi:type="bpmn:tFormalExpression"/>
*/

function getLoopDefinition(elem) {
	let bo = getBusinessObject(elem);
	if (!bo) return null;
	let lc = bo.get('loopCharacteristics');
	if (!lc) return null;
	if (!is(lc, "bpmn:StandardLoopCharacteristics")) return null;
	return lc;
}

function getLoopCondition(elem) {
	let bo = getLoopDefinition(elem);
	return bo ? bo.get('loopCondition') : null;
}

const LOOP_SOURCES = [
	'bpmn:Task',
	'bpmn:UserTask',
	'bpmn:ScriptTask',
	'bpmn:ServiceTask',
	'bpmn:CallActivity',
	'bpmn:SubProcess'
];

module.exports = function (group, element, bpmnFactory, translate) {
	if (!isAny(element, LOOP_SOURCES))
		return;

	let loopDef = getLoopDefinition(element);
	if (!loopDef) return;

	let textBox = entryFactory.textBox(translate, {
		id: 'loop-condition',
		label: translate('Loop Condition'),
		modelProperty: 'body',
		isScript: true,
		get(elem, node) {
			let loopCond = getLoopCondition(elem);
			return loopCond ? { body: loopCond.body } : {};
		},
		set(elem, values, node) {
			let commands = [];

			let loopProps = {
				body: values.body
			};

			let loopCond = getLoopCondition(elem);
			if (!loopCond) {
				let loopDef = getLoopDefinition(elem)
				let expr = elementHelper.createElement(
					'bpmn:FormalExpression',
					loopProps,
					loopDef,
					bpmnFactory
				);
				commands.push(cmdHelper.updateBusinessObject(element, loopDef, { loopCondition: expr }));
			}
			else
				commands.push(cmdHelper.updateBusinessObject(element, loopCond, { body: values.body }));
			return commands;
		}
	});

	group.entries.push(textBox);

	group.entries.push(entryFactory.checkbox(translate, {
		id: 'test-before',
		label: 'Test Before',
		modelProperty: 'testBefore',
		get(elem, node) {
			let loopDef = getLoopDefinition(elem);
			return loopDef ? { testBefore: loopDef.testBefore } : {};
		},
		set(elem, values, node) {
			let loopDef = getLoopDefinition(elem);
			if (!loopDef) return [];
			let vals = {
				testBefore: values && values.testBefore ? true : false
			};
			return cmdHelper.updateBusinessObject(elem, loopDef, vals);
		}
	}));

	let loopMaxEntry = entryFactory.validationAwareTextField(translate, {
		id: 'loop-maximum',
		label: 'Loop Maximum',
		modelProperty: 'loopMaximum'
	});
	loopMaxEntry.get = function(elem, node) {
		let loopDef = getLoopDefinition(elem);
		return loopDef ? { loopMaximum: loopDef.loopMaximum || "" } : {};
	};
	loopMaxEntry.set = function(elem, values, node) {
		let loopDef = getLoopDefinition(elem);
		if (!loopDef) return [];
		let fixVal = parseFloat(values.loopMaximum);
		if (isNaN(fixVal))
			fixVal = 0;
		let vals = {
			loopMaximum: fixVal ? fixVal.toFixed(0) : ''
		};
		return cmdHelper.updateBusinessObject(elem, loopDef, vals);
	};
	loopMaxEntry.validate = function(elem, values) {
		let lm = values.loopMaximum;
		if (lm == '') return {};
		let f = parseFloat(lm);
		if (!isNaN(f)) return {};
		return {
			loopMaximum: 'The value must be a number'
		};
	};
	group.entries.push(loopMaxEntry);
};


