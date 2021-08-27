
import entryFactory from '../../lib/factory/entryFactory';

import { validateId } from 'bpmn-js-properties-panel/lib/Utils';
import { is } from 'bpmn-js/lib/util/ModelUtil';
import { isAny } from 'bpmn-js/lib/features/modeling/util/ModelingUtil';

import extensionElementsImpl from './impl/extensionElements';
import cmdHelper from 'bpmn-js-properties-panel/lib/helper/CmdHelper';

const VAR_TYPE_OPTIONS = [
	{ name: 'String', value: 'String' },
	{ name: 'Number', value: 'Number' },
	{ name: 'Boolean', value: 'Boolean' },
	{ name: 'Object', value: 'Object' },
	{ name: 'BigInt', value: 'BigInt' },
	{ name: 'Guid',   value: 'Guid' }
];

const VAR_DIR_OPTIONS = [
	{ name: 'In', value: 'In' },
	{ name: 'Out', value: 'Out' },
	{ name: 'InOut', value: 'InOut' },
	{ name: 'Local', value: 'Local' }
];

export default function VariablesDetailProps(group, element, translate) {

	function setValue(prop, elem, values, node) {
		let vars = extensionElementsImpl.getSelectedVariableObject(node, elem);
		if (!vars) return;
		let update = {}
		update[prop] = values[prop];
		return cmdHelper.updateBusinessObject(elem, vars, update);
	}

	let nameProp = entryFactory.validationAwareTextField(translate, {
		id: 'var_name',
		label: 'Name',
		modelProperty: 'Name'
	});
	nameProp.get = (elem, node) => {
		return extensionElementsImpl.getSelectedVariableObject(node, elem) || {};
	};
	nameProp.set = (elem, values, node) => {
		return setValue('Name', elem, values, node);
	};
	nameProp.validate = (elem, values, node) => {
		let nameValue = values.Name || '';
		var idErr = validateId(nameValue, translate);
		return idErr ? { Name: idErr } : {};
	};

	group.entries.push(nameProp);

	group.entries.push(entryFactory.selectBox(translate, {
		id: 'var_type',
		label: 'Type',
		selectOptions: VAR_TYPE_OPTIONS,
		modelProperty: 'Type',
		get(elem, node) {
			return extensionElementsImpl.getSelectedVariableObject(node, elem) || {};
		},
		set(elem, values, node) {
			return setValue('Type', elem, values, node);
		}
	}));

	group.entries.push(entryFactory.selectBox(translate, {
		id: 'var_dir',
		label: 'Direction',
		selectOptions: VAR_DIR_OPTIONS,
		modelProperty: 'Dir',
		get(elem, node) {
			return extensionElementsImpl.getSelectedVariableObject(node, elem) || {};
		},
		set(elem, values, node) {
			return setValue('Dir', elem, values, node);
		}
	}));


	let valueProp = entryFactory.validationAwareTextField(translate, {
		id: 'var_value',
		label: 'Value',
		modelProperty: 'Value'
	});
	valueProp.get = (elem, node) => {
		return extensionElementsImpl.getSelectedVariableObject(node, elem) || {};
	};
	valueProp.set = (elem, values, node) => {
		return setValue('Value', elem, values, node);
	};
	valueProp.validate = (elem, values, node) => {
		return {};
	};

	group.entries.push(valueProp);

	if (isAny(element, ['bpmn:Process', 'bpmn:Collaboration','bpmn:Participant'])) {
		group.entries.push(entryFactory.checkbox(translate, {
			id: 'var_external',
			label: 'External',
			modelProperty: 'External',
			get(elem, node) {
				return extensionElementsImpl.getSelectedVariableObject(node, elem) || {};
			},
			set(elem, values, node) {
				return setValue('External', elem, values, node);
			}
		}));
	}
}