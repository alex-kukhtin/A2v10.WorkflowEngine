
import elementHelper from 'bpmn-js-properties-panel/lib/helper/ElementHelper';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';

import extensionElementsImpl from './impl/extensionElements';
import { isAny } from 'bpmn-js/lib/features/modeling/util/ModelingUtil';

const variablesHtml = `
<div class="bpp-row bpp-element-list">
	<label for=wf-variables-list>Activity variables</label>
	<div class=bpp-field-wrapper>
		<select size="5" class="bpp-variables-list" name="wf-variables-list"
			data-list-entry-container data-on-change=selectElement>
		</select>
		<button class="action-button add" data-action="createElement"><span>+</span></button>
		<button class="action-button clear" 
				data-action="removeElement" data-disable="disableRemove">
			<span>x</span>
		</button>
	</div>
</div>
`;

export default function addVariables(group, element, bpmnFactory, translate) {

	if (!isAny(element, ['bpmn:Process', 'bpmn:SubProcess', 'bpmn:Collaboration','bpmn:Participant']))
		return;
	group.entries.push({
		id: 'Variables',
		html: variablesHtml,
		get(elem, node) {
			let ee = extensionElementsImpl.getExtensionElement(elem, 'wf:Variables');
			return ee ? ee.values : undefined;
		},
		set(elem, values, node) {
			let action = this.__action;
			delete this.__action;
			if (!action) return;
			let varel = extensionElementsImpl.getOrCreateExtensionElement(elem, 'wf:Variables', bpmnFactory);
			let commands = varel.commands;
			if (action.id === 'add-variable') {
				commands.push(cmdHelper.addElementsTolist(elem, varel.elem, 'values', [action.value]));
			} else if (action.id === 'remove-variable') {
				commands.push(cmdHelper.removeElementsFromList(elem, varel.elem, 'values', null, [action.value]));
			}
			return commands;
		},
		createElement(elem, node) {
			let selbox = node.querySelector('select[name=wf-variables-list]');

			let vars = extensionElementsImpl.getExtensionElement(elem, 'wf:Variables');
			var variable = elementHelper.createElement('wf:Variable', { Name: '', Type: 'String', Dir: 'Local', External: false, Value:'' }, vars, bpmnFactory);
			var optTemplate = this.createListEntryTemplate(variable);
			let tmpsel = document.createElement('select');
			tmpsel.innerHTML = optTemplate;
			selbox.appendChild(tmpsel.firstChild);
			selbox.lastChild.selected = 'selected';
			this.__action = {
				id: 'add-variable',
				value: variable
			};
			return true;
		},
		removeElement(elem, node) {
			let selbox = node.querySelector('select[name=wf-variables-list]');
			let ix = selbox.selectedIndex;
			if (ix === -1)
				return;
			selbox.removeChild(selbox[ix]);
			let vars = extensionElementsImpl.getExtensionElement(elem, 'wf:Variables');
			this.__action = {
				id: 'remove-variable',
				value: vars.values[ix]
			}
			return true;
		},
		disableRemove(elem, entryNode, node, scope) {
			let sel = extensionElementsImpl.getSelectedVariable(node);
			return !sel || sel.idx === -1;
		},
		selectElement(elem, node, event, scope) {
		},
		setControlValue(elem, node, option, property, value, idx) {
			let ee = extensionElementsImpl.getExtensionElement(elem, 'wf:Variables');
			if (!ee || idx >= ee.values.length) return;
			let varb = ee.values[idx];
			let tmlText = this.createTemplateText(varb);
			node.text = tmlText;
		},
		createTemplateText(value) {
			let name = value.Name || '<unnamed>'
			return `${name}: ${value.Type}  [${value.Dir}]`;
		},
		createListEntryTemplate(value) {
			/*'data-value' is required
			 *'data-name' is parameter 'property' for 'setControlValue'.
			 */
			return `<option value="" data-value data-name="variableText">${this.createTemplateText(value)}</option>`;
		}
	});
}
