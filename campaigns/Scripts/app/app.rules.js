// http://www.bennadel.com/blog/2612-using-the-http-service-in-angularjs-to-make-ajax-requests.htm
(function () {
    'use strict';

    angular.module('app.rules', []);
    angular.module('app.rules').value('apiUrl', '/api/rules/root');
    angular.module('app.rules').factory('rules', RulesFactory);

    RulesFactory.$inject = ['apiUrl', '$http', '$q'];
    function RulesFactory(apiUrl, $http, $q) {
        console.log('getting rules data...');

        return {
            getRules: getRules,
        };

        // public interface

        function getRules() {
            var request = $http.get(apiUrl);

            return request.then(handleSuccess, handleError);
        };

        // private data

        function handleSuccess(response) {
            console.log('got rules data', response);

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
    }
})();
