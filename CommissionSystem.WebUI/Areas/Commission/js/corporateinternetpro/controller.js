function CorporateInternetProCtrl($scope, $http, $modal) {
    $scope.agentID = '';
    $scope.agents = [];

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

    $scope.showCommission = function () {
        if ($scope.agentID == '') {
            bootbox.alert('Please select agent');
            return;
        }

        var dateFrom = $scope.dateFrom;
        var dateTo = $scope.dateTo;

        if (dateFrom == null || dateTo == null) {
            bootbox.alert('Date From and Date To are required');
            return;
        }

        _dateFrom = utils.getDateStr(dateFrom);
        _dateTo = utils.getDateStr(dateTo);

        var o = {
            AgentID: $scope.agentID,
            DateFrom: _dateFrom,
            DateTo: _dateTo
        };

        var url = route.corporateinternetpro.commission;
        utils.blockUI();
        $http.post(url, o).success(function (data) {
            utils.unblockUI();
            if (data.success == 1) {
                $scope.result = data.result;
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

        var url = route.corporateinternetpro.mail;
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

    $scope.init = function () {
        var url = route.corporateinternetpro.agents;
        $http.get(url).success(function (data) {
            $scope.agents = data;
        });
    }
}