var route = (function () {

    var fibreplus = {
        commission: utils.getUrl('/Commission/FibrePlus/Commission'),
        agents: utils.getUrl('/Commission/FibrePlus/Agents'),
        mail: utils.getUrl('/Commission/FibrePlus/Mail')
    };

    var speedplus = {
        commission: utils.getUrl('/Commission/SpeedPlus/Commission'),
        agents: utils.getUrl('/Commission/SpeedPlus/Agents'),
        mail: utils.getUrl('/Commission/SpeedPlus/Mail')
    };

    var data = {
        commission: utils.getUrl('/Commission/Data/Commission'),
        agents: utils.getUrl('/Commission/Data/Agents'),
        mail: utils.getUrl('/Commission/Data/Mail')
    };

    var dcs = {
        commission: utils.getUrl('/Commission/DiscountedCallService/Commission'),
        agents: utils.getUrl('/Commission/DiscountedCallService/Agents'),
        mail: utils.getUrl('/Commission/DiscountedCallService/Mail')
    };

    return {
        fibreplus: fibreplus,
        speedplus: speedplus,
        data: data,
        dcs: dcs
    };
}());