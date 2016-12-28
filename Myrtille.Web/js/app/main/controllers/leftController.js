(function () {
    'use strict';

    app.controller('LeftController', LeftController);

    LeftController.$inject = ['RemoteDesktop', '$interval'];
    function LeftController(RemoteDesktop, $interval) {
        var vm = this;
        var cancelRefreshCurrentSessions;

        vm.availableConfigs = window.availableConfigs;
        vm.currentSessions = [];
        vm.openSessionId = "";
        vm.startConnection = startConnection;
        vm.joinConnection = joinConnection;

        activate();

        ////////////

        function activate() {
            cancelRefreshCurrentSessions = $interval(function () {
                RemoteDesktop.currentSessions().then(function (currentSessions) {
                    vm.currentSessions = currentSessions;
                });
            }, 2000);
        }
        
        function startConnection(configID, program) {
            RemoteDesktop.startConnection({ configID: configID, program: program }).then(function (sessionId) {
                joinConnection(sessionId);
            });
        }

        function joinConnection(sessionId) {
            vm.openSessionId = sessionId;
            startMyrtille(sessionId, true, 8181, null, false, false, false);
        }
    }
})();