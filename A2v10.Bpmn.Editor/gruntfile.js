/// <binding />
module.exports = function (grunt) {
	grunt.initConfig({
		clean: ['/dist/*'],
		browserify: {
			options: {
				browserifyOptions: {
					debug: false
				},
				plugin: [
					'esmify',
				]
			},
			app: {
				files: {
					'dist/bpmn-editor.js': ['app/bpmn-editor.js'],
					'dist/bpmn-viewer.js': ['app/bpmn-viewer.js']
				}
			}
		},
		terser: {
			options: {
				ecma: '2016',
				mangle: false /*does not work for modeler*/
			},
			app: {
				files: {
					"dist/bpmn-editor.min.js": ['dist/bpmn-editor.js'],
					"dist/bpmn-viewer.min.js": ['dist/bpmn-viewer.js']
				}
			},
		},
		watch: {
			files: ["app/**/*.js"],
			tasks: ["browserify"]
		}
	});

	grunt.loadNpmTasks('grunt-contrib-clean');
	grunt.loadNpmTasks('grunt-browserify');
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-terser');

	grunt.registerTask("all", ['clean', 'browserify', 'terser']);
};
