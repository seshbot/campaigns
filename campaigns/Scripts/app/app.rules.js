// http://www.bennadel.com/blog/2612-using-the-http-service-in-angularjs-to-make-ajax-requests.htm
(function () {
    'use strict';


    //
    // rules API
    //

    angular.module('app.rules', []);
    angular.module('app.rules').value('rulesApiUrl', '/api/rules');
    angular.module('app.rules').factory('rules', RulesFactory);

    RulesFactory.$inject = ['rulesApiUrl', '$http', '$q'];
    function RulesFactory(rulesApiUrl, $http, $q) {
        //console.log('getting rules data...');

        return {
            getAttributes: getAttributes,
            getCategoryAttributes: getCategoryAttributes,
            getContributions: getContributions,
        };

        // public interface

        function getAttributes() {
            var request = $http.get(rulesApiUrl + '/attributes');

            return request.then(handleSuccess, handleError);
        };

        function getCategoryAttributes(category) {
            var request = $http.get(rulesApiUrl + '/' + category);

            return request.then(handleSuccess, handleError);
        };

        function getContributions() {
            var request = $http.get(rulesApiUrl + '/contributions');

            return request.then(handleSuccess, handleError);
        };
    }


    //
    // characters API
    //

    angular.module('app.characters', []);
    angular.module('app.characters').value('charactersApiUrl', '/api/characters');
    angular.module('app.characters').factory('characters', CharactersFactory);

    CharactersFactory.$inject = ['charactersApiUrl', '$http', '$q'];
    function CharactersFactory(charactersApiUrl, $http, $q) {
        //console.log('getting character data...');

        return {
            getCharacter: getCharacter,
        };

        // public interface

        function getCharacter(id) {
            var request = $http.get(charactersApiUrl + '/' + id);

            return request.then(handleSuccess, handleError);
        };
    }


    //
    // helper functions
    //

    function handleSuccess(response) {
        //console.log('got rules data', response);

        return response.data;
    }

    function handleError(response) {
        console.log('error getting rules data', response);
        if (!angular.isObject(response.data) || !response.data.message) {
            $q.reject({
                statusMessage: response.statusText,
                errorMessage: 'An unknown error occurred',
                errorMessageDetail: ''
            });
        } else {
            $q.reject({
                statusText: response.statusText,
                message: response.data.message,
                messageDetail: response.data.messageDetail
            });
        }
    }
})();
