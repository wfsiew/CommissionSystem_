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

    var adsl = {
        commission: utils.getUrl('/Commission/ADSL/Commission'),
        agents: utils.getUrl('/Commission/ADSL/Agents'),
        mail: utils.getUrl('/Commission/ADSL/Mail')
    };

    return {
        fibreplus: fibreplus,
        speedplus: speedplus,
        adsl: adsl
    };
}());