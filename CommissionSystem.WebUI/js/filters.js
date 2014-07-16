'use strict';

angular.module('mvcappFilters', []).filter('datefilter', function () {
    return function (input) {
        if (input != null) {
            var v = input.replace('/Date(', '').replace(')/', '');
            var i = parseInt(v);
            return i;
        }

        return null;
    };
});