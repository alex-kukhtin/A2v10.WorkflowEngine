
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
				click: (mi, bw) => send('FILE.SAVEAS', bw)
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

function send(chanell, bw) {
	bw.webContents.send(chanell);
}

module.exports = {
	mainMenu
};
