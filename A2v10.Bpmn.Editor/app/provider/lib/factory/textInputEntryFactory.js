﻿'use strict';


import { domify, query } from 'min-dom';
import { escapeHTML } from 'bpmn-js-properties-panel/lib/Utils';

import entryFieldDescription from 'bpmn-js-properties-panel/lib/factory//EntryFieldDescription';


var textField = function (translate, options, defaultParameters) {

	// Default action for the button next to the input-field
	var defaultButtonAction = function (element, inputNode) {
		var input = query('input[name="' + options.modelProperty + '"]', inputNode);
		input.value = '';

		return true;
	};

	// default method to determine if the button should be visible
	var defaultButtonShow = function (element, inputNode) {
		var input = query('input[name="' + options.modelProperty + '"]', inputNode);

		return input.value !== '';
	};


	var resource = defaultParameters,
		label = options.label || resource.id,
		dataValueLabel = options.dataValueLabel,
		buttonLabel = (options.buttonLabel || 'X'),
		actionName = (typeof options.buttonAction != 'undefined') ? options.buttonAction.name : 'clear',
		actionMethod = (typeof options.buttonAction != 'undefined') ? options.buttonAction.method : defaultButtonAction,
		showName = (typeof options.buttonShow != 'undefined') ? options.buttonShow.name : 'canClear',
		showMethod = (typeof options.buttonShow != 'undefined') ? options.buttonShow.method : defaultButtonShow,
		canBeDisabled = !!options.disabled && typeof options.disabled === 'function',
		canBeHidden = !!options.hidden && typeof options.hidden === 'function',
		description = options.description;

	resource.html =
		domify('<label for="wf-' + escapeHTML(resource.id) + '" ' +
			(canBeDisabled ? 'data-disable="isDisabled" ' : '') +
			(canBeHidden ? 'data-show="isHidden" ' : '') +
			(dataValueLabel ? 'data-value="' + escapeHTML(dataValueLabel) + '"' : '') + '>' + escapeHTML(label) + '</label>' +
			'<div class="bpp-field-wrapper" ' +
			(canBeDisabled ? 'data-disable="isDisabled"' : '') +
			(canBeHidden ? 'data-show="isHidden"' : '') +
			'>' +
			'<input id="wf-' + escapeHTML(resource.id) + '" type="text" name="' + escapeHTML(options.modelProperty) + '" ' +
			(canBeDisabled ? 'data-disable="isDisabled"' : '') +
			(canBeHidden ? 'data-show="isHidden"' : '') +
			' />' +
			'<button class="action-button ' + escapeHTML(actionName) + '" data-action="' + escapeHTML(actionName) + '" data-show="' + escapeHTML(showName) + '" ' +
			(canBeDisabled ? 'data-disable="isDisabled"' : '') +
			(canBeHidden ? ' data-show="isHidden"' : '') + '>' +
			'<span>' + escapeHTML(buttonLabel) + '</span>' +
			'</button>' +
			'</div>');

	// add description below text input entry field
	if (description) {
		resource.html.appendChild(entryFieldDescription(translate, description, { show: canBeHidden && 'isHidden' }));
	}

	resource[actionName] = actionMethod;
	resource[showName] = showMethod;

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

	resource.cssClasses = ['bpp-textfield'];

	return resource;
};

module.exports = textField;
