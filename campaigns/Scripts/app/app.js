(function () {
    'use strict';

    angular.module('app', ['app.rules']);

    angular.module('app').controller('CharacterSheetDetailsCtrl', Controller);
        
    Controller.$inject = ['$scope', '$http', 'rules'];
    function Controller($scope, $http, rules) {
        $scope.loaded = false;
        $scope.errorMessage = '';
        $scope.errorMessageDetail = '';

        $scope.abilities = [];
        $scope.skills = [];

        $scope.selectedAbilityIds = [];

        activate();

        function activate() {
            // TODO: use a service https://github.com/johnpapa/angular-styleguide#style-y035
            console.log('loading application data');

            $scope.loaded = false;
            $scope.errorMessage = '';
            $scope.errorMessageDetail = '';
            rules.getCategoryAttributes('abilities').then(
                handleData('abilities'), handleError
            );
            rules.getCategoryAttributes('skills').then(
                handleData('skills'), handleError
            );

            function handleData(category) {
                return function (data) {
                    $scope.loaded = true;
                    $scope[category] = data;
                    console.log('got ' + category + ' data: ', data);
                };
            };

            function handleError(errorInfo) {
                $scope.loaded = true;
                $scope.errorMessage = 'could not retrieve data - ' + errorInfo.statusText;
                $scope.errorMessageDetail = errorInfo.message + '\n' + errorInfo.messageDetail;
                console.log('got error: ', errorInfo.message, errorInfo.messageDetail);
            };
        };
    }
})();
