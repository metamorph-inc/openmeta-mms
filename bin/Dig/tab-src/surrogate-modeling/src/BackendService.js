import { ExampleData } from './StaticData';
import { DependentVarState } from './Enums';

class BackendService {
  constructor() {
    this.hasShiny = (window.parent.Shiny !== undefined);

    this.requestPromiseCallbacks = new Map();
    this.currentMessageId = 0;

    this.pendingRequestQueue = [];
    this.requestInProgress = false;

    if(this.hasShiny) { // Don't register Shiny callbacks if we're not in a frame
      window.parent.Shiny.addCustomMessageHandler('externalResponse', (message) => {
        console.info('Received reply from Shiny', message);

        if(this.requestPromiseCallbacks.has(message.id)) {
          const callbacks = this.requestPromiseCallbacks.get(message.id);
          this.requestPromiseCallbacks.delete(message.id);

          callbacks.resolve(message.data);

          if(this.pendingRequestQueue.length > 0) {
            const nextRequest = this.pendingRequestQueue.shift();
            this.performRequest(nextRequest);
          } else {
              this.requestInProgress = false;
          }
        } else {
          console.error('Received unexpected reply from Shiny: ', message);
        }
      });
    }
  }

  performRequest(request) {
    window.parent.Shiny.onInputChange('SurrogateModeling-externalRequest', request);
  }

  makeShinyRequest(command, requestData) {
    const request = {
      id: this.currentMessageId,
      command: command,
      data: requestData
    };

    const promise = new Promise((resolve, reject) => {
      const callbacks = { resolve: resolve, reject: reject };
      this.requestPromiseCallbacks.set(this.currentMessageId, callbacks);

      if(!this.requestInProgress) {
        this.requestInProgress = true;
        this.performRequest(request);
      } else {
        this.pendingRequestQueue.push(request);
      }

      this.currentMessageId++;
    });

    return promise;
  }

  getIndependentVarNames() {
    if(this.hasShiny) {
      return this.makeShinyRequest('listIndependentVars', '').then((result) => {
        // Because Shiny serializes empty lists as null
        if(result === null) {
          return [];
        } else {
          return result;
        }
      });
    } else {
      return Promise.resolve(ExampleData.independentVarNames);
    }
  }

  getDependentVarNames() {
    if(this.hasShiny) {
      return this.makeShinyRequest('listDependentVars', '').then((result) => {
        // Because Shiny serializes empty lists as null
        if(result === null) {
          return [];
        } else {
          return result;
        }
      });
    } else {
      return Promise.resolve(ExampleData.dependentVarNames);
    }
  }

  getDiscreteIndependentVars() {
    if(this.hasShiny) {
      return this.makeShinyRequest('getDiscreteVarInfo', '').then((result) => {
        // Because Shiny serializes empty lists as null
        if(result === null) {
          return [];
        } else {
          return result;
        }
      });
    } else {
      return Promise.resolve(ExampleData.discreteIndependentVars);
    }
  }

  evaluateSurrogateAtPoints(independentVars, discreteVars) {
    console.log(discreteVars);

    if(this.hasShiny) {
      const requestArgs = {
        independentVars: independentVars,
        discreteVars: discreteVars
      };

      return this.makeShinyRequest('evaluateSurrogateAtPoints', requestArgs).then((result) => {
        // Because Shiny serializes empty lists as null
        if(result === null) {
          return [];
        } else {
          return result;
        }
      });
    } else {
      console.log(independentVars);
      const dependentVarsLength = ExampleData.dependentVarNames.length;

      const resultArray = Array(independentVars.length);

      for(let i = 0; i < resultArray.length; i++) {
        resultArray[i] = Array(dependentVarsLength);
        for(let j = 0; j < resultArray[i].length; j++) {
          resultArray[i][j] = [DependentVarState.COMPUTED, 1.0, 1.0];
        }
      }

      console.log(resultArray);

      return new Promise((resolve, reject) => {
        window.setTimeout(() => resolve(resultArray), 1000);
      });
    }
  }
}

export default BackendService;
