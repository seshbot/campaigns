(function () {
    'use strict';
    // http://www.joshgreenwald.com/journal/make-a-message-board-with-mvc5-and-signalr2

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
            sendDiceRoll: sendDiceRollHandler,
            setUserHandle: setUserHandleHandler,
            client: hub.client,
            getConnectionId: getConnectionIdHandler,
        };

        function getConnectionIdHandler() {
            return $.connection.hub.id;
        };

        function joinSessionHandler(sessionId) {
            var cid = $.connection.hub.id;
            return $.connection.sessionHub.server.joinSession(sessionId);
        };

        function sendMessageHandler(messageText) {
            return $.connection.sessionHub.server.sendMessage(messageText);
        };

        function sendDiceRollHandler(rollFormula) {
            return $.connection.sessionHub.server.sendDiceRoll(rollFormula);
        };

        function setUserHandleHandler(userHandle) {
            return $.connection.sessionHub.server.setUserHandle(userHandle);
        };
    }

})();

(function () {
    'use strict';

    angular.module('app', ['app.messageHub']);
    angular.module('app').filter("sanitize", SanitizeFilter);
    angular.module('app').directive('ngEsc', EscDirective);
    angular.module('app').controller('SessionsCtrl', Controller);

    Controller.$inject = ['$scope', '$http', 'messageHub', 'sessionId'];
    function Controller($scope, $http, messageHub, sessionId) {
        console.log('starting messages service...');

        $scope.isConnected = false;
        $scope.connectionId = '';
        $scope.sessionUserCount = 0;

        $scope.sender = 'Anonymous';
        $scope.newSender = '';

        $scope.editingSender = false;
        $scope.editSender = onEditSender;
        $scope.updateSender = onUpdateSender;
        $scope.cancelEditSender = onCancelEditSender;

        $scope.outgoingMessage = '';
        $scope.outgoingMessageIsRollFormula = isOutgoingMessageRollFormula;
        $scope.isEditingRollFormula = false;
        $scope.editRollFormula = onEditRollFormula;
        $scope.addDieToFormula = onAddDieToFormula;

        $scope.messages = [];

        $scope.formatDate = formatDate;

        $scope.sendMessage = sendMessageHandler;
        messageHub.client.onNewMessage = handleNewMessage;
        messageHub.client.onNewDiceRoll = handleNewMessage;
        messageHub.client.onUsersUpdated = handleUsersUpdated;

        console.log('waiting for hub before joining session...');
        messageHub.startDeferred.done(function () {
            console.log('hub started. joining session...');
            messageHub.joinSession(sessionId).done(function () {
                console.log('joined session');

                $scope.$apply(function () {
                    $scope.connectionId = messageHub.getConnectionId();
                    $scope.isConnected = true;
                });
            });
        });

        function isOutgoingMessageRollFormula() {
            var formula = $scope.outgoingMessage.trim();
            if (formula.substr(0, '/r'.length) === '/r') return true;
            if (formula.substr(0, '='.length) === '=') return true;
            return false;
        };

        function getOutgoingMessageRollFormula() {
            if (!isOutgoingMessageRollFormula()) return '';
            var re = /^(\/r|=)[ ]*/;
            return $scope.outgoingMessage.trim().replace(re, '');
        }

        function onEditRollFormula() {
            $scope.isEditingRollFormula = !$scope.isEditingRollFormula;
        };

        function onAddDieToFormula(die, count) {
            var newOutgoingMessage = $scope.outgoingMessage;
            if (!isOutgoingMessageRollFormula()) {
                newOutgoingMessage = '/r';
            }

            var formula = getOutgoingMessageRollFormula().trim();
            if (formula.length > 0) {
                newOutgoingMessage = newOutgoingMessage.trimRight() + ' +';
            }

            newOutgoingMessage = newOutgoingMessage.trimRight() + ' ' + count + die;

            $scope.outgoingMessage = newOutgoingMessage;
        };

        function formatDate(date) {
            return moment(date).calendar();
        };

        function sendMessageHandler() {
            if (isOutgoingMessageRollFormula()) {
                messageHub.sendDiceRoll(getOutgoingMessageRollFormula())
                    .done(function () {
                        $scope.$apply(function () {
                            $scope.outgoingMessage = '';
                            $scope.isEditingRollFormula = false;
                        });
                    });
            }
            else {
                messageHub.sendMessage($scope.outgoingMessage.trim())
                    .done(function () {
                        $scope.$apply(function () {
                            $scope.outgoingMessage = '';
                            $scope.isEditingRollFormula = false;
                        });
                    });
            }
        };

        function MessageBlock(senderName, senderId, timeStamp) {
            this.senderName = senderName;
            this.senderConnectionId = senderId;
            this.isMyMessage = senderId === $scope.connectionId;
            this.timeStamp = timeStamp;
            this.messages = [];
            this.text = '';
        }

        MessageBlock.prototype._tryAddText = function (text) {
            var self = this;

            if (self.text.trim() !== '') {
                self.text += '<br />';
            }

            self.text += text;
        }

        MessageBlock.prototype.addMessage = function (message) {
            var self = this;
            self.messages.push(message);
            self.timeStamp = message.TimeStamp;

            if (typeof (message.Text) !== 'undefined') {
                self._tryAddText(message.Text);
            }

            if (typeof (message.RollFormula) !== 'undefined') {
                // the formula part
                var partFormula = '<span class="text-muted">';
                for (var idx = 0, len = message.RollDiceGroupRolls.length; idx < len; idx++) {
                    if (idx > 0) partFormula += ' + ';
                    var group = message.RollDiceGroupRolls[idx];
                    partFormula += group.DiceGroupDiceCount + 'd' + group.DiceGroupDiceSides;
                }
                partFormula += '</span>';

                // the group results part
                var partGroupResults = '<span class="pull-right">';
                for (var idx = 0, len = message.RollDiceGroupRolls.length; idx < len; idx++) {
                    if (idx > 0) partGroupResults += ' + ';
                    var group = message.RollDiceGroupRolls[idx];
                    partGroupResults += '(';
                    partGroupResults += group.Results.join('+') + '=' + group.Total;
                    partGroupResults += ')';
                }
                partGroupResults += '</span>';
                
                // the grand total part
                var partTotal = '<span><strong>' + message.RollTotal + '</strong></span>';

                self._tryAddText(partFormula + ' = ' + partTotal + partGroupResults);
            }
        }

        function handleNewMessage(message) {
            $scope.$apply(function () {
                var messageBlock = getOrAddMessageBlock();
                messageBlock.addMessage(message);

                function areCloseInTime(firstMessage, secondMessage) {                    
                    var firstTime = moment(firstMessage);
                    var secondTime = moment(secondMessage);
                    var secondsDifference = moment.duration(secondTime.diff(firstTime)).seconds();

                    return secondsDifference <= 10;
                }

                function getOrAddMessageBlock() {
                    if ($scope.messages.length > 0) {
                        var front = $scope.messages[0];
                        if (front.senderConnectionId === message.SenderConnectionId && areCloseInTime(front.timeStamp, message.TimeStamp)) {
                            return front;
                        }
                    }
                    var newMessageBlock = new MessageBlock(message.SenderName, message.SenderConnectionId, message.TimeStamp);
                    $scope.messages.unshift(newMessageBlock);
                    return newMessageBlock;
                }
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

    SanitizeFilter.$inject = ['$sce'];
    function SanitizeFilter($sce) {
        return function (htmlCode) {
            return $sce.trustAsHtml(htmlCode);
        }
    };

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