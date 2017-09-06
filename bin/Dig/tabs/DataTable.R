title <- "Data Table"
footer <- TRUE

ui <- function(id) {
  ns <- NS(id)
  
  tabPanel("Data Table",
    br(),
    # fluidRow(
    #   column(4, 
    #     tags$div(title = "Rounds data in all tables to a set number of decimal places", 
    #     checkboxInput(ns("roundTables"), "Round Data Table", value = FALSE))),
    #   column(8, 
    #     conditionalPanel(paste0("input.", ns("roundTables"), " == '1'"), 
    #       tags$div(title = "Maximum number of decimals to show in data tables.", 
    #       sliderInput(ns("numDecimals"), "Decimal Places", min = 0, max = 11, step = 1, value = 4))))
    # ),
    # checkboxInput(ns("autoRanking"), "Automatic Refresh", value = TRUE),
    wellPanel(
      h4("Data Processing"),
      checkboxInput(ns("use_filtered"), "Apply Filters", value=si(ns("use_filtered"), TRUE)),
      fluidRow(column(4,selectInput(ns("process_method"), "Method", choices = c("None", "TOPSIS"), selected = si(ns("process_method"), "None")))),
      conditionalPanel(condition = paste0("input['", ns("process_method"), "'] != 'None'"),
        # conditionalPanel(condition = paste0("input['", ns("autoRanking"), "'] == false"),
        #   actionButton(ns("applyRanking"), "Apply Ranking"),
        #   br(), br()
        # ),
        fluidRow(
          column(4,
            selectInput(ns("weightMetrics"),
                        "Ranking Metrics:",
                        c(),
                        multiple = TRUE,
                        selected = NULL),
            actionButton(ns("clearMetrics"), "Clear Metrics")
          )
        ),
        conditionalPanel(condition = paste0("input['", ns("weightMetrics"), "'] != null"),
          hr(),
          fluidRow(
            column(3, strong(h4("Variable Name"))),
            column(2, strong(h4("Ranking Mode"))),
            # conditionalPanel(condition = paste0("input['", ns("process_method"), "'] == 'TOPSIS'"),
              column(7, strong(h4("Weight Amount")))
            # ),
            # conditionalPanel(condition = paste0("input['", ns("process_method"), "'] == 'Simple Metric w/ TxFx'"),
            #   column(3, strong(h4("Weight Amount"))),
            #   column(4, strong(h4("Transfer Function")))
            # )
          ),
          uiOutput(ns("rankings")),
          hr(),
          actionButton(ns("save_ranking"), "Add Ranking as Classification")
        )
      )
    ),
    wellPanel(
      style = "overflow-x:auto",
      DT::dataTableOutput(ns("dataTable"))  #,
      # downloadButton(ns("exportPoints"), "Export Selected Points"), 
      # actionButton(ns("colorRanked"), "Color by Selected Rows")
      #checkboxInput(ns("transpose"), "Transpose Table", value = FALSE)
    )
  )
}



server <- function(input, output, session, data) {
  
  ns <- session$ns

  var_names <- data$pre$var_names
  var_range_nums_and_ints <- data$pre$var_range_nums_and_ints
  var_range_nums_and_ints_list <- data$pre$var_range_nums_and_ints_list
  
  LocalData <- reactive({
    if (input$use_filtered) {
      local_data <- data$Filtered()
    } else {
      local_data <- data$raw$df
    }
    local_data
  })
  
  ## Setup Data Frame for Transfer Functions
  transfer_functions <- data.frame()
  transfer_functions <- transfer_functions[1:4,]
  row.names(transfer_functions) <- c("Values", "Scores", "Slopes", "Y_ints")
  
  makeReactiveBinding("transfer_functions")
  
  SlowLocalData <- eventReactive(input$updateDataTable, {
    LocalData()
    if(input$roundTables)
      RoundDataFrame(LocalData(), input$numDecimals)
  })
  
  RoundDataFrame <- function(x, digits) {
    # round all numeric variables
    # x: data frame 
    # digits: number of digits to round
    numeric_columns <- sapply(x, class) == 'numeric'
    x[numeric_columns] <-  round(x[numeric_columns], digits)
    x
  }
  
  #Dynamic metrics list
  MetricsList <- reactive({
    idx = NULL
    req(input$weightMetrics)
    # print("Getting Metrics List.")
    for(choice in 1:length(input$weightMetrics)) {
      mm <- match(input$weightMetrics[choice],var_names())
      if(mm > 0) { idx <- c(idx,mm) }
    }
    # print(idx)
    idx
  })
  
  #Event handler for "clear metrics" button
  observe({
    selected <- isolate(input$weightMetrics)
    saved <- si_read(ns("weightMetrics"))
    if(input$clearMetrics || input$process_method == "None") {
      selected <- NULL
    } else if (!is.null(saved) && ((length(saved) == 0)
               || saved %in% c(var_range_nums_and_ints(), ""))) {
      selected <- si(ns("weightMetrics"))
    }
    updateSelectInput(session,
                      "weightMetrics",
                      choices = var_range_nums_and_ints_list(),
                      selected = selected)
  })
  
  #UI class for each metric UI
  generateMetricUI <- function(current, slider, radio, util, func) {
    
    if(missing(slider) & missing(radio) & missing(util) & missing(func)){
      slider_val <- input[[paste0('rnk', current)]]
      if(is.null(slider_val))
        slider_val <- 1
      
      radio_val <- input[[paste0('sel', current)]]
      if(is.null(radio_val))
        radio_val <- "Min"
      
      util_val <- input[[paste0('util', current)]]
      if(is.null(util_val))
        util_val <- FALSE
      
      func_val <- input[[paste0('func', current)]]
    }
    else{
      slider_val <- slider
      radio_val <- radio
      util_val <- util
      func_val <- func
    }
    
    if(input$process_method == 'TOPSIS')
      TOPSISMetricUI(current, radio_val, slider_val)
    else if(input$process_method == 'Simple Metric w/ TxFx')
      SimpleMetricUI(current, radio_val, slider_val, util_val, func_val)
  }
  
  TOPSISMetricUI <- function(current, radio_val, slider_val) {
    
    varName <- var_names()[current]
    
    fluidRow(
      column(3, h5(varName)),
      column(2, radioButtons(ns(paste0('sel', current)),
                             NULL,
                             choices = c("Min", "Max"),
                             selected = radio_val,
                             inline = TRUE)),
      column(7, sliderInput(ns(paste0('rnk', current)),
                            NULL,
                            step = 0.01,
                            min = 0.01,
                            max = 1,
                            value = slider_val))
    )
  }
  
  SimpleMetricUI <- function(current, radio_val, slider_val, util_val, func_val) {
    
    varName <- var_names()[current]
    transferCondition = toString(paste0("input.util",current," == true"))
    
    fluidRow(
      column(3, h5(varName)),
      column(2, radioButtons(ns(paste0('sel', current)),
                             NULL,
                             choices = c("Min", "Max"),
                             selected = radio_val,
                             inline = TRUE)),
      column(3, sliderInput(ns(paste0('rnk', current)),
                            NULL,
                            step = 0.01,
                            min = 0,
                            max = 1,
                            value = slider_val)),
      column(1, checkboxInput(ns(paste0('util', current)),
                              "Add Transfer Function",
                              value = util_val)),
      column(3, conditionalPanel(condition = transferCondition,
                                 textInput(ns(paste0('func', current)),
                                           "Enter Data Points",
                                           placeholder = paste0("Value = Score | e.g. ",
                                                                min(LocalData()[[varName]]),
                                                                " = 1, ",
                                                                max(LocalData()[[varName]]),
                                                                "= 0.5"),
                                           value = func_val),
                                 UtilityPlot(current)))
    )
  }
  
  FullMetricUI <- reactive({
    a <- input$process_method # Force reaction
    lapply(MetricsList(), function(column) {
      isolate(generateMetricUI(column))
    })
  })
  
  #Output plot of transfer function
  UtilityPlot <- function(current){
    plot_name <- paste0("transfer_plot", current)
    f <- list(
      family = "Courier New, monospace",
      size = 18,
      color = "#7f7f7f"
    )
    x <- list(
      title = var_names()[current],
      titlefont = f
    )
    y <- list(
      title = "Score",
      titlefont = f
    )
    renderPlot({
      plot_points <- ParseUserInputPoints(current)
      req(plot_points)
      par(mar = c(4.5,4.5,1,1))
      p <- plot(x = unlist(lapply(names(plot_points), as.numeric)),
                y = unname(plot_points),
                xlab = var_names()[current],
                ylab = "Score")
      if(length(plot_points) > 1){
        for(i in 1:(length(plot_points)-1)){
          segments(x0 = as.numeric(names(plot_points)[i]),
                   y0 = plot_points[i],
                   x1 = as.numeric(names(plot_points)[i+1]),
                   y1 = plot_points[i+1])
          
        }
      }
      p
    }, height = 150)
  }
  
  
  
  #Calculate line slopes & intercepts of transfer function
  ProcessLineEquations <- function(current, data_set){
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
    transfer_functions[[toString(current)]] <<- list(names(data_set), data_set, slopes, y_ints)
  }
  
  #Parse transfer function text input
  ParseUserInputPoints <- function(current){
    x_vals <- NULL
    y_vals <- NULL
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
      x_val <- current_val
      y_val <- as.numeric(current_point[2])
      if(y_val < low)
        y_val = low
      else if(y_val > hi)
        y_val = hi
      x_vals <- c(x_vals, x_val)
      y_vals <- c(y_vals, y_val)
    }
    
    if(is.null(x_vals) | is.null(y_vals)){
      transfer_functions[[toString(current)]] <<- NULL
      output_vals <- NULL
    }
    else{
      #Sort list by 'x_vals' (while paired with y_vals)
      unsorted_vals <- x_vals
      names(unsorted_vals) <- y_vals
      sorted_vals <- sort(unsorted_vals)
      
      #Flip-flop names&values of a named list
      output_vals <- sapply(names(sorted_vals), as.numeric)
      names(output_vals) <- sorted_vals
      ProcessLineEquations(current, output_vals)
    }
    
    output_vals
  }
  
  
  
  
  #Dynamic UI rendering for weighted metrics list
  output$rankings <- renderUI({
    req(MetricsList())
    # print("In render ranking sliders")
    FullMetricUI()
  })
  
  
  #Process metric weights
  RankData <- reactive({
    req(MetricsList())
    # print("In calculate ranked data")
    data <- LocalData()[var_range_nums_and_ints()]
    norm_data <- data.frame(t(t(data)/apply(data,2,max)))
    
    score_data <- sapply(row.names(norm_data) ,function(x) 0)
    
    for(i in 1:length(MetricsList())) {
      column <- var_names()[MetricsList()[i]]
      rank_name <- paste0("rnk", toString(MetricsList()[i]))
      weight <- input[[rank_name]]
      req(weight)
      
      transfer_function <- transfer_functions[[toString(MetricsList()[i])]]
      transfer_active <- input[[paste0('util', MetricsList()[i])]]
      if(is.null(transfer_active))
        transfer_active <- TRUE
      if(!is.null(transfer_function) & transfer_active){
        norm_val <- max(data[[column]])
        transfer_data <- data[[column]]
        for(t in 1:length(transfer_data)){
          item <- transfer_data[t]
          transfer_data[t] <- linearly_interpolate(item, transfer_function)
        }
        score_data <- score_data + transfer_data*weight
      }
      else{
        radio_select <- paste0("sel", toString(MetricsList()[i]))
        if(input[[radio_select]] == "Min"){
          colMin <- min(norm_data[column])
          for(j in 1:length(unlist(norm_data[column]))) {
            item <- norm_data[j,column]
            norm_data[j,column] <- 1 -item + colMin
          }
        }
        score_data <- score_data + unlist(unname(weight*norm_data[column]))
      }
    }
    score_data <- score_data/max(score_data)
    
    score_data <- sort(score_data, decreasing = TRUE)
    score <- score_data
    rank <- seq(length(score))
    
    data <- LocalData()[names(score_data), ]
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
  
  SlowRankData <- eventReactive(input$applyRanking, {
    RankData()
  })
  
  TOPSISData <- reactive({
    req(MetricsList())
    # print("In calculate topsis data")
    # print(names(LocalData()))
    data <- LocalData()[var_range_nums_and_ints()]
    weights <- NULL
    impacts <- NULL
    for(i in 1:length(var_range_nums_and_ints())){
      global_index <- match(var_range_nums_and_ints()[i], var_names())
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
    output_data <- LocalData()[sorted_topsis$alt.row,]
    output_data <- cbind(rank, score, output_data)
    output_data
  })
  
  SlowTOPSISData <- eventReactive(input$applyRanking, {
    TOPSISData()
  })
  
  output$dataTable <- DT::renderDataTable({
    if(length(input$weightMetrics) > 0){
      if(input$process_method == 'TOPSIS'){
        if(TRUE)  #input$autoRanking)
          table_data <- TOPSISData()
        else
          table_data <- SlowTOPSISData()
      }
      else{
        if(TRUE)  #input$autoRanking)
          table_data <- RankData()
        else
          table_data <- SlowRankData()
      }
    }
    else{
      table_data <- LocalData()
    }
    # if(input$roundTables)
    #   data <- RoundDataFrame(data, input$numDecimals)
    #if(input$transpose)
    #  data <- t(data)
    names(table_data) <- sapply(names(table_data), function(name) {
      data$meta$variables[[name]]$name_with_units
    })
    table_data
  })
  
  #Download handler
  output$exportPoints <- downloadHandler(
    filename = function() { paste('ranked_points-', Sys.Date(), '.csv', sep='') },
    content = function(file) { 
      if(input$process_method != "None")
        write.csv(RankData()[input$dataTable_rows_selected, ], file)
      else
        write.csv(LocalData()[input$dataTable_rows_selected, ], file) 
    }
  )
  
  
  observeEvent(input$save_ranking, {
    number <- 1
    name <- paste0("rank", number)
    while(!is.null(data$meta$variables[[name]])) {
      number <- number + 1
      name <- paste0("rank", number)
    }
    data$meta$variables[[name]] <- list(type="Classification",
                                        date=toString(Sys.time()),
                                        name_with_units=name,
                                        user=Sys.info()[["user"]])
    new_column <- RankData()[c("rank", "GUID")]
    names(new_column) <- c(name, "GUID")
    # merge based on GUID
    print(head(new_column))
    print(paste0("Saved Ranking: ", name))
  })
  
}