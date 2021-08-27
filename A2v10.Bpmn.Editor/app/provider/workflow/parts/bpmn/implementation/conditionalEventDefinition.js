'use strict';

import entryFactory from '../../../../lib/factory/entryFactory';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';

import { is } from 'bpmn-js/lib/util/ModelUtil';

import { isEventSubProcess } from 'bpmn-js/lib/util/DiUtil';

module.exports = function (group, element, bpmnFactory, conditionalEventDefinition, elementRegistry, translate) {

	var getValue = function (modelProperty) {
		return function (element) {
			var modelPropertyValue = conditionalEventDefinition.get('wf:' + modelProperty);
			var value = {};

			value[modelProperty] = modelPropertyValue;
			return value;
		};
	};

	var setValue = function (modelProperty) {
		return function (element, values) {
			var props = {};

			props['wf:' + modelProperty] = values[modelProperty] || undefined;

			return cmdHelper.updateBusinessObject(element, conditionalEventDefinition, props);
		};
	};

	group.entries.push(entryFactory.textField(translate, {
		id: 'variableName',
		label: translate('Variable Name'),
		modelProperty: 'variableName',

		get: getValue('variableName'),
		set: setValue('variableName')
	}));

	var isConditionalStartEvent =
		is(element, 'bpmn:StartEvent') && !isEventSubProcess(element.parent);

	if (!isConditionalStartEvent) {
		group.entries.push(entryFactory.textField(translate, {
			id: 'variableEvents',
			label: translate('Variable Events'),
			description: translate('Specify more than one variable change event as a comma separated list.'),
			modelProperty: 'variableEvents',

			get: getValue('variableEvents'),
			set: setValue('variableEvents')
		}));
	}
};
