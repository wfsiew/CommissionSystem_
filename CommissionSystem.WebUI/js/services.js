'use strict';

app.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push('noCacheInterceptor');
}])
.factory('noCacheInterceptor', function () {
    return {
        request: function (config) {
            if (config.method == 'GET' && (config.url.indexOf('ngview/') < 0 && config.url.indexOf('.html') < 0)) {
                var sep = config.url.indexOf('?') === -1 ? '?' : '&';
                config.url = config.url + sep + 'noCache=' + new Date().getTime();
            }

            return config;
        }
    };
});