(function () {
    'use strict';

    app.component('modalComponent', {
    templateUrl: 'js/app/common/modal/templates/modalTemplate.html',
    bindings: {
        resolve: '<',
        close: '&',
        dismiss: '&'
    },
    controller: function ($scope, $document) {
        var $ctrl = this;

        var EVENT_TYPES = "keydown keypress"

        activate();

        ////////////

        function activate() {
            $document.bind(EVENT_TYPES, eventHandler);

            $scope.$on('$destroy', function () {
                $document.unbind(EVENT_TYPES, eventHandler);
            })
        }

        function eventHandler(event) {
            if (event.which === 13) {
                $ctrl.ok();
                event.preventDefault();
                return false;
            }
        }

        $ctrl.$onInit = function () {
            $ctrl.title = $ctrl.resolve.title || "Message";
            $ctrl.messages = angular.isArray($ctrl.resolve.content) ? $ctrl.resolve.content : [$ctrl.resolve.content || ""];
            $ctrl.showCancelButton = $ctrl.resolve.showCancelButton;
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