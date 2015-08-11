angular.module('CharSheetApp', [])
    .controller('CharGenCtrl', function ($scope, $http) {
        $scope.testThing = false;
        $scope.toggleTest = function () { $scope.testThing = !$scope.testThing; };
        $scope.loading = false;

        //$scope.getThing = function () {
        //  $http.get("/api/chargen").success(function (data, status, headers, config) {
        //      $scope.loading = false;
        //  }).error(function (data, status, headers, config) {
        //      $scope.loading = false;
        //  });
        //};

        //$scope.setThing = function (object) {
        //    $http.post('/api/chargen', { 'prop1': option.prop1, 'prop2': option.prop2 }).success(function (data, status, headers, config) {
        //        $scope.correctAnswer = (Boolean(data) === true);
        //        $scope.loading = false;
        //    }).error(function (data, status, headers, config) {
        //        $scope.loading = false;
        //    });
        //};
    });