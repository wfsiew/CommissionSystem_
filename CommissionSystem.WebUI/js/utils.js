var utils = (function () {
    var alertTimeout = 5000;

    function getUrl(a) {
        //var app = '/CommissionSystem';
        var app = "";
        return app + a;
    }

    function getDateStr(date) {
        var s = '';

        if (date == null || date == '')
            return s;

        if (typeof date == 'string')
            return date;

        s = date.getFullYear() + '-' + paddZero(date.getMonth() + 1) + '-' + paddZero(date.getDate());
        return s;
    }

    function getTimeStr(date) {
        var s = '';

        if (date == null || date == '')
            return s;

        if (typeof date == 'string')
            return date;

        var t = date.getHours() < 12 ? 'AM' : 'PM';
        var hour = date.getHours() % 12;
        if (hour == 0)
            hour = 12;

        var min = date.getMinutes();
        s = paddZero(hour) + ':' + paddZero(min) + ' ' + t;
        return s;
    }

    function paddZero(a) {
        var s = a < 10 ? '0' + a : a;
        return s;
    }

    function getDate(a) {
        if (a != null) {
            var v = a.replace('/Date(', '').replace(')/', '');
            var i = parseInt(v);
            var date = new Date(i);
            return date;
        }

        return null;
    }

    function blockUI() {
        $.blockUI({ message: '<h3>Loading...</h3>' });
    }

    function unblockUI() {
        $.unblockUI();
    }

    function initToastr() {
        toastr.options = {
            closeButton: true,
            positionClass: 'toast-top-full-width',
            timeout: alertTimeout
        };
    }

    return {
        getUrl: getUrl,
        getDateStr: getDateStr,
        getTimeStr: getTimeStr,
        getDate: getDate,
        blockUI: blockUI,
        unblockUI: unblockUI,
        initToastr: initToastr
    };
}());