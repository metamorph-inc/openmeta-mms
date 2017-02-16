### This file does not currently match the new formatting standard

title <- "Data Table"

ui <- function() {
  
  tabPanel("Data Table",
           br(),
           fluidRow(
             column(4, 
                    tags$div(title = "Rounds data in all tables to a set number of decimal places", 
                             checkboxInput("roundTables", "Round Data Table", value = FALSE))),
             conditionalPanel("input.roundTables == '1'", 
                              column(8, 
                                     tags$div(title = "Maximum number of decimals to show in data tables.", 
                                              sliderInput("numDecimals", "Decimal Places", min = 0, max = 11, step = 1, value = 4))))
           ),
           checkboxInput("autoRanking", "Automatic Refresh", value = TRUE),
           radioButtons("activateRanking", "Output Preference", choices = c("Unranked", "TOPSIS", "Simple Metric w/ TxFx"), selected = "Unranked"),
           wellPanel(
             style = "overflow-x:auto",
             DT::dataTableOutput("dataTable"),
             downloadButton("exportPoints", "Export Selected Points"), 
             actionButton("colorRanked", "Color by Selected Rows")
             #checkboxInput("transpose", "Transpose Table", value = FALSE)
             
           ),
           conditionalPanel(condition = "input.activateRanking != 'Unranked'",
                            wellPanel(
                              conditionalPanel(condition = "input.autoRanking == false",
                                               actionButton("applyRanking", "Apply Ranking"),
                                               br(), br()
                              ),
                              fluidRow(
                                column(4,
                                       selectInput("weightMetrics",
                                                   "Ranking Metrics:",
                                                   c(),
                                                   multiple = TRUE),
                                       actionButton("clearMetrics", "Clear Metrics")
                                )
                              ),
                              conditionalPanel(condition = "input.weightMetrics != null",
                                               hr()),
                              fluidRow(
                                column(3, strong(h4("Variable Name"))),
                                column(2, strong(h4("Ranking Mode"))),
                                conditionalPanel(condition = "input.activateRanking == 'TOPSIS'",
                                                 column(7, strong(h4("Weight Amount")))),
                                conditionalPanel(condition = "input.activateRanking == 'Simple Metric w/ TxFx'",
                                                 column(3, strong(h4("Weight Amount"))),
                                                 column(4, strong(h4("Transfer Function"))))
                              ),
                              uiOutput("rankings")#, 
                              #br(), hr(), 
                              #br()
                            )
           )
  )
  
}



server <- function(input, output, session, data) {
  
  
  
  varNames <- names(data$raw)
  varClass <- sapply(data$raw,class)
  varNum <- varNames[varClass != "factor"]
  varRangeNum <- function(...)
  {
    varNum
  }
  
  ## Setup Data Frame for Transfer Functions
  xFuncs <- data.frame()
  xFuncs <- xFuncs[1:4,]
  row.names(xFuncs) <- c("Values", "Scores", "Slopes", "Y_ints")
  
  makeReactiveBinding("xFunc")
  
  filterData <- function(...)
  {
    data$raw
  }
  
  slowFilterData <- eventReactive(input$updateDataTable, {
    filterData()
    if(input$roundTables)
      round_df(filterData(), input$numDecimals)
  })
  
  
  
  round_df <- function(x, digits) {
    # round all numeric variables
    # x: data frame 
    # digits: number of digits to round
    numeric_columns <- sapply(x, class) == 'numeric'
    x[numeric_columns] <-  round(x[numeric_columns], digits)
    x
  }
  
  output$table <- DT::renderDataTable({
    print("In render data table")
    if(input$autoData == TRUE){
      data <- filterData()
    }
    else {
      data <- slowFilterData()
    }
    if(input$roundTables)
      data <- round_df(filterData(), input$numDecimals)
    #data
    DT::datatable(data, options = list(scrollX = T))
  })
  
  #Dynamic metrics list
  metricsList <- reactive({
    idx = NULL
    req(input$weightMetrics)
    print("Getting Metrics List.")
    for(choice in 1:length(input$weightMetrics)) {
      mm <- match(input$weightMetrics[choice],varNames)
      if(mm > 0) { idx <- c(idx,mm) }
    }
    print(idx)
    idx
  })
  
  #Event handler for "clear metrics" button
  observe({
    if(input$clearMetrics | input$activateRanking == "Unranked")
      updateSelectInput(session, "weightMetrics", choices = varRangeNum(), selected = NULL) 
  })
  
  #UI class for each metric UI
  generateMetricUI <- function(current, slider, radio, util, func) {
    
    if(missing(slider) & missing(radio) & missing(util) & missing(func)){
      sliderVal <- input[[paste0('rnk', current)]]
      if(is.null(sliderVal))
        sliderVal <- 1
      
      radioVal <- input[[paste0('sel', current)]]
      if(is.null(radioVal))
        radioVal <- "Min"
      
      utilVal <- input[[paste0('util', current)]]
      if(is.null(utilVal))
        utilVal <- FALSE
      
      funcVal <- input[[paste0('func', current)]]
    }
    else{
      sliderVal <- slider
      radioVal <- radio
      utilVal <- util
      funcVal <- func
    }
    
    if(input$activateRanking == 'TOPSIS')
      topsisMetricUI(current, radioVal, sliderVal)
    else if(input$activateRanking == 'Simple Metric w/ TxFx')
      simpleMetricUI(current, radioVal, sliderVal, utilVal, funcVal)
  }
  
  topsisMetricUI <- function(current, radioVal, sliderVal) {
    
    varName <- varNames[current]
    
    fluidRow(
      column(3, h5(varName)),
      column(2, radioButtons(paste0('sel', current),
                             NULL,
                             choices = c("Min", "Max"),
                             selected = radioVal,
                             inline = TRUE)),
      column(7, sliderInput(paste0('rnk', current),
                            NULL,
                            step = 0.01,
                            min = 0.01,
                            max = 1,
                            value = sliderVal))
    )
  }
  
  simpleMetricUI <- function(current, radioVal, sliderVal, utilVal, funcVal) {
    
    varName <- varNames[current]
    transferCondition = toString(paste0("input.util",current," == true"))
    
    fluidRow(
      column(3, h5(varName)),
      column(2, radioButtons(paste0('sel', current),
                             NULL,
                             choices = c("Min", "Max"),
                             selected = radioVal,
                             inline = TRUE)),
      column(3, sliderInput(paste0('rnk', current),
                            NULL,
                            step = 0.01,
                            min = 0,
                            max = 1,
                            value = sliderVal)),
      column(1, checkboxInput(paste0('util', current),
                              "Add Transfer Function",
                              value = utilVal)),
      column(3, conditionalPanel(condition = transferCondition,
                                 textInput(paste0('func', current),
                                           "Enter Data Points",
                                           placeholder = paste0("Value = Score | e.g. ",
                                                                min(filterData()[[varName]]),
                                                                " = 1, ",
                                                                max(filterData()[[varName]]),
                                                                "= 0.5"),
                                           value = funcVal),
                                 utilityPlot(current)))
    )
  }
  
  fullMetricUI <- reactive({
    a <- input$activateRanking # Force reaction
    lapply(metricsList(), function(column) {
      isolate(generateMetricUI(column))
    })
  })
  
  #Output plot of transfer function
  utilityPlot <- function(current){
    plotName <- paste0("transferPlot", current)
    f <- list(
      family = "Courier New, monospace",
      size = 18,
      color = "#7f7f7f"
    )
    x <- list(
      title = varNames[current],
      titlefont = f
    )
    y <- list(
      title = "Score",
      titlefont = f
    )
    renderPlot({
      plotPoints <- parseUserInputPoints(current)
      req(plotPoints)
      par(mar = c(4.5,4.5,1,1))
      p <- plot(x = unlist(lapply(names(plotPoints), as.numeric)),
                y = unname(plotPoints),
                xlab = varNames[current],
                ylab = "Score")
      if(length(plotPoints) > 1){
        for(i in 1:(length(plotPoints)-1)){
          segments(x0 = as.numeric(names(plotPoints)[i]),
                   y0 = plotPoints[i],
                   x1 = as.numeric(names(plotPoints)[i+1]),
                   y1 = plotPoints[i+1])
          
        }
      }
      p
    }, height = 150)
  }
  
  
  
  #Calculate line slopes & intercepts of transfer function
  processLineEquations <- function(current, data_set){
    slopes <- NULL
    y_ints <- NULL
    if(length(data_set) > 1){
      for(i in 1:(length(data_set)-1)){
        rise <- data_set[i+1] - data_set[i]
        run <- as.numeric(names(data_set)[i+1]) - as.numeric(names(data_set)[i])
        current_slope <- rise/run
        slopes <- c(slopes, current_slope)
        b <- data_set[i] - current_slope*as.numeric(names(data_set)[i])
        y_ints <- c(y_ints, b)
      }
    }
    else{
      slopes <- 0
      y_ints <- data_set[1]
    }
    xFuncs[[toString(current)]] <<- list(names(data_set), data_set, slopes, y_ints)
  }
  
  #Parse transfer function text input
  parseUserInputPoints <- function(current){
    xVals <- NULL
    yVals <- NULL
    raw_text <- input[[paste0('func', current)]]
    points <- unlist(strsplit(raw_text, ","))
    for(i in 1:length(points)){
      current_point <- unlist(strsplit(points[i], "="))
      if(length(current_point) != 2)
        break
      current_val <- as.numeric(current_point[1])
      low <- 0
      hi <- 1
      #This line below enforces range limits on transfer function
      #req(findInterval(current_val, c(low, hi), rightmost.closed = TRUE) == 1) 
      xVal <- current_val
      yVal <- as.numeric(current_point[2])
      if(yVal < low)
        yVal = low
      else if(yVal > hi)
        yVal = hi
      xVals <- c(xVals, xVal)
      yVals <- c(yVals, yVal)
    }
    
    if(is.null(xVals) | is.null(yVals)){
      xFuncs[[toString(current)]] <<- NULL
      outputVals <- NULL
    }
    else{
      #Sort list by 'xVals' (while paired with yVals)
      unsortedVals <- xVals
      names(unsortedVals) <- yVals
      sortedVals <- sort(unsortedVals)
      
      #Flip-flop names&values of a named list
      outputVals <- sapply(names(sortedVals), as.numeric)
      names(outputVals) <- sortedVals
      processLineEquations(current, outputVals)
    }
    
    outputVals
  }
  
  
  
  
  #Dynamic UI rendering for weighted metrics list
  output$rankings <- renderUI({
    req(metricsList())
    print("In render ranking sliders")
    fullMetricUI()
  })
  
  
  #Process metric weights
  rankData <- reactive({
    req(metricsList())
    print("In calculate ranked data")
    data <- filterData()[varRangeNum()]
    normData <- data.frame(t(t(data)/apply(data,2,max)))
    
    scoreData <- sapply(row.names(normData) ,function(x) 0)
    
    for(i in 1:length(metricsList())) {
      column <- varNames[metricsList()[i]]
      rnkName <- paste0("rnk", toString(metricsList()[i]))
      weight <- input[[rnkName]]
      req(weight)
      
      xFunc <- xFuncs[[toString(metricsList()[i])]]
      txActive <- input[[paste0('util', metricsList()[i])]]
      if(is.null(txActive))
        txActive <- TRUE
      if(!is.null(xFunc) & txActive){
        normValue <- max(data[[column]])
        transferData <- data[[column]]
        for(t in 1:length(transferData)){
          item <- transferData[t]
          transferData[t] <- linearly_interpolate(item, xFunc)
        }
        scoreData <- scoreData + transferData*weight
      }
      else{
        radioSelect <- paste0("sel", toString(metricsList()[i]))
        if(input[[radioSelect]] == "Min"){
          colMin <- min(normData[column])
          for(j in 1:length(unlist(normData[column]))) {
            item <- normData[j,column]
            normData[j,column] <- 1 -item + colMin
          }
        }
        scoreData <- scoreData + unlist(unname(weight*normData[column]))
      }
    }
    scoreData <- scoreData/max(scoreData)
    
    scoreData <- sort(scoreData, decreasing = TRUE)
    score <- scoreData
    rank <- seq(length(score))
    
    data <- filterData()[names(scoreData), ]
    data <- cbind(rank, score, data)
    data
    
  })
  
  #Score individual point by transfer function
  linearly_interpolate <- function(current, dataset){
    choices <- as.numeric(unlist(dataset[1]))
    slot <- findInterval(current,
                         choices,
                         rightmost.closed = TRUE)
    if(slot == 0){
      #Points below range of transfer function
      unlist(dataset[2])[1]
    }
    else if(slot > (length(choices)-1)){
      #Points above range of transfer function
      unlist(dataset[2])[length(choices)]
    }
    else{
      slope <- unlist(dataset[3])[slot]
      y_int <- unlist(dataset[4])[slot]
      current*slope + y_int
    }
  }
  
  slowRankData <- eventReactive(input$applyRanking, {
    rankData()
  })
  
  topsisData <- reactive({
    req(metricsList())
    print("In calculate topsis data")
    data <- filterData()[varRangeNum()]
    weights <- NULL
    impacts <- NULL
    for(i in 1:length(varRangeNum())){
      global_index <- match(varRangeNum()[i], varNames)
      if(!is.null(input[[paste0("rnk", global_index)]])){
        weights <- c(weights, input[[paste0("rnk", global_index)]])
        impacts <- c(impacts, input[[paste0("sel", global_index)]])
      }
      else{
        weights <- c(weights, 0.01)
        impacts <- c(impacts, '+')
      }
    }
    impacts[impacts == "Max"] <- '+'
    impacts[impacts == "Min"] <- '-'
    t <- topsis(as.matrix(data), weights, impacts)
    sorted_topsis <- t[order(t$rank),]
    rank <- sorted_topsis$rank
    score <- sorted_topsis$score
    output_data <- filterData()[sorted_topsis$alt.row,]
    output_data <- cbind(rank, score, output_data)
    output_data
  })
  
  slowTopsisData <- eventReactive(input$applyRanking, {
    topsisData()
  })
  
  output$dataTable <- DT::renderDataTable({
    if(length(input$weightMetrics) > 0){
      if(input$activateRanking == 'TOPSIS'){
        if(input$autoRanking)
          data <- topsisData()
        else
          data <- slowTopsisData()
      }
      else{
        if(input$autoRanking)
          data <- rankData()
        else
          data <- slowRankData()
      }
    }
    else{
      data <- filterData()
    }
    if(input$roundTables)
      data <- round_df(data, input$numDecimals)
    #if(input$transpose)
    #  data <- t(data)
    data
  })
  
  #Download handler
  output$exportPoints <- downloadHandler(
    filename = function() { paste('ranked_points-', Sys.Date(), '.csv', sep='') },
    content = function(file) { 
      if(input$activateRanking != "Unranked")
        write.csv(rankData()[input$dataTable_rows_selected, ], file)
      else
        write.csv(filterData()[input$dataTable_rows_selected, ], file) 
    }
  )
  
}