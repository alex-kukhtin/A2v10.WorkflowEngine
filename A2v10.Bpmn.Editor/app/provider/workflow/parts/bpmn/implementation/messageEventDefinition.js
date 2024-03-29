﻿'use strict';

import eventDefinitionReference from './eventDefinitionReference';
import elementReferenceProperty from './elementReferenceProperty';


module.exports = function (group, element, bpmnFactory, messageEventDefinition, translate) {

	group.entries = group.entries.concat(eventDefinitionReference(element, messageEventDefinition, bpmnFactory, {
		label: translate('Message'),
		elementName: 'message',
		elementType: 'bpmn:Message',
		referenceProperty: 'messageRef',
		newElementIdPrefix: 'Message_'
	}));


	group.entries = group.entries.concat(
		elementReferenceProperty(element, messageEventDefinition, bpmnFactory, translate, {
			id: 'message-element-name',
			label: translate('Message Name'),
			referenceProperty: 'messageRef',
			modelProperty: 'name',
			shouldValidate: true
		})
	);

};
