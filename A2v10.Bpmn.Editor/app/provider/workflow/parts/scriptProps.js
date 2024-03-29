﻿
import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import extensionElementsImpl from './impl/extensionElements';
import { isAny } from 'bpmn-js/lib/features/modeling/util/ModelingUtil';

import entryFactory from '../../lib/factory/EntryFactory';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';


export default function scriptProps(group, element, bpmnFactory, translate) {
	if (is(element, 'bpmn:ScriptTask')) {
		let textBox = entryFactory.scriptBox(translate, {
			id: 'script',
			label: translate('Script'),
			modelProperty: 'script',
			isScript: true,
			element,
			minLines: 10,
			maxLines: 10,
			get(elem, node) {
				let bo = getBusinessObject(elem);
				let res = {};
				res.script = bo.get('script');
				return res;
			},
			set(elem, values, node) {
				let props = {};
				if (values)
					props.script = values.script;
				return cmdHelper.updateProperties(elem, values);
			}
		});
		group.entries.push(textBox);
	}
	else if (isAny(element, ["bpmn:UserTask", "bpmn:StartEvent", "bpmn:EndEvent", "bpmn:CallActivity"])) {
		let textBox = entryFactory.scriptBox(translate, {
			id: 'script',
			label: translate('Script'),
			description: translate('Executed after completion'),
			isScript: true,
			modelProperty: 'text',
			element,
			get(elem, node) {
				let ee = extensionElementsImpl.getExtensionElement(elem, "wf:Script");
				return ee ? {text: ee.text} : {};
			},
			set(elem, values, node) {
				let ee = extensionElementsImpl.getOrCreateExtensionElement(elem, 'wf:Script', bpmnFactory);
				ee.commands.push(cmdHelper.updateBusinessObject(elem, ee.elem, { text: values.text }));
				return ee.commands;
			}
		});
		group.entries.push(textBox);
	}
};
