(function () {
    'use strict';

    angular.module('app', ['app.rules', 'angular-loading-bar']);

    angular.module('app').controller('CharacterSheetDetailsCtrl', Controller);

    Controller.$inject = ['$scope', '$http', 'rules'];
    function Controller($scope, $http, rules) {
        $scope.loaded = false;
        $scope.errorMessage = '';
        $scope.errorMessageDetail = '';

        $scope.abilities = [];
        $scope.skills = [];
        $scope.contributions = [];
        $scope.$watch('contributions', handleContributionsUpdated);

        $scope.contributionsByTarget = {};
        $scope.contributionsBySource = {};
        $scope.currentAttributeId = -1;

        $scope.selectedAbilityIds = [];

        $scope.getContributingAttributeNames = getContributingAttributeNames;
        $scope.shouldHighlightAttribute = shouldHighlightAttribute;
        $scope.hilightAttribute = hilightAttribute;

        activate();

        function activate() {
            // TODO: use a service https://github.com/johnpapa/angular-styleguide#style-y035
            console.log('loading application data');

            $scope.start = function () {
                console.log('xxx');
                cfpLoadingBar.start();
            }
            $scope.stop = function () {
                console.log('xxx');
                cfpLoadingBar.complete();
            }
            $scope.loaded = false;
            $scope.errorMessage = '';
            $scope.errorMessageDetail = '';
            rules.getCategoryAttributes('abilities').then(
                handleData('abilities'), handleError
            );
            rules.getCategoryAttributes('skills').then(
                handleData('skills'), handleError
            );
            rules.getContributions().then(
                handleData('contributions'), handleError
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

        function getContributingAttributeNames(attributeId) {
            var sourcesToHighlight = $scope.contributionsByTarget[attributeId];
            if (!sourcesToHighlight) return '';

            var result = '';
            for (var idx = 0; idx < sourcesToHighlight.length; ++idx) {
                if (idx > 0) result += ',';
                result += sourcesToHighlight[idx].sourceName;
            }
            return result;
        };

        function shouldHighlightAttribute(attributeId) {
            var selectedId = $scope.currentAttributeId;
            if (selectedId == -1) return false;
            if (selectedId == attributeId) return true;

            var targetsToHilight = $scope.contributionsBySource[selectedId];
            if (targetsToHilight) {
                for (var idx = 0; idx < targetsToHilight.length; ++idx) {
                    if (targetsToHilight[idx].targetId == attributeId) return true;
                }
            }

            var sourcesToHighlight = $scope.contributionsByTarget[selectedId];
            if (sourcesToHighlight) {
                for (var idx = 0; idx < sourcesToHighlight.length; ++idx) {
                    if (sourcesToHighlight[idx].sourceId == attributeId) return true;
                }
            }

            return false;
        };

        function hilightAttribute(attributeId) {
            $scope.currentAttributeId = attributeId;
        };

        //
        // private implementation
        //

        function handleContributionsUpdated(newValue, oldValue) {
            var bySrc = $scope.contributionsBySource;
            var byTgt = $scope.contributionsByTarget;
            for (var member in bySrc) delete bySrc[member];
            for (var member in byTgt) delete byTgt[member];
            $.each(newValue, function (idx, contrib) {
                if (!bySrc[contrib.sourceId]) {
                    bySrc[contrib.sourceId] = [];
                }
                if (!byTgt[contrib.targetId]) {
                    byTgt[contrib.targetId] = [];
                }
                bySrc[contrib.sourceId].push(contrib);
                byTgt[contrib.targetId].push(contrib);
            });
        };
    }
})();
