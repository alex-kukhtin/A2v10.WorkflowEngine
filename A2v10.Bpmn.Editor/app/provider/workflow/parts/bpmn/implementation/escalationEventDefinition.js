﻿'use strict';

import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';
import entryFactory from '../../../../lib/factory/entryFactory';

import eventDefinitionReference from './eventDefinitionReference';
import elementReferenceProperty from './elementReferenceProperty';


module.exports = function (group, element, bpmnFactory, escalationEventDefinition, showEscalationCodeVariable, translate) {

	group.entries = group.entries.concat(eventDefinitionReference(element, escalationEventDefinition, bpmnFactory, {
		label: translate('Escalation'),
		elementName: 'escalation',
		elementType: 'bpmn:Escalation',
		referenceProperty: 'escalationRef',
		newElementIdPrefix: 'Escalation_'
	}));


	group.entries = group.entries.concat(
		elementReferenceProperty(element, escalationEventDefinition, bpmnFactory, translate, {
			id: 'escalation-element-name',
			label: translate('Escalation Name'),
			referenceProperty: 'escalationRef',
			modelProperty: 'name',
			shouldValidate: true
		})
	);


	group.entries = group.entries.concat(
		elementReferenceProperty(element, escalationEventDefinition, bpmnFactory, translate, {
			id: 'escalation-element-code',
			label: translate('Escalation Code'),
			referenceProperty: 'escalationRef',
			modelProperty: 'escalationCode'
		})
	);


	if (showEscalationCodeVariable) {
		group.entries.push(entryFactory.textField(translate, {
			id: 'escalationCodeVariable',
			label: translate('Escalation Code Variable'),
			modelProperty: 'escalationCodeVariable',

			get: function (element) {
				var codeVariable = escalationEventDefinition.get('wf:escalationCodeVariable');
				return {
					escalationCodeVariable: codeVariable
				};
			},

			set: function (element, values) {
				return cmdHelper.updateBusinessObject(element, escalationEventDefinition, {
					'wf:escalationCodeVariable': values.escalationCodeVariable || undefined
				});
			}
		}));
	}
};
