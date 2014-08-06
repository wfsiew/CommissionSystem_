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
})
.filter('mycurrency', ['$filter', function($filter) {
    return function(amount, currencySymbol) {
        var currency = $filter('currency');

        if (amount < 0)
            return currency(amount, currencySymbol).replace('(', '-').replace(')', '');

        return currency(amount, currencySymbol);
    }
}]);