(function () {
    'use strict';

    app.controller('MainController', MainController);

    MainController.$inject = ['RemoteDesktop', '$uibModal'];
    function MainController(RemoteDesktop, $uibModal) {
        var vm = this;

        vm.disconnect = disconnect;

        ////////////

        function disconnect() {
            if (RemoteDesktop.session.sessionId) {
                var modalInstance = $uibModal.open({
                    component: 'modalComponent',
                    resolve: {
                        title: function () {
                            return "Do you want to close this session? ";
                        },
                        content: function () {
                            return ["Other users on this session will also be disconnected.",
                            "If you do not want to force them to disconnect, just close the browser tab or reload the page."];
                        },
                        showCancelButton: function () {
                            return true;
                        }
                    }
                });

                modalInstance.result.then(function () {
                    RemoteDesktop.disconnect().then(function () {

                    }, function (response) {
                        $uibModal.open({
                            component: 'modalComponent',
                            resolve: {
                                title: function () {
                                    return "Oops!";
                                },
                                content: function () {
                                    return response.data.message || ["It seems that it was not possible to disconnect from the session!", "Please, Try again reloading the page."];
                                }
                            }
                        });
                    });
                });
            }
        }
    }
})();