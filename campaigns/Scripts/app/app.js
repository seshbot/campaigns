(function () {
    'use strict';

    // TODO: cache this inside the actual character service
    function Character(data, rules) {
        var self = this;
        self._rules = rules;
        self.name = data.name;
        self.attributeValues = data.attributeValues;
        self._attribIds = [];
        self._attribValuesByAttribId = [];

        if (typeof data.attributeValues === 'undefined') return;
        $.each(data.attributeValues, function (idx, attribVal) {
            self._attribIds.push(attribVal.attributeId);
            self._attribValuesByAttribId[attribVal.attributeId] = attribVal;
        });
    };

    Character.prototype = {
        getAttributeValueById: function (attribId) {
            var self = this;

            var byId = self._attribValuesByAttribId;

            if (typeof byId[attribId] === 'undefined') return '';
            return byId[attribId].value;
        },
        getAttributeValueByName: function (categoryName, attributeShortName) {
            var self = this;

            var attrib = self._rules.getAttribute(categoryName, attributeShortName);
            if (!attrib) return;

            return self.getAttributeValueById(attrib.id);
        },
        getAttributeByCategory: function (categoryName) {
            var self = this;

            var attribs = self._rules.attributesByCategory[categoryName];
            if (!attribs) return;

            for (var idx = 0; idx < attribs.length; ++idx) {
                var attrib = attribs[idx];
                if (-1 !== self._attribIds.indexOf(attrib.id)) {
                    return attrib;
                }
            }
        }
    };


    angular.module('app', ['app.rules', 'app.characters', 'angular-loading-bar']);

    angular.module('app').controller('CharacterSheetDetailsCtrl', Controller);

    Controller.$inject = ['$scope', '$http', 'rules', 'characters', 'characterId'];
    function Controller($scope, $http, rules, characters, characterId) {
        $scope.errorMessage = '';
        $scope.errorMessageDetail = '';

        $scope.character = {};
        $scope.attributes = [];
        $scope.contributions = [];

        $scope.$watch('attributes', handleAttributesUpdated);
        $scope.$watch('contributions', handleContributionsUpdated);

        $scope.attributesByCategory = {};
        $scope.contributionsByTarget = {};
        $scope.contributionsBySource = {};
        $scope.currentAttributeId = -1;

        $scope.selectedAbilityIds = [];

        $scope.getAttribute = getAttribute;
        $scope.getContributingAttributeNames = getContributingAttributeNames;
        $scope.shouldHighlightAttribute = shouldHighlightAttribute;
        $scope.hilightAttribute = hilightAttribute;

        activate();

        function activate() {
            // TODO: use a service https://github.com/johnpapa/angular-styleguide#style-y035
            console.log('loading application data');

            $scope.errorMessage = '';
            $scope.errorMessageDetail = '';

            //characters.getCharacter(characterId).then(
            //    handleCharacterData, handleError
            //);

            rules.getAttributes().then(
                handleData('attributes'), handleError
            );

            rules.getContributions().then(
                handleData('contributions'), handleError
            );

            function handleData(category) {
                return function (data) {
                    $scope[category] = data;
                    console.log('got ' + category + ' data: ', data);
                };
            };

            function handleError(errorInfo) {
                $scope.errorMessage = 'could not retrieve data - ' + errorInfo.statusText;
                $scope.errorMessageDetail = errorInfo.message + '\n' + errorInfo.messageDetail;
                console.log('got error: ', errorInfo.message, errorInfo.messageDetail);
            };
        };

        //
        // interface implementation
        //

        function getAttribute(categoryName, attributeShortName) {
            var catAttribs = $scope.attributesByCategory[categoryName];
            if (!catAttribs) return;

            for (var idx = 0; idx < catAttribs.length; ++idx) {
                var attrib = catAttribs[idx];
                if (attrib.name == attributeShortName) {
                    return attrib;
                }
            }
            // TODO: log error?
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

        function handleCharacterData(data) {
            console.log('got character data: ', data);

            // TODO: inject the actual rules service
            var _rules = {
                getAttribute: $scope.getAttribute,
                attributesByCategory: $scope.attributesByCategory
            };
            $scope.character = new Character(data, _rules);
        };

        function handleAttributesUpdated(newValue, oldValue) {
            var byCategory = $scope.attributesByCategory;
            for (var member in byCategory) delete byCategory[member];
            $.each(newValue, function (idx, attrib) {
                if (!byCategory[attrib.category]) {
                    byCategory[attrib.category] = [];
                }
                byCategory[attrib.category].push(attrib);
            });
        };

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
