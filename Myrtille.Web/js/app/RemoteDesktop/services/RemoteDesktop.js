(function () {
    'use strict';

    app.factory('RemoteDesktop', RemoteDesktop);

    RemoteDesktop.$inject = ['$http'];
    function RemoteDesktop($http) {
        var service = {
            session: { sessionId: undefined },
            startConnection: startConnection,
            disconnect: disconnect,
            currentSessions: currentSessions
        };

        return service;

        ////////////

        function startConnection(data) {
            return $http.post('StartConnection.aspx', data).then(function (response) {
                return response.data.sessionId;
            });
        }

        function disconnect() {
            return $http.post('Disconnect.aspx', { sessionId: service.session.sessionId }).then(function (response) {
                service.session.sessionId = undefined;
                return;
            });
        }

        function currentSessions() {
            return $http.post('CurrentSessions.aspx').then(function (response) {
                return response.data.currentSessions;
            });
        }
    }
})();