'use strict';

import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import { isAny } from 'bpmn-js/lib/features/modeling/util/ModelingUtil';

import eventDefinitionHelper from 'bpmn-js-properties-panel/lib/helper/EventDefinitionHelper';

import forEach from 'lodash/forEach';

import message from './implementation/messageEventDefinition';
import signal from './implementation/signalEventDefinition';
import error from './implementation/errorEventDefinition';
import escalation from './implementation/escalationEventDefinition';
import timer from './implementation/timerEventDefinition';
import compensation from './implementation/compensateEventDefinition';
import condition from './implementation/conditionalEventDefinition';

module.exports = function (group, element, bpmnFactory, elementRegistry, translate) {
	var events = [
		'bpmn:StartEvent',
		'bpmn:EndEvent',
		'bpmn:IntermediateThrowEvent',
		'bpmn:BoundaryEvent',
		'bpmn:IntermediateCatchEvent'
	];

	// Message and Signal Event Definition
	forEach(events, function (event) {
		if (is(element, event)) {

			var messageEventDefinition = eventDefinitionHelper.getMessageEventDefinition(element),
				signalEventDefinition = eventDefinitionHelper.getSignalEventDefinition(element);

			if (messageEventDefinition) {
				message(group, element, bpmnFactory, messageEventDefinition, translate);
			}

			if (signalEventDefinition) {
				signal(group, element, bpmnFactory, signalEventDefinition, translate);
			}

		}
	});

	// Special Case: Receive Task
	if (is(element, 'bpmn:ReceiveTask')) {
		message(group, element, bpmnFactory, getBusinessObject(element), translate);
	}

	// Error Event Definition
	var errorEvents = [
		'bpmn:StartEvent',
		'bpmn:BoundaryEvent',
		'bpmn:EndEvent'
	];

	forEach(errorEvents, function (event) {
		if (is(element, event)) {

			var errorEventDefinition = eventDefinitionHelper.getErrorEventDefinition(element);

			if (errorEventDefinition) {

				error(group, element, bpmnFactory, errorEventDefinition, translate);
			}
		}
	});

	// Escalation Event Definition
	var escalationEvents = [
		'bpmn:StartEvent',
		'bpmn:BoundaryEvent',
		'bpmn:IntermediateThrowEvent',
		'bpmn:EndEvent'
	];

	forEach(escalationEvents, function (event) {
		if (is(element, event)) {

			var showEscalationCodeVariable = is(element, 'bpmn:StartEvent') || is(element, 'bpmn:BoundaryEvent');

			// get business object
			var escalationEventDefinition = eventDefinitionHelper.getEscalationEventDefinition(element);

			if (escalationEventDefinition) {
				escalation(group, element, bpmnFactory, escalationEventDefinition, showEscalationCodeVariable,
					translate);
			}
		}

	});

	// Timer Event Definition
	var timerEvents = [
		'bpmn:StartEvent',
		'bpmn:BoundaryEvent',
		'bpmn:IntermediateCatchEvent'
	];

	forEach(timerEvents, function (event) {
		if (is(element, event)) {

			// get business object
			var timerEventDefinition = eventDefinitionHelper.getTimerEventDefinition(element);

			if (timerEventDefinition) {
				timer(group, element, bpmnFactory, timerEventDefinition, translate);
			}
		}
	});

	// Compensate Event Definition
	var compensationEvents = [
		'bpmn:EndEvent',
		'bpmn:IntermediateThrowEvent'
	];

	forEach(compensationEvents, function (event) {
		if (is(element, event)) {

			// get business object
			var compensateEventDefinition = eventDefinitionHelper.getCompensateEventDefinition(element);

			if (compensateEventDefinition) {
				compensation(group, element, bpmnFactory, compensateEventDefinition, elementRegistry, translate);
			}
		}
	});


	// Conditional Event Definition
	var conditionalEvents = [
		'bpmn:StartEvent',
		'bpmn:BoundaryEvent',
		'bpmn:IntermediateThrowEvent',
		'bpmn:IntermediateCatchEvent'
	];

	if (isAny(element, conditionalEvents)) {

		// get business object
		var conditionalEventDefinition = eventDefinitionHelper.getConditionalEventDefinition(element);

		if (conditionalEventDefinition) {
			condition(group, element, bpmnFactory, conditionalEventDefinition, elementRegistry, translate);
		}
	}

};
