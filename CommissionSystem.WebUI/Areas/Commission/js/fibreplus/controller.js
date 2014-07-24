function FibrePlusCtrl($scope, $http, $modal) {
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

    $scope.getAgentDisplay = function (id, level) {
        var v = '';
        var a = _.find($scope.agentlist[level], function (x) {
            return x.AgentTeam == id;
        });
        if (a != null)
        {
            if (a.AgentTeamType != '')
                v = id + ' (' + a.AgentTeamType + '): ' + a.AgentTeamName;
        }

        return v;
    }

    $scope.getCssRow = function (a) {
        var v = 'list-group-item';

        if (a.TotalCommission > 0)
            v +=  ' ' + v + '-info';

        return v;
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

                _.each($scope.agentlevels, function (i) {
                    $scope.groups[i] = _.groupBy($scope.agentlist[i], 'AgentTeam');
                });
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
    }

    $scope.items = ['item1', 'item2', 'item3'];

    $scope.open = function (size) {
        var mi = $modal.open({
            templateUrl: '/ngview/test.html',
            controller: ModalInstanceCtrl,
            size: size,
            resolve: {
                items: function () {
                    return $scope.items;
                }
            }
        });

        mi.result.then(function (x) {
            $scope.selected = x;
        }, function () {

        });
    }
}

function ModalInstanceCtrl($scope, $modalInstance, items) {
    $scope.items = items;

    $scope.selected = {
        item: $scope.items[0]
    };

    $scope.ok = function () {
        $modalInstance.close($scope.selected.item);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
}