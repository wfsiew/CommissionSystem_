'use strict';

function PageCtrl($scope, Page, Menu) {
    $scope.Page = Page;
    $scope.Menu = Menu;
}

function IndexCtrl($scope, Page, Menu) {
    Page.setTitle('Index');
    Menu.setMenu('home');
}

function UserCtrl($scope, $http, $modal, Page, Menu) {
    Page.setTitle('User Management');
    Menu.setMenu('usermgmt');

    $scope.currentSort = '';

    $scope.init = function () {
        $http.get(route.user.issuperadmin).success(function (data) {
            $scope.isSuperAdmin = data.status == 1 ? true : false;
            $scope.gotoPage(1);
        });
    }

    $scope.search = function () {
        $scope.gotoPage(1);
    }

    $scope.gotoPage = function (page) {
        var params = {
            SearchString: ($scope.SearchString == '' ? null : $scope.SearchString),
            sortOrder: $scope.currentSort,
            page: page
        };

        $http.get(route.user.search, { params: params }).success(function (data) {
            if (data.error == 1) {
                toastr.error(data.message);
                $scope.pager = null;
                $scope.model = null;
            }

            else {
                $scope.pager = data.pager;
                $scope.model = data.model;
            }
        });
    }

    $scope.sort = function (a) {
        if (a == 'Name') {
            if ($scope.currentSort == null || $scope.currentSort == '')
                $scope.currentSort = 'Name_desc';

            else
                $scope.currentSort = '';
        }

        else if (a == 'Email') {
            if ($scope.currentSort == 'Email')
                $scope.currentSort = 'Email_desc';

            else
                $scope.currentSort = 'Email';
        }

        else if (a == 'PhoneNo') {
            if ($scope.currentSort == 'PhoneNo')
                $scope.currentSort = 'PhoneNo_desc';

            else
                $scope.currentSort = 'PhoneNo';
        }

        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.getSortCss = function (a) {
        var up = 'icon-chevron-up';
        var down = 'icon-chevron-down';

        if (($scope.currentSort == null || $scope.currentSort == '') && a == 'Name')
            return up;

        if ($scope.currentSort.indexOf(a) == 0) {
            if ($scope.currentSort.indexOf('desc') > 0)
                return down;

            else
                return up;
        }

        return null;
    }

    $scope.addNew = function () {
        var mi = $modal.open({
            templateUrl: route.user.form,
            controller: UserCreateCtrl,
            resolve: {
                items: function () {
                    return {};
                }
            }
        });

        mi.result.then(function (x) {
            $scope.saveCreate(x);
        }, function () {
        });
    }

    $scope.editItem = function (o) {
        utils.blockUI();
        $http.get(route.user.edit + '/' + o.ID).success(function (data) {
            if (data.error == 1) {
                toastr.error(data.message);
            }

            else {
                $scope.showEdit(data);
            }

            utils.unblockUI();
        });
    }

    $scope.showEdit = function (o) {
        var mi = $modal.open({
            templateUrl: route.user.form,
            controller: UserEditCtrl,
            resolve: {
                items: function () {
                    return o;
                }
            }
        });

        mi.result.then(function (x) {
            $scope.saveEdit(x);
        }, function () {
        });
    }

    $scope.removeItem = function (o) {
        var mi = $modal.open({
            templateUrl: route.user.confirmdelete,
            controller: ConfirmDeleteCtrl,
            resolve: {
                items: function () {
                    return o;
                }
            }
        });

        mi.result.then(function (x) {
            $scope.deleteItem(o);
        }, function () {
        });
    }

    $scope.saveCreate = function (o) {
        toastr.success(o.message);
        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.saveEdit = function (o) {
        toastr.success(o.message);
        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.deleteItem = function (o) {
        $http.post(route.user.del, { id: o.ID }).success(function (data) {
            if (data.success == 1) {
                toastr.success(data.message);
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }

    $scope.init();
}

function ConfirmDeleteCtrl($scope, $modalInstance, items) {

    $scope.ok = function () {
        $modalInstance.close(items);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
}

function UserCreateCtrl($scope, $http, $modalInstance, items) {
    $scope.title = 'Create User';

    $scope.save = function () {
        var _selectedRoles = _.where($scope.Roles, { Assigned: true });
        var selectedRoles = _.map(_selectedRoles, function (o) {
            return o.RoleID;
        });

        var o = {
            Email: $scope.model.Email,
            PhoneNo: $scope.model.PhoneNo,
            selectedRoles: selectedRoles
        };

        $http.post(route.user.create, o).success(function (data) {
            if (data.success == 1) {
                items = data;
                $modalInstance.close(items);
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    }

    $scope.dismissAlert = function () {
        $scope.success = false;
        $scope.error = false;
    }

    $scope.model = {};

    $http.get(route.user.roles).success(function (data) {
        $scope.Roles = data;
    });

    $http.get(route.user.unregisteredUsers).success(function (data) {
        $scope.Emails = data;
    });
}

function UserEditCtrl($scope, $http, $modalInstance, items) {
    $scope.title = 'Edit User';

    $scope.save = function () {
        var _selectedRoles = _.where($scope.Roles, { Assigned: true });
        var selectedRoles = _.map(_selectedRoles, function (o) {
            return o.RoleID;
        });

        var o = {
            ID: $scope.model.ID,
            Email: $scope.model.Email,
            Name: $scope.model.Name,
            PhoneNo: $scope.model.PhoneNo,
            selectedRoles: selectedRoles,
            Roles: selectedRoles
        };

        $http.post(route.user.edit, o).success(function (data) {
            if (data.success == 1) {
                items = data;
                $modalInstance.close(data);
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    }

    $scope.model = items;
    $scope.Roles = items.Roles;
    $scope.Emails = items.Emails;
}

function OrderCreateCtrl($scope, $http, $timeout, Page, Menu) {
    Page.setTitle('Create Order');
    Menu.setMenu('createorder');

    $scope.SalesPersons = [
        { ID: 1, Name: 'Ben' },
        { ID: 2, Name: 'NNN' }
    ];

    $scope.openReceivedDate = function () {
        $timeout(function () {
            $scope.openedReceivedDate = true;
        });
    }

    $scope.openBookedInstallDate = function () {
        $timeout(function () {
            $scope.openedBookedInstallDate = true;
        });
    }

    $scope.save = function () {
        
        $scope.model = {

        };
        return;

        var o = {
            Name: $scope.model.Name,
            Age: $scope.model.Age
        };

        $http.post(route.order.create, o).success(function (data) {
            if (data.success == 1) {
                $scope.model = {

                };
                //window.location.href = '#/createorder';
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }
}


function IndexCtrl_($scope, $timeout) {
    $scope.db = 'CallBilling2';
    $scope.companycode = '10';

    $scope.openDateFrom = function () {
        $timeout(function () {
            $scope.openedDateFrom = true;
        });
    }

    $scope.openDateTo = function () {
        $timeout(function () {
            $scope.openedDateTo = true;
        });
    }

    $scope.exportInvoice2007 = function () {
        if ($scope.db == 'ROSSInterface')
            $scope.exportInvoice(2);

        else
            $scope.exportInvoice(0);
    }

    $scope.exportInvoice2003 = function () {
        if ($scope.db == 'ROSSInterface')
            $scope.exportInvoice(3);

        else
            $scope.exportInvoice(1);
    }

    $scope.exportPayment2007 = function () {
        $scope.exportPayment(0);
    }

    $scope.exportPayment2003 = function () {
        $scope.exportPayment(1);
    }

    $scope.exportInvoice = function (t) {
        var dateFrom = $scope.dateFrom;
        var dateTo = $scope.dateTo;
        var db = $scope.db;
        var companycode = $scope.companycode;

        if (dateFrom == null || dateTo == null) {
            alert('Date From and Date To are required');
            return;
        }

        _dateFrom = utils.getDateStr(dateFrom);
        _dateTo = utils.getDateStr(dateTo);
        var url = utils.getUrl('/Home/ExportInvoice/?t=' + t + '&db=' + db + '&cp=' + companycode + '&from=' + _dateFrom + '&to=' + _dateTo);

        $('#exportFrame').attr('src', url);
    }

    $scope.exportPayment = function (t) {
        var dateFrom = $scope.dateFrom;
        var dateTo = $scope.dateTo;
        var db = $scope.db;

        if (dateFrom == null || dateTo == null) {
            alert('Date From and Date To are required');
            return;
        }

        _dateFrom = utils.getDateStr(dateFrom);
        _dateTo = utils.getDateStr(dateTo);
        var url = utils.getUrl('/Home/ExportPayment/?t=' + t + '&db=' + db + '&from=' + _dateFrom + '&to=' + _dateTo);

        $('#exportFrame').attr('src', url);
    }
}