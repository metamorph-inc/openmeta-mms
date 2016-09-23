library(shiny)
library(DT)
#options(shiny.trace=TRUE)
#options(shiny.fullstacktrace = TRUE)
#options(error = function() traceback(2))
#options(shiny.error = function() traceback(2))


#---------------------Global Variables-------------------------#
palette(c("#E41A1C", "#377EB8", "#4DAF4A", "#984EA3",
         "#FF7F00", "#FFFF33", "#A65628", "#F781BF", "#999999"))

importData <- NULL #import session data

#Initialize empty data frame for transfer functions in Ranking tab
gX <- data.frame()
gX <- gX[1:4,]
row.names(gX) <- c("Values", "Scores", "Slopes", "Y_ints")
#-------------------End Global Variables-----------------------#


shinyServer(function(input, output, session) {
  
  xFuncs <- gX
  makeReactiveBinding("xFuncs")

  importFlags <- reactiveValues(tier1 = FALSE, tier2 = FALSE, ranking = FALSE)
  
  session$onSessionEnded(function() {
    stopApp()
  })
  
  # Get Data -----------------------------------------------------------------
  raw <- c()
  query <- parseQueryString(isolate(session$clientData$url_search))

  # output$debug <- renderText({
  #   paste(names(query), query, sep = "=", collapse=", ")
  # })

  if (!is.null(query[['csvfilename']])) {
    print("In read raw")
    # raw.csv(paste0(dirname(sys.frame(1)$ofile), "/../webserver/public/csvs/", query[['csvfilename']]), fill=T)
    # raw = read.csv(paste0(dirname("/csvs/", query[['csvfilename']]), fill=T)
    raw = read.csv(paste("/media/sf_kevin/Downloads/", query[['csvfilename']], sep=''), fill=T)
  }
  else if (nzchar(Sys.getenv('DIG_INPUT_CSV')))
  {
    raw = read.csv(Sys.getenv('DIG_INPUT_CSV'), fill=T)
  }
  else
  {
    # raw = read.csv("../data.csv", fill=T)
    raw = read.csv("../../../results/mergedPET.csv", fill=T)
    # raw = iris
  }
  
  # Import/Export Session Settings -------------------------------------------
  
  #Credit: Henrik Bengtsson source:https://stat.ethz.ch/pipermail/r-help/2007-June/133564.html
  fileChoose <- function(...) {
    pathname <- NULL;
    tryCatch({
      pathname <- file.choose();
    }, error = function(ex) {
    })
    pathname;
  }
  
  initImport <- observeEvent(input$importSession, {
    file <- fileChoose()
    req(file)
    importData <<- read.csv(file, header = TRUE, strip.white = TRUE)
    importFlags$tier1 <- TRUE
  })
  
  tier1Flush <- observeEvent(importFlags$tier1, {
    if(importFlags$tier1){
      print("In tier 1 upload")
      
      tier1CheckBox <- c("removeMissing",
                         "removeOutliers",
                         "stickyFilters",
                         "roundTables",
                         "autoRender",
                         "trendLines",
                         "upperPanel",
                         "autoInfo",
                         "autoData",
                         "autoRange",
                         "viewAllFilters")
      
      tier1Selects <- c("colVarNum",
                        "display",
                        "xInput",
                        "yInput",
                        "colType",
                        "colVarFactor",
                        "numDevs",
                        "numDecimals",
                        "pointSize",
                        "pointStyle",
                        "radio",
                        "weightMetrics")
                     
      tier1Colors <- c("normColor", 
                       "minColor", 
                       "maxColor", 
                       "midColor", 
                       "highlightColor",
                       "rankColor")
      
      for(i in 1:length(tier1CheckBox)){
        current <- tier1CheckBox[i]
        trimmedValue <- gsub("^\\s+|\\s+$", "", importData[[current]])
        updateCheckboxInput(session, current, value = as.logical(trimmedValue))
      }
      for(i in 1:length(tier1Selects)){
        current <- tier1Selects[i]
        parsedValue <- as.list(strsplit(toString(importData[[current]]), ", ")[[1]])
        trimmedValue <- gsub("^\\s+|\\s+$", "", parsedValue)
        updateSelectInput(session,
                          current,
                          selected = trimmedValue)
      }
      for(i in 1:length(tier1Colors)){
        current <- tier1Colors[i]
        trimmedValue <- gsub("^\\s+|\\s+$", "", importData[[current]])
        updateColourInput(session, current, value = trimmedValue)
      }
      updateTabsetPanel(session, "inTabset", "Pairs Plot")
    }
  })
  
  tier2Flush <- observeEvent(importFlags$tier2, {
    if(importFlags$tier2){
      print("In tier 2 upload - updating colslider/sliders")
      
      rng <- as.numeric(unlist(strsplit(toString(importData[['colSlider']]), ", ")))
      updateSliderInput(session, 'colSlider', value = rng)
      
      for(i in 1:length(colnames(importData))){
        
        current <- colnames(importData)[i]
        column <- as.numeric(gsub("[^0-9]", "", current)) #Extract number
        # print(paste("Current:",current, "Column:",column))
        
        if(!is.null(importData[[current]]) & !is.na(column)){
          if (varClass[column] == "factor" & length(names(table(raw[varNames[column]]))) > 1) {
            parsedValue <- as.list(strsplit(toString(importData[[current]]), ", ")[[1]])
            trimmedValue <- gsub("^\\s+|\\s+$", "", parsedValue)
            updateSelectInput(session, current, selected = trimmedValue)
          }
          else {
            if(varClass[column] == "numeric" | varClass[column] == "integer") {
              rng <- as.numeric(unlist(strsplit(toString(importData[[current]]), ", ")))
              updateSliderInput(session, current, value = rng)
            }
          }
        }
      }
    }
  })
  
  #Call when user saves data
  output$exportSession <- downloadHandler(
    filename = function() { 
      if (input$sessionName == ""){
        paste0('session_', Sys.Date(), Sys.time(), '.csv')
      }
      else {
        paste0(input$sessionName, '.csv')
      }
    },
    
    content = function(file) {
      write.table(
        x = lapply(reactiveValuesToList(input), toString),
        file,
        row.names = FALSE,
        col.names = TRUE,
        sep = ", ",
        quote = TRUE
      )
    }
  )
  
  raw_plus <- reactive({
    print("In raw plus")
    data <- raw
    
    if(input$removeMissing){
      #Filter out rows with missing data
      data <- data[complete.cases(data),]
      
    }
    
    if(input$removeOutliers){
      #Filter out rows by standard deviation
      for(column in 1:length(varNum)) {
        a <- sapply(data[varNum[column]], 
          function(x) {
            m <- mean(x, na.rm = TRUE)
            s <- sd(x, na.rm = TRUE)
            x >= m - input$numDevs*s &
            x <= m + input$numDevs*s
          }
        )
        data <- subset(data, a)
      }
    }
    data
  })

  # Pre-processing -----------------------------------------------------------
  
  print("Starting Preprocessing of the Data -----------------------------------------")
  
  varNames = names(raw)
  varClass = sapply(raw,class)
  print(paste("varClass:"))
  print(paste(varClass))
  
  varFac <- varNames[varClass == "factor"]
  print(paste("varFac:"))
  print(paste(varFac))
  
  varNum <- varNames[varClass != "factor"]
  print(paste("varNum:"))
  print(paste(varNum))
  
  rawAbsMin <- reactive({
    print("In rawAbsMin")
    apply(raw_plus()[varNum], 2, min, na.rm=TRUE)
  })
  
  rawAbsMax <- reactive({
    print("In rawAbsMax")
    apply(raw_plus()[varNum], 2, max, na.rm=TRUE)
  }) 
  
  varRangeNum <- reactive({
    print(paste("varRangeNum:"))
    answer <- varNum[(rawAbsMin() != rawAbsMax()) & (rawAbsMin() != Inf)]
    print(paste(answer))
    answer
  })
  
  varRangeFac <- reactive({
    print(paste("varRangeFac:"))
    answer <- varFac[apply(raw_plus()[varFac], 2, function(x) (length(names(table(x))) > 1))]
    print(paste(answer))
    answer
  })
  
  varRange <- reactive({
    print(paste("varRange:"))
    answer <- c(varRangeFac(), varRangeNum())
    print(paste(answer))
    answer
  })
  

  observe({
    
    isolate({
    print("Updating Panel Selections...")
    updateSelectInput(session, "colVarFactor", choices = varRangeFac())  
    updateSelectInput(session, "colVarNum", choices = varRangeNum())#, selected = varRangeNum()[c(1)])
    updateSelectInput(session, "display", choices = varRange(), selected = varRange()[c(1,2)])
    updateSelectInput(session, "weightMetrics", choices = varRangeNum(), selected = NULL)
    updateSelectInput(session, "xInput", choices = varRange(), selected = varRange()[c(1)])
    updateSelectInput(session, "yInput", choices = varRange(), selected = varRange()[c(2)])
    })   
    
  })
     
  resetPlotOptions <- observeEvent(input$resetOptions, {
    print("In resetPlotOptions()")
    updateSelectInput(session, "display", selected = varRange()[c(1,2)])
    updateSelectInput(session, "colType", selected = "None")
  })

  
  resetViewerOptions <- observeEvent(input$resetSettings, {
    print("In resetViewerSettings()")
    
    # Processing
    updateCheckboxInput(session, "removeMissing", value = TRUE)
    updateCheckboxInput(session, "removeOutliers", value = FALSE)
    
    # Render
    updateCheckboxInput(session, "autoRender", value = TRUE)
    updateCheckboxInput(session, "trendLines", value = FALSE)
    updateCheckboxInput(session, "upperPanel", value = FALSE)
    
    # Data point style
    updateRadioButtons(session, "pointStyle", selected = "1")
    updateRadioButtons(session, "pointSize", selected = "1")
    
    # Automatically update
    updateCheckboxInput(session, "autoInfo", value = TRUE)
    updateCheckboxInput(session, "autoData", value = TRUE)
    updateCheckboxInput(session, "autoRange", value = TRUE)
    
    # Color
    updateColourInput(session, "normColor", "Normal", "black")
    updateColourInput(session, "maxColor", "Worst", "#E74C3C")
    updateColourInput(session, "midColor", "In Between", "#F1C40F")
    updateColourInput(session, "minColor", "Best", "#2ECC71")
    updateColourInput(session, "highlightColor", "Highlighted", "#377EB8")
  })

  print(paste("Finished Preprocessing the Data ----------------------------------------------------"))
  
  # Filters (Enumerations, Sliders) and Constants ----------------------------
  
  output$filters <- renderUI({
    req(input$display)
    print("In render filters")
    fullFilterUI()
  })
  
  filterVars <- reactive({
    req(input$display)
    if(input$viewAllFilters)
      match(varRange(), varNames)
    else
      varsList()
  })
  
  fullFilterUI <- reactive({
    vars <- filterVars()
    data <- raw_plus()
    
    facVars <- NULL
    intVars <- NULL
    numVars <- NULL
    
    for(i in 1:length(vars)){
      current <- vars[i]
      if(varClass[current] == "factor")
        facVars <- c(facVars, current)
      else if(varClass[current] == "integer")
        intVars <- c(intVars, current)
      else if(varClass[current] == "numeric")
        numVars <- c(numVars, current)
    }
    
    isolate({
      wellPanel(
        fluidRow(
          lapply(facVars, function(column) {
              generateEnumUI(column)
          })
        ),
        fluidRow(
          lapply(intVars, function(column) {
            generateIntegerSliderUI(column)
          }),
          lapply(numVars, function(column) {
            generateNumericSliderUI(column)
          })
        )
      )
    })
  })
  
  generateEnumUI <- function(current) {
    items <- names(table(raw_plus()[varNames[current]]))
    
    selectVal <- input[[paste0('inp', current)]]
    if(is.null(selectVal) | !input$stickyFilters)
      selectVal <- items
    
    column(2, selectInput(paste0('inp', current),
                          varNames[current],
                          multiple = TRUE,
                          selectize = FALSE,
                          choices = items,
                          selected = selectVal)
    )
    
    
  }
  
  generateNumericSliderUI <- function(current) {
    
    max <- as.numeric(unname(rawAbsMax()[varNames[current]]))
    min <- as.numeric(unname(rawAbsMin()[varNames[current]]))
    step <- max((max-min)*0.01, abs(min)*0.001, abs(max)*0.001)
    
    if (min != max) {
    
      sliderVal <- input[[paste0('inp', current)]]
      if(is.null(sliderVal) | !input$stickyFilters)
        sliderVal <- c(signif(min-step*10, digits = 4), signif(max+step*10, digits = 4))
      
      column(2, sliderInput(paste0('inp', current),
                            varNames[current],
                            step = signif(step, digits = 4),
                            min = signif(min-step*10, digits = 4),
                            max = signif(max+step*10, digits = 4),
                            value = sliderVal)
      )
    }
  }
  
  generateIntegerSliderUI <- function(current) {
    
    max <- as.numeric(unname(rawAbsMax()[varNames[current]]))
    min <- as.numeric(unname(rawAbsMin()[varNames[current]]))
    
    if(min != max) {
      
      sliderVal <- input[[paste0('inp', current)]]
      if(is.null(sliderVal) | !input$stickyFilters)
        sliderVal <- c(min, max)
      
      column(2, sliderInput(paste0('inp', current),
                            varNames[current],
                            min = min,
                            max = max,
                            value = sliderVal)
      )
    }
    
  }
  
  
  output$constants <- renderUI({
    print("In render constants")
    fluidRow(
      lapply(1:length(varNames), function(column) {
        # print(paste(column, varNames[column], varClass[column]))
        if(varClass[column] == "numeric") {
          max <- as.numeric(unname(rawAbsMax()[varNames[column]]))
          min <- as.numeric(unname(rawAbsMin()[varNames[column]]))
          diff <- (max-min)
          if (diff == 0) {column(2, p(strong(paste0(varNames[column],":")), min))}
        } else {
          if (varClass[column] == "integer") {
            max <- as.integer(unname(rawAbsMax()[varNames[column]]))
            min <- as.integer(unname(rawAbsMin()[varNames[column]]))
            if (min == max) {column(2, p(strong(paste0(varNames[column],":")), min))}
          } else {
            if (varClass[column] == "factor" & length(names(table(raw_plus()[varNames[column]]))) == 1) {
              column(2, p(strong(paste0(varNames[column],":")), names(table(raw_plus()[varNames[column]]))))
            }
          }
        }
      })
    )
  })
  
  
  
  observeEvent(input$resetSliders, {
    print("In resetDefaultSliders()")
    for(column in 1:length(varNames)) {
      if(varClass[column] == "numeric") {
        max <- as.numeric(unname(rawAbsMax()[varNames[column]]))
        min <- as.numeric(unname(rawAbsMin()[varNames[column]]))
        diff <- (max-min)
        if (diff != 0) {
          step <- max(diff*0.01, abs(min)*0.001, abs(max)*0.001)
          updateSliderInput(session, paste0('inp', column), value = c(signif(min-step*10, digits = 4), signif(max+step*10, digits = 4)))
        }
      } else {
        if(varClass[column] == "integer") {
          max <- as.integer(unname(rawAbsMax()[varNames[column]]))
          min <- as.integer(unname(rawAbsMin()[varNames[column]]))
          if(min != max) {
            updateSliderInput(session, paste0('inp', column), value = c(min, max))
          }
        } else {
          if(varClass[column] == "factor") {
            updateSelectInput(session, paste0('inp', column), selected = names(table(raw_plus()[varNames[column]])))
          }
        }
      }
    }
  })
  
  # Data processing ----------------------------------------------------------
    
  filterData <- reactive({
    print("In filterData()")
    data <- raw_plus()
    # print(paste("Length of VarNames:", length(varNames)))
    for(column in 1:length(varNames)) {
      inpName=paste("inp",toString(column),sep="")
      nname = varNames[column]
      rng = input[[inpName]]
      # print(paste("column: ", column, "Checking", nname, "rng", rng[1], "(", rawAbsMin()[column], ",", rawAbsMax()[column], ")", rng[2]))
      if(length(rng) != 0) {
        if((varClass[column]=="numeric" | varClass[column]=="integer")) {
          #print(paste("Filtering", nname, "between", rng[1], "and", rng[2]))
          isolate({
            above <- (data[[nname]] >= rng[1])
            below <- (data[[nname]] <= rng[2])
            inRange <- above & below
          })
        } else {
          if (varClass[column]=="factor") {
            # print(paste(varNames[column],class(rng)))
            # print(paste(rng))
            inRange <- (data[[nname]] %in% rng)
          }
        }
        inRange <- inRange | is.na(data[[nname]])
        data <- subset(data, inRange)
      }
      
      # cat("-----------", inpName, nname, rng, length(data[nname]), sep = '\n')
    }
    print("Data Filtered")
    data
  })
  
  colorData <- reactive({
    print("In colorData()")
    data <- filterData()
    data$color <- character(nrow(data))
    data$color <- input$normColor
     if (input$colType == "Max/Min") {
      name <- input$colVarNum
      bottom <- input$colSlider[1]
      top <- input$colSlider[2]
      print(paste("Coloring Data:", name, bottom, top))
      data$color[(data[[name]] >= bottom) & (data[[name]] <= top)] <- input$midColor
      if (input$radio == "max") {
        data$color[data[[name]] < bottom] <- input$maxColor
        data$color[data[[name]] > top] <- input$minColor
      } else {
        data$color[data[[name]] < bottom] <- input$minColor
        data$color[data[[name]] > top] <- input$maxColor
      }
     } 
     else {
       if (input$colType == "Discrete") {
         varList = names(table(raw_plus()[input$colVarFactor]))
         for(i in 1:length(varList)){
           data$color[(data[[input$colVarFactor]] == varList[i])] <- palette()[i]
         }
       }
       else {
         if (input$colType == "Highlighted") {
           if (!is.null(input$plot_brush)){
             if(varClass[input$xInput] == "factor" & varClass[input$yInput] == "factor"){
               xRange <- FALSE
               yRange <- FALSE
             }
             else{
               if(varClass[input$xInput] == "factor"){
                 lower <- ceiling(input$plot_brush$xmin)
                 upper <- floor(input$plot_brush$xmax)
                 xRange <- FALSE
                 for (i in lower:upper){
                   xRange <- xRange | data[input$xInput] == names(table(raw_plus()[input$xInput]))[i]
                 }
                 if (lower > upper){
                   xRange <- FALSE
                 }
               }
               else {
                 xUpper <- data[input$xInput] < input$plot_brush$xmax
                 xLower <- data[input$xInput] > input$plot_brush$xmin
                 xRange <- xUpper & xLower
               }
               if(varClass[input$yInput] == "factor"){
                 lower <- ceiling(input$plot_brush$ymin)
                 upper <- floor(input$plot_brush$ymax)
                 yRange <- FALSE
                 for (i in lower:upper){
                   yRange <- yRange | data[input$yInput] == names(table(raw_plus()[input$yInput]))[i]
                 }                 
                 if (lower > upper){
                   yRange <- FALSE
                 }
               }
               else{
                 yUpper <- data[input$yInput] < input$plot_brush$ymax
                 yLower <- data[input$yInput] > input$plot_brush$ymin
                 yRange <- yUpper & yLower
               }
             }
             data$color[xRange & yRange] <- input$highlightColor #light blue
           }
         }
         else {
           if (input$colType == "Ranked"){
             data[input$rankTable_rows_selected, "color"] <- input$rankColor
           }
         }
       }
     }
     print("Data Colored")
     data
  })

  output$colorLegend <- renderUI({
    print("In color legend")
    if(input$colVarFactor != ""){
      listSize <- length(names(table(raw_plus()[input$colVarFactor])))
      rawLabel <- ""
      for(i in 1:listSize){
        rawLabel <- HTML(paste(rawLabel, "<font color=", palette()[i], "<b>", "&#9632", " ",
                           names(table(raw_plus()[input$colVarFactor]))[i], '<br/>'))
      }
      rawLabel
    }
  })

  # Pairs Tab ----------------------------------------------------------------
  
  pairs_data <- reactive({
    print("In pairs data")
    if (input$autoRender == TRUE) {
      data <- colorData()
    } else {
      data <- slowData()
    }
    
    data
  })
  
  pairs_vars <- reactive({
    print("In pairs vars")
    if (input$autoRender == TRUE) {
      vars <- varsList()
    } else {
      vars <- slowVarsList()
    }
    
    vars
  })
  
  pairsTrendline <- function(...){
    print("In pairs trendline")
    
    if(input$upperPanel) {
      if(input$trendLines) {
        p <- pairs(pairs_data()[pairs_vars()], lower.panel = panel.smooth, upper.panel = panel.smooth, col = pairs_data()$color, pch = as.numeric(input$pointStyle), cex = as.numeric(input$pointSize))
      }
      else {
        p <- pairs(pairs_data()[pairs_vars()], col = pairs_data()$color, pch = as.numeric(input$pointStyle), cex = as.numeric(input$pointSize))
      }
    }
    else {
      if(input$trendLines) {
        p <- pairs(pairs_data()[pairs_vars()], lower.panel = panel.smooth, upper.panel = NULL, col = pairs_data()$color, pch = as.numeric(input$pointStyle), cex = as.numeric(input$pointSize))
      }
      else {
        p <- pairs(pairs_data()[pairs_vars()], upper.panel = NULL, col = pairs_data()$color, pch = as.numeric(input$pointStyle), cex = as.numeric(input$pointSize))
      }
    }
    
  }
  
  output$pairsDisplay <- renderUI({
    print("In pairs display")
    plotOutput("pairsPlot", dblclick = "pairs_click", height=700)
  })
  
  output$pairsPlot <- renderPlot({
    
    print("In render plot")
    
    output$displayError <- renderText("")
    output$filterError <- renderText("")
    
    if (length(input$display) >= 2 & nrow(filterData()) > 0) {
      print("Rendering Plot.")
      pairsTrendline()
      print("Plot Rendered.")
    }
    else {
      if (nrow(filterData()) == 0) {
        output$filterError <- 
          renderText(
            "No data points fit the current filtering scheme")
      }
      if (length(input$display) < 2) {
        output$displayError <- 
          renderText(
            "Please select two or more display variables.")
      }
    }
  })
  
  output$filterError <- renderUI({
    print("In filter error")
    h4(textOutput("filterVars"), align = "center")
  })
  
  output$displayError <- renderUI({
    print("In display error")
    h4(textOutput("displayVars"), align = "center")
  })
  
  
  #Change to single plot when user clicks a plot on pairs matrix
  observeEvent(input$pairs_click, {
    print("In observe pairs click")
    num_vars <- length(input$display)
    x_pos <- num_vars*input$pairs_click$x
    y_pos <- num_vars*input$pairs_click$y
    print(paste0("X: ", round(x_pos, 2), " Y: ", round(y_pos,2)))
    x_var <- NULL
    y_var <- NULL
    margin <- 0.1
    plot <- 0.91
    buffer <- 0.05
    if(num_vars > 6){
      margin <- 0
      plot <- 0.9
      buffer <- 0.1
    }
    if(num_vars > 11){
      margin <- -0.5
      buffer <- 0.2
    }
    if(num_vars > 12){
      buffer <- 0.15
      margin <- -0.6
    }
    if(num_vars > 18){
      margin <- -1.51
      buffer <- 0.25
    }
    xlimits <- c(margin, plot+margin)
    ylimits <- xlimits
    if(num_vars > 18){
      ylimits <- c(-2.5, -1.4)
    }
    
    
    for(i in 1:(num_vars-1)){
      if(findInterval(x_pos, xlimits) == 1){
        x_var <- input$display[i]
      }
      if(findInterval(y_pos, ylimits) == 1){
        y_var <- rev(input$display)[i]
      }
      if(!is.null(x_var) & !is.null(y_var)){
        updateSelectInput(session, "xInput", selected = x_var)
        updateSelectInput(session, "yInput", selected = y_var)
        updateTabsetPanel(session, "inTabset", selected = "Single Plot")
        break
      }
      xlimits <- xlimits + plot + buffer
      if(num_vars > 18){
        ylimits <- ylimits + 0.9 + 0.35
      }
      else {
        ylimits <- xlimits        
      }
    }
  })
  
  
  varsList <- reactive({
    print("Getting Variable List.")
    idx = NULL
    for(choice in 1:length(input$display)) {
      mm <- match(input$display[choice],varNames)
      if(mm > 0) { idx <- c(idx,mm) }
    }
    print(idx)
    idx
  })
  
  slowVarsList <- eventReactive(input$renderPlot, {
    print("Getting Variable List.")
    idx = 0
    for(choice in 1:length(input$display)) {
      mm <- match(input$display[choice],varNames)
      if(mm > 0) { idx <- c(idx,mm) }
    }
    print(idx)
    idx
  })
  
  slowData <- eventReactive(input$renderPlot, {
    colorData()
  })
  
  output$stats <- renderText({
    print("In render stats")
    if(nrow(filterData()) > 0){
      if(input$autoInfo == TRUE){
        table <- infoTable()
      }
      else {
        table <- slowInfoTable()
      }
    }
    else
      table <- "No data points fit the filtering scheme"
    if(importFlags$tier1){
      importFlags$tier1 <- FALSE
      importFlags$tier2 <- TRUE
      importFlags$ranking <- TRUE
    }
    table
})

  infoTable <- function(...){
    print("In info table")
    tb <- table(factor(colorData()$color, 
                       c(input$midColor, 
                         input$minColor, 
                         input$highlightColor, 
                         input$rankColor,
                         input$maxColor, 
                         input$normColor)))
    if (input$colType == 'Max/Min') {
      paste0("Total Points: ", nrow(raw),
             "\nCurrent Points: ", nrow(filterData()),
             "\nColored Points: ", sum(tb[[input$minColor]], tb[[input$midColor]], tb[[input$maxColor]], tb[[input$normColor]]),
             "\nWorst Points: ", tb[[input$maxColor]],
             "\nIn Between Points: ", tb[[input$midColor]],
             "\nBest Points: ", tb[[input$minColor]]
      )
    } 
    else if(input$colType == 'Discrete') {
      tb <- table(factor(colorData()$color, palette()))
      d_vars <- names(table(raw_plus()[input$colVarFactor]))
      output_string <- paste0("Total Points: ", nrow(raw),
             "\nCurrent Points: ", nrow(filterData()),
             "\nColored Points: ", sum(tb[[palette()[1]]], 
                                       tb[[palette()[2]]], 
                                       tb[[palette()[3]]], 
                                       tb[[palette()[4]]], 
                                       tb[[palette()[5]]], 
                                       tb[[palette()[6]]], 
                                       tb[[palette()[7]]], 
                                       tb[[palette()[8]]], 
                                       tb[[palette()[9]]]))
      for(i in 1:length(d_vars)){
             output_string <- paste0(output_string, 
                                     "\n", d_vars[i]," Points: ", tb[[palette()[i]]])
      }
      output_string
    }
    else if(input$colType == 'Highlighted') {
      paste0("Total Points: ", nrow(raw),
             "\nCurrent Points: ", nrow(filterData()),
             "\nHighlighted Points: ", tb[[input$highlightColor]]
      )
    }
    else if(input$colType == 'Ranked') {
      paste0("Total Points: ", nrow(raw),
             "\nCurrent Points: ", nrow(filterData()),
             "\nRanked Points: ", tb[[input$rankColor]]
      )
    }
    else{
      paste0("Total Points: ", nrow(raw),
             "\nCurrent Points: ", nrow(filterData()),
             "\nColored Points: ", tb[[input$normColor]]
      )
    }
  }
  
  slowInfoTable <- eventReactive(input$updateStats, {
    infoTable()
  })

  output$exportData <- downloadHandler(
    filename = function() { paste('data-', Sys.Date(), '.csv', sep='') },
    content = function(file) { write.csv(filterData(), file) }
  )
  
  output$exportRanges <- downloadHandler(
    filename = function() { paste('ranges-', Sys.Date(), '.csv', sep='') },
    content = function(file) { write.csv(do.call(rbind, lapply(filterData(), summary)), file) }
  )
  
  output$exportPlot <- downloadHandler(
    filename = function() { paste('plot-', Sys.Date(), '.pdf', sep='') },
    content = function(file) {
      pdf(paste('plot-', Sys.Date(), '.pdf', sep=''), width = 10, height = 10)
      pairs(colorData()[varsList()],lower.panel = panel.smooth,upper.panel=NULL, col=colorData()$color)
      dev.off()
      file.copy(paste('plot-', Sys.Date(), '.pdf', sep=''), file)
    }
  )
  
  # Single Plot Tab ----------------------------------------------------------

  output$singlePlot <- renderPlot({
    print("In single plot")
    data <- filterData()
    plot(data[[paste(input$xInput)]], data[[paste(input$yInput)]], xlab = paste(input$xInput), ylab = paste(input$yInput), pch = as.numeric(input$pointStyle))
  })
  
  output$info <- renderPrint({
    t(nearPoints(filterData(), input$plot_click, xvar = input$xInput, yvar = input$yInput, maxpoints = 8))
  })
  
  # Data Table Tab --------------------------------------------------------------------------------
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
  
  datatable_display <- function(...){
    df <- filterData()
    colnames(df) <- sapply(names(df), function(x) abbreviate(unlist(strsplit(x, "[.]"))[2], 9))
    df
  }
  
  output$table <- DT::renderDataTable({
    print("In render data table")
    if(input$autoData == TRUE){
      data <- filterData()
      if(input$roundTables)
        data <- round_df(filterData(), input$numDecimals)
    }
    else {
      data <- slowFilterData()
    }
    data
  })
  
  # Data Ranking Tab ---------------------------------------------------------
  
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
  observeEvent(input$clearMetrics, {
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
    
    transferCondition = toString(paste0("input.util",current," == true"))
    
    
    column(3, 
      h4(varNames[current]),
      radioButtons(paste0('sel', current), 
                   "Score By:",
                   choices = c("Min", "Max"),
                   selected = radioVal,
                   inline = TRUE),
      sliderInput(paste0('rnk', current),
                  "Weight:",
                  step = 0.01,
                  min = 0,
                  max = 1,
                  value = sliderVal),
      checkboxInput(paste0('util', current),
                    "Add Transfer Function",
                    value = utilVal),
      conditionalPanel(condition = transferCondition,
                       textInput(paste0('func', current),
                                 "Enter Data Points",
                                  placeholder = paste0("Value = Score | e.g. ", 
                                    rawAbsMin()[current],
                                    " = 1, ",
                                    rawAbsMax()[current],
                                    "= 0.5"),
                                 value = funcVal),
                       utilityPlot(current)),
      br(), br(), br()
    )
  }
  
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
    })
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
      req(length(current_point) == 2)
      current_val <- as.numeric(current_point[1])
      low <- rawAbsMin()[current]
      hi <- rawAbsMax()[current]
      #This line below enforces range limits on transfer function
      #req(findInterval(current_val, c(low, hi), rightmost.closed = TRUE) == 1) 
      xVals <- c(xVals, current_val)
      yVals <- c(yVals, as.numeric(current_point[2]))
    }
    #Sort list by 'xVals' (while paired with yVals)
    unsortedVals <- xVals
    names(unsortedVals) <- yVals
    sortedVals <- sort(unsortedVals)
    
    #Flip-flop names&values of a named list
    outputVals <- sapply(names(sortedVals), as.numeric)
    names(outputVals) <- sortedVals
    processLineEquations(current, outputVals)
    outputVals
  }
  
  
  fullMetricUI <- reactive({
    if(!importFlags$ranking){
      fluidRow(
        lapply(metricsList(), function(column) {
          isolate(generateMetricUI(column))
        })
      )
    }
    else{
      fluidRow(
        lapply(metricsList(), function(column) {
          importedSlider <- importData[[paste0('rnk', column)]]
          importedRadio <- importData[[paste0('sel', column)]]
          importedUtil <- importData[[paste0('util', column)]]
          importedFunc <- importData[[paste0('func', column)]]
          isolate(generateMetricUI(column, 
                           importedSlider, 
                           importedRadio,
                           importedUtil,
                           importedFunc))
        })
      )
    }
  })

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
      radioSelect <- paste0("sel", toString(metricsList()[i]))
      if(input[[radioSelect]] == "Min"){
        colMin <- min(normData[column])
        for(j in 1:length(unlist(normData[column]))) {
          item <- normData[j,column]
          normData[j,column] <- 1 -item + colMin
        }
      }
      xFunc <- xFuncs[[toString(metricsList()[i])]]
        if(!is.null(xFunc)){
          normValue <- max(data[[column]])
          transferData <- data[[column]]
          for(t in 1:length(transferData)){
            item <- transferData[t]
            transferData[t] <- linearly_interpolate(item, xFunc)
          }
          scoreData <- scoreData + transferData
        }
      scoreData <- scoreData + unlist(unname(weight*normData[column]))
    }
    importFlags$ranking <- FALSE
    scoreData <- sort(scoreData, decreasing = TRUE)
    filterData()[names(scoreData),varRangeNum()]
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
  
  #Output ranked data table
  output$rankTable <- DT::renderDataTable({
    print("In render ranked data table")
    data <- rankData()
    if(input$roundTables)
      data <- round_df(rankData(), input$numDecimals)
    data
  })
  
  #Event handler for "Color by selected rows"
  observeEvent(input$colorRanked, {
    req(input$rankTable_rows_selected)
    updateTabsetPanel(session, "inTabset", selected = "Pairs Plot")
    updateSelectInput(session, "colType", selected = "Ranked")
  })
  
  #Download handler
  output$exportPoints <- downloadHandler(
    filename = function() { paste('ranked_points-', Sys.Date(), '.csv', sep='') },
    content = function(file) { 
      write.csv(filterData()[input$rankTable_rows_selected, ], file) }
  )
  
  # Ranges Table Tab --------------------------------------------------------------------------------
  slowRangeData <- eventReactive(input$updateRanges, {
    do.call(rbind, lapply(filterData(), summary))
  })
  
  output$ranges <- renderPrint({
    if(input$autoRange == TRUE){
      do.call(rbind, lapply(filterData(), summary))
    }
    else {
      slowRangeData()
    }
  })

  colSliderSettings <- reactive({
    if(input$colVarNum != ""){
      print("In colSlider settings")
      variable <- input$colVarNum
      raw_data <- raw_plus()
      isolate({
      data <- filterData()
        min <- min(data[[variable]], na.rm=TRUE)
        max <- max(data[[variable]], na.rm=TRUE)
        thirtythree <- quantile(data[[variable]], 0.33, na.rm=TRUE)
        sixtysix <- quantile(data[[variable]], 0.66, na.rm=TRUE)
        absMin <- as.numeric(unname(rawAbsMin()[variable]))
        absMax <- as.numeric(unname(rawAbsMax()[variable]))
        absStep <- max((max-min)*0.01, abs(min)*0.001, abs(max)*0.001)
        numericMin <- signif(absMin-absStep*10, digits = 4)
        numericMax <- signif(absMax+absStep*10, digits = 4)
        numericStep <- signif(absStep, digits = 4)
        lower <- unname(thirtythree)
        upper <- unname(sixtysix)
      })
      print("colSlider settings complete")
      colSlider <- data.frame(variable, min, max, lower, upper, 
                              numericMin, numericMax, numericStep,
                              absMin, absMax, absStep)
      colSlider
    }
  })

  # UI Adjustments -----------------------------------------------------------
  updateColorSlider <- observeEvent(colSliderSettings(), {
    print("In updateColorSlider")
    if(input$colVarNum != ""){
      if(varClass[[colSliderSettings()$variable]] == "numeric") {
        updateSliderInput(session,
                          "colSlider",
                          step = colSliderSettings()$numericStep,
                          min = colSliderSettings()$numericMin,
                          max = colSliderSettings()$numericMax,
                          value = c(colSliderSettings()$lower, colSliderSettings()$upper))
      }
      else if(varClass[[colSliderSettings()$variable]] == "integer") {
        updateSliderInput(session,
                          "colSlider",
                          min = colSliderSettings()$absMin,
                          max = colSliderSettings()$absMax,
                          value = c(floor(colSliderSettings()$lower), ceiling(colSliderSettings()$upper)))
      }
      print("updateColorSlider() done.")
    }
    else{
      print("updateColorSlider stalled")
    }
    if(importFlags$tier2){
      importFlags$tier2 <- FALSE
    }
  })
  
  updateXSlider <- observeEvent(input$updateX, {
    print("in update Xslider")
    updateSlider(input$xInput, input$plot_brush$xmin, input$plot_brush$xmax)
  })
  
  updateYSlider <- observeEvent(input$updateY, {
    print("in update Yslider")
    updateSlider(input$yInput, input$plot_brush$ymin, input$plot_brush$ymax)
  })
  
  updateBothSlider <- observeEvent(input$updateBoth, {
    updateSlider(input$xInput, input$plot_brush$xmin, input$plot_brush$xmax)
    updateSlider(input$yInput, input$plot_brush$ymin, input$plot_brush$ymax)
  })
  
  observeEvent(input$highlightData, {
    print("In observe highlight data")
    updateTabsetPanel(session, "inTabset", selected = "Pairs Plot")
    updateSelectInput(session, "colType", selected = "Highlighted")
  })
  
  updateSlider <- function(varName, min, max) {
    if(!is.null(min) & !is.null(max)) {
      if(varName %in% varRangeNum()) {
        print(paste0("Updating ", varName, " Slider: ", min, " to ", max))
        updateSliderInput(session,
                          paste0("inp", match(varName, varNames)),
                          value = c(min, max))
      } else {
        if(varClass[[varName]] == "factor") {
          selectedFactors <- ceiling(min):floor(max)
          names <- levels(filterData()[[varName]])[selectedFactors]
          print(paste0("Updating ", varName, ": ", names))
          updateSelectInput(session, paste0('inp', match(varName, varNames)), selected = names)
        } else {print(paste0("Error: can't update slider for '", varName, "'."))}
      }
    } else {print("Error: no selection available for update.")}
  }

  constrainXVariable <- eventReactive (input$updateX, {
    print("In constrainXVariable()")
    #print(paste("new upper value", input$xInput, "=", input$plot_brush$xmax))
    #updateSliderInput()
  })
  
})
