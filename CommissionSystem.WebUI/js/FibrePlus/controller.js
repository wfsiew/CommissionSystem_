function FibrePlusCtrl($scope, $http) {
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
        var a = _.find($scope.agents, function (x) { return x.AgentID == $scope.agentID; });

        var o = {
            agentID: $scope.agentID,
            agentLevel: a.AgentLevel,
            from: _dateFrom,
            to: _dateTo
        };

        var url = utils.getUrl('/FibrePlus/Commission');
        utils.blockUI();
        $http.post(url, o).success(function (data) {
            utils.unblockUI();
            if (data.success == 1)
                $scope.commission = data.commission;

            else
                toastr.error(data.message);
        })
    }

    $scope.init = function () {
        var url = utils.getUrl('/FibrePlus/Agents');
        $http.get(url).success(function (data) {
            $scope.agents = data;
        });
    }

    $scope.init();
}