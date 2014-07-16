var utils = (function () {
    var alertTimeout = 5000;

    function getUrl(a) {
        //var app = '/FibreSystem';
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

    function isValidInstallDate(dt) {
        var b = true;

        if (dt != null) {
            var d = dt.getDay();
            if (d == 0 || d == 6)
                b = false;
        }

        return b;
    }

    function isValidInstallTime(s) {
        var b = true;

        try {
            if (s != null && s != '') {
                var h;
                var m;
                var k = 0;

                if (s.indexOf('PM') >= 0)
                    k = 12;

                var a = s.replace(' AM', '');
                a = a.replace(' PM', '');

                var t = a.split(':');
                h = parseInt(t[0]);
                m = parseInt(t[1]);

                if (h < 12)
                    h += k;

                if (h < 9 || h > 17)
                    b = false;

                if (h == 17 && m > 0)
                    b = false;
            }
        }

        catch (e) {

        }

        return b;
    }

    function blockUI() {
        $.blockUI({ message: '<h3>Loading...</h3>' });
    }

    function unblockUI() {
        $.unblockUI();
    }

    function initDrop() {
        $(document).bind('dragover', function (e) {
            var dropZone = $('#dropzone'),
                timeout = window.dropZoneTimeout;

            if (!timeout) {
                dropZone.addClass('in');
            }

            else {
                clearTimeout(timeout);
            }

            var found = false,
                node = e.target;

            do {
                if (node === dropZone[0]) {
                    found = true;
                    break;
                }
                node = node.parentNode;
            } while (node != null);

            if (found) {
                dropZone.addClass('hover');
            }

            else {
                dropZone.removeClass('hover');
            }

            window.dropZoneTimeout = setTimeout(function () {
                window.dropZoneTimeout = null;
                dropZone.removeClass('in hover');
            }, 100);
        });
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
        isValidInstallDate: isValidInstallDate,
        isValidInstallTime: isValidInstallTime,
        blockUI: blockUI,
        unblockUI: unblockUI,
        initDrop: initDrop,
        initToastr: initToastr
    };
}());