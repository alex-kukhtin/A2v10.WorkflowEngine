
import { is } from 'bpmn-js/lib/util/ModelUtil';
import extensionElementsImpl from './impl/extensionElements';

import entryFactory from '../../lib/factory/EntryFactory';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';


export default function globalScriptProps(group, element, bpmnFactory, translate) {
	if (is(element, "bpmn:Collaboration") || is(element, "bpmn:Process")) {
		let textBox = entryFactory.textBox(translate, {
			id: 'script',
			label: translate('Global Script'),
			modelProperty: 'text',
			isScript: true,
			get(elem, node) {
				let ee = extensionElementsImpl.getExtensionElement(elem, "wf:GlobalScript");
				return ee ? { text: ee.text } : {};
			},
			set(elem, values, node) {
				let ee = extensionElementsImpl.getOrCreateExtensionElement(elem, 'wf:GlobalScript', bpmnFactory);
				ee.commands.push(cmdHelper.updateBusinessObject(elem, ee.elem, { text: values.text }));
				return ee.commands;
			}
		});
		group.entries.push(textBox);
	}
};
