library(shiny)
#options(shiny.trace=TRUE)
#options(shiny.fullstacktrace = TRUE)
#options(error = function() traceback(2))
#options(shiny.error = function() traceback(2))

palette(c("#E41A1C", "#377EB8", "#4DAF4A", "#984EA3",
         "#FF7F00", "#FFFF33", "#A65628", "#F781BF", "#999999"))

shinyServer(function(input, output, clientData, session) {
  # Get Data -----------------------------------------------------------------
  raw <- c()
  query <- parseQueryString(isolate(session$clientData$url_search))

  # output$debug <- renderText({
  #   paste(names(query), query, sep = "=", collapse=", ")
  # })

  if (!is.null(query[['csvfilename']])) {
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
    raw = read.csv("../../results/mergedPET.csv", fill=T)
  }
  
  
  #Credit: Henrik Bengtsson source:https://stat.ethz.ch/pipermail/r-help/2007-June/133564.html
  fileChoose <- function(...) {
    pathname <- NULL;
    tryCatch({
      pathname <- file.choose();
    }, error = function(ex) {
    })
    pathname;
  }
  
  filedata <- eventReactive(
    input$importSession, {
      file <- fileChoose()
      if (!is.null(file)){
        read.csv(file, header = TRUE, stringsAsFactors = FALSE)
      }
      else{
        return(NULL)
      }
  })
  
  

  #Changes values based on user uploaded csv file
  observe({
    
    if(!is.null(filedata())){
      #updateSelectInput(session, "display", selected = varRange[c(1,2)])
      withProgress(message = 'Uploading Session File', {
        for(i in 1:length(colnames(filedata()))){
          
          incProgress(1/length(colnames(filedata())))
          
          current <- colnames(filedata())[i]
          column <- as.numeric(gsub("[^0-9]", "", current)) #Extract number
          
          if(!is.null(filedata()[current]) & !is.na(column)){
            if (varClass[column] == "factor" & length(names(table(raw[varNames[column]]))) > 1) {
              parsedValue <- as.list(strsplit(toString(filedata()[current]), ", ")[[1]])
              trimmedValue <- gsub("^\\s+|\\s+$", "", parsedValue)
              updateSelectInput(
                session,
                current,
                selected = trimmedValue
              )
            }
            else {
              if(varClass[column] == "numeric") {
                updateSliderInput(
                  session,
                  current,
                  value = as.numeric(unlist(strsplit(toString(filedata()[current]), ", ")))
                )
              }
              if (varClass[column] == "integer") {
                updateSliderInput(
                  session,
                  current,
                  value = as.numeric(unlist(strsplit(toString(filedata()[current]), ", ")))
                )
              }
            }
          }
          else {
            if(current == 'colSlider'){
              updateColorSlider()
              print("Updated colslider from csv")
              newValues <- as.numeric(unlist(strsplit(toString(filedata()[current]), ", ")))
              updateSliderInput(
                session,
                current,
                value = c(newValues[1], newValues[2])
              )
            }
            else {
              if (current == 'autoRender' | current == 'removeMissing'){
                trimmedValue <- gsub("^\\s+|\\s+$", "", filedata()[current])
                updateCheckboxInput(
                  session,
                  current,
                  value = as.logical(trimmedValue)
                )
              }
              else {
                if (current == 'plot_brush'){
                  parsedValue <- as.list(strsplit(toString(filedata()[current]), ", ")[[1]])
                  trimmedValue <- gsub("^\\s+|\\s+$", "", parsedValue)
                  
                }
                else{
                  parsedValue <- as.list(strsplit(toString(filedata()[current]), ", ")[[1]])
                  trimmedValue <- gsub("^\\s+|\\s+$", "", parsedValue)
                  updateSelectInput(
                    session,
                    current,
                    selected = trimmedValue
                  )
                }
              }
            }
          }
        }
      })
    }
  })
  
  #Save all fields to csv
  formData <- reactive({
    presets <- c()
    # Saving Filter Data
    for(column in 1:length(varNames)) {
      newitem <- paste0('inp', column)
      
      if (varClass[column] == "factor" & length(names(table(raw[varNames[column]]))) > 1) {
        newpreset <- c(newitem, toString(input[[newitem]]))
        presets <- cbind(presets, newpreset)
      }
      
      if (varClass[column] == "numeric") {
        max <- as.numeric(unname(rawAbsMax()[varNames[column]]))
        min <- as.numeric(unname(rawAbsMin()[varNames[column]]))
        diff <- (max-min)
        # print(paste(column, "min", min, "max", max, "diff", diff))
        if (diff != 0) {
          newpreset <- c(newitem, toString(input[[newitem]]))  
          presets <- cbind(presets, newpreset)
        }
      } 
      
      if (varClass[column] == "integer") {
        max <- as.integer(unname(rawAbsMax()[varNames[column]]))
        min <- as.integer(unname(rawAbsMin()[varNames[column]]))
        if (min != max) {
          newpreset <- c(newitem, toString(input[[newitem]]))  
          presets <- cbind(presets, newpreset)
        }
      }
    }
    
    # Saving additional plot options
    display <- c('display', toString(input$display))
    color <- c('colType', input$colType)
    colVarNum <- c('colVarNum', input$colVarNum)
    colVarFac <- c('colVarFactor', input$colVarFactor)
    varMinMax <- c('radio', input$radio)
    colSlider <- c('colSlider', toString(input$colSlider))
    xInput <- c('xInput', input$xInput)
    yInput <- c('yInput', input$yInput)
    removeMissing <- c('removeMissing', input$removeMissing)
    removeOutliers <- c('removeOutliers', input$removeOutliers)
    numDevs <- c('numDevs', input$numDevs)
    autoRender <- c('autoRender', input$autoRender)
    pointStyle <- c('pointStyle', input$pointStyle)
    pointSize <- c('pointSize', input$pointSize)
    
    #plot_brush <- c('plot_brush', toString(input$plot_brush))
    
    presets <- cbind(presets, display, 
                     color, colVarNum, colVarFac, varMinMax, colSlider, 
                     xInput, yInput, 
                     removeMissing, removeOutliers, input$numDevs, autoRender,
                     pointStyle, pointSize)
    presets
    
  })
  
  #Call when user saves data
  output$exportSession <- downloadHandler(
    filename = function() { 
      if (input$sessionName == ""){
        paste0('session_', Sys.Date(), '.csv')
      }
      else {
        paste0(input$sessionName, '.csv')
      }
    },
    content = function(file) { 
      write.table(
        x = formData(),
        file,
        row.names = FALSE, 
        col.names = FALSE,
        sep = ", ",
        quote = TRUE
      )
    }
  )
  
  raw_plus <- reactive({
    data <- raw
    
    if(input$removeOutliers){
      for(column in 1:length(varNames)) {
        
        nname = varNames[column]
        rng <- c(0,1)
        
        if((varClass[column]=="numeric" | varClass[column]=="integer")) {
          suppressWarnings(stdDev <- sd(data[[nname]], na.rm = TRUE))
          suppressWarnings(mean <- mean(data[[nname]], na.rm = TRUE))
          rng[1] <- round(mean - as.integer(input$numDevs)*stdDev, 6)
          rng[2] <- round(mean + as.integer(input$numDevs)*stdDev, 6)
          if(varClass[column] == "integer"){
            rng[2] <- round(rng[2])
          }
          above <- (data[[nname]] >= rng[1])
          below <- (data[[nname]] <= rng[2])
          inRange <- above & below | is.na(data[[nname]])
          data <- subset(data, inRange)
        }
        
      }
    }
    
    if(input$removeMissing){
      for(column in 1:length(varNames)) {
        nname = varNames[column]
        inRange <- !is.na(data[[nname]])
        data <- subset(data, inRange)
      }
    }
    
    data
  })

  # Pre-processing -----------------------------------------------------------
  # print(str(raw))
  
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
    apply(raw_plus()[varNum], 2, min, na.rm=TRUE)
  })
  
  rawAbsMax <- reactive({
    apply(raw_plus()[varNum], 2, max, na.rm=TRUE)
  }) 
  
  varRangeNum <- reactive({
    varNum[(rawAbsMin() != rawAbsMax()) & (rawAbsMin() != Inf)]
  })
  print(paste("varRangeNum:"))
  
  varRangeFac <- varFac[apply(raw[varFac], 2, function(x) (length(names(table(x))) > 1))]
  print(paste("varRangeFac:"))
  print(paste(varRangeFac))
  
  varRange <- reactive({
    c(varRangeFac, varRangeNum())
  })
  print(paste("varRange:"))
  
  observe({

    print("Updating Panel Selections...")
    
    isolate({
      updateSelectInput(session, "colVarNum", choices = c(varRangeNum()), selected = varRangeNum()[c(1)])
      updateSelectInput(session, "display", choices = varRange(), selected = varRange()[c(1,2)])
      updateSelectInput(session, "xInput", choices = varRange(), selected = varRange()[c(1)])
      updateSelectInput(session, "yInput", choices = varRange(), selected = varRange()[c(2)])
    })
    
  })
    
  resetPlotOptions <- observeEvent(input$resetOptions, {
    print("In resetPlotOptions()")
    updateSelectInput(session, "display", selected = varRange()[c(1,2)])
    updateCheckboxInput(session, "color", value = FALSE)
    # updateSelectInput(session, "colType", selected = "Max/Min")
  })

  
  # resetViewerOptions <- observeEvent(input$resetSettings, {
  #   print("In resetViewerSettings()")
  #   updateCheckboxInput(session, "autoRender", value = TRUE)
  #   updateRadioButtons(session, "pointStyle", choices = c("Normal" = 1,"Filled" = 19), selected = "Normal")
  #   updateRadioButtons(session, "pointSize", choices = c("Small" = 1,"Medium" = 1.5,"Large" = 2), selected = "Small")
  # })
  
  print(paste("Finished Preprocessing the Data ----------------------------------------------------"))
  
  
  #Initializing -------------------------------------------------------
  updateSelectInput(
    session,
    "colVarFactor",
    choices = varFac
  )

  # Sliders ------------------------------------------------------------------
  output$enums <- renderUI({
    fluidRow(
      lapply(1:length(varNames), function(column) {
        if (varClass[column] == "factor" & length(names(table(raw[varNames[column]]))) > 1) {
          column(2,
                 selectInput(paste0('inp', column),
                             varNames[column],
                             multiple = TRUE,
                             selectize = FALSE,
                             choices = names(table(raw[varNames[column]])),
                             selected = names(table(raw[varNames[column]])))
          )
        }
      })
    )
  })
  
  output$sliders <- renderUI({
    fluidRow(
      lapply(1:length(varNames), function(column) {
        
        if(varClass[column] == "numeric") {
          max <- as.numeric(unname(rawAbsMax()[varNames[column]]))
          min <- as.numeric(unname(rawAbsMin()[varNames[column]]))
          diff <- (max-min)
          # print(paste(column, "min", min, "max", max, "diff", diff))
          if (diff != 0) {
            #print(paste(column, varNames[column], as.numeric(unname(rawAbsMax()[varNames[column]]))))
            step <- max(diff*0.01, abs(min)*0.001, abs(max)*0.001)
            # cat("step", diff*0.01, abs(min)*0.001, abs(max)*0.001, "\n", sep = " ")
            column(2,
              sliderInput(paste0('inp', column),
                          varNames[column],
                          step = signif(step, digits = 4),
                          min = signif(min-step*10, digits = 4),
                          max = signif(max+step*10, digits = 4),
                          value = c(signif(min-step*10, digits = 4), signif(max+step*10, digits = 4)))
            )
          }
        } 
        else {
          if (varClass[column] == "integer") {
            max <- as.integer(unname(rawAbsMax()[varNames[column]]))
            min <- as.integer(unname(rawAbsMin()[varNames[column]]))
            if (min != max) {
              column(2, 
                     sliderInput(paste0('inp', column),
                            varNames[column],
                            min = min,
                            max = max,
                            value = c(min, max))
              )
            }
          }
        }
      })
    )
  })
  
  output$constants <- renderUI({
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
            if (varClass[column] == "factor" & length(names(table(raw[varNames[column]]))) == 1) {
              column(2, p(strong(paste0(varNames[column],":")), names(table(raw[varNames[column]]))))
            }
          }
        }
      })
    )
  })
  
  resetDefaultSliders <- observeEvent(input$resetSliders, {
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
            updateSelectInput(session, paste0('inp', column), selected = names(table(raw[varNames[column]])))
          }
        }
      }
    }
  })
  
  
  # Data functions -----------------------------------------------------------
  # if(input$removeOutliers){
  #   if(round(mean + as.integer(input$numDevs)*stdDev, 6) < max){
  #     max <- round(mean + as.integer(input$numDevs)*stdDev, 6)
  #   }
  #   if(round(mean - as.integer(input$numDevs)*stdDev, 6) > min){
  #     min <- round(mean - as.integer(input$numDevs)*stdDev, 6)
  #   }
  # }
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
          above <- (data[[nname]] >= rng[1])
          below <- (data[[nname]] <= rng[2])
          inRange <- above & below
        } else {
          if (varClass[column]=="factor") {
            print(paste(varNames[column],class(rng)))
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
    slider <- input$colSlider
    data <- filterData()
    data$color <- character(nrow(data))
    data$color <- input$normColor
     if (input$colType == "Max/Min") {
      name <- isolate(input$colVarNum)
      bottom <- slider[1]
      top <- slider[2]
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
         varList = names(table(raw[input$colVarFactor]))
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
                   xRange <- xRange | data[input$xInput] == names(table(raw[input$xInput]))[i]
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
                 upper <- floor(input$plot_brush$ymay)
                 yRange <- FALSE
                 for (i in lower:upper){
                   yRange <- yRange | data[input$yInput] == names(table(raw[input$yInput]))[i]
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
       }
     }
    print("Data Colored")
    data
  })

  output$colorLegend <- renderUI({
    if(input$colVarFactor != ""){
      listSize <- length(names(table(raw_plus()[input$colVarFactor])))
      rawLabel <- ""
      for(i in 1:listSize){
        rawLabel <- HTML(paste(rawLabel, "<font color=", palette()[i], "<b>", "&#9632", " ",
                           names(table(raw[input$colVarFactor]))[i], '<br/>'))
      }
      rawLabel
    }
  })
  
  rangesData <- reactive({
    # for(column in 1:length(varNames)) {
    #   inpName=paste("inp",toString(column),sep="")
    #   nname = varNames[column]
    #   rng = input[[inpName]]
    #   if(length(rng) != 0) {
    #     if((varClass[column]=="numeric" | varClass[column]=="integer")) {
    #       # print(paste("Filtering between", rng[1], "and", rng[2]))
    #       data <- data[data[nname] >= rng[1],]
    #       data <- data[data[nname] <= rng[2],]
    #     }
    #   }
    # }
    maxes <- apply(isolate(filterData()), 2, function(x) max(x, na.rm = TRUE))
    print(paste(maxes))
  })

  # Pairs Tab ----------------------------------------------------------------
  
  pairs_data <- reactive({
    if (input$autoRender == TRUE) {
      data <- colorData()
    } else {
      data <- slowData()
    }
    
    data
  })
  
  pairs_vars <- reactive({
    if (input$autoRender == TRUE) {
      vars <- varsList()
    } else {
      vars <- slowVarsList()
    }
    
    vars
  })
  
  output$pairsPlot <- renderPlot({
    
    output$displayVars <- renderText("")
    output$filterVars <- renderText("")
    
    if (length(input$display) >= 2 & nrow(filterData()) > 0) {
      print("Rendering Plot.")
      # if(input$colType == 'Discrete') {
      #   print("Printing 'Discrete' plot.")
      #   pairs(data[vars],lower.panel = panel.smooth,upper.panel=NULL, col=data$color, pch = as.numeric(input$pointStyle))
      #   legend('topright',legend=levels(colorData()[[paste(varFactor[1])]]),pch=1,title=paste(varFactor[1]))
      # } else {
        # print(as.numeric(input$pointStyle))
      pairs(pairs_data()[pairs_vars()],
         lower.panel = panel.smooth,
         upper.panel=NULL,
         col = pairs_data()$color,
         pch = as.numeric(input$pointStyle),
         cex = as.numeric(input$pointSize))
          # pairs2(pairs_data()[pairs_vars()],
          #        pairs_data()[pairs_vars()],
          #      panel = panel.smooth,
          #      #upper.panel=NULL,
          #      col = pairs_data()$color,
          #      pch = as.numeric(input$pointStyle),
          #      cex = as.numeric(input$pointSize))
      # }
      print("Plot Rendered.")
    }
    else {
      if (nrow(filterData()) == 0) {
        output$filterVars <- 
          renderText(
            "No data points fit the current filtering scheme")
      }
      if (length(input$display) < 2) {
        output$displayVars <- 
          renderText(
            "Please select two or more display variables.")
      }
    }
  })
  
  output$pairsDisplay <- renderUI({
    plotOutput("pairsPlot", dblclick = "pairs_click", height=700)
  })
  
  output$filterError <- renderUI({
    h4(textOutput("filterVars"), align = "center")
  })
  
  output$displayError <- renderUI({
    h4(textOutput("displayVars"), align = "center")
  })
  
  # output$pairs_info <- renderPrint({
  #   t(brushedPoints(pairs_data(), input$pairs_brush,
  #                   xvar = input$display[1],
  #                   yvar = input$display[2]))
  # })
  
  
  #Change to single plot when user clicks a plot on pairs matrix
  observeEvent(input$pairs_click, {
    num_vars <- length(input$display)
    x_pos <- num_vars*input$pairs_click$x
    y_pos <- num_vars*input$pairs_click$y
    x_var <- NULL
    y_var <- NULL
    a <- 0.1
    b <- 0.9
    c <- 0.05
    limits <- c(a, b+a)
    for(i in 1:(num_vars-1)){
      if(findInterval(x_pos, limits) == 1){
        x_var <- input$display[i]
      }
      if(findInterval(y_pos, limits) == 1){
        y_var <- rev(input$display)[i]
      }
      if(!is.null(x_var) & !is.null(y_var)){
        updateTabsetPanel(session, "inTabset", selected = "Single Plot")
        updateSelectInput(session, "xInput", selected = x_var)
        updateSelectInput(session, "yInput", selected = y_var)
        break
      }
      limits <- limits + b + c
    }
    print("Printing vars")
    print(x_var)
    print(y_var)
    print("Done printing vars")
  })
  
  
  varsList <- reactive({
    print("Getting Variable List.")
    idx = 0
    for(choice in 1:length(input$display)) {
      mm <- match(input$display[choice],varNames)
      if(mm > 0) { idx <- c(idx,mm) }
    }
    print(idx)
    idx
  })
  
  slowVarsList <- eventReactive(input$renderPlot, {
    print(paste("input$renderPlot:", input$renderPlot))
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
    if(input$autoInfo == TRUE){
      infoTable()
    }
    else {
      slowInfoTable()
    }
  })

  infoTable <- eventReactive(colorData(), {
    tb <- table(factor(colorData()$color, c(input$midColor, input$minColor, input$highlightColor, input$maxColor, input$normColor)))
    if (input$colType == 'Max/Min') {
      paste0("Total Points: ", nrow(raw),
             "\nCurrent Points: ", nrow(filterData()),
             "\nVisible Points: ", sum(tb[[input$minColor]], tb[[input$midColor]], tb[[input$maxColor]], tb[[input$normColor]]),
             "\nWorst Points: ", tb[[input$maxColor]],
             "\nIn Between Points: ", tb[[input$midColor]],
             "\nBest Points: ", tb[[input$minColor]]
      )
    } 
    else if(input$colType == 'Discrete') {
      tb <- table(factor(colorData()$color, palette()))
      d_vars <- names(table(raw[input$colVarFactor]))
      output_string <- paste0("Total Points: ", nrow(raw),
             "\nCurrent Points: ", nrow(filterData()),
             "\nVisible Points: ", sum(tb[[palette()[1]]], 
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
             "\nVisible Points: ", sum(tb[[input$highlightColor]], tb[[input$normColor]]),
             "\nHighlighted Points: ", tb[[input$highlightColor]]
      )
    }
    else{
      paste0("Total Points: ", nrow(raw),
             "\nCurrent Points: ", nrow(filterData()),
             "\nVisible Points: ", tb[[input$normColor]]
      )
    }
  })
  
  slowInfoTable <- eventReactive(input$updateStats, {
    infoTable()
  })

  output$exportData <- downloadHandler(
    filename = function() { paste('data-', Sys.Date(), '.csv', sep='') },
    content = function(file) { write.csv(filterData(), file) }
  )
  
  output$exportRanges <- downloadHandler(
    filename = function() { paste('ranges-', Sys.Date(), '.csv', sep='') },
    content = function(file) { write.csv(rangesData(), file) }
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
    data <- filterData()
    plot(data[[paste(input$xInput)]], data[[paste(input$yInput)]], xlab = paste(input$xInput), ylab = paste(input$yInput), pch = as.numeric(input$pointStyle))
  })
  
  info <- renderPrint({
    brushedPoints(filterData(), 
                 input$plot_brush,
                 xvar = input$xInput,
                 yvar = input$yInput)
  })
  
  
  # Data Table Tab --------------------------------------------------------------------------------
  slowFilterData <- eventReactive(input$updateDataTable, {
    filterData()
  })
  
  output$table <- renderDataTable({
    if(input$autoData == TRUE){
      filterData()
    }
    else {
      data <- slowFilterData()
    }
  })
  
  # Ranges Table Tab --------------------------------------------------------------------------------
  slowRangeData <- eventReactive(input$updateRanges, {
    t(summary(filterData()))
  })
  
  output$ranges <- renderPrint({
    if(input$autoRange == TRUE){
      t(summary(filterData()))
    }
    else {
      slowRangeData()
    }
  })
  
  # UI Adjustments -----------------------------------------------------------
  updateColorSlider <- function() {
    data <- isolate(filterData())
    min <- min(data[[paste(input$colVarNum)]], na.rm=TRUE)
    max <- max(data[[paste(input$colVarNum)]], na.rm=TRUE)
    print(paste("colSlider:", isolate(input$colSlider[1]), isolate(input$colSlider[2])))
    print(paste("In updateColorSlider(). colVarNum:", input$colVarNum, min, max))
    thirtythree <- quantile(data[[paste(input$colVarNum)]], 0.33, na.rm=TRUE)
    sixtysix <- quantile(data[[paste(input$colVarNum)]], 0.66, na.rm=TRUE)
    
    absMin <- as.numeric(unname(rawAbsMin()[paste(input$colVarNum)]))
    absMax <- as.numeric(unname(rawAbsMax()[paste(input$colVarNum)]))
    absStep <- max((max-min)*0.01, abs(min)*0.001, abs(max)*0.001)
    # print(paste("class(max)", class(max), "class(min)", class(min)))
    # print(paste("class(absMax)", class(absMax), "class(absMin)", class(absMin), "class(absStep)", class(absStep)))
    # print(paste(absMin, min, max, absMax))
    if(varClass[[input$colVarNum]] == "numeric") {
      # print("In updated slider: numeric")
      # if (absMax == absMin) {absMax <- (absMax + 1)}
      updateSliderInput(session,
                        "colSlider",
                        step = signif(absStep, digits = 4),
                        min = signif(absMin-absStep*10, digits = 4),
                        max = signif(absMax+absStep*10, digits = 4),
                        value = c(unname(thirtythree), unname(sixtysix))
      )
    }
    if(varClass[[input$colVarNum]] == "integer") {
      # print("In updated slider: integer")
      # if (absMax == absMin) {absMax <- (absMax + 1)}
      updateSliderInput(session,
                        "colSlider",
                        min = absMin,
                        max = absMax,
                        value = c(floor(thirtythree), ceiling(sixtysix))
      )
    }
  }

  updateXSlider <- observeEvent(input$updateX, {
    updateSlider(input$xInput, input$plot_brush$xmin, input$plot_brush$xmax)
  })
  
  updateYSlider <- observeEvent(input$updateY, {
    updateSlider(input$yInput, input$plot_brush$ymin, input$plot_brush$ymax)
  })
  
  updateBothSlider <- observeEvent(input$updateBoth, {
    updateSlider(input$xInput, input$plot_brush$xmin, input$plot_brush$xmax)
    updateSlider(input$yInput, input$plot_brush$ymin, input$plot_brush$ymax)
  })
  
  observeEvent(input$highlightData, {
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
  
  observe({
    print("Observing.")
    if (!is.null(isolate(colorData())) & as.character(input$colVarNum) != "") {
      updateColorSlider()
    }
  })
})
