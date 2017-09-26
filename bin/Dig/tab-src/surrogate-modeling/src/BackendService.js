import StaticData, { ExampleData } from './StaticData';
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

      window.parent.Shiny.addCustomMessageHandler('externalError', (message) => {
        console.info('Received reply from Shiny', message);

        if(this.requestPromiseCallbacks.has(message.id)) {
          const callbacks = this.requestPromiseCallbacks.get(message.id);
          this.requestPromiseCallbacks.delete(message.id);

          callbacks.reject(message.data);

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
      }).then((discreteVarList) => {
        // Workaround for visualizer bug where single-element list doesn't get
        // serialized as a list
        discreteVarList.forEach((discreteVarObj) => {
          if(!Array.isArray(discreteVarObj.available)) {
            discreteVarObj.available = [discreteVarObj.available];
          }
        });

        return discreteVarList;
      });
    } else {
      return Promise.resolve(ExampleData.discreteIndependentVars);
    }
  }

  evaluateSurrogateAtPoints(independentVars, discreteVars, surrogateModelName) {
    console.log(discreteVars);

    if(this.hasShiny) {
      const requestArgs = {
        independentVars: independentVars,
        discreteVars: discreteVars,
        surrogateModel: surrogateModelName
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

  getSurrogateGraphData(independentVars, discreteVars, selectedIndependentVarIndex, surrogateModelName) {
    if(this.hasShiny) {
      const requestArgs = {
        independentVars: independentVars,
        discreteVars: discreteVars,
        selectedIndependentVarIndex: selectedIndependentVarIndex,
        surrogateModel: surrogateModelName
      };

      return this.makeShinyRequest('getGraph', requestArgs).then((result) => {
        return result;
      });
    } else {
      const result = {
        xAxisPoints: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
        yAxisPoints: [[1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
                      [1, 1.5, 3, 3.5, 5, 5.5, 7, 7.5, 9, 9.5]],
        yAxisErrors:  [[1, 2, 1, 2, 1, 2, 1, 2, 1, 2],
                      [2, 1.8, 1.6, 1.4, 1.2, 1.0, 0.8, 0.6, 0.4, 0.2]]
      };

      return new Promise((resolve, reject) => {
        window.setTimeout(() => resolve(result), 1000);
      });
    }
  }

  pushIndependentVarState(newIndependentVars) {
    if(this.hasShiny) {
      // We don't need a response from this, and we'd like to take advantage of
      // Shiny's ability to rate-limit/batch these updates, so we directly call
      // onInputChange here rather than using makeShinyRequest
      // SHINY HACK: Shiny flattens the 2D array we send if we send it directly,
      // but doesn't if we wrap it in an object first
      window.parent.Shiny.onInputChange('SurrogateModeling-independentVarState', {ivars: newIndependentVars});
    } else {
      console.info("Would have pushed new independent var state to Shiny, but not connected");
    }
  }

  pushDiscreteVarState(newDiscreteVars) {
    if(this.hasShiny) {
      window.parent.Shiny.onInputChange('SurrogateModeling-discreteVarState', {dvars: newDiscreteVars});
    } else {
      console.info("Would have pushed new discrete var state to Shiny, but not connected");
    }
  }

  pushDisplaySettingsState(newDisplaySettings) {
    if(this.hasShiny) {
      window.parent.Shiny.onInputChange('SurrogateModeling-displaySettingsState', {settings: newDisplaySettings});
    } else {
      console.info("Would have pushed new display settings state to Shiny, but not connected");
    }
  }

  pushSurrogateModelState(newSurrogateModel) {
    if(this.hasShiny) {
      window.parent.Shiny.onInputChange('SurrogateModeling-surrogateModelState', {selectedSurrogateModel: newSurrogateModel});
    } else {
      console.info("Would have pushed new surrogate model state to Shiny, but not connected");
    }
  }

  getIndependentVarState() {
    if(this.hasShiny) {
      return this.makeShinyRequest('getIndependentVarState', '').then((result) => {
        // Because Shiny serializes empty lists as null
        if(result === null) {
          return [];
        } else {
          if(result.ivars === null) {
            return [];
          } else {
            return result.ivars;
          }
        }
      });
    } else {
      return Promise.resolve(ExampleData.independentVarData);
    }
  }

  getDisplaySettingsState() {
    if(this.hasShiny) {
      return this.makeShinyRequest('getDisplaySettingsState', '').then((result) => {
        // Because Shiny serializes empty lists as null
        if(result === null) {
          return StaticData.displaySettings;
        } else {
          if(result.settings === null) {
            return StaticData.displaySettings;
          } else {
            return result.settings;
          }
        }
      });
    } else {
      return Promise.resolve(StaticData.displaySettings);
    }
  }

  getSurrogateModelState() {
    if(this.hasShiny) {
      return this.makeShinyRequest('getSurrogateModelState', '').then((result) => {
        // Because Shiny serializes empty lists as null
        if(result === null) {
          return StaticData.selectedSurrogateModel;
        } else {
          if(result.selectedSurrogateModel === null) {
            return StaticData.selectedSurrogateModel;
          } else {
            return result.selectedSurrogateModel;
          }
        }
      });
    } else {
      return Promise.resolve(StaticData.selectedSurrogateModel);
    }
  }
}




export default BackendService;
