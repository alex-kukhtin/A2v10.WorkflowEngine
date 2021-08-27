'use strict';

import { domify } from 'min-dom';
import { escapeHTML } from 'bpmn-js-properties-panel/lib/Utils';
import entryFieldDescription from 'bpmn-js-properties-panel/lib/factory//EntryFieldDescription';


var textBox = function (translate, options, defaultParameters) {

	var resource = defaultParameters,
		label = options.label || resource.id,
		canBeShown = !!options.show && typeof options.show === 'function',
		description = options.description,
		className = options.isScript ? 'class="js-text" ' : '';

	resource.html =
		domify('<label for="wf-' + escapeHTML(resource.id) + '" ' +
			(canBeShown ? 'data-show="isShown"' : '') +
			'>' + label + '</label>' +
			'<div class="bpp-field-wrapper" ' +
			(canBeShown ? 'data-show="isShown"' : '') +
			'>' +
			'<div contenteditable=true spellcheck=false id="wf-' + escapeHTML(resource.id) + '" ' + className +
			'name="' + escapeHTML(options.modelProperty) + '" />' +
			'</div>');

	// add description below text box entry field
	if (description) {
		resource.html.appendChild(entryFieldDescription(translate, description, { show: canBeShown && 'isShown' }));
	}

	if (canBeShown) {
		resource.isShown = function () {
			return options.show.apply(resource, arguments);
		};
	}

	resource.cssClasses = ['bpp-textbox'];

	return resource;
};

module.exports = textBox;
