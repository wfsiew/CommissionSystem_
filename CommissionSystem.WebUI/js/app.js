'use strict';

/* App Module */

var app = angular.module('mvcapp', ['ngRoute', 'ngSanitize', 'ngAnimate', 'mvcappFilters', 'ui.bootstrap', 'ui.utils', 'ui.select2', 'mgcrea.ngStrap', 'chieffancypants.loadingBar']);

//app.config(['$routeProvider', function ($routeProvider) {
//    $routeProvider.
//        when('/index', { templateUrl: route.home.index, controller: IndexCtrl }).
//        when('/user', { templateUrl: route.user.index, controller: UserCtrl }).
//        when('/order/create', { templateUrl: route.order.form, controller: OrderCreateCtrl }).
//        otherwise({ redirectTo: '/index' });
//}]);

//app.factory('Page', function () {
//    var title = 'Index';
//    var message = {
//        text: null,
//        show: false
//    };
//    return {
//        title: function () {
//            return title;
//        },
//        setTitle: function (a) {
//            title = a;
//        },
//        message: function () {
//            return message;
//        },
//        setMessage: function (a) {
//            message.text = a;
//            message.show = true;
//        },
//        resetMessage: function () {
//            message.text = null;
//            message.show = false;
//        }
//    };
//});

//app.factory('Menu', function () {
//    var menu = {
//        home: true,
//        createorder: false,
//        searchorders: false,
//        usermgmt: false
//    };

//    return {
//        menu: function () {
//            return menu;
//        },
//        setMenu: function (a) {
//            _.each(menu, function (v, k, l) {
//                menu[k] = false;
//                if (a == k)
//                    menu[k] = true;
//            });
//        }
//    };
//});

utils.initToastr();