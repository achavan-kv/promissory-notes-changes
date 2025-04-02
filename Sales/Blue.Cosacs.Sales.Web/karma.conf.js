'use strict';
function karmaConf(config) {
    config.set({
                   // frameworks to use
                   frameworks: ['browserify', 'jasmine'],

                   // list of files / patterns to load in the browser
                   files: [
                       //"lib/jquery/dist/jquery.js",
                       //"lib/angular/angular.js",
                       //"lib/angular-route/angular-route.js",
                       //"lib/angular-bootstrap/ui-bootstrap.js",
                       //"js/modules/scope.onready.js",
                       'test/vendor/**/*.js',
                       'test/**/*Spec.js'
                   ],

                   // list of files to exclude
                   exclude: [
                       "lib/angular/angular.js"
                   ],

                   // test results reporter to use
                   // possible values: 'dots', 'progress', 'junit', 'growl', 'coverage'
                   reporters: ['progress', 'coverage'],

                   // web server port
                   port: 9876,

                   // cli runner port
                   runnerPort: 9100,

                   // enable / disable colors in the output (reporters and logs)
                   colors: true,

                   // level of logging
                   // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
                   logLevel: 'LOG_DEBUG',

                   // enable / disable watching file and executing tests whenever any file changes
                   autoWatch: true,

                   // Start these browsers, currently available:
                   // - Chrome
                   // - ChromeCanary
                   // - Firefox
                   // - Opera
                   // - Safari (only Mac)
                   // - PhantomJS
                   // - IE (only Windows)
                   browsers: ['PhantomJS'],

                   // If browser does not capture in given timeout [ms], kill it
                   captureTimeout: 60000,

                   // Continuous Integration mode
                   // if true, it capture browsers, run tests and exit
                   // singleRun: true,

                   // Browserify config (all optional)
                   browserify: {
                       prebundle: function (bundle) {
                           bundle.external('sinon');
                           bundle.external('jasmine-sinon');

                       },
                       // extensions: ['.coffee'],
                       // ignore: [],
                       // transform: ['coffeeify'],
                       // debug: true,
                       noParse: ['jquery', 'angular'],
                       watch: true,
                       debug: true,
                       transform: ['brfs',
                           [{
                                ignore: ['**/lib/**', '**/*/*test.js']
                            }, 'browserify-istanbul' ]
                       ]
                       //transform: [ "browserify-istanbul"]
                       //transform: ["browserify-shim", "browserify-istanbul"]
                   },
                   "coverageReporter": {
                       "reporters": [
                           {"type": "html"},
                           {"type": "text-summary"}
                       ]
                   },

                   // Add browserify to preprocessors
                   preprocessors: {
                       'test/**/*Spec.js': ['browserify']
                   }

               });

}

module.exports = karmaConf;