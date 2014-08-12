﻿var route = (function () {

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

    var corporateinternetpro = {
        commission: utils.getUrl('/Commission/CorporateInternetPro/Commission'),
        agents: utils.getUrl('/Commission/CorporateInternetPro/Agents'),
        mail: utils.getUrl('/Commission/CorporateInternetPro/Mail')
    };

    var corporateinternetpremium = {
        commission: utils.getUrl('/Commission/CorporateInternetPremium/Commission'),
        agents: utils.getUrl('/Commission/CorporateInternetPremium/Agents'),
        mail: utils.getUrl('/Commission/CorporateInternetPremium/Mail')
    };

    var metroe = {
        commission: utils.getUrl('/Commission/MetroE/Commission'),
        agents: utils.getUrl('/Commission/MetroE/Agents'),
        mail: utils.getUrl('/Commission/MetroE/Mail')
    };

    var vsat = {
        commission: utils.getUrl('/Commission/VSAT/Commission'),
        agents: utils.getUrl('/Commission/VSAT/Agents'),
        mail: utils.getUrl('/Commission/VSAT/Mail')
    };

    return {
        fibreplus: fibreplus,
        speedplus: speedplus,
        data: data
    };
}());