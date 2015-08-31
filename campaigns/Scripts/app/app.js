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

        $scope.selectedAbilityId = -1;
        $scope.selectedSkillId = -1;

        activate();

        function activate() {
            // TODO: use a service https://github.com/johnpapa/angular-styleguide#style-y035
            console.log('loading application data');

            $scope.loaded = false;
            $scope.errorMessage = '';
            $scope.errorMessageDetail = '';
            rules.getRules().then(
                function (data) {
                    $scope.loaded = true;
                    $scope.abilities = data.abilities;
                    $scope.skills = data.skills;
                    console.log('got data: ', data);
                },
                function (errorInfo) {
                    $scope.loaded = true;
                    $scope.errorMessage = 'could not retrieve data - ' + errorInfo.statusText;
                    $scope.errorMessageDetail = errorInfo.message + '\n' + errorInfo.messageDetail;
                    console.log('got error: ', errorInfo.message, errorInfo.messageDetail);
                }
            );
        };

        //$scope.init = function (idToLoad) {
        //    $scope.loadCharacterSheet(idToLoad);
        //};
    }
})();
