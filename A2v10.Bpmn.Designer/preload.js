
const { contextBridge } = require('electron');

contextBridge.exposeInMainWorld('testApi', {
	settings: () => {

	}
});