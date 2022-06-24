

import extensionElementsImpl from './impl/extensionElements';
import { isAny } from 'bpmn-js/lib/features/modeling/util/ModelingUtil';

import entryFactory from '../../lib/factory/EntryFactory';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';


export default function inboxProps(group, element, bpmnFactory, translate) {
	if (!isAny(element, ["bpmn:UserTask"]))
		return;
	let inboxText = entryFactory.textBox(translate, {
		id: 'inbox',
		label: translate('Inbox'),
		isScript: true,
		modelProperty: 'text',
		get(elem, node) {
			let ee = extensionElementsImpl.getExtensionElement(elem, "wf:Inbox");
			return ee ? { text: ee.text } : {};
		},
		set(elem, values, node) {
			let ee = extensionElementsImpl.getOrCreateExtensionElement(elem, 'wf:Inbox', bpmnFactory);
			ee.commands.push(cmdHelper.updateBusinessObject(elem, ee.elem, { text: values.text }));
			return ee.commands;
		}
	});

	let bookmarkBox = entryFactory.textField(translate, {
		id: 'bookmark',
		label: translate('Bookmark'),
		modelProperty: 'bookmark'
	});

	group.entries.push(bookmarkBox);
	group.entries.push(inboxText);
};
