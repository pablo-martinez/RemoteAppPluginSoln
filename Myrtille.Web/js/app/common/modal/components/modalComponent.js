(function () {
    'use strict';

    app.component('modalComponent', {
    templateUrl: 'js/app/common/modal/templates/modalTemplate.html',
    bindings: {
        resolve: '<',
        close: '&',
        dismiss: '&'
    },
    controller: function () {
        var $ctrl = this;

        $ctrl.$onInit = function () {
            $ctrl.title = $ctrl.resolve.title || "Message";
            $ctrl.content = $ctrl.resolve.content || "";
        };

        $ctrl.ok = function () {
            $ctrl.close({ $value: 'ok' });
        };

        $ctrl.cancel = function () {
            $ctrl.dismiss({ $value: 'cancel' });
        };
    }
    });
})();