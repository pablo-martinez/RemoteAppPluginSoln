(function () {
    'use strict';

    app.controller('LeftController', LeftController);

    LeftController.$inject = ['RemoteDesktop', '$interval', '$uibModal'];
    function LeftController(RemoteDesktop, $interval, $uibModal) {
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
            }, function (result) {
                $uibModal.open({
                    component: 'modalComponent',
                    resolve: {
                        title: function () {
                            return "Oops!";
                        },
                        content: function () {
                            return result.data.message;
                        }
                    }
                });
            });
        }

        function joinConnection(sessionId) {
            // This is a horrible way of dealing with elements visibility, but this is just a POC
            angular.element(document.getElementById("main-pane").children[0]).removeClass("hidden");

            vm.openSessionId = sessionId;
            startMyrtille(sessionId, true, 8181, null, false, false, false);
        }
    }
})();