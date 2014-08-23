function SIPCtrl($scope, $http, $modal) {
    $scope.agentID = '';
    $scope.agents = [];
    $scope.page = 1;

    $scope.openDateFrom = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.openedDateFrom = true;
    }

    $scope.openDateTo = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.openedDateTo = true;
    }

    $scope.pageChanged = function () {
        var o = $scope.validate();
        if (o == null)
            return;

        o.Load = true;
        o.Page = $scope.page;

        $scope.getResult(o);
    }

    $scope.showCommission = function () {
        var o = $scope.validate();
        if (o == null)
            return;

        o.Page = 1;
        $scope.getResult(o);
    }

    $scope.getResult = function (o) {
        var url = route.sip.commission;
        utils.blockUI();
        $http.post(url, o).success(function (data) {
            utils.unblockUI();
            if (data.success == 1) {
                $scope.result = data.result;
                $scope.pager = data.pager;
                $scope.page = $scope.pager.PageNum;
            }

            else
                toastr.error(data.message);
        }).error(function (data, status) {
            utils.unblockUI();
            bootbox.alert('Request failed');
        });
    }

    $scope.sendMail = function () {
        var dateFrom = $scope.dateFrom;
        var dateTo = $scope.dateTo;

        var _dateFrom = utils.getDateStr(dateFrom);
        var _dateTo = utils.getDateStr(dateTo);

        var o = {
            AgentID: $scope.agentID,
            DateFrom: _dateFrom,
            DateTo: _dateTo
        };

        var url = route.sip.mail;
        utils.blockUI();
        $http.post(url, o).success(function (data) {
            utils.unblockUI();
            if (data.success == 1)
                toastr.success('Mail has been sent successfully');

            else
                toastr.error(data.message);
        }).error(function (data, status) {
            utils.unblockUI();
            bootbox.alert('Request failed');
        });
    }

    $scope.validate = function () {
        var o = null;

        if ($scope.agentID == '') {
            bootbox.alert('Please select agent');
            return o;
        }

        var dateFrom = $scope.dateFrom;
        var dateTo = $scope.dateTo;

        if (dateFrom == null || dateTo == null) {
            bootbox.alert('Date From and Date To are required');
            return o;
        }

        _dateFrom = utils.getDateStr(dateFrom);
        _dateTo = utils.getDateStr(dateTo);

        o = {
            AgentID: $scope.agentID,
            DateFrom: _dateFrom,
            DateTo: _dateTo,
            Load: false
        };

        return o;
    }

    $scope.getStatus = function (o) {
        var a = '';

        if (o.Status == 0)
            a = '(TERMINATED)'

        else if (o.Status == 4)
            a = '(SUSPENDED)';

        return a;
    }

    $scope.getRowCss = function (o) {
        var a = '';

        if (o.Status == 0)
            a = 'alert alert-danger';

        else if (o.Status == 4)
            a = 'alert alert-warning';

        return a;
    }

    $scope.init = function () {
        var url = route.sip.agents;
        $http.get(url).success(function (data) {
            $scope.agents = data;
        })
    }
}