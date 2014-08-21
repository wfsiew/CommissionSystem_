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

    var sip = {
        commission: utils.getUrl('/Commission/SIP/Commission'),
        agents: utils.getUrl('/Commission/SIP/Agents'),
        mail: utils.getUrl('/Commission/SIP/Mail')
    }

    var e1 = {
        commission: utils.getUrl('/Commission/E1/Commission'),
        agents: utils.getUrl('/Commission/E1/Agents'),
        mail: utils.getUrl('/Commission/E1/Mail')
    }

    return {
        fibreplus: fibreplus,
        speedplus: speedplus,
        data: data,
        dcs: dcs,
        sip: sip,
        e1: e1
    };
}());