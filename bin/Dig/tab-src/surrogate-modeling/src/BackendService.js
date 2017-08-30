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
      return Promise.resolve(['My Indep Var 1', 'My Indep Var 2', 'My Indep Var 3', 'My Indep Var 4']);
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
      return Promise.resolve(['My Dep Var 1', 'My Dep Var 2', 'My Dep Var 3']);
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
      return Promise.resolve([
        {
          varName: "CfgId",
          selected: "Some Configuration",
          available: ["My Configuration", "Some Configuration", "Another Configuration"]
        },
        {
          varName: "Stuff",
          selected: "Foo",
          available: ["Foo", "Bar", "Baz"]
        },
        {
          varName: "Something Else",
          selected: "AAA",
          available: ["AAA", "BBB", "CCC", "DDD"]
        },
        {
          varName: "Material",
          selected: "Stone",
          available: ["Steel", "Plastic", "Stone", "Potatoes"]
        }
      ]);
    }
  }
}

export default BackendService;
