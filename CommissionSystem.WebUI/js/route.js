﻿var route = (function () {

    var fibreplus = {
        commission: utils.getUrl('/Commission/FibrePlus/Commission'),
        agents: utils.getUrl('/Commission/FibrePlus/Agents')
    };

    var speedplus = {
        commission: utils.getUrl('/Commission/SpeedPlus/Commission'),
        agents: utils.getUrl('/Commission/SpeedPlus/Agents')
    };

    var adsl = {
        commission: utils.getUrl('/Commission/ADSL/Commission'),
        agents: utils.getUrl('/Commission/ADSL/Agents')
    };

    return {
        fibreplus: fibreplus,
        speedplus: speedplus,
        adsl: adsl
    };
}());