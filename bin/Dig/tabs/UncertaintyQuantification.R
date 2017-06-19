source('utils/bayesian_utils.r')
source('utils/uq.r')

uiInitialized <- FALSE
directions <- list()
types <- list()
params <- list()
probability_queries <- NULL
query_id <- NULL

title <- "Uncertainty Quantification"
footer <- FALSE

ui <- function(id) {
  ns <- NS(id)
  
  fluidPage(
  	tags$head(tags$style(".uqVar{height:250px;}")),
    br(),
    tabsetPanel(
      tabPanel("Weighting",
        br(),
        fluidRow(
          column(3,
            checkboxInput(ns('design_configs_present'),
                          "Multiple Design Configurations Present",
                          value = si(ns('design_configs_present'), FALSE))
          )
        ),
        fluidRow(
          conditionalPanel(condition = paste0('input["', ns('design_configs_present'), '"] == true'),
            column(3,
              selectInput(ns('design_config_var'), "Design Configuration Identifier",
                choices = c(),
                multiple = F)
            )
          ),
          conditionalPanel(condition = paste0('input["', ns('design_configs_present'), '"] == true & input["', ns('design_config_var'), '"] != null'),
            column(3,
              selectInput(ns('design_config_choice'), "Selection",
                choices = c(),
                multiple = F)
            )
          )
        ),
        fluidRow(
          column(3,
            checkboxInput(ns('display_all'),
                          "Display All Variables",
                          value = si(ns('display_all'), TRUE)),
            conditionalPanel(condition = paste0('input["', ns('display_all'), '"] == false'), 
              selectInput(
                ns('display'),
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
              uiOutput(ns("vars_ui")), br()#, height = 200)
            )
          ),
          column(6,
            wellPanel(h4("Variable Plots"), br(),
              uiOutput(ns("vars_plots"))
            )
          )
        ),
        actionButton(ns('run_forward_uq'), 'Run Forward UQ')
      ),
            
      tabPanel("Design Ranking",
        br(),
        actionButton(ns('run_probabilities'), 'Compute Probabilities'),
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
          uiOutput(ns("probabilities_weight_ui"))
        ),
        h4("Rankings"),
        wellPanel(
          DT::dataTableOutput(ns("probabilities_table"))
        )
      ),
      id = ns("tabset"),
      selected = si(ns("tabset"), NULL)
    ),
    conditionalPanel(paste0("output['", ns('display_queries'), "']"),
      hr(),
      h4("Probability Queries:"),
      wellPanel(
        fluidRow(
          column(1, actionButton(ns('add_probability'), 'Add')),
          column(3, h5(strong("Variable Name:"))),
          column(2, h5(strong("Direction:"))),
          column(2, h5(strong("Threshold:"))),
          column(2, h5(strong("Value:"))),
          column(2)
        ), br(),
        tags$div(id = 'probabilities_ui'),
        hr(),
        actionButton(ns('run_probabilities_queries'), 'Evaluate')
      )
    ),
    fluidRow(
	    column(3, tags$div(title = "Color of histogram bars",
	    	colourpicker::colourInput(ns("hist_color"),
	    	                          "Histogram",
	    	                          si(ns("hist_color"), "wheat")))),
	    column(3, tags$div(title = "Color of the PDF used to reshape the data",
	      colourpicker::colourInput(ns("orig_color"),
	                                "Reshaping Distribution",
	                                si(ns("orig_color"), "#000000")))),
	    column(3, tags$div(title = "Color of the PDF that represents the resampled data",
	      colourpicker::colourInput(ns("resamp_color"),
	                                "Resampled Distribution",
	                                si(ns("resamp_color"), "#5CC85C")))),
	    column(3, tags$div(title = "Color of forward uncertainty quantification posterior",
	      colourpicker::colourInput(ns("post_color"),
	                                "Posterior Distribution",
	                                si(ns("post_color"), "orange"))))
    )
  )
}

server <- function(input, output, session, data) {
  ns <- session$ns
  
  makeReactiveBinding("uiInitialized")
  makeReactiveBinding("directions")
  makeReactiveBinding("types")
  makeReactiveBinding("params")
  
  # Get data, isolating it from its reactive context
  raw_data <- isolate(data$raw$df)
  raw_info <- data$meta
  
  varNames <- isolate({
    data$pre$var_names()[unlist(lapply(data$pre$var_names(), function(var) {
      data$meta$variables[[var]]$type == "Design Variable" ||
      data$meta$variables[[var]]$type == "Objective"
    }))]
  })
  varNums <- isolate({
    data$pre$var_nums_and_ints()[unlist(lapply(data$pre$var_nums_and_ints(), function(var) {
      data$meta$variables[[var]]$type == "Design Variable" ||
      data$meta$variables[[var]]$type == "Objective"
    }))]
  })
  var_facs <- c(isolate(data$pre$var_facs()), "CfgID")
  abs_min <- isolate(data$pre$abs_min())
  abs_max <- isolate(data$pre$abs_max())
  
  varsList <- reactive({
    # print("Getting Variable List.")
    idx = NULL
    req(length(input$display) > 0)
    for(choice in 1:length(input$display)) {
      mm <- match(input$display[choice],varNums)
      if(mm > 0) { idx <- c(idx,mm) }
    }
    # print(idx)
    idx
  })
  
  observe({
    input$design_configs_present
    selected <- var_facs[1]
    saved <- si_read(ns('design_config_var'))
    if(!is.null(saved) && saved %in% var_facs) {
      selected <- si(ns('design_config_var'), NULL)
    }
    updateSelectInput(session,
                      "design_config_var",
                      choices = var_facs,
                      selected = selected)
  })
  
  observe({
    choices <- levels(raw_data[[input$design_config_var]])
    selected <- choices[1]
    saved <- si_read(ns('design_config_choice'))
    if(!is.null(saved) && saved %in% choices) {
      selected <- si(ns('design_config_choice'), NULL)
    }
    updateSelectInput(session,
                      "design_config_choice",
                      choices = choices,
                      selected = selected)
  })
  
  observe({
    choices <- varNums
    selected <- choices[1]
    saved <- si_read(ns("display"))
    if (is.empty(saved)) {
      si(ns("display"), NULL)
    } else if (all(saved %in% choices)) {
      selected <- si(ns("display"), NULL)
    }
    updateSelectInput(session,
                      "display",
                      choices = choices,
                      selected = selected)
  })

  filtered_data <- reactive({
    filData <- raw_data
    if(input$design_configs_present &&
       !is.null(input$design_config_var) &&
       !is.na(input$design_config_var) &&
       !(input$design_config_var == "") &&
       !is.null(input$design_config_choice) &&
       !is.na(input$design_config_choice) &&
       !(input$design_config_choice == "")) {
      filData <- subset(filData, filData[[paste0(input$design_config_var)]] == input$design_config_choice)
    }
    filData
  })
      
  uqData <- reactive({
    # print("In uqData()")
    
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
  
  output$vars_ui <- renderUI({
    # print("In vars_ui()")
    var_directions <- c("Input",
                        "Output")
    data_mean <- apply(filtered_data()[varNums], 2, mean)
    data_sd <- apply(filtered_data()[varNums], 2, function(x) {sd(x)})
    
    choices <- varNums
    if(!input$display_all)
      choices <- varNums[varsList()]
    
    lapply(choices, function(var) {
      # UI calculations
      global_index <- which(varNames == var) # globalId
      #gaussianCondition = toString(paste0("input.gaussian",i," == true"))
      #spacefilCondition = toString(paste0("input.gaussian",i," == false"))
      
      # Persist Values
      this_direction <- isolate(input[[paste0('varDirection', global_index)]])
      if(is.null(this_direction)) {
        if(raw_info$variables[[var]]$type == "Design Variable") 
          this_direction_default <- "Input"
        else
          this_direction_default <- "Output"
        this_direction <- si(ns(paste0('varDirection', global_index)), this_direction_default)
      }
      this_gaussian <- isolate(input[[paste0('gaussian_', global_index)]])
      if(is.null(this_gaussian)) {
        this_gaussian <- si(ns(paste0('gaussian_', global_index)), FALSE)
      }
      this_gaussian_mean <- isolate(input[[paste0('gaussian_mean', global_index)]])
      if(is.null(this_gaussian_mean)) {
        this_gaussian_mean <- si(ns(paste0('gaussian_mean', global_index)), data_mean[[var]])
      }
      this_gaussian_sd <- isolate(input[[paste0('gaussian_sd', global_index)]])
      if(is.null(this_gaussian_sd)) {
        this_gaussian_sd <- si(ns(paste0('gaussian_sd', global_index)), data_sd[[var]])
      }
      this_constraint <- isolate(input[[paste0("fuq_constraint_enable", global_index)]])
      if(is.null(this_constraint)) {
        this_constraint <- si(ns(paste0("fuq_constraint_enable", global_index)), FALSE)
      }
      this_constraint_value <- isolate(input[[paste0("fuq_constraint_value", global_index)]])
      if(is.null(this_constraint_value)) {
        this_constraint_value <- si(ns(paste0("fuq_constraint_value", global_index)), data_mean[[var]])
      }
      
      fluidRow(class = "uqVar", column(12,
        # Type select
        fluidRow(
          hr(),
          column(8,
                 
                 selectInput(
                   ns(paste0('varDirection', global_index)),
                   label = var,
                   choices = var_directions,
                   selected = this_direction)
          ),
          column(4)
        ),
        conditionalPanel(condition = paste0('input["', ns(paste0('varDirection', global_index)), '"] == "Input"'),
          # Gaussian
          fluidRow(
            column(4, 
              checkboxInput(
                inputId = ns(paste0('gaussian_', global_index)),
                label = "Reshape to Gaussian",
                value = this_gaussian)
            ),
            #conditionalPanel(condition = toString(paste0('input.gaussian', global_index, ' == true')),
              column(4,
                     textInput(ns(paste0('gaussian_mean', global_index)),
                               HTML("&mu;:"),
                               placeholder = "Mean",
                               value = this_gaussian_mean)
              ),
              column(4,
                     textInput(ns(paste0('gaussian_sd',global_index)),
                               HTML("&sigma;:"),
                               placeholder = "StdDev",
                               value = this_gaussian_sd)
              )
          #  )
          ),
          # Constraint
          fluidRow(
            column(4, checkboxInput(inputId = ns(paste0("fuq_constraint_enable", global_index)),
                                    label = "Enable Constraint",
                                    value = this_constraint)),
          #  conditionalPanel(condition = toString(paste0('input.fuq_constraint_enable', global_index, ' == true')),
              column(4, textInput(inputId = ns(paste0("fuq_constraint_value", global_index)),
                                  label = NULL,
                                  value = this_constraint_value)),
              column(4)
          #  )
            
          )
        )
      ))
    })
    # print("Done with vars_ui()")
  })
  
  uqCalc <- observe({
    for(i in 1:length(varNums)){
      var <- varNums[i]
      global_index <- which(varNames == var)
      dir <- input[[paste0('varDirection', global_index)]]
      is_gaus <- input[[paste0('gaussian_', global_index)]]
      req(dir)
      directions[[var]] <<- input[[paste0('varDirection', global_index)]]
      if (directions[[var]] == "Input") {
        if(input[[paste0('gaussian_', global_index)]]) {
          input_mean <- as.numeric(input[[paste0('gaussian_mean', global_index)]])
          input_sd <- as.numeric(input[[paste0('gaussian_sd', global_index)]])
          var_sd <- sd(filtered_data()[[var]])
          if(!is.na(input_mean) && !is.na(input_sd) &&
             input_mean > abs_min[[var]] && input_mean < abs_max[[var]] &&
             input_sd > 0.1*var_sd && input_sd < 1.1*var_sd) {
            types[[var]] <<- "norm"
            params[[var]]$mean <<- input_mean
            params[[var]]$stdDev <<- input_sd
          }
        }
        else {
          types[[var]] <<- "unif"
          params[[var]]$min <<- unname(abs_min[[var]])
          params[[var]]$max <<- unname(abs_max[[var]])
        }
      }
    }
    uiInitialized <<- TRUE
  })
  
  output$vars_plots <- renderUI({
    # print("In vars_plots")
    # print(varsList())
    data <- uqData()$dist
    variables <- varNums
    # print(!input$display_all)
    if(!input$display_all){
      print("In not display all")
      variables <- varNums[varsList()]
    }
    if(is.null(data)) {
      verbatimTextOutput(ns("uq_plot_initializing"))
    }
    else {
      lapply(variables, function(var) {
        fluidRow(class = "uqVar",
          column(12,
            plotOutput(ns(paste0("uq_plot_", var)), height = 248)
          )
        )
      })
    }
  })
  
  output$uq_plot_initializing <- renderText("Initializing...")
  
  observe({
    data <- uqData()$dist
    variables <- varNums
    if(!input$display_all)
      variables <- varNums[varsList()]
    if(!is.null(data)) {
      lapply(variables, function(var) {
        par(mar = rep(2, 4))
        filtered_data_histo <- hist(filtered_data()[[var]], freq = FALSE, breaks=30)
        x_bounds <- c(min(filtered_data_histo$breaks, data[[var]][["xOrig"]], data[[var]][["xResampled"]]),
                      max(filtered_data_histo$breaks, data[[var]][["xOrig"]], data[[var]][["xResampled"]]))
        y_bounds <- c(0,
                      max(filtered_data_histo$density, data[[var]][["yOrig"]], data[[var]][["yResampled"]]))
        # print(paste(var, x_bounds, y_bounds))
        output[[paste0("uq_plot_", var)]] <- renderPlot({
          hist(filtered_data()[[var]],
               freq = FALSE,
               col = input$hist_color,
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
          if (directions[[var]] == "Input") {
            lines(data[[var]][["xOrig"]],
                  data[[var]][["yOrig"]],
                  col = input$orig_color, lwd=2)
          }
          lines(data[[var]][["xResampled"]],
                data[[var]][["yResampled"]],
                col = input$resamp_color, lwd=2)
          if (!is.null(forwardUQData()) & !is.null(forwardUQData()[[var]])) {
            lines(forwardUQData()[[var]]$postPoints,
                  forwardUQData()[[var]]$postPdf,
                  col=input$post_color, lwd=2)
          }
          box(which = "plot", lty = "solid", lwd=2, col=boxColor(var))
        }, height = 248)
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
  
  output$display_queries <- reactive({
    display <- !(input$tabset == "Design Ranking")
  })
  
  outputOptions(output, "display_queries", suspendWhenHidden=FALSE)
  
  if(is.empty(tab_data)) {
    probability_queries <<- reactiveValues(rows = c())
    query_id <<- 0
  } else {
    probability_queries <<- reactiveValues(rows = tab_data$probability_queries)
    query_id <<- tab_data$query_id
    lapply(tab_data$probability_queries, function(row) {
      id <- as.numeric(row)
      insertUI(
        selector = '#probabilities_ui',
        ui = tags$div(
          fluidRow(
            column(1, actionButton(ns(paste0('removeProbability', id)), 'Delete')),
            column(3, selectInput(ns(paste0('queryVariable', id)),
                                  NULL,
                                  choices = varNums,
                                  selected = si(ns(paste0('queryVariable', id)), varNums[[1]]))),
            column(2, selectInput(ns(paste0('queryDirection', id)),
                                  NULL,
                                  choices = c("Above", "Below"),
                                  selected = si(ns(paste0('queryDirection', id)),"Above"))),
            column(2, textInput(ns(paste0('queryThreshold', id)),
                                NULL,
                                value = si(ns(paste0('queryThreshold', id))))),
            column(2, textOutput(ns(paste0('queryValue', id)))),
            column(2)
          ),
          id = paste0('probabilityQuery', id)
        )
      )
    })
    i <- 0
    while(i < query_id) {
      si(ns(paste0('queryVariable', i)), NULL)
      si(ns(paste0('queryDirection', i)), NULL)
      si(ns(paste0('queryThreshold', i)), NULL)
      i <- i + 1
    }
  }
  
  observeEvent(input$add_probability, {
    id <- query_id
    query_id <<- query_id + 1
    insertUI(
      selector = '#probabilities_ui',
      ui = tags$div(
        fluidRow(
          column(1, actionButton(ns(paste0('removeProbability', id)), 'Delete')),
          column(3, selectInput(ns(paste0('queryVariable', id)), NULL, choices = varNums, selected = varNums[[1]])),
          column(2, selectInput(ns(paste0('queryDirection', id)), NULL, choices = c("Above", "Below")), selected = "Above"),
          column(2, textInput(ns(paste0('queryThreshold', id)), NULL)),
          column(2, textOutput(ns(paste0('queryValue', id)))),
          column(2)
        ),
        id = paste0('probabilityQuery', id)
      )
    )
    probability_queries$rows <<- c(probability_queries$rows, toString(id))
  })
  
  removeProbability <- observe({
    lapply(probability_queries$rows, function(id) {
      observeEvent(input[[paste0('removeProbability', id)]], {
        removeUI(
          ## pass in appropriate div id
          selector = paste0('#probabilityQuery', id)
        )
        if (length(probability_queries$rows) == 1) {
          probability_queries$rows <<- NULL
        } else {
          probability_queries$rows <<- probability_queries$rows[sapply(probability_queries$rows, function(x) {x!=id})]
        }
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
  
  runQueries <- observeEvent(input$run_probabilities_queries, {
    print("Started Calculating Probabilities.")
    lapply(1:length(probability_queries$rows), function(i) {
      id <- probability_queries$rows[i]
      name <- input[[paste0('queryVariable', id)]]
      direction <- input[[paste0('queryDirection', id)]]
      threshold <-input[[paste0('queryThreshold', id)]]
      req(threshold)
      data <- uqData()$dist[[name]]
      
      value <- integrateData(data$xResampled,
                             data$yResampled,
                             min(data$xResampled),
                             as.numeric(threshold))
      value <- max(0,min(1,value))
      if (direction == "Above") {
        value <- (1-value)
      }
      
      print(paste("Query: ", name, direction, threshold, value))
      
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
  #     #   column(2, checkboxInput(ns(paste0("fuq_constraint_enable", id)), NULL)),
  #     #   column(6, paste(input)),
  #     #   column(4, textInput(ns(paste0("fuq_constraint_value", id)), NULL, value = toString(apply(filtered_data()[input], 2, mean))))
  #     # )
  #   })
  # })
  
  forwardUQData <- reactive({
    temp_results <- NULL
    if(input$run_forward_uq){
      isolate({
        numberOfInputs <- 0
        for (i in 1:length(uqInputs())) {
          global_i = which(uqInputs()[i] == varNames)
          if (input[[paste0("fuq_constraint_enable", global_i)]]) {
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
      if (input[[paste0("fuq_constraint_enable", global_i)]]) {
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
      observations[[paste(constrainedInputs[i])]] = as.numeric(input[[paste0("fuq_constraint_value", which(constrainedInputs[i] == varNames))]])
      observationsIndex <- c(observationsIndex, which(names(originalDataTrimmed) == constrainedInputs[i]))
    }
    observations <- as.data.frame(observations)
    
    print("Finished Processing ForwardUQ.")
    
    results <- forwardUq(originalDataTrimmed, resampledDataTrimmed, rhoTrimmed, observations, observationsIndex)[[1]]
  }
    
  #-------------REMOVE ME------------------------
  # output$fvars_plots <- renderUI({
  #   data <- forwardUQData()
  #   lapply(uqOutputs(), function(output) {
  #     renderText(paste("Forward UQ plots for", output, "here."))
  #   })
  # })
  
  # Design Ranking -----------------------------------------------------------
  
  runFullProbability <- eventReactive(input$run_probabilities, {
    data <- data.frame(Config = character(0), stringsAsFactors=FALSE)
    for(i in 1:length(probability_queries$rows)) {
      id <- probability_queries$rows[i]
      data[[paste0('Query', id)]] <- numeric(0)
    }
    configs <- levels(raw_data[[paste0(input$design_config_var)]])
    # print(data)
    for (i in 1:length(configs)) {
      config <- configs[i]
      # print(paste(config))
      configData <- subset(raw_data, raw_data[[paste0(input$design_config_var)]] == config)
      configData <- configData[varNums]
      resampledData <- resampleData(configData, directions, types, params)$dist
      answers <- c(paste0(config))
      for(j in 1:length(probability_queries$rows)) {
        id <- probability_queries$rows[j]
        name <- input[[paste0('queryVariable', id)]]
        direction <- input[[paste0('queryDirection', id)]]
        threshold <-input[[paste0('queryThreshold', id)]]
        req(threshold)
        value <- integrateData(resampledData[[name]][["xResampled"]],
                               resampledData[[name]][["yResampled"]],
                               min(resampledData[[name]][["xResampled"]]),
                               as.numeric(threshold))
        value <- max(0,min(1,value))
        if (direction == "Above") {
          value <- (1-value)
        }
        answers <- c(answers, value)
      }
      data[nrow(data)+1,] <- answers
    }
    # print(data)
    data
  })
  
  output$probabilities_weight_ui <- renderUI({
    lapply(probability_queries$rows, function(id) {
      variable <- input[[paste0('queryVariable', id)]]
      direction <- input[[paste0('queryDirection', id)]]
      threshold <-input[[paste0('queryThreshold', id)]]
      
      fluidRow(
        column(1, paste0("Query", id)),
        column(1, paste0("Config")),
        column(3, paste(variable, tolower(direction), threshold)),
        column(4, selectInput(ns(paste0("probImpact", id)), NULL, choices = c("Positive", "Negative"), selected = "Positive")),
        column(3, sliderInput(ns(paste0("probWeight", id)), NULL, 0, 1, 1, step = 0.05))
      )
    })
  })
  
  topsisProbability <- reactive({
    outputData <- runFullProbability()
    decisions <- data.matrix(outputData[,-1])
    weights <- NULL
    impacts <- NULL
    for(i in 1:length(probability_queries$rows)) {
      id <- probability_queries$rows[i]
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
        outputData <- cbind("Rank" = 1:nrow(decisions), outputData[rev(order(outputData[[paste0("Query", probability_queries$rows[1])]])),])
      }
      else {
        outputData <- cbind("Rank" = 1:nrow(decisions), outputData[order(outputData[[paste0("Query", probability_queries$rows[1])]]),])
      }
    }
    outputData
  })
  
  output$probabilities_table <- DT::renderDataTable({
    topsisProbability()
  })
  
}

TabData <- function() {
  list(probability_queries=isolate(probability_queries$rows),
       query_id=query_id)
}