/*globals angular,Plotly,$ */
/*eslint-env es6*/
'use strict';

const app = angular.module('surrogateApp', ['ui.bootstrap']);

/**
 Wraps a request and response from Shiny in a Javascript-style promise.

 Queues requests if one is sent before the previous message is responded to,
 because Shiny will ignore requests if they come in too fast (and ignore all
 but the last one).
 */
app.service('shinyRequestService', ['$window', '$q', '$log', function($window, $q, $log) {
    const self = this;

    self.requestDeferreds = new Map();
    self.currentMessageId = 0;

    self.pendingRequestQueue = [];
    self.requestInProgress = false;

    $window.parent.Shiny.addCustomMessageHandler('angularResponse', function(message) {
        $log.info('Received reply from Shiny: ', message);

        if(self.requestDeferreds.has(message.id)) {
            const deferred = self.requestDeferreds.get(message.id);
            self.requestDeferreds.delete(message.id);

            deferred.resolve(message.data);

            if(self.pendingRequestQueue.length > 0) {
                const nextRequest = self.pendingRequestQueue.shift();
                performRequest(nextRequest);
            } else {
                self.requestInProgress = false;
            }
        } else {
            $log.error('Received unexpected reply from Shiny: ', message);
        }
    });

    function performRequest(request) {
        $window.parent.Shiny.onInputChange('SurrogateModeling-angularRequest', request);
    };

    self.makeShinyRequest = function(command, requestData) {
        const request = {
            id: self.currentMessageId,
            command: command,
            data: requestData
        };

        const requestDeferred = $q.defer();
        self.requestDeferreds.set(self.currentMessageId, requestDeferred);

        if(!self.requestInProgress) {
            self.requestInProgress = true;
            performRequest(request);
        } else {
            self.pendingRequestQueue.push(request);
        }

        self.currentMessageId++;

        return requestDeferred.promise;
    };
}]);

app.controller('surrogateListCtrl', ['$scope', '$log', '$document', '$window', '$uibModal', 'shinyRequestService', function($scope, $log, $document, $window, $uibModal, shinyRequestService) {
    $scope.test = 'Test Text';

    $scope.ivarsData = {};
    $scope.dvarsData = {};

    let messageId = 0;

    const $ctrl = this;
    $ctrl.items = ['item1', 'item2', 'item3'];

    $ctrl.animationsEnabled = true;

    $window.parent.Shiny.addCustomMessageHandler('textFieldChanged', function(message) {
        $scope.$apply(function() {
            $log.info('Received message: ', message);
            $scope.test = message;
        });
    });

    $window.parent.Shiny.addCustomMessageHandler('ivarsChanged', function(message) {
        $scope.$apply(function() {
            $log.info('Received ivars changed message: ', message);
            $scope.ivarsData = message;
            $window.parent.Shiny.onInputChange('SurrogateModeling-messageFromBrowser', {
                id: messageId,
                msg: 'Hello World'
            });

            shinyRequestService.makeShinyRequest('echo', 42).then(function(response) {
                $log.info('Successfully received response from Shiny: ', response);
            });
            shinyRequestService.makeShinyRequest('echo', 43).then(function(response) {
                $log.info('Successfully received response from Shiny: ', response);
            });
            shinyRequestService.makeShinyRequest('echo', 44).then(function(response) {
                $log.info('Successfully received response from Shiny: ', response);
            });
            shinyRequestService.makeShinyRequest('echo', 45).then(function(response) {
                $log.info('Successfully received response from Shiny: ', response);
            });
            $ctrl.open();
            messageId++;
        });
    });

    $window.parent.Shiny.addCustomMessageHandler('dvarsChanged', function(message) {
        $scope.$apply(function() {
            $log.info('Received dvars changed message: ', message);
            $scope.dvarsData = message;
        });
    });



    $ctrl.open = function(size, parentSelector) {
        const parentElem = parentSelector ?
            angular.element($document[0].querySelector('.modal-demo ' + parentSelector)) : undefined;
        const modalInstance = $uibModal.open({
            animation: $ctrl.animationsEnabled,
            ariaLabelledBy: 'modal-title',
            ariaDescribedBy: 'modal-body',
            templateUrl: 'myModalContent.html',
            controller: 'ModalInstanceCtrl',
            controllerAs: '$ctrl',
            size: size,
            appendTo: parentElem,
            resolve: {
                items: function() {
                    return $ctrl.items;
                }
            }
        });

        modalInstance.result.then(function(selectedItem) {
            $ctrl.selected = selectedItem;
        }, function() {
            $log.info('Modal dismissed at: ' + new Date());
        });
    };
}]);

app.controller('ModalInstanceCtrl', function($uibModalInstance, items) {
    const $ctrl = this;
    $ctrl.items = items;
    $ctrl.selected = {
        item: $ctrl.items[0]
    };

    $ctrl.ok = function() {
        $uibModalInstance.close($ctrl.selected.item);
    };

    $ctrl.cancel = function() {
        $uibModalInstance.dismiss('cancel');
    };
});
