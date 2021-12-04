
import BpmnViewer from "bpmn-js/lib/NavigatedViewer";

import modeling from 'bpmn-js/lib/features/modeling';


/*see: https://github.com/bpmn-io/bpmn-js/tree/develop/lib */

window.BpmnViewer = {
	create: function (canvas) {
		if (typeof (canvas) === 'string')
			canvas = document.getElementById(canvas);
		return new BpmnViewer({
			container: canvas,
			keyboard: {
				bindTo: document.getRootNode()
			},
			additionalModules: [
				modeling
			]
		});
	}
};
