library(DiceKriging)
library(randomForest)


title <- "Surrogate Modeling"
footer <- TRUE



ui <- function(id) {
  ns <- NS(id)

  fluidPage(
    br(),
    tags$head(tags$link(rel = "stylesheet", type = "text/css", href = "surrogateModelingStyle.css")),
    tags$head(tags$script(src="third_party/iframeResizer.min.js")),
    tags$head(tags$style("iframe { width: 100%; }")),
    tags$iframe(src="surrogateModeling/index.html"),
    tags$script("iFrameResize({log:false, heightCalculationMethod: 'lowestElement'});")
  )
}

server <- function(input, output, session, data) {
  ns <- session$ns

  observeEvent(input$externalRequest, {
    tryCatch({
      if(!is.null(input$externalRequest) && input$externalRequest != "") {
        print("Received request from browser")
        print(input$externalRequest)
        
        if(input$externalRequest$command == "echo") {
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=input$externalRequest$data
          ))
        } else if(input$externalRequest$command == "listIndependentVars") {
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=data$pre$var_range_nums_and_ints_list()[['Design Variable']]
          ))
        } else if(input$externalRequest$command == "listDependentVars") {
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=data$pre$var_range_nums_and_ints_list()[['Objective']]
          ))
        } else if(input$externalRequest$command == "getDiscreteVarInfo") {
          configs = data$meta$pet$selected_configurations
          configIdObject = list(varName="CfgID", selected=configs[[1]], available=configs)
          discreteVarsList = lapply(data$pre$var_range_facs_list()[["Design Variable"]], function(name) {
            options = names(table(raw[[name]]))
            newVar = list(varName=name, selected=options[[1]], available=options)
            return(newVar)
          })
          
          discreteVarsList = c(list(configIdObject), discreteVarsList)
          
          savedDiscreteVarsList = input$discreteVarState$dvars
          if(is.null(savedDiscreteVarsList)) {
            savedDiscreteVarsList = si(ns("discreteVarState"), NULL)$dvars
          }
          
          if(!is.null(savedDiscreteVarsList) && length(savedDiscreteVarsList) == length(discreteVarsList)) {
            for(i in 1:length(savedDiscreteVarsList)) {
              if((savedDiscreteVarsList[[i]]$varName == discreteVarsList[[i]]$varName) && is.element(savedDiscreteVarsList[[i]]$selected, discreteVarsList[[i]]$available)) {
                discreteVarsList[[i]]$selected = savedDiscreteVarsList[[i]]$selected
              } else {
                print("Warning: mismatch between saved discrete vars list and dataset")
                break
              }
            }
          }
          
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=discreteVarsList
          ))
        } else if(input$externalRequest$command == "getIndependentVarState") {
          result = input$independentVarState
          if(is.null(result)) {
            result = si(ns("independentVarState"), NULL)
          }
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=result
          ))
        } else if(input$externalRequest$command == "getDisplaySettingsState") {
          result = input$displaySettingsState
          if(is.null(result)) {
            result = si(ns("displaySettingsState"), NULL)
          }
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=result
          ))
        } else if(input$externalRequest$command == "getSurrogateModelState") {
          result = input$surrogateModelState
          if(is.null(result)) {
            result = si(ns("surrogateModelState"), NULL)
          }
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=result
          ))
        } else if(input$externalRequest$command == "evaluateSurrogateAtPoints") {
          result = evaluateSurrogate(input$externalRequest$data$independentVars,
                                     input$externalRequest$data$discreteVars,
                                     input$externalRequest$data$surrogateModel)
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=result
          ))
        } else if(input$externalRequest$command == "trainSurrogateAtPoints") {
          result = trainSurrogate(input$externalRequest$data$independentVars,
                                     input$externalRequest$data$discreteVars,
                                     input$externalRequest$data$surrogateModel)
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=result
          ))
        } else if(input$externalRequest$command == "getGraph") {
          result = getGraph(input$externalRequest$data$independentVars,
                            input$externalRequest$data$discreteVars,
                            input$externalRequest$data$selectedIndependentVar,
                            input$externalRequest$data$surrogateModel)
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=result
          ))
        }
      }
    },
    error = function(e) {
      print("Error occurred")
      replyData = list(
        message = e$message
      )
      session$sendCustomMessage(type="externalError", list(
        id=input$externalRequest$id,
        data=replyData
      ))
    })
    
  })
  
  trainSurrogate <- function(indepVars, discreteVars, surrogateModel) {
    if(!is.null(data$meta$pet$pet_config_filename)) {
      results_directory <- dirname(pet_config_filename)
      project_directory <- dirname(results_directory)
      
      if("metadata.json" %in% dir(results_directory)) {
        
        ivarDf = data.frame(matrix(unlist(indepVars), nrow=length(indepVars), byrow=T))
        names(ivarDf) = unlist(data$pre$var_range_nums_and_ints_list()[['Design Variable']])
        
        config_id = NULL
        
        for(discreteVar in discreteVars) {
          if(discreteVar$varName == "CfgID") {
            config_id = discreteVar$selected
          } else {
            ivarDf[discreteVar$varName] = rep(discreteVar$selected, nrow(ivarDf))
          }
        }
        
        surrogateCsvPath = paste(results_directory, "surrogate-model-reexec.csv", sep="\\")
        
        write.csv(ivarDf, file = surrogateCsvPath, row.names=FALSE, quote=FALSE)
        
        result = system2("..\\Python27\\Scripts\\python.exe",
                args = c("..\\RunMergedPetAtPoints.py",
                         shQuote(results_directory),
                         shQuote(config_id),
                         shQuote(surrogateCsvPath)),
                stdout = file.path(results_directory, "RunMergedPetAtPoints_stdout.log"),
                stderr = file.path(results_directory, "RunMergedPetAtPoints_stderr.log"),
                wait = TRUE)
        
        if(result == 0) {
          result = list(
            success = TRUE,
            message = ""
          )
          
          return(result)
        } else {
          stop("PET re-execution failed--  see RunMergedPetAtPoints_stdout.log for more details.")
        }
      } else {
        stop("Results Browser metadata.json required to add training points.")
      }
    } else {
      stop("Training must be done on a merged PET.")
    }
  }
  
  getGraph <- function(indepVars, discreteVars, selectedIndependentVar, surrogateModel) {
    GRAPH_RESOLUTION = 200
    
    trainingData = data$Filtered()
    for(discreteVar in discreteVars) {
      trainingData = trainingData[trainingData[[discreteVar$varName]] == discreteVar$selected, ]
    }
    trainingDataIndep = subset(trainingData, select=unlist(data$pre$var_range_nums_and_ints_list()[['Design Variable']]))
    trainingDataDep = subset(trainingData, select=unlist(data$pre$var_range_nums_and_ints_list()[['Objective']]))
    
    ivarDf = data.frame(indepVars)
    
    names(ivarDf) = unlist(data$pre$var_range_nums_and_ints_list()[['Design Variable']])
    
    expanded = as.data.frame(lapply(ivarDf, rep, GRAPH_RESOLUTION))
    # The below is supposed to work, but doesn't when there's only one column
    # expanded = ivarDf[rep(seq_len(nrow(ivarDf)), each=GRAPH_RESOLUTION),]
    
    expanded[, selectedIndependentVar] = seq(from=min(trainingData[, selectedIndependentVar]), to=max(trainingData[, selectedIndependentVar]), length.out=200)

    # Need at least two training points (I think?)
    if(nrow(trainingDataIndep) < (ncol(trainingDataIndep) + ncol(trainingDataDep))) {
      stop("Insufficient training data; relax filters or run PET with more points.")
    }
    
    yPointsArray = array(rep(0, nrow(expanded) * ncol(trainingDataDep)), c(ncol(trainingDataDep), nrow(expanded)))
    yErrorsArray = array(rep(0, nrow(expanded) * ncol(trainingDataDep)), c(ncol(trainingDataDep), nrow(expanded)))
    
    for(colIndex in 1:ncol(trainingDataDep)) {
      if(surrogateModel == "Kriging Surrogate") {
        model = km(design=trainingDataIndep, response=trainingDataDep[, colIndex])
        predictResults = predict(model, expanded, type='SK')
  
        yPointsArray[colIndex, ] = predictResults$mean
        yErrorsArray[colIndex, ] = predictResults$sd
      } else if(surrogateModel == "Random Forest") {
        model = randomForest(x=trainingDataIndep, y=trainingDataDep[, colIndex])
        predictResults = predict(model, expanded)
        
        yPointsArray[colIndex, ] = predictResults
      } else {
        stop("Unknown surrogate selected")
      }
    }
    
    results = list(
      xAxisPoints = expanded[, selectedIndependentVar],
      yAxisPoints = yPointsArray,
      yAxisErrors = yErrorsArray
    )
    
    return(results)
  }
  
  evaluateSurrogate <- function(indepVars, discreteVars, surrogateModel) {
    trainingData = data$Filtered()
    for(discreteVar in discreteVars) {
      trainingData = trainingData[trainingData[[discreteVar$varName]] == discreteVar$selected, ]
    }
    trainingDataIndep = subset(trainingData, select=unlist(data$pre$var_range_nums_and_ints_list()[['Design Variable']]))
    trainingDataDep = subset(trainingData, select=unlist(data$pre$var_range_nums_and_ints_list()[['Objective']]))
    
    ivarDf = data.frame(indepVars)
    names(ivarDf) = unlist(data$pre$var_range_nums_and_ints_list()[['Design Variable']])
    
    resultArray = NULL
    
    if(surrogateModel == "Kriging Surrogate") {
      resultArray = array(rep(0, nrow(ivarDf) * ncol(trainingDataDep) * 3), c(nrow(ivarDf), ncol(trainingDataDep), 3))
    } else if(surrogateModel == "Random Forest") {
      resultArray = array(rep(0, nrow(ivarDf) * ncol(trainingDataDep) * 2), c(nrow(ivarDf), ncol(trainingDataDep), 2))
    }
    
    # Need at least two training points (I think?)
    if(nrow(trainingDataIndep) < (ncol(trainingDataIndep) + ncol(trainingDataDep))) {
      stop("Insufficient training data; relax filters or run PET with more points.")
    }
    
    for(colIndex in 1:ncol(trainingDataDep)) {
      if(surrogateModel == "Kriging Surrogate") {
        model = km(design=trainingDataIndep, response=trainingDataDep[, colIndex])
        predictResults = predict(model, ivarDf, type='SK')
        
        resultArray[, colIndex, 1] = rep(2, nrow(ivarDf)) # COMPUTED from DependentVarState enum
        resultArray[, colIndex, 2] = predictResults$mean
        resultArray[, colIndex, 3] = predictResults$sd
      } else if(surrogateModel == "Random Forest") {
        model = randomForest(x=trainingDataIndep, y=trainingDataDep[, colIndex])
        predictResults = predict(model, ivarDf)
        
        resultArray[, colIndex, 1] = rep(2, nrow(ivarDf)) # COMPUTED from DependentVarState enum
        resultArray[, colIndex, 2] = predictResults
      } else {
        stop("Unknown surrogate selected")
      }
    }
    
    return(resultArray)
  }
}
