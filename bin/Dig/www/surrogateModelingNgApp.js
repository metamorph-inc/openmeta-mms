/*globals angular,Plotly,$ */
/*eslint-env es6*/
'use strict';

const app = angular.module('surrogateApp', ['ui.bootstrap']);

app.controller('surrogateListCtrl', ['$scope', '$log', '$document', '$uibModal', function($scope, $log, $document, $uibModal) {
  $scope.test = "Test Text";
  
  $scope.ivarsData = {};
  $scope.dvarsData = {};
  
  let messageId = 0;
  
  var $ctrl = this;
  $ctrl.items = ['item1', 'item2', 'item3'];

  $ctrl.animationsEnabled = true;
  
  parent.Shiny.addCustomMessageHandler('textFieldChanged', function(message) {
    $scope.$apply(function() {
      $log.info("Received message: ", message);
      $scope.test = message;
    });
  });
  
  parent.Shiny.addCustomMessageHandler('ivarsChanged', function(message) {
    $scope.$apply(function() {
      $log.info("Received ivars changed message: ", message);
      $scope.ivarsData = message;
      parent.Shiny.onInputChange('SurrogateModeling-messageFromBrowser', {id: messageId, msg:'Hello World'});
      $ctrl.open();
      messageId++;
    });
  });
  
  parent.Shiny.addCustomMessageHandler('dvarsChanged', function(message) {
    $scope.$apply(function() {
      $log.info("Received dvars changed message: ", message);
      $scope.dvarsData = message;
    });
  });
  
  

  $ctrl.open = function (size, parentSelector) {
    var parentElem = parentSelector ? 
      angular.element($document[0].querySelector('.modal-demo ' + parentSelector)) : undefined;
    var modalInstance = $uibModal.open({
      animation: $ctrl.animationsEnabled,
      ariaLabelledBy: 'modal-title',
      ariaDescribedBy: 'modal-body',
      templateUrl: 'myModalContent.html',
      controller: 'ModalInstanceCtrl',
      controllerAs: '$ctrl',
      size: size,
      appendTo: parentElem,
      resolve: {
        items: function () {
          return $ctrl.items;
        }
      }
    });

    modalInstance.result.then(function (selectedItem) {
      $ctrl.selected = selectedItem;
    }, function () {
      $log.info('Modal dismissed at: ' + new Date());
    });
  };
}]);

app.controller('ModalInstanceCtrl', function ($uibModalInstance, items) {
  var $ctrl = this;
  $ctrl.items = items;
  $ctrl.selected = {
    item: $ctrl.items[0]
  };

  $ctrl.ok = function () {
    $uibModalInstance.close($ctrl.selected.item);
  };

  $ctrl.cancel = function () {
    $uibModalInstance.dismiss('cancel');
  };
});