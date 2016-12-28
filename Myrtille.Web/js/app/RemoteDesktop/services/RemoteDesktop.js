(function () {
    'use strict';

    app.factory('RemoteDesktop', RemoteDesktop);

    RemoteDesktop.$inject = ['$http'];
    function RemoteDesktop($http) {
        var service = {
            startConnection: startConnection,
            currentSessions: currentSessions
        };

        return service;

        ////////////

        function startConnection(data) {
            return $http.post('StartConnection.aspx', data).then(function (response) {
                return response.data.sessionId;
            });
        }

        function currentSessions(data) {
            return $http.post('CurrentSessions.aspx').then(function (response) {
                return response.data.currentSessions;
            });
        }
    }
})();