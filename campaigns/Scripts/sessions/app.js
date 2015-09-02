(function () {
    'use strict';
    // http://www.joshgreenwald.com/journal/make-a-message-board-with-mvc5-and-signalr2

    //$.connection.boardHub.server.addComment(this.id, comment, vm.username())
    //        .done(function () {
    //            $('input[name="comment"]', context).val('');
    //        });

    //$.connection.boardHub.server.writePost(vm.username(), $('#message').val())
    //        .done(function () {
    //            $('#message').val('');
    //        });
    //});

    angular.module('app.messages', []);
    angular.module('app.messages').value('apiUrl', '/api/rules');
    angular.module('app.messages').factory('messages', ServiceFactory);

    ServiceFactory.$inject = ['$http', '$q'];
    function ServiceFactory($http, $q) {
        console.log('getting message data...');

        var hub = $.connection.sessionHub;
        $.connection.hub.logging = true;
        $.connection.hub.start().done(function () {
            console.log('hub started');
        });

        return {
            sendMessage: sendMessageHandler,
            setUserHandle: setUserHandleHandler,
            client: hub.client,
        };

        function sendMessageHandler(sessionId, messageText) {
            return $.connection.sessionHub.server.sendMessage(sessionId, messageText);
        };

        function setUserHandleHandler(userHandle) {
            return $.connection.sessionHub.server.setUserHandle(userHandle);
        };
    }

})();

(function () {
    'use strict';

    angular.module('app', ['app.messages']);
    angular.module('app').directive('ngEsc', EscDirective);
    angular.module('app').controller('SessionsCtrl', Controller);
    
    Controller.$inject = ['$scope', '$http', 'messages'];
    function Controller($scope, $http, messages) {
        $scope.init = initHandler;
        $scope.sessionId = 'session ID not set';
        $scope.sessionUserCount = 0;

        $scope.sender = 'Anonymous';
        $scope.newSender = '';

        $scope.editingSender = false;
        $scope.editSender = onEditSender;
        $scope.updateSender = onUpdateSender;
        $scope.cancelEditSender = onCancelEditSender;

        $scope.outgoingMessage = '';
        $scope.messages = [];

        $scope.formatDate = formatDate;

        $scope.sendMessage = sendMessageHandler;
        messages.client.onNewMessage = handleNewMessage;
        messages.client.onUsersUpdated = handleUsersUpdated;

        function initHandler(sessionId) {
            $scope.sessionId = sessionId;
        }

        function formatDate(date) {
            return moment(date).calendar();
        };

        function sendMessageHandler() {
            messages.sendMessage($scope.sessionId, $scope.outgoingMessage.trim())
                .done(function () {
                    $scope.$apply(function () {
                        $scope.outgoingMessage = '';
                    });
                });
        };

        function handleNewMessage(message) {
            $scope.$apply(function () {
                var front = $scope.messages.length > 0 ? $scope.messages[0] : {};
                if (front.SenderName === message.SenderName) {
                    var frontTime = moment(front.TimeStamp);
                    var messageTime = moment(message.TimeStamp);
                    var secondsDifference = moment.duration(messageTime.diff(frontTime)).seconds();

                    console.log(secondsDifference);
                    if (secondsDifference < 10) {
                        front.Text += '\n\n' + message.Text;
                        return;
                    }
                }
                $scope.messages.unshift(message);
            });
        };

        function handleUsersUpdated(userCount) {
            $scope.$apply(function () {
                $scope.sessionUserCount = userCount;
            });
        };

        function onEditSender() {
            $scope.newSender = $scope.sender;
            $scope.editingSender = true;
        };

        function onUpdateSender() {
            $scope.sender = $scope.newSender;
            $scope.editingSender = false;
            messages.setUserHandle($scope.sender);
        };

        function onCancelEditSender() {
            $scope.editingSender = false;
        };
    }

    function EscDirective() {
        return function (scope, element, attrs) {
            element.bind("keydown keypress", function (event) {
                if (event.which === 27) {
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEsc);
                    });

                    event.preventDefault();
                }
            });
        };
    }
})();