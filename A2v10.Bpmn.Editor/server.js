'use strict';

var express = require('express');
var path = require('path');
var port = process.env.PORT || 3000;

var app = express();
var staticPath = path.join(__dirname, '/');
app.use(express.static(staticPath));
app.set('port', port);

var server = app.listen(app.get('port'), () => {
	console.log('listening');
});

