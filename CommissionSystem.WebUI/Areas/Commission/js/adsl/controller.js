﻿function ADSLCtrl($scope, $http, $modal) {
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
        var a = null;

        if ($scope.agentID != '0')
            a = _.find($scope.agents, function (x) { return x.AgentID == $scope.agentID; });

        var o = {
            AgentID: $scope.agentID,
            AgentType: a == null ? '' : a.AgentType,
            DateFrom: _dateFrom,
            DateTo: _dateTo
        };
    }

    $scope.init = function () {
        var url = route.adsl.agents;
        $http.get(url).success(function (data) {
            $scope.agents = data;
        });
    }
}