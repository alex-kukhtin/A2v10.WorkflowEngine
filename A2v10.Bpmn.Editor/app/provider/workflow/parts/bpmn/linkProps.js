'use strict';

import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';
import entryFactory from '../../../lib/factory/entryFactory';

import forEach from 'lodash/forEach';

function getLinkEventDefinition(element) {

	var bo = getBusinessObject(element);

	var linkEventDefinition = null;
	if (bo.eventDefinitions) {
		forEach(bo.eventDefinitions, function (eventDefinition) {
			if (is(eventDefinition, 'bpmn:LinkEventDefinition')) {
				linkEventDefinition = eventDefinition;
			}
		});
	}

	return linkEventDefinition;
}

module.exports = function (group, element, translate) {
	var linkEvents = ['bpmn:IntermediateThrowEvent', 'bpmn:IntermediateCatchEvent'];

	forEach(linkEvents, function (event) {
		if (is(element, event)) {

			var linkEventDefinition = getLinkEventDefinition(element);

			if (linkEventDefinition) {
				var entry = entryFactory.textField(translate, {
					id: 'link-event',
					label: translate('Link Name'),
					modelProperty: 'link-name'
				});

				entry.get = function () {
					return { 'link-name': linkEventDefinition.get('name') };
				};

				entry.set = function (element, values) {
					var newProperties = {
						name: values['link-name']
					};
					return cmdHelper.updateBusinessObject(element, linkEventDefinition, newProperties);
				};

				group.entries.push(entry);
			}
		}
	});
};

