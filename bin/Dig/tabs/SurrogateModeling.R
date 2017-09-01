library(DiceKriging)

title <- "Surrogate Modeling"
footer <- TRUE



ui <- function(id) {
  ns <- NS(id)

  fluidPage(
    br(),
    #textInput(ns("someText"), label="Text to pass to Angular"),
    #actionButton(ns("incrementButton"), label="Increment something"),
    tags$head(tags$link(rel = "stylesheet", type = "text/css", href = "surrogateModelingStyle.css")),
    tags$head(tags$script(src="third_party/iframeResizer.min.js")),
    tags$head(tags$style("iframe { width: 100%; }")),
    tags$iframe(src="surrogateModeling/index.html"),
    tags$script("iFrameResize({log:false, heightCalculationMethod: 'lowestElement'});")
  )
}

server <- function(input, output, session, data) {
  ns <- session$ns

  observeEvent(input$someText, {
    print("Text changed")
    session$sendCustomMessage(type="textFieldChanged", input$someText)
  })

  # indepVarNames <- isolate({
  #   data$pre$var_names()[unlist(lapply(data$pre$var_names(), function(var) {
  #     data$meta$variables[[var]]$type == "Design Variable"
  #   }))]
  # })
  #
  # depVarNames <- isolate({
  #   data$pre$var_names()[unlist(lapply(data$pre$var_names(), function(var) {
  #       data$meta$variables[[var]]$type == "Objective"
  #   }))]
  # })

  indepVars <- data.frame(
    someX1 = c(1.0, 2.0, 3.0),
    someX2 = c(4.0, 5.0, 6.0),
    someX3 = c(7.0, 8.0, 9.0)
  )

  depVars <- data.frame(
    someY1 = c(10.0, 20.0, 30.0),
    someY2 = c(40.0, 50.0, 60.0)
  )

  vars = reactiveValues(iv=indepVars, dv=depVars)
  #
  observe({print(vars$iv)})

  observeEvent(input$incrementButton, {
    print("Button clicked")
    vars$iv[1,1] = vars$iv[1,1] + 1
    vars$dv[1,1] = vars$dv[1,1] + 1
  })

  observeEvent(vars$iv, {
    session$sendCustomMessage(type="ivarsChanged", jsonlite::toJSON(vars$iv))
  })

  observeEvent(vars$dv, {
    session$sendCustomMessage(type="dvarsChanged", jsonlite::toJSON(vars$dv))
  })

  observeEvent(input$externalRequest, {
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
        session$sendCustomMessage(type="externalResponse", list(
          id=input$externalRequest$id,
          data=c(list(configIdObject), discreteVarsList)
        ))
      } else if(input$externalRequest$command == "evaluateSurrogateAtPoints") {
        result = evaluateSurrogate(input$externalRequest$data$independentVars,
                          input$externalRequest$data$discreteVars)
        session$sendCustomMessage(type="externalResponse", list(
          id=input$externalRequest$id,
          data=result
        ))
      }
    }
  })
  
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
      # TODO: Figure out error handling/messaging; for now, we return a "stale"
      # result set with values of 0
      return(resultArray)
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

  observeEvent(input$messageFromBrowser, {
    print("Rx")
    if(!is.null(input$messageFromBrowser) && input$messageFromBrowser != "") {
      print("Received message from browser")
      print(input$messageFromBrowser)
    }
  })
  #
  # addRow <- observeEvent(input$add_row, {
  #   print("Button clicked")
  #   vars$iv[nrow(vars$iv) + 1, ] = rep(0.0, ncol(vars$iv))
  #   vars$dv[nrow(vars$dv) + 1, ] = rep(0.0, ncol(vars$dv))
  #   vars$dv[1,1] = vars$dv[1,1] + 1
  # })
  #
  # output$uqPredictions <- renderUI({
  #   # Make header row, with column headings
  #   print(vars$iv)
  #
  #   headerRow = vector("list", ncol(vars$iv) + ncol(vars$dv) + 1)
  #
  #   for(i in 1:ncol(vars$iv)) {
  #     headerRow[[i]] = tags$th(colnames(vars$iv)[i])
  #   }
  #
  #   for(i in 1:ncol(vars$dv)) {
  #     headerRow[[ncol(vars$iv) + i]] = tags$th(colnames(vars$dv)[i])
  #   }
  #
  #   headerRow[[ncol(vars$iv) + ncol(vars$dv) + 1]] = tags$th("")
  #
  #   # Make table content
  #   tableRows = vector("list", nrow(vars$iv))
  #
  #   for(i in 1:nrow(vars$iv)) {
  #     tableRow = vector("list", ncol(vars$iv) + ncol(vars$dv) + 1)
  #
  #     for(j in 1:ncol(vars$iv)) {
  #       tableRow[[j]] = tags$td(numericInput(inputId=ns(paste0("indepVar_", i, j)), label=NULL, value=vars$iv[i, j]))
  #     }
  #
  #     for(j in 1:ncol(vars$dv)) {
  #       tableRow[[ncol(vars$iv) + j]] = tags$td(vars$dv[i, j])
  #     }
  #
  #     deleteButtonName = paste0("delete_", i)
  #     tableRow[[ncol(vars$iv) + ncol(vars$dv) + 1]] = tags$td(actionButton(inputId=ns(deleteButtonName), label=NULL, icon=icon("remove", lib="glyphicon"), class="btn-danger"))
  #
  #     tableRows[[i]] = tags$tr(tableRow)
  #   }
  #
  #   tags$table(class="table table-striped table-bordered uq-predictions-table",
  #     tags$thead(
  #       tags$tr(
  #         headerRow
  #       )
  #     ),
  #     tableRows
  #   )
  # })
  #
  # deleting = list(val=0)
  #
  # ivarFieldsChanged <- observe({
  #   if(deleting$val > 0) {
  #     print("a")
  #     deleting$val = deleting$val - 1
  #   } else {
  #     ivars_isolated = isolate(vars$iv)
  #     dvars_isolated = isolate(vars$dv)
  #     print("Change detected")
  #     changed = FALSE
  #     for(i in 1:nrow(ivars_isolated)) {
  #       for(j in 1:ncol(dvars_isolated)) {
  #         input_name = paste0("indepVar_", i, j)
  #         if(!is.null(input[[input_name]])) {
  #           isolate({vars$iv[i, j] = input[[input_name]]})
  #           changed = TRUE
  #         }
  #       }
  #     }
  #     if(changed) {
  #       compute()
  #     }
  #   }
  # })
  #
  # deleteButtonClick <- observe({
  #   ivars_isolated = isolate(vars$iv)
  #   dvars_isolated = isolate(vars$dv)
  #   for(i in 1:nrow(ivars_isolated)) {
  #     buttonName = paste0("delete_", i)
  #     if(!is.null(input[[buttonName]]) && input[[buttonName]] > 0) {
  #       print("Delete detected")
  #       print(i)
  #       vars$iv <- ivars_isolated[-i,]
  #       vars$dv <- dvars_isolated[-i,]
  #       deleting$val = 2
  #     }
  #   }
  # })
  #
  # output$uqXTable <- renderTable({vars$iv})
  # output$uqYTable <- renderTable({vars$dv})
  #
  # compute <- function() {
  #   vars$dv[,1] = vars$iv[,1] + vars$iv[,2] + vars$iv[,3]
  #   vars$dv[,2] = (vars$iv[,1] + vars$iv[,2] + vars$iv[,3]) * 0.1
  # }
  #
}
