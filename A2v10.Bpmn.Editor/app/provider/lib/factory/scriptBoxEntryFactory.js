'use strict';

import { domify } from 'min-dom';
import { escapeHTML } from 'bpmn-js-properties-panel/lib/Utils';
import entryFieldDescription from 'bpmn-js-properties-panel/lib/factory/EntryFieldDescription';

var scriptBox = function (translate, options, defaultParameters) {
	let resource = defaultParameters,
		label = options.label || resource.id,
		canBeShown = !!options.show && typeof options.show === 'function',
		description = options.description;
	let resid = 'wf-' + escapeHTML(resource.id);
	resource.html =
		domify('<label for="wf-' + resid + '" ' +
			(canBeShown ? 'data-show="isShown"' : '') +
			'>' + label + '</label>' +
			'<div class="bpp-field-wrapper ace-wrapper" ' +
			(canBeShown ? 'data-show="isShown"' : '') +
			'>' +
			'<div name=text class=js-script id="' + resid + '">' +
			'</div>' + 
			'<button class="script-edit-button" data-action="editScript"><span>Edit</span></button>');
	// add description below text box entry field
	if (description) {
		resource.html.appendChild(entryFieldDescription(translate, description, { show: canBeShown && 'isShown' }));
	}

	if (canBeShown) {
		resource.isShown = function () {
			return options.show.apply(resource, arguments);
		};
	}

	resource.cssClasses = ['bpp-scriptbox'];
	resource.set = options.set;
	resource.get = options.get;
	let wfScript = resource.html.getElementById(resid);
	resource.editScript = function () {
		let text = this.get(options.element, null);
		let sc = document.getElementById('script-editor');
		if (sc._open_)
			sc._open_(text[options.modelProperty] || '', (edited) => {
				let pp = window.PropertiesPanel
				let values = {};
				values[options.modelProperty] = edited;
				resource.scripteditor.setValue(edited, 1);
				if (typeof resource.set === 'function') {
					pp.applyChanges(resource, values, resource.html);
					pp.updateState(resource, resource.html);
				}
			});
	};
	resource.scripteditor = ace.edit(wfScript, {
		useWorker: false,
		tabSize: 2,
		showGutter: false,
		highlightActiveLine: false,
		theme: 'ace/theme/default',
		mode: 'ace/mode/javascript',
		showPrintMargin:false,
		minLines: options.minLines || 3,
		maxLines: options.maxLines || 5
	});
	let val = options.get(options.element);
	if (val)
		resource.scripteditor.setValue(val[options.modelProperty], 1);
	resource.scripteditor.renderer.setScrollMargin(5, 10, 0, 0);
	resource.scripteditor.on('blur', function () {
		let val = resource.scripteditor.getValue();
		let pp = window.PropertiesPanel
		let values = {};
		values[options.modelProperty] = val;
		if (typeof resource.set === 'function') {
			pp.applyChanges(resource, values, resource.html);
			pp.updateState(resource, resource.html);
		}
	});

	return resource;
};

module.exports = scriptBox;
