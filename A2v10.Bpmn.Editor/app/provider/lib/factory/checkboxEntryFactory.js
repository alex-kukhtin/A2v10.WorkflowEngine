'use strict';

import { domify } from 'min-dom';
import { escapeHTML } from 'bpmn-js-properties-panel/lib/Utils';

import { getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';

import entryFieldDescription from 'bpmn-js-properties-panel/lib/factory//EntryFieldDescription';


var checkbox = function (translate, options, defaultParameters) {
	var resource = defaultParameters,
		id = resource.id,
		label = options.label || id,
		canBeDisabled = !!options.disabled && typeof options.disabled === 'function',
		canBeHidden = !!options.hidden && typeof options.hidden === 'function',
		description = options.description;

	resource.html =
		domify('<input id="wf-' + escapeHTML(id) + '" ' +
			'type="checkbox" ' +
			'name="' + escapeHTML(options.modelProperty) + '" ' +
			(canBeDisabled ? 'data-disable="isDisabled"' : '') +
			(canBeHidden ? 'data-show="isHidden"' : '') +
			' />' +
			'<label for="wf-' + escapeHTML(id) + '" ' +
			(canBeDisabled ? 'data-disable="isDisabled"' : '') +
			(canBeHidden ? 'data-show="isHidden"' : '') +
			'>' + escapeHTML(label) + '</label>');

	// add description below checkbox entry field
	if (description) {
		resource.html.appendChild(entryFieldDescription(translate, description, { show: canBeHidden && 'isHidden' }));
	}

	resource.get = function (element) {
		var bo = getBusinessObject(element),
			res = {};

		res[options.modelProperty] = bo.get(options.modelProperty);

		return res;
	};

	resource.set = function (element, values) {
		var res = {};

		res[options.modelProperty] = !!values[options.modelProperty];

		return cmdHelper.updateProperties(element, res);
	};

	if (typeof options.set === 'function') {
		resource.set = options.set;
	}

	if (typeof options.get === 'function') {
		resource.get = options.get;
	}

	if (canBeDisabled) {
		resource.isDisabled = function () {
			return options.disabled.apply(resource, arguments);
		};
	}

	if (canBeHidden) {
		resource.isHidden = function () {
			return !options.hidden.apply(resource, arguments);
		};
	}

	resource.cssClasses = ['bpp-checkbox'];

	return resource;
};

module.exports = checkbox;
