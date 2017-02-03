library(shiny)
source('bayesian_utils.r')
source('uq.r')

uiInitialized <- FALSE
directions <- list()
types <- list()
params <- list()

title <- "Uncertainty Quantification"

ui <- function() {
  fluidPage(
    br(),
    tabsetPanel(
      tabPanel("Weighting",
        br(),
        fluidRow(
          column(3,
            checkboxInput('designConfigsPresent', "Multiple Design Configurations Present", value = F)
          )
        ),
        fluidRow(
          conditionalPanel(condition = 'input.designConfigsPresent == true',
            column(3,
              selectInput('designConfigVar', "Design Configuration Identifier",
                choices = c(),
                multiple = F)
            )
          ),
          conditionalPanel(condition = 'input.designConfigsPresent == true & input.designConfigVar != null',
            column(3,
              selectInput('designConfigChoice', "Selection",
                choices = c(),
                multiple = F)
            )
          )
        ),
        fluidRow(
          column(3,
            checkboxInput('displayAll', "Display All Variables", value = T),
            conditionalPanel(condition = 'input.displayAll == false', 
              selectInput(
                'uqDisplayVars',
                "Display Variables",
                choices = c(),
                multiple = T
              )
            )
          )
        ),
        fluidRow(
          column(6, 
            wellPanel(h4("Variable Configuration"),
              uiOutput("uqControlUI"), br()#, height = 200)
            )
          ),
          column(6,
            wellPanel(h4("Variable Plots"), br(),
              uiOutput("uqPlots")
            )
          )
        ),
        actionButton('runFUQ', 'Run Forward UQ')
      ),
      # --------REMOVE THIS SECTION---------------
      # tabPanel("Forward UQ",
      #   br(),
      #   h4("Constraints:"),
      #   fluidRow(
      #     column(6,
      #       wellPanel(
      #         fluidRow(
      #           column(2, h5(strong("Enable"))),
      #           column(6, h5(strong("Variable:"))),
      #           column(4, h5(strong("Value:")))
      #         ),
      #         uiOutput("fuqConstraintsUI")
      #       )
      #     ), column(6)
      #   ), br(),
      #   # actionButton('runFUQ', 'Run Forward UQ'),
      #   br(),
      #   hr(),
      #   uiOutput("fuqPlots")
      # ),
      tabPanel("Design Ranking",
        br(),
        actionButton('runProbability', 'Compute Probabilities'),
        br(), br(),
        h4("Weights"),
        wellPanel(
          fluidRow(
            column(1, h5(strong("ID:"))),
            column(1, h5(strong("Source:"))),
            column(3, h5(strong("Description:"))),
            column(4, h5(strong("Impact:"))),
            column(3, h5(strong("Weight:")))
          ), br(),
          uiOutput("probabilityWeightUI")
        ),
        h4("Rankings"),
        wellPanel(
          DT::dataTableOutput("probabilityTable")
        )
      ),
      id = "uqTabset"
    ),
    conditionalPanel("output.displayQueries",
      hr(),
      h4("Probability Queries:"),
      wellPanel(
        fluidRow(
          column(1, actionButton('addProbability', 'Add')),
          column(3, h5(strong("Variable Name:"))),
          column(2, h5(strong("Direction:"))),
          column(2, h5(strong("Threshold:"))),
          column(2, h5(strong("Value:"))),
          column(2)
        ), br(),
        tags$div(id = 'probabilityUI'),
        hr(),
        actionButton('runProbabilityQueries', 'Evaluate')
      )
    )
  )
}

server <- function(input, output, session, raw_data, raw_info) {

  makeReactiveBinding("uiInitialized")
  makeReactiveBinding("directions")
  makeReactiveBinding("types")
  makeReactiveBinding("params")
  
  varNames <- names(raw_data)
  varClass <- sapply(raw_data,class)
  varNums <- varNames[varClass != "factor"]
  varFacs <- varNames[varClass == "factor"]
  rawAbsMin <- apply(raw_data[varNums], 2, min, na.rm=TRUE)
  rawAbsMax <- apply(raw_data[varNums], 2, max, na.rm=TRUE)
  
  varsList <- reactive({
    print("Getting Variable List.")
    idx = NULL
    for(choice in 1:length(input$uqDisplayVars)) {
      mm <- match(input$uqDisplayVars[choice],varNums)
      if(mm > 0) { idx <- c(idx,mm) }
    }
    print(idx)
    idx
  })
  
  observeEvent(input$designConfigsPresent, {
    updateSelectInput(session, "designConfigVar", choices = varFacs)
  })
  
  observeEvent(input$designConfigVar, {
    print(paste(input$designConfigVar))
    updateSelectInput(session, "designConfigChoice", choices = levels(raw_data[[input$designConfigVar]]))
  })
  
  filtered_data <- reactive({
    filData <- raw_data
    if(input$designConfigsPresent & !is.na(input$designConfigVar) & !is.na(input$designConfigChoice)) {
      filData <- subset(filData, filData[[paste0(input$designConfigVar)]] == input$designConfigChoice)
    }
    filData
  })
      
  uqData <- reactive({
    print("In uqData()")
    
    variables <- varNums
    input_data <- filtered_data()[variables]
    
    # Real Resample
    req(uiInitialized)
    if (uiInitialized) {
      output_data <- resampleData(input_data, directions, types, params)
    }
    else
    {
      output_data <- NULL
    }
    output_data
  })
  
  output$uqControlUI <- renderUI({
    print("In uqControlUI()")
    var_directions <- c("Input",
                        "Output")
    data_mean <- apply(filtered_data()[varNums], 2, mean)
    data_sd <- apply(filtered_data()[varNums], 2, function(x) {sd(x)})
    
    choices <- varNums
    if(!input$displayAll)
      choices <- varNums[varsList()]
    
    lapply(choices, function(var) {
      # UI calculations
      global_index <- which(varNames == var) # globalId
      #gaussianCondition = toString(paste0("input.gaussian",i," == true"))
      #spacefilCondition = toString(paste0("input.gaussian",i," == false"))
      
      #Defaults
      this_gaussian <- TRUE
      this_gauss_mean <- data_mean[[var]]
      this_sd <- data_sd[[var]]
      
      #From petConfig.json
      if(raw_info$variables[[var]]$type == "Design Variable") 
        this_direction <- "Input"
      else
        this_direction <- "Output"
      
      fluidRow(class = "uqVar", column(12,
        # Type select
        fluidRow(
          hr(),
          column(8,
                 
                 selectInput(
                   paste0('varDirection', global_index),
                   label = var,
                   choices = var_directions,
                   selected = this_direction)
          ),
          column(4)
        ),
        conditionalPanel(condition = toString(paste0('input.varDirection', global_index, " == 'Input'")),
          # Gaussian
          fluidRow(
            column(4, 
                   checkboxInput(
                    paste0('gaussian', global_index),
                    label = "Enable Gaussian",
                    value = this_gaussian)
            ),
            #conditionalPanel(condition = toString(paste0('input.gaussian', global_index, ' == true')),
              column(4,
                     textInput(paste0('gaussian_mean', global_index),
                              HTML("&mu;:"),
                              placeholder = "Mean",
                              value = this_gauss_mean)
              ),
              column(4,
                     textInput(paste0('gaussian_sd',global_index),
                              HTML("&sigma;:"),
                              placeholder = "StdDev",
                              value = this_sd)
              )
          #  )
          ),
          # Constraint
          fluidRow(
            column(4, checkboxInput(paste0("fuqConstraintEnable", global_index), "Enable Constraint")),
          #  conditionalPanel(condition = toString(paste0('input.fuqConstraintEnable', global_index, ' == true')),
              column(4, textInput(paste0("fuqConstraintValue", global_index), NULL, value = data_mean[[var]])),
              column(4)
          #  )
            
          )
        )
      ))
    })
    #print("Done with uqControlUI()")
  })
  
  uqCalc <- observe({
    for(i in 1:length(varNums)){
      var <- varNums[i]
      global_index <- which(varNames == var)
      dir <- input[[paste0('varDirection', global_index)]]
      is_gaus <- input[[paste0('gaussian', global_index)]]
      req(dir)
      directions[[var]] <<- input[[paste0('varDirection', global_index)]]
      if (directions[[var]] == "Input") {
        if(input[[paste0('gaussian', global_index)]]) {
          types[[var]] <<- "norm"
          params[[var]]$mean <<- as.numeric(input[[paste0('gaussian_mean', global_index)]])
          params[[var]]$stdDev <<- as.numeric(input[[paste0('gaussian_sd', global_index)]])
        }
        else {
          types[[var]] <<- "unif"
          params[[var]]$min <<- unname(rawAbsMin[[var]])
          params[[var]]$max <<- unname(rawAbsMax[[var]])
        }
      }
    }
    uiInitialized <<- TRUE
  })
  
  output$uqPlots <- renderUI({
    print("In uqPlots()")
    data <- uqData()$dist
    variables <- varNums
    if(!input$displayAll)
      variables <- varNums[varsList()]
    
    if(is.null(data)) {
      verbatimTextOutput("Initializing...")
    }
    else {
      lapply(variables, function(var) {
        par(mar = rep(2, 4))
        filtered_data_histo <- hist(filtered_data()[[var]], freq = FALSE, breaks=30)
        x_bounds <- c(min(filtered_data_histo$breaks, data[[var]][["xOrig"]], data[[var]][["xResampled"]]),
                      max(filtered_data_histo$breaks, data[[var]][["xOrig"]], data[[var]][["xResampled"]]))
        y_bounds <- c(0,
                      max(filtered_data_histo$density, data[[var]][["yOrig"]], data[[var]][["yResampled"]]))
        fluidRow(class = "uqVar",
          column(12,
                 renderPlot({
                   hist(filtered_data()[[var]],
                        freq = FALSE,
                        col = input$bayHistColor,
                        border = "#C0C0C0",
                        #type = "l",
                        main = "", 
                        xlab = "", ylab = "", 
                        yaxt = "n", 
                        xlim = x_bounds,
                        ylim = y_bounds,
                        # las = 1,
                        #asp = 1.3,
                        breaks = 30,
                        bty = "o")
                   lines(data[[var]][["xOrig"]],
                         data[[var]][["yOrig"]],
                         col = input$bayOrigColor, lwd=2)
                   lines(data[[var]][["xResampled"]],
                         data[[var]][["yResampled"]],
                         col = input$bayResampledColor, lwd=2)
                   if (!is.null(forwardUQData()) & !is.null(forwardUQData()[[var]])) {
                     lines(forwardUQData()[[var]]$postPoints,
                           forwardUQData()[[var]]$postPdf,
                           col="orange", lwd=2)
                   }
                   box(which = "plot", lty = "solid", lwd=2, col=boxColor(var))
                 }, height = 248)
          )
        )
      })
    }
  })
  
  boxColor <- function (var) {
    if (directions[[var]] == "Input")
    {
      "gold"
    }
    else if (directions[[var]] == "Output")
    {
      "deepskyblue"
    }
    else
    {
      "black"
    }
  }
  
  # Uncertainty Quantification Footer -----------------------------------------
  
  output$displayQueries <- reactive({
    display <- !(input$uqTabset == "Design Ranking")
  })
  
  outputOptions(output, "displayQueries", suspendWhenHidden=FALSE)
  
  probabilityQueries <- reactiveValues(rows = c())
  
  observeEvent(input$addProbability, {
    id <- input$addProbability
    insertUI(
      selector = '#probabilityUI',
      ui = tags$div(
        fluidRow(
          column(1, actionButton(paste0('removeProbability', id), 'Delete')),
          column(3, selectInput(paste0('queryVariable', id), NULL, choices = varNums, selected = varNums[[1]])),
          column(2, selectInput(paste0('queryDirection', id), NULL, choices = c("Above", "Below")), selected = "Above"),
          column(2, textInput(paste0('queryThreshold', id), NULL)),
          column(2, textOutput(paste0('queryValue', id))),
          column(2)
        ),
        id = paste0('probabilityQuery', id)
      )
    )
    probabilityQueries$rows <<- c(probabilityQueries$rows, toString(id))
  })
  
  removeProbability <- observe({
    lapply(probabilityQueries$rows, function(id) {
      observeEvent(input[[paste0('removeProbability', id)]], {
        removeUI(
          ## pass in appropriate div id
          selector = paste0('#probabilityQuery', id)
        )
        probabilityQueries$rows <<- probabilityQueries$rows[sapply(probabilityQueries$rows, function(x) {x!=id})]
      })
    })
  })
  
  uqInputs <- reactive({
    inputs <- sapply(varNums, function(var) {directions[[var]] == "Input"})
    subset(varNums, inputs)
  })
  
  uqOutputs <- reactive({
    inputs <- sapply(varNums, function(var) {directions[[var]] == "Output"})
    subset(varNums, inputs)
  })
  
  runQueries <- observeEvent(input$runProbabilityQueries, {
    print("Started Calculating Probabilities.")
    lapply(1:length(probabilityQueries$rows), function(i) {
      id <- probabilityQueries$rows[i]
      name <- input[[paste0('queryVariable', id)]]
      direction <- input[[paste0('queryDirection', id)]]
      threshold <-input[[paste0('queryThreshold', id)]]
      req(threshold)
      data <- uqData()$dist[[name]]
      
      value <- integrateData(data$xResampled,
                             data$yResampled,
                             min(data$xResampled),
                             as.numeric(threshold))
      
      print(paste("Query: ",name, direction, threshold, value))
      
      if (direction == "Above") {
        value <- (1-value)
      }
      output[[paste0('queryValue', id)]] <- renderText(toString(value))
    })
    print("Probabilites Calculated.")
  })
  
  # Forward UQ ---------------------------------------------------------------
  
  #-----------REMOVE ME---------------------
  # output$fuqConstraintsUI <- renderUI({
  #   lapply(uqInputs(), function(input) {
  #     id <- which(uqInputs() == input)
  #     # fluidRow(
  #     #   column(2, checkboxInput(paste0("fuqConstraintEnable", id), NULL)),
  #     #   column(6, paste(input)),
  #     #   column(4, textInput(paste0("fuqConstraintValue", id), NULL, value = toString(apply(filtered_data()[input], 2, mean))))
  #     # )
  #   })
  # })
  
  forwardUQData <- reactive({
    temp_results <- NULL
    if(input$runFUQ){
      isolate({
        numberOfInputs <- 0
        for (i in 1:length(uqInputs())) {
          global_i = which(uqInputs()[i] == varNames)
          if (input[[paste0("fuqConstraintEnable", global_i)]]) {
            numberOfInputs <- numberOfInputs + 1
          }
        }
        
        if (numberOfInputs > 0) {
          print("Started Forward UQ.")
          temp_results <- processForwardUQ(filtered_data()[varNums], uqData(), uqInputs())
          print("Completed Forward UQ.")
        }
      })
    }
    results <- temp_results
  })
  
  processForwardUQ <- function(originalData, uqData, uqInputs) {
    
    resampledData <- uqData$resampledData
    rho <- buildGuassianCopula(resampledData)
    
    constrainedInputs <- c()
    columnsToRemove <- c()
    for (i in 1:length(uqInputs)) {
      global_i = which(uqInputs[i] == varNames)
      if (input[[paste0("fuqConstraintEnable", global_i)]]) {
        constrainedInputs <- c(constrainedInputs, uqInputs[i])
      }
      else {
        columnsToRemove <- c(columnsToRemove, which(names(originalData) == uqInputs[i]))
      }
    }
    
    if(!is.null(columnsToRemove)){
      originalDataTrimmed <- originalData[,-columnsToRemove]
      resampledDataTrimmed <- resampledData[,-columnsToRemove]
      rhoTrimmed <- rho[-columnsToRemove,-columnsToRemove]
    }
    else{
      originalDataTrimmed <- originalData
      resampledDataTrimmed <- resampledData
      rhoTrimmed <- rho
    }
    
    observations <- list()
    observationsIndex <- c()
    for (i in 1:length(constrainedInputs)) {
      observations[[paste(constrainedInputs[i])]] = as.numeric(input[[paste0("fuqConstraintValue", which(constrainedInputs[i] == varNames))]])
      observationsIndex <- c(observationsIndex, which(names(originalDataTrimmed) == constrainedInputs[i]))
    }
    observations <- as.data.frame(observations)
    
    print("Finished Processing ForwardUQ.")
    
    results <- forwardUq(originalDataTrimmed, resampledDataTrimmed, rhoTrimmed, observations, observationsIndex)[[1]]
  }
    
  #-------------REMOVE ME------------------------
  # output$fuqPlots <- renderUI({
  #   data <- forwardUQData()
  #   lapply(uqOutputs(), function(output) {
  #     renderText(paste("Forward UQ plots for", output, "here."))
  #   })
  # })
  
  # Design Ranking -----------------------------------------------------------
  
  runFullProbability <- eventReactive(input$runProbability, {
    data <- data.frame(Config = character(0), stringsAsFactors=FALSE)
    print(data)
    for(i in 1:length(probabilityQueries$rows)) {
      id <- probabilityQueries$rows[i]
      data[[paste0('Query', id)]] <- numeric(0)
    }
    configs <- levels(raw_data[[paste0(input$designConfigVar)]])
    print(data)
    for (i in 1:length(configs)) {
      config <- configs[i]
      print(paste(config))
      configData <- subset(raw_data, raw_data[[paste0(input$designConfigVar)]] == config)
      configData <- configData[varNums]
      resampledData <- resampleData(configData, directions, types, params)$dist
      answers <- c(paste0(config))
      for(j in 1:length(probabilityQueries$rows)) {
        id <- probabilityQueries$rows[j]
        name <- input[[paste0('queryVariable', id)]]
        direction <- input[[paste0('queryDirection', id)]]
        threshold <-input[[paste0('queryThreshold', id)]]
        req(threshold)
        value <- integrateData(resampledData[[name]][["xResampled"]],
                               resampledData[[name]][["yResampled"]],
                               min(resampledData[[name]][["xResampled"]]),
                               as.numeric(threshold))
        if (direction == "Above") {
          value <- (1-value)
        }
        answers <- c(answers, value)
      }
      data[nrow(data)+1,] <- answers
    }
    print(data)
    data
  })
  
  output$probabilityWeightUI <- renderUI({
    lapply(probabilityQueries$rows, function(id) {
      variable <- input[[paste0('queryVariable', id)]]
      direction <- input[[paste0('queryDirection', id)]]
      threshold <-input[[paste0('queryThreshold', id)]]
      
      fluidRow(
        column(1, paste0("Query", id)),
        column(1, paste0("Config")),
        column(3, paste(variable, tolower(direction), threshold)),
        column(4, selectInput(paste0("probImpact", id), NULL, choices = c("Positive", "Negative"), selected = "Positive")),
        column(3, sliderInput(paste0("probWeight", id), NULL, 0, 1, 1, step = 0.05))
      )
    })
  })
  
  topsisProbability <- reactive({
    outputData <- runFullProbability()
    decisions <- data.matrix(outputData[,-1])
    weights <- NULL
    impacts <- NULL
    for(i in 1:length(probabilityQueries$rows)) {
      id <- probabilityQueries$rows[i]
      if(input[[paste0("probImpact", id)]] == "Positive")
        impacts <- c(impacts, '+')
      else
        impacts <- c(impacts, '-')
      weights <- c(weights, as.numeric(input[[paste0("probWeight", id)]]))
    }
    
    if(ncol(decisions)>1) {
      topsisData <- topsis(decisions, weights, impacts)
      outputData <- cbind(topsisData[,-1], outputData)
    }
    else {
      if (impacts[1] == "+") {
        outputData <- cbind("Rank" = 1:nrow(decisions), outputData[rev(order(outputData[[paste0("Query", probabilityQueries$rows[1])]])),])
      }
      else {
        outputData <- cbind("Rank" = 1:nrow(decisions), outputData[order(outputData[[paste0("Query", probabilityQueries$rows[1])]]),])
      }
    }
    outputData
  })
  
  output$probabilityTable <- DT::renderDataTable({
    topsisProbability()
  })
  
}