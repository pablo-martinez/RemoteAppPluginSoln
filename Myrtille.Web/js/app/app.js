var app = angular.module("myrtille", ['ui.router', 'ui.bootstrap']);

app.config(function ($stateProvider, $urlRouterProvider) {

    $urlRouterProvider.otherwise('/');

    $stateProvider
        // HOME STATES AND NESTED VIEWS ========================================
        .state('home', {
            url: '/',
            //templateUrl: 'templates/partial-home.html',
            views: {
                'left': {
                    templateUrl: 'templates/partial-home-left.html',
                    controller: 'LeftController as left'
                },
                'main': {
                    templateUrl: 'templates/partial-home-main.html',
                    controller: 'MainController as main'
                },
                'right': {
                    templateUrl: 'templates/partial-home-right.html',
                    controller: 'RightController as right'
                },
            }
        });
});