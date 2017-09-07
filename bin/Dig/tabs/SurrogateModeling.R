library(DiceKriging)

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
        } else if(input$externalRequest$command == "evaluateSurrogateAtPoints") {
          result = evaluateSurrogate(input$externalRequest$data$independentVars,
                                     input$externalRequest$data$discreteVars)
          session$sendCustomMessage(type="externalResponse", list(
            id=input$externalRequest$id,
            data=result
          ))
        } else if(input$externalRequest$command == "getGraph") {
          result = getGraph(input$externalRequest$data$independentVars,
                            input$externalRequest$data$discreteVars,
                            input$externalRequest$data$selectedIndependentVar)
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
  
  getGraph <- function(indepVars, discreteVars, selectedIndependentVar) {
    GRAPH_RESOLUTION = 200
    
    trainingData = data$Filtered()
    for(discreteVar in discreteVars) {
      trainingData = trainingData[trainingData[[discreteVar$varName]] == discreteVar$selected, ]
    }
    trainingDataIndep = trainingData[, unlist(data$pre$var_range_nums_and_ints_list()[['Design Variable']])]
    trainingDataDep = trainingData[, unlist(data$pre$var_range_nums_and_ints_list()[['Objective']])]
    
    ivarDf = data.frame(indepVars)
    
    names(ivarDf) = unlist(data$pre$var_range_nums_and_ints_list()[['Design Variable']])
    
    expanded = ivarDf[rep(seq_len(nrow(ivarDf)), each=GRAPH_RESOLUTION),]
    
    expanded[, selectedIndependentVar] = seq(from=min(trainingData[, selectedIndependentVar]), to=max(trainingData[, selectedIndependentVar]), length.out=200)

    # Need at least two training points (I think?)
    if(nrow(trainingDataIndep) < (ncol(trainingDataIndep) + ncol(trainingDataDep))) {
      stop("Insufficient training data; relax filters or run PET with more points.")
    }
    
    yPointsArray = array(rep(0, nrow(expanded) * ncol(trainingDataDep)), c(ncol(trainingDataDep), nrow(expanded)))
    yErrorsArray = array(rep(0, nrow(expanded) * ncol(trainingDataDep)), c(ncol(trainingDataDep), nrow(expanded)))
    
    for(colIndex in 1:ncol(trainingDataDep)) {
      model = km(design=trainingDataIndep, response=trainingDataDep[, colIndex])
      predictResults = predict(model, expanded, type='SK')

      yPointsArray[colIndex, ] = predictResults$mean
      yErrorsArray[colIndex, ] = predictResults$sd
    }
    
    results = list(
      xAxisPoints = expanded[, selectedIndependentVar],
      yAxisPoints = yPointsArray,
      yAxisErrors = yErrorsArray
    )
    
    return(results)
  }
  
  evaluateSurrogate <- function(indepVars, discreteVars) {
    trainingData = data$Filtered()
    for(discreteVar in discreteVars) {
      trainingData = trainingData[trainingData[[discreteVar$varName]] == discreteVar$selected, ]
    }
    trainingDataIndep = trainingData[, unlist(data$pre$var_range_nums_and_ints_list()[['Design Variable']])]
    trainingDataDep = trainingData[, unlist(data$pre$var_range_nums_and_ints_list()[['Objective']])]
    
    ivarDf = data.frame(indepVars)
    names(ivarDf) = unlist(data$pre$var_range_nums_and_ints_list()[['Design Variable']])
    
    resultArray = array(rep(0, nrow(ivarDf) * ncol(trainingDataDep) * 3), c(nrow(ivarDf), ncol(trainingDataDep), 3))
    
    # Need at least two training points (I think?)
    if(nrow(trainingDataIndep) < (ncol(trainingDataIndep) + ncol(trainingDataDep))) {
      stop("Insufficient training data; relax filters or run PET with more points.")
    }
    
    for(colIndex in 1:ncol(trainingDataDep)) {
      model = km(design=trainingDataIndep, response=trainingDataDep[, colIndex])
      predictResults = predict(model, ivarDf, type='SK')
      
      resultArray[, colIndex, 1] = rep(2, nrow(ivarDf)) # COMPUTED from DependentVarState enum
      resultArray[, colIndex, 2] = predictResults$mean
      resultArray[, colIndex, 3] = predictResults$sd
    }
    
    return(resultArray)
  }
}
