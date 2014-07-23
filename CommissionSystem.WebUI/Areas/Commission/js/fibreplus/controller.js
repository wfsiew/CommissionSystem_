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
        var a = null;

        if ($scope.agentID != '0')
            a = _.find($scope.agents, function (x) { return x.AgentID == $scope.agentID; });

        var o = {
            AgentID: $scope.agentID,
            AgentType: a == null ? '' : a.AgentType,
            DateFrom: _dateFrom,
            DateTo: _dateTo
        };

        var url = route.fibreplus.commission;
        utils.blockUI();
        $http.post(url, o).success(function (data) {
            utils.unblockUI();
            if (data.success == 1) {
                $scope.result = data;
                $scope.commission = data.commission;
                $scope.commissionrate = data.commissionrate;
                $scope.tiercommissionrate = data.tiercommissionrate;
                $scope.agentlevels = data.agentlevels;
                $scope.agentlist = data.agentlist;
                $scope.groups = {};

                for (var i in $scope.agentlevels) {
                    $scope.groups[i] = _.groupBy($scope.agentlist[i], 'AgentTeam');
                }
            }

            else
                toastr.error(data.message);
        })
    }

    $scope.init = function () {
        var url = route.fibreplus.agents;
        $http.get(url).success(function (data) {
            $scope.agents = data;
        });

        $scope.data =
            [
                { department: "engineering", name: "Scott" },
                { department: "engineering", name: "Joy" },
                { department: "sales", name: "Sara" }
            ];
        $scope.groups_ = _.groupBy($scope.data, "department");
    }
}