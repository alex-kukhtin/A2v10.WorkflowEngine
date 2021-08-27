'use strict';

import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import { isAny } from 'bpmn-js/lib/features/modeling/util/ModelingUtil';

import { query } from 'min-dom';
import { escapeHTML } from 'bpmn-js-properties-panel/lib/Utils';

import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';
import elementHelper from 'bpmn-js-properties-panel/lib/helper/ElementHelper';

import eventDefinitionHelper from 'bpmn-js-properties-panel/lib/helper/EventDefinitionHelper';

	//scriptImplementation = require('./implementation/Script');


module.exports = function (group, element, bpmnFactory, translate) {
	let bo = getBusinessObject(element);

	if (!bo) return;

	var conditionalEventDefinition = eventDefinitionHelper.getConditionalEventDefinition(element);

	if (!(is(element, 'bpmn:SequenceFlow') && isConditionalSource(element.source))
		&& !conditionalEventDefinition) {
		return;
	}

	//var script = scriptImplementation('language', 'body', true, translate);
	group.entries.push({
		id: 'condition',
		label: translate('Condition'),
		html:
		'<div class="bpp-row">' +
			'<label for="wf-condition-type">' + escapeHTML(translate('Condition Type')) + '</label>' +
			'<div class="bpp-field-wrapper">' +
				'<select id="wf-condition-type" name="conditionType" data-value>' +
					'<option value="expression">' + escapeHTML(translate('Expression')) + '</option>' +
					'<option value="" selected></option>' +
			'</select>' +
			'</div>' +
		'</div>' +
		// expression
		'<div class="bpp-row">' +
			'<label for="wf-condition" data-show="isExpression">' + escapeHTML(translate('Expression')) + '</label>' +
			'<div class="bpp-field-wrapper" data-show="isExpression">' +
				'<input id="wf-condition" type="text" name="condition" />' +
				'<button class="action-button clear" data-action="clear" data-show="canClear">' +
				'<span>X</span>' +
				'</button>' +
			'</div>' +
		'</div>',

		get: function (element, propertyName) {
			var conditionalEventDefinition = eventDefinitionHelper.getConditionalEventDefinition(element);

			var conditionExpression = conditionalEventDefinition
				? conditionalEventDefinition.condition
				: bo.conditionExpression;

			var values = {},
				conditionType = '';

			if (conditionExpression) {
				conditionType = 'expression';
				values.condition = conditionExpression.get('body');
			}

			values.conditionType = conditionType;

			return values;
		},

		set: function (element, values, node) {

			var commands = [];

			var conditionProps = {
				body: values.condition
			};


			let conditionOrConditionExpression = elementHelper.createElement(
				'bpmn:FormalExpression',
				conditionProps,
				conditionalEventDefinition || bo,
				bpmnFactory
			);

			var source = element.source;

			// if default-flow, remove default-property from source
			if (source && source.businessObject.default === bo) {
				commands.push(cmdHelper.updateProperties(source, { 'default': undefined }));
			}

			var update = conditionalEventDefinition
				? { condition: conditionOrConditionExpression }
				: { conditionExpression: conditionOrConditionExpression };

			commands.push(cmdHelper.updateBusinessObject(element, conditionalEventDefinition || bo, update));

			return commands;
		},

		validate: function (element, values) {
			var validationResult = {};

			if (!values.condition && values.conditionType === 'expression') {
				validationResult.condition = translate('Must provide a value');
			}

			return validationResult;
		},

		isExpression: function (element, inputNode) {
			var conditionType = query('select[name=conditionType]', inputNode);
			if (conditionType.selectedIndex >= 0) {
				return conditionType.options[conditionType.selectedIndex].value === 'expression';
			}
		},

		clear: function (element, inputNode) {

			// clear text input
			query('input[name=condition]', inputNode).value = '';

			return true;
		},

		canClear: function (element, inputNode) {
			var input = query('input[name=condition]', inputNode);

			return input.value !== '';
		},

		cssClasses: ['bpp-textfield']
	});
};


// utilities //////////////////////////

var CONDITIONAL_SOURCES = [
	'bpmn:Activity',
	'bpmn:ExclusiveGateway',
	'bpmn:InclusiveGateway',
	'bpmn:ComplexGateway'
];

function isConditionalSource(element) {
	return isAny(element, CONDITIONAL_SOURCES);
}
