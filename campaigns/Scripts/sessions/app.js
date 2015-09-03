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

    angular.module('app.messageHub', []);
    angular.module('app.messageHub').value('apiUrl', '/api/rules');
    angular.module('app.messageHub').factory('messageHub', ServiceFactory);

    ServiceFactory.$inject = ['$http', '$q'];
    function ServiceFactory($http, $q) {
        console.log('starting hub...');

        var hub = $.connection.sessionHub;
        $.connection.hub.logging = true;
        var startDeferred = $.connection.hub.start();

        return {
            startDeferred: startDeferred,
            joinSession: joinSessionHandler,
            sendMessage: sendMessageHandler,
            setUserHandle: setUserHandleHandler,
            client: hub.client,
        };

        function joinSessionHandler(sessionId) {
            return $.connection.sessionHub.server.joinSession(sessionId);
        };

        function sendMessageHandler(messageText) {
            return $.connection.sessionHub.server.sendMessage(messageText);
        };

        function setUserHandleHandler(userHandle) {
            return $.connection.sessionHub.server.setUserHandle(userHandle);
        };
    }

})();

(function () {
    'use strict';

    angular.module('app', ['app.messageHub']);
    angular.module('app').directive('ngEsc', EscDirective);
    angular.module('app').controller('SessionsCtrl', Controller);
    
    Controller.$inject = ['$scope', '$http', 'messageHub', 'sessionId'];
    function Controller($scope, $http, messageHub, sessionId) {
        console.log('starting messages service...');

        $scope.isConnected = false;
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
        messageHub.client.onNewMessage = handleNewMessage;
        messageHub.client.onUsersUpdated = handleUsersUpdated;

        console.log('waiting for hub before joining session...');
        messageHub.startDeferred.done(function () {
            console.log('hub started. joining session...');
            messageHub.joinSession(sessionId).done(function () {
                console.log('joined session');

                $scope.$apply(function () {
                    $scope.isConnected = true;
                });
            });
        });

        function formatDate(date) {
            return moment(date).calendar();
        };

        function sendMessageHandler() {
            messageHub.sendMessage($scope.outgoingMessage.trim())
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
            messageHub.setUserHandle($scope.sender);
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