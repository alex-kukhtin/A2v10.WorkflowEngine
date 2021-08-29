
const mainMenu = [
	{
		label: 'File',
		submenu: [
			{
				label: 'Open...',
				accelerator: 'CmdOrCtrl+O',
				click: (mi, bw) => send('FILE.OPEN', bw)
			},
			{
				label: 'Save',
				accelerator: 'CmdOrCtrl+S',
				click: (mi, bw) => send('FILE.SAVE', bw)
			},
			{
				label: 'Save As...',
				click: (mi, bw) => send('FILE.SAVE', bw, {saveAs: true})
			},
			{ type: 'separator' },
			{ role: 'quit' }
		]
	},
	{
		label: 'View',
		submenu: [
			{ role: 'reload' },
			{ role: 'forceReload' },
			{ type: 'separator' },
			{ role: 'toggleDevTools' }
		]
	}
];

function send(chanell, bw, arg) {
	bw.webContents.send(chanell, arg);
}

module.exports = {
	mainMenu
};
