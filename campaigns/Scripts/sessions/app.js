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

    Controller.$inject = ['$scope', '$http', 'messageHub', 'sessionId', 'diceUpdateCallback'];
    function Controller($scope, $http, messageHub, sessionId, diceUpdateCallback) {
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

        $scope.messageBlocks = [];

        $scope.formatDate = formatDate;

        $scope.sendMessage = sendMessageHandler;
        $scope.saveMessage = saveMessageHandler;
        $scope.clearMessage = clearMessageHandler;

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
            diceUpdateCallback();
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

            diceUpdateCallback();
        };

        function saveMessageHandler() {
            $scope.outgoingMessage = '';
            $scope.isEditingRollFormula = false;

            diceUpdateCallback();
        };

        function clearMessageHandler() {
            $scope.outgoingMessage = '';
            $scope.isEditingRollFormula = false;

            diceUpdateCallback();
        };

        function MessageBlock(senderName, senderId, timeStamp) {
            this.senderName = senderName;
            this.senderConnectionId = senderId;
            this.isMine = senderId === $scope.connectionId;
            this.earliestTimeStamp = timeStamp;
            this.latestTimeStamp = timeStamp;
            this.messages = [];
        }

        MessageBlock.prototype.addMessage = function (message) {
            var self = this;
            self.messages.push(message);
            self.latestTimeStamp = message.TimeStamp;

            message.hasTextMessage = typeof (message.Text) !== 'undefined';
            message.hasRoll = typeof (message.RollFormula) !== 'undefined';

            if (message.hasRoll) {
                // the formula part
                var partFormula = '';
                for (var idx = 0, len = message.RollDiceGroupRolls.length; idx < len; idx++) {
                    if (idx > 0) partFormula += ' + ';
                    var group = message.RollDiceGroupRolls[idx];
                    if (1 == group.DiceGroupDiceSides) {
                        partFormula += group.DiceGroupDiceCount;
                    } else {
                        partFormula += group.DiceGroupDiceCount + 'd' + group.DiceGroupDiceSides;
                    }
                }
                message.formulaText = partFormula;

                // the group results part
                var partGroupResults = '';
                for (var idx = 0, len = message.RollDiceGroupRolls.length; idx < len; idx++) {
                    if (idx > 0) partGroupResults += ' + ';
                    var group = message.RollDiceGroupRolls[idx];
                    if (1 == group.DiceGroupDiceSides) {
                        partGroupResults += group.Total;
                    } else {
                        partGroupResults += '(';
                        partGroupResults += group.Results.join('+') + '=' + group.Total;
                        partGroupResults += ')';
                    }
                }
                message.groupResultsText = partGroupResults;
                
                // the grand total part
                message.totalText = message.RollTotal;
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
                    if ($scope.messageBlocks.length > 0) {
                        var front = $scope.messageBlocks[0];
                        var sameSender = front.senderConnectionId === message.SenderConnectionId;
                        if (sameSender && areCloseInTime(front.latestTimeStamp, message.TimeStamp)) {
                            return front;
                        }
                    }
                    var newMessageBlock = new MessageBlock(message.SenderName, message.SenderConnectionId, message.TimeStamp);
                    $scope.messageBlocks.unshift(newMessageBlock);
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