library(shiny)
library(DT)
library(topsis)
library(jsonlite)
library(shinyBS)
library(htmlwidgets)

customTabFiles <- list.files('tabs', pattern = "*.R")

customTabEnvironments <- lapply(customTabFiles, function(filename) {
  env <- new.env()
  source(file.path('tabs',filename), local = env)
  # debugSource(file.path('tabs',filename), local = env)
  env
})

# Launch with testing datasets
if (Sys.getenv('DIG_INPUT_CSV') == "") {
  Sys.setenv(DIG_INPUT_CSV=file.path('datasets','WindTurbineForOptimization','mergedPET.csv'))
  # Sys.setenv(DIG_INPUT_CSV=file.path('datasets','WindTurbine','mergedPET.csv'))
}

#options(shiny.trace=TRUE)
#options(shiny.fullstacktrace = TRUE)
#options(error = function() traceback(2))
#options(shiny.error = function() traceback(2))


#---------------------Global Variables-------------------------#
palette(c("#E41A1C", "#377EB8", "#4DAF4A", "#984EA3",
         "#FF7F00", "#FFFF33", "#A65628", "#F781BF", "#999999"))

importData <- NULL #import session data

EnumerationMaxDisplay = 3

defaultNameLength = 25

#Initialize empty data frame for transfer functions in Ranking tab
xFuncs <- data.frame()
xFuncs <- xFuncs[1:4,]
row.names(xFuncs) <- c("Values", "Scores", "Slopes", "Y_ints")
openToolTip <- NULL
openToolTip <- data.frame()

bayesianDirection <- list()
bayesianType <- list()
bayesianParams <- list()
#customFilePath < list()
path <- NULL

bayesianUIInitialized <- FALSE
#-------------------End Global Variables-----------------------#


shinyServer(function(input, output, session) {
  
  makeReactiveBinding("xFuncs")
  makeReactiveBinding("bayesianUIInitialized")
  makeReactiveBinding("bayesianDirection")
  makeReactiveBinding("bayesianType")
  makeReactiveBinding("bayesianParams")

  uiElements <- reactiveValues(constants = FALSE)
  importFlags <- reactiveValues(tier1 = FALSE, tier2 = FALSE, ranking = FALSE, ranges = FALSE, bayesian = FALSE)
  
  session$onSessionEnded(function() {
    stopApp()
  })
  
  # Get Data -----------------------------------------------------------------
  raw <- c()
  query <- parseQueryString(isolate(session$clientData$url_search))

  # output$debug <- renderText({
  #   paste(names(query), query, sep = "=", collapse=", ")
  # })
  
  petConfig = NULL

  # For the Docker version of the Visualizer
  if (!is.null(query[['csvfilename']])) {
    print("In read raw")
    # raw.csv(paste0(dirname(sys.frame(1)$ofile), "/../webserver/public/csvs/", query[['csvfilename']]), fill=T)
    # raw = read.csv(paste0(dirname("/csvs/", query[['csvfilename']]), fill=T)
    raw = read.csv(paste("/media/sf_kevin/Downloads/", query[['csvfilename']], sep=''), fill=T)
  }
  
  # For launching the Visualizer from the Results Browser 
  else if (nzchar(Sys.getenv('DIG_INPUT_CSV')))
  {
    raw = read.csv(Sys.getenv('DIG_INPUT_CSV'), fill=T)
    petConfigFilename = gsub("mergedPET.csv", "pet_config.json", Sys.getenv('DIG_INPUT_CSV'))
    if(file.exists(petConfigFilename))
      petConfig = fromJSON(petConfigFilename)
  }
  
  # For development
  else
  {
    # Needed setup for regression testing:
    # raw = read.csv("RegressionTestingDataset.csv", fill=T)
    # mapping = read.csv("RegressionTestingMapping.csv", fill=T)
    
    # Useful test setups:
    # raw = read.csv("../../../results/mergedPET.csv", fill=T)
    # petConfig = fromJSON("../../../results/pet_config.json", fill=T)
    
    # raw = iris
    # petConfig = read.csv("iris_config.json", fill = T)
  }
  
  petConfigPresent <- !is.null(petConfig)
  
  # Process PET Configuration File ('pet_config.json') -----------------------
  designVariableNames <- NULL
  numericDesignVariables <- FALSE
  enumeratedDesignVariables <- FALSE
  designVariables <- NULL
  objectiveNames <- NULL
  units <- NULL
  reverseUnits <- NULL
  
  addUnits <- function(name) {
    if(is.null(units) | !(name %in% names(units))) {
      name
    } else {
      units[[name]]$nameWithUnit
    }
  }
  
  removeUnits <- function(nameWithUnits) {
    if(is.null(reverseUnits) | !(nameWithUnits %in% names(reverseUnits))) {
      nameWithUnits
    } else {
      reverseUnits[[nameWithUnits]]
    }
  }
  
  if(petConfigPresent) {
    designVariableNames <- names(petConfig$drivers[[1]]$designVariables)
    numericDesignVariables <- lapply(petConfig$drivers[[1]]$designVariables, function(x) {"RangeMax" %in% names(x)})
    enumeratedDesignVariables <- lapply(petConfig$drivers[[1]]$designVariables, function(x) {"type" %in% names(x)})
    dvTypes <- unlist(lapply(numericDesignVariables, function(x) { if(x) "Numeric" else "Enumeration"}))
    dvSelections <- unlist(lapply(petConfig$drivers[[1]]$designVariables, function(x) {if("type" %in% names(x) && x$type == "enum") paste0(unlist(x$items), collapse=",") else paste0(c(x$RangeMin, x$RangeMax), collapse=",")}))
    designVariables <- data.frame(VarName=designVariableNames, Type=dvTypes, Selection=dvSelections)
    objectiveNames <- names(petConfig$drivers[[1]]$objectives)
    petConfigNumSamples <- unlist(strsplit(as.character(petConfig$drivers[[1]]$details$Code),'='))[2]
    petConfigSamplingMethod <- petConfig$drivers[[1]]$details$DOEType
    petGeneratedConfigurationModel <- petConfig$GeneratedConfigurationModel
    petSelectedConfigurations <- petConfig$SelectedConfigurations
    petName <- petConfig$PETName
    petMgaName <- petConfig$MgaFilename
    
    # Generate Units Tables
    units <- list()
    reverseUnits <- list()
    for (i in 1:length(designVariableNames))
    {
      unit <-petConfig$driver[[1]]$designVariables[[designVariableNames[i]]]$units
      if(is.null(unit)) {
        unit <- ""
        nameWithUnit <- designVariableNames[[i]]
      }
      else
      {
        unit <- gsub("\\*\\*", "^", unit) #replace Python '**' with '^'
        unit <- gsub("inch", "in", unit)  #replace 'inch' with 'in' since 'in' is a Python reserved word
        unit <- gsub("yard", "yd", unit)  #replace 'yard' with 'yd' since 'yd' is an OpenMDAO reserved word
        nameWithUnit <- paste0(designVariableNames[i]," (",unit,")")
      }
      units[[designVariableNames[[i]]]] <- list("unit"=unit, "nameWithUnit"=nameWithUnit)
      reverseUnits[[nameWithUnit]] <- designVariableNames[[i]]
    }
    for (i in 1:length(objectiveNames))
    {
      unit <-petConfig$driver[[1]]$objectives[[objectiveNames[i]]]$units
      if(is.null(unit)) {
        unit <- ""
        nameWithUnit <- objectiveNames[[i]]
      }
      else
      {
        unit <- gsub("\\*\\*", "^", unit)
        nameWithUnit <- paste0(objectiveNames[i]," (",unit,")")
      }
      units[[objectiveNames[[i]]]] <- list("unit"=unit, "nameWithUnit"=nameWithUnit)
      reverseUnits[[nameWithUnit]] <- objectiveNames[[i]]
    }
  }
  
  output$petConfigPresent <- reactive({
    print(paste("petConfigPresent:",petConfigPresent))
    petConfigPresent
  })
  
  output$numericDesignVariables <- reactive({
    TRUE %in% numericDesignVariables
  })
  
  output$enumerationDesignVariables <- reactive({
    TRUE %in% enumeratedDesignVariables
  })
  
  outputOptions(output, "petConfigPresent", suspendWhenHidden=FALSE)
  outputOptions(output, "numericDesignVariables", suspendWhenHidden=FALSE)
  outputOptions(output, "enumerationDesignVariables", suspendWhenHidden=FALSE)
  
  output$noPetConfigMessage <- renderText(paste("No pet_config.json file was found."))
  
  # Build info data frame
  variables <- lapply(names(raw), function(varName) {
    list(name = varName,
         nameWithUnits = addUnits(varName),
         type = if (varName %in% designVariableNames) "Design Variable" else "Objective"
    )
  })
  names(variables) <- names(raw)
  
  info <- list(variables = variables)
  
  # Call the different Server functions for the different tabs ---------------
  lapply(customTabEnvironments, function(customEnv) {
    do.call(customEnv$server,
            list(input, output, session, raw, info))
  })
  
  # Replace add units to column names in raw
  colnames(raw) <- sapply(colnames(raw), addUnits)
  
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
  
  resetImportFlags <- function(...){
    importFlags$tier1 <- F
    importFlags$tier2 <- F
    importFlags$ranking <- F
    importFlags$ranges <- F
    importFlags$bayesian <- F
  }
  
  initImport <- observeEvent(input$importSession, {
    print("in import session")
    if(input$loadSessionName == ""){
      path <- fileChoose()
      req(path)
    }
    else
      path <- input$loadSessionName
    req(file.exists(path))
    importData <<- read.csv(path, header = TRUE, strip.white = TRUE)
    importFlags$tier1 <- TRUE
  })
  
  tier1Import <- observeEvent(importFlags$tier1, {
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
                         "viewAllFilters",
                         
                         "transpose",
                         "bayesianDisplayAll")
      
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
                        "activateRanking",
                        "weightMetrics",
                        "bayesianDisplayVars")
                     
      tier1Colors <- c("normColor", 
                       "minColor", 
                       "maxColor", 
                       "midColor", 
                       "highlightColor",
                       "rankColor",
                       "bayHistColor",
                       "bayOrigColor",
                       "bayResampledColor")
      
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
  
  tier2Import <- observeEvent(importFlags$tier2, {
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
              #if(grepl("new", current))
              #  updateSelectInput(session, current, selected = rng)
              #else
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
    
    # importFlags$tier1 <- F
    # importFlags$tier2 <- F
    # importFlags$ranking <- F
    # importFlags$ranges <- F
    # importFlags$bayesian <- F
    
    data
  })

  # Pre-processing -----------------------------------------------------------
  
  if(is.null(petConfig))
  
  print("Starting Preprocessing of the Data -----------------------------------------")
  
  varNames = names(raw)
  varClass = sapply(raw,class)
  print(paste("varClass:"))
  print(paste(varClass))
  
  abbrvNames <- function(widthInfo) {
    windowWidth <- widthInfo[1]		
    screenRes <- widthInfo[2]		
    
    abbrevLength <- as.integer((windowWidth/screenRes)*41.406 - 6.129)		
    
    if(abbrevLength < 10)		
      abbrevLength <- defaultNameLength		
    
    #print(paste0(windowWidth, ":", abbrevLength))		
    abbreviate(varNames, abbrevLength)		
  }
  
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
  
  varConstant <- reactive({
    print(paste("varConstant:"))
    answer <- subset(varNames, !(varNames %in% varRange()))
    if(length(answer) > 0)
      uiElements$constants <- T
    print(paste(answer))
    answer
  })

  observe({
    
    isolate({
    print("Updating Panel Selections...")
    updateSelectInput(session, 'bayesianDisplayVars', choices = varRangeNum(), selected = varRangeNum()[1:2])
    updateSelectInput(session, "colVarFactor", choices = varRangeFac())  
    updateSelectInput(session, "colVarNum", choices = varRangeNum())#, selected = varRangeNum()[c(1)])
    updateSelectInput(session, "display", choices = varRange(), selected = varRange()[c(1,2)])
    updateSelectInput(session, "weightMetrics", choices = varRangeNum(), selected = NULL)
    updateSelectInput(session, "xInput", choices = varRange(), selected = varRange()[c(1)])
    updateSelectInput(session, "yInput", choices = varRange(), selected = varRange()[c(2)])
    setupToolTip()
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
    updateCheckboxInput(session, "roundTables", value = FALSE)
    updateCheckboxInput(session, "stickyFilters", value = TRUE)
    
    # Render
    updateCheckboxInput(session, "autoRender", value = TRUE)
    updateCheckboxInput(session, "trendLines", value = FALSE)
    updateCheckboxInput(session, "upperPanel", value = FALSE)
    
    # Data point style
    updateRadioButtons(session, "pointStyle", selected = "1")
    updateRadioButtons(session, "pointSize", selected = "1")
    
    # Automatically Refresh
    updateCheckboxInput(session, "autoInfo", value = TRUE)
    updateCheckboxInput(session, "autoRanking", value = TRUE)
    updateCheckboxInput(session, "autoRange", value = TRUE)
    
    # Color
    updateColourInput(session, "normColor", "Normal", "black")
    updateColourInput(session, "maxColor", "Worst", "#E74C3C")
    updateColourInput(session, "midColor", "In Between", "#F1C40F")
    updateColourInput(session, "minColor", "Best", "#2ECC71")
    updateColourInput(session, "highlightColor", "Highlighted", "#377EB8")
    updateColourInput(session, "rankColor", "Ranked", "#D13ABA")
    updateColourInput(session, "bayHistColor", "Histogram", "wheat")
    updateColourInput(session, "bayOrigColor", "Original", "#000000")
    updateColourInput(session, "bayResampledColor", "Resampled", "#5CC85C")
  })

  print(paste("Finished Preprocessing the Data ----------------------------------------------------"))
  
  # Filters (Enumerations, Sliders) and Constants ----------------------------
  
  output$displayFilters <- reactive({
    display <- !(input$inTabset == "Options" | input$inTabset == "Uncertainty Quantification")
  })
  
  outputOptions(output, "displayFilters", suspendWhenHidden=FALSE)
  
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
  
  setupToolTip <- function(...){
    varsList <- match(varRangeNum(), varNames)
    openToolTip <<- openToolTip[1:length(varsList),]
    row.names(openToolTip) <<- unlist(strsplit(toString(varsList), ", "))
    openToolTip$display <<- F
    openToolTip$valApply <<- 0
  }
  
  
  
  fullFilterUI <- reactive({
    vars <- filterVars()
    data <- raw_plus()
    
    facVars <- NULL
    intVars <- NULL
    numVars <- NULL
    
    for(i in 1:length(vars)){
      current <- vars[i]
      switch(varClass[current],
             "factor" = {facVars <- c(facVars, current)},
             "integer" = {intVars <- c(intVars, current)},
             "numeric" = {numVars <- c(numVars, current)}
      )
    }
    
    div(
      fluidRow(
        lapply(facVars, function(column) {
          generateEnumUI(column)
        })
      ),
      fluidRow(
        lapply(intVars, function(column) {
          label <- abbrvNames(input$dimension)[column]
          isolate(generateSliderUI(column, label, "integer"))
        }),
        lapply(numVars, function(column) {
          label <- abbrvNames(input$dimension)[column]
          isolate(generateSliderUI(column, label, "numeric"))
        })
      )
    )
  })
  
  actionButton <- function(inputId, label, btn.style = "" , css.class = "") {
    if ( btn.style %in% c("primary","info","success","warning","danger","inverse","link"))
      btn.css.class <- paste("btn",btn.style,sep="-")
    else btn.css.class = ""
    tags$button(id=inputId, type="button", class=paste("btn action-button",btn.css.class,css.class,collapse=" "), label)
  }

  
  generateEnumUI <- function(current) {
    items <- names(table(raw_plus()[varNames[current]]))
    
    for(i in 1:length(items)){
      items[i] <- paste0(i, '. ', items[i])
    }
    
    selectVal <- input[[paste0('inp', current)]]
    if(is.null(selectVal) | !input$stickyFilters)
      selectVal <- items
    
    column(2, selectInput(inputId = paste0('inp', current),
                          label = varNames[current],
                          multiple = TRUE,
                          selectize = FALSE,
                          choices = items,
                          selected = selectVal)
    )
    
    
  }
  
  generateSliderUI <- function(current, label, mode) {
    
    if (varNames[current] %in% varRangeNum()) {
      
      sliderVal <- input[[paste0('inp', current)]]
      
      if(mode == "numeric"){
        min <- as.numeric(unname(rawAbsMin()[varNames[current]]))
        max <- as.numeric(unname(rawAbsMax()[varNames[current]]))
        step <- signif(max((max-min)*0.01, abs(min)*0.001, abs(max)*0.001), digits = 4)
        sliderMin <- signif(as.numeric(unname(rawAbsMin()[varNames[current]])) - step*10, digits = 4)
        sliderMax <- signif(as.numeric(unname(rawAbsMax()[varNames[current]])) + step*10, digits = 4)
      }
      else{
        step <- 0
        sliderMax <- as.numeric(unname(rawAbsMax()[varNames[current]]))
        sliderMin <- as.numeric(unname(rawAbsMin()[varNames[current]]))
      }
      
      if(is.null(sliderVal) | !input$stickyFilters)
        sliderVal <- c(signif(sliderMin-step*10, digits = 4), signif(sliderMax+step*10, digits = 4))
      
      column(2, 
             useShinyjs(),
             wellPanel(id = paste0("slider_tooltip", current), 
                       style = "position: absolute; z-index: 65; box-shadow: 10px 10px 15px grey; width: 20vw; left: 1vw; top: -275%; display: none;",
                       h4(label),
                       textInput(paste0("min_inp", current), "Min:"),
                       textInput(paste0("max_inp", current), "Max:"),
                       actionButton(paste0("submit", current), "Apply", "success")),
             sliderInput(paste0('inp', current),
                         label,
                         step = step,
                         min = sliderMin,
                         max = sliderMax,
                         value = sliderVal)
      )
    }
  }
  
  openSliderToolTip <- function(current) {
    toggle(paste0("slider_tooltip", current))
    openToolTip[toString(current), "display"] <<- !openToolTip[toString(current), "display"]
    for(i in 1:length(openToolTip[,"display"])){
      row = row.names(openToolTip)[i]
      if(row != current && openToolTip[row,"display"]){
        toggle(paste0("slider_tooltip", row))
        openToolTip[row,"display"] <<- F
      }
    }
    
  }
  
  # Slider tooltip handler
  observe({
    lapply(varNames, function(name) {
      if(name %in% varRangeNum()){
        current = match(name, varNames)
        
        onevent("dblclick", paste0("inp", current), openSliderToolTip(current))
        observe({
          input$lastkeypresscode
          input[[paste0("submit", current)]] 
          
          isolate({
            currentValOfApply <- openToolTip[toString(current), "valApply"]
            if(((!is.null(input$lastkeypresscode) && input$lastkeypresscode == 13) || input[[paste0("submit", current)]] != currentValOfApply) && openToolTip[toString(current), "display"]){
              if(input[[paste0("submit", current)]] != currentValOfApply) 
                openToolTip[toString(current), "valApply"] <<- input[[paste0("submit", current)]]
              sliderVal = input[[paste0('inp', current)]]
              newMin = input[[paste0("min_inp", current)]]
              newMax = input[[paste0("max_inp", current)]]
              updateTextInput(session, paste0("min_inp", current), value = "")
              updateTextInput(session, paste0("max_inp", current), value = "")
              suppressWarnings({ #Suppress warnings from non-numeric inputs
                if(!is.null(newMin) && newMin != "" && !is.na(as.numeric(newMin)))
                  sliderVal = as.numeric(c(newMin, sliderVal[2]))
                if(!is.null(newMax) && newMax != "" && !is.na(as.numeric(newMax)))
                  sliderVal = as.numeric(c(sliderVal[1], newMax))
              })
              updateSliderInput(session, paste0('inp', current), value = sliderVal)
              toggle(paste0("slider_tooltip", current))
              openToolTip[toString(current), "display"] <<- F
            }
          })
        })
      }
    })
  })
  
  output$constants <- renderUI({
    print("In render constants")
    
    fluidRow(
      lapply(varConstant(), function(column) {

        switch(varClass[column],
               "numeric" = column(2, p(strong(paste0(column,":")), unname(raw_plus()[1,column]))),
               "integer" = column(2, p(strong(paste0(column,":")), unname(raw_plus()[1,column]))),
               "factor"  = column(2, p(strong(paste0(column,":")), unname(raw_plus()[1,column])))
        )
      })
      # lapply(varConstant(), function(x) {column(2, p(strong(paste0(addUnits(x),":")), unname(raw_plus()[1,column])))})
      # lapply(varConstant(), function(x) {column(2, p(strong(paste0(x,":")), unname(raw_plus()[1,column])))})
    )
  })
  
  output$constantsPresent <- reactive({
    present <-uiElements$constants # TODO: Fix this to actually detect constants
  })
  
  outputOptions(output, "constantsPresent", suspendWhenHidden=FALSE)
  
  observeEvent(input$resetSliders, {
    print("In resetDefaultSliders()")
    for(column in 1:length(varNames)){
      switch(varClass[column],
             "numeric" = 
             {
               max <- as.numeric(unname(rawAbsMax()[varNames[column]]))
               min <- as.numeric(unname(rawAbsMin()[varNames[column]]))
               diff <- (max-min)
               if (diff != 0) {
                 step <- max(diff*0.01, abs(min)*0.001, abs(max)*0.001)
                 updateSliderInput(session, paste0('inp', column), value = c(signif(min-step*10, digits = 4), signif(max+step*10, digits = 4)))
               }
             },
             "integer" = 
             {
               max <- as.integer(unname(rawAbsMax()[varNames[column]]))
               min <- as.integer(unname(rawAbsMin()[varNames[column]]))
               if(min != max) {
                 updateSliderInput(session, paste0('inp', column), value = c(min, max))
               }
             },
             "factor"  = updateSelectInput(session, paste0('inp', column), selected = names(table(raw_plus()[varNames[column]])))
      )
    }
  })
  
  # Data processing ----------------------------------------------------------
    
  filterData <- reactive({
    print("In filterData()")
    data <- raw_plus()
    for(column in 1:length(varNames)) {
      inpName=paste("inp",toString(column),sep="")
      nname = varNames[column]
      rng = input[[inpName]]
      if(length(rng) != 0) {
        if((varClass[column]=="numeric" | varClass[column]=="integer")) {
          isolate({
            above <- (data[[nname]] >= rng[1])
            below <- (data[[nname]] <= rng[2])
            inRange <- above & below
          })
        } 
        else if (varClass[column]=="factor") {
            rng <- unlist(lapply(rng, function(factor){
              sub("[0-9]+. ","", factor)}))
            inRange <- (data[[nname]] %in% rng)
        }
        inRange <- inRange | is.na(data[[nname]])
        data <- subset(data, inRange)
      }
    }
    print("Data Filtered")
    data
  })
  
  colorData <- reactive({
    print("In colorData()")
    data <- filterData()
    data$color <- character(nrow(data))
    data$color <- input$normColor

    colorType = input$colType
    
    switch(colorType,
           "Max/Min" = 
           {
             name <- input$colVarNum
             bottom <- input$colSlider[1]
             top <- input$colSlider[2]
             print(paste("Coloring Data:", name, bottom, top))
             data$color[(data[[name]] >= bottom) & (data[[name]] <= top)] <- input$midColor
             if (input$radio == "max") {
               data$color[data[[name]] < bottom] <- input$maxColor
               data$color[data[[name]] > top] <- input$minColor
             } 
             else {
               data$color[data[[name]] < bottom] <- input$minColor
               data$color[data[[name]] > top] <- input$maxColor
             }
           },
           "Discrete" = 
           {
             varList = names(table(raw_plus()[input$colVarFactor]))
             for(i in 1:length(varList)){
               data$color[(data[[input$colVarFactor]] == varList[i])] <- palette()[i]
             }
           },
           "Highlighted" = 
           {
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
           },
           "Ranked" = data[input$dataTable_rows_selected, "color"] <- input$rankColor
    )
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
  
  pairsSetup <- function(...){
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
    p
  }
  
  
  output$pairsPlot <- renderPlot({
    
    print("In render plot")
    
    output$displayError <- renderText("")
    output$filterError <- renderText("")
    
    if (length(input$display) >= 2 & nrow(filterData()) > 0) {
      print("Rendering Plot.")
      
      pairsSetup()
      
      print("Plot Rendered.")
    }
    else { 
      if (nrow(filterData()) == 0) {
        output$filterError <- renderText(
            "<br/>No data points fit the current filtering scheme.")
      }
      if (length(input$display) < 2) {
        output$displayError <- renderText(
          "<br/>Please select two or more Display Variables.")
      }
    }
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
    else if(num_vars > 11){
      margin <- -0.5
      plot <- 0.9
      buffer <- 0.2
    }
    else if(num_vars > 12){
      buffer <- 0.15
      plot <- 0.9
      margin <- -0.6
    }
    else if(num_vars > 18){
      margin <- -1.51
      plot <- 0.9
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
    varsList()
  })
  
  slowData <- eventReactive(input$renderPlot, {
    colorData()
  })
  
  output$stats <- renderText({
    print("In render stats")
    if(nrow(filterData()) > 0){
      if(input$autoInfo == TRUE){
        table <- infoTable(input$colType)
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
      importFlags$ranges <- TRUE
      importFlags$bayesian <- TRUE
    }
    table
})

  infoTable <- function(colorType){
    print("In info table")
    tb <- table(factor(colorData()$color, 
                       c(input$midColor, 
                         input$minColor, 
                         input$highlightColor, 
                         input$rankColor,
                         input$maxColor, 
                         input$normColor)))
    switch(colorType,
           "Max/Min" = paste0("Total Points: ", nrow(raw),
                              "\nCurrent Points: ", nrow(colorData()),
                              "\nColored Points: ", sum(tb[[input$minColor]], tb[[input$midColor]], tb[[input$maxColor]], tb[[input$normColor]]),
                              "\nWorst Points: ", tb[[input$maxColor]],
                              "\nIn Between Points: ", tb[[input$midColor]],
                              "\nBest Points: ", tb[[input$minColor]]
           ),
           "Discrete" = {tb <- table(factor(colorData()$color, palette()))
           d_vars <- names(table(raw_plus()[input$colVarFactor]))
           output_string <- paste0("Total Points: ", nrow(raw),
                                   "\nCurrent Points: ", nrow(colorData()),
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
           output_string},
           "Highlighted" = paste0("Total Points: ", nrow(raw),
                                  "\nCurrent Points: ", nrow(colorData()),
                                  "\nHighlighted Points: ", tb[[input$highlightColor]]
           ),
           "Ranked" = paste0("Total Points: ", nrow(raw),
                             "\nCurrent Points: ", nrow(colorData()),
                             "\nRanked Points: ", tb[[input$rankColor]]
           ),
           paste0("Total Points: ", nrow(raw),
                  "\nCurrent Points: ", nrow(colorData()),
                  "\nColored Points: ", tb[[input$normColor]]
           )
    )
  }
  
  slowInfoTable <- eventReactive(input$updateStats, {
    infoTable(input$colType)
  })

  output$exportData <- downloadHandler(
    filename = function() { paste('data-', Sys.Date(), '.csv', sep='') },
    content = function(file) { write.csv(filterData(), file) }
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
  
  # datatable_display <- function(...){
  #   df <- filterData()
  #   colnames(df) <- sapply(names(df), function(x) abbreviate(unlist(strsplit(x, "[.]"))[2], 9))
  #   df
  # }
  
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
                            min = 0,
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
                                    rawAbsMin()[[varName]],
                                    " = 1, ",
                                    rawAbsMax()[[varName]],
                                    "= 0.5"),
                                 value = funcVal),
                       utilityPlot(current)))
    )
  }
  
  fullMetricUI <- reactive({
    a <- input$activateRanking # Force reaction
    if(!importFlags$ranking){
      lapply(metricsList(), function(column) {
        isolate(generateMetricUI(column))
      })
    }
    else{
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
    }
  })
  
  # reactToActivateTransferFunctions <- observe({
  #   lapply(metricsList(), function(column) {
  #     observeEvent(input[[paste0('util', column)]], {
  #       if(!input[[paste0('util', column)]])
  #         xFuncs[[toString(column)]] <<- NULL
  #     })
  #   })
  # })
  
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
    
    importFlags$ranking <- FALSE
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
  
  #Event handler for "Color by selected rows"
  observeEvent(input$colorRanked, {
    req(input$dataTable_rows_selected)
    updateTabsetPanel(session, "inTabset", selected = "Pairs Plot")
    updateSelectInput(session, "colType", selected = "Ranked")
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
  
  # PET Refinement Tab -------------------------------------------------------
  all_ranges <- list()  #List of all ranges: 1 for all numerics and individual ones for each factor
  
  # slowNumericRangeData <- eventReactive(input$updateRanges, {
  #   all_ranges$numerics <<- do.call(rbind, lapply(filterData()[varRangeNum()], summary))
  # })
  
  output$petDriverConfig <- renderUI({
    fluidRow(
      column(3, selectInput("petSamplingMethod", "New Sampling Method:", choices = c("Full Factorial", "Uniform", "Central Composite", "Opt Latin Hypercube"), selected = petConfigSamplingMethod)),
      column(3, textInput("petNumSamples", "New Number of Samples:", value = petConfigNumSamples))
    )
  })
  
  output$petRename <- renderUI({
    fluidRow(
      column(12, h5(strong("Original MGA Filename: ")), textOutput("mgaFilenameText")),
      column(12, h5(strong("Original PET Name: ")), textOutput("currentPetNameText"), br()),
      column(12, textInput("newPetName", "New PET Name:", value = paste0(petName, "_Refined")))
    )
  })
  
  output$originalDriverSettings <- renderText(paste(petConfigSamplingMethod," sampling with 'num_samples=", petConfigNumSamples,"' yielded ", nrow(raw), " points.", sep = ""))
  output$mgaFilenameText <- renderText(petMgaName)
  output$generatedConfigurationModelText <- renderText(petGeneratedConfigurationModel)
  output$currentPetNameText <- renderText(petName)
  
  output$original_numeric_ranges <- renderUI({
    
    lapply(rownames(designVariables), function(row){
      
      var = addUnits(levels(droplevels(designVariables[row, "VarName"])))
      type = gsub("^\\s+|\\s+$", "", levels(droplevels(designVariables[row, "Type"])))
      selection = unlist(strsplit(gsub("^\\s+|\\s+$", "", levels(droplevels(designVariables[row, "Selection"]))), ","))
      
      if(type == "Numeric"){
        global_index = which(varNames == var)
        
        original_min <- as.numeric(selection[1])
        original_max <- as.numeric(selection[2])
        
        min_input <- NULL
        max_input <- NULL
        if(importFlags$ranges){
          max_input <- importData[[paste0('newMax', global_index)]]
          min_input <- importData[[paste0('newMin', global_index)]]
          NULL #This makes sure nothing appears in the UI
        }
        
        
        refined <- filterData()[var]
        if(dim(refined)[1] == 0){
          min_refined <- "No data available in table"
          max_refined <- "No data available in table"
        }
        else{
          min_refined <- sprintf("%.3e", min(filterData()[var]))
          max_refined <- sprintf("%.3e", max(filterData()[var]))
        }
        
        fluidRow(
          column(2, h5(strong(var))),
          column(1, actionButton(paste0('applyOriginalRange', global_index), 'Apply')),
          column(1, h5(sprintf("%.3e", original_min))),
          column(1, h5(sprintf("%.3e", original_max))),
          column(1, actionButton(paste0('applyRefinedRange', global_index), 'Apply')),
          column(1, h5(min_refined)),
          column(1, h5(max_refined)),
          column(2,
                 textInput(paste0('newMin', global_index),
                           NULL,
                           placeholder = "Enter min",
                           value = min_input)
          ),
          column(2,
                 textInput(paste0('newMax', global_index),
                           NULL,
                           placeholder = "Enter max",
                           value = max_input)
          ),
          hr()
        )
      }
    })
  })
  
  output$original_enumeration_ranges <- renderUI({
    
    lapply(rownames(designVariables), function(row){
      
      var = addUnits(levels(droplevels(designVariables[row, "VarName"])))
      type = gsub("^\\s+|\\s+$", "", levels(droplevels(designVariables[row, "Type"])))
      selection = gsub(",", ", ", levels(droplevels(designVariables[row, "Selection"])))
      
      if(type == "Enumeration"){
        global_index = which(varNames == var)
        
        original <- selection
        if(length(unlist(strsplit(original, ","))) > EnumerationMaxDisplay)
          original = paste0("List of ", length(unlist(strsplit(original, ","))), " Enumerations.")
        
        refined <- toString(unique(filterData()[var])[,1])
        if(length(unlist(strsplit(refined, ","))) > EnumerationMaxDisplay)
          refined = paste0("List of ", length(unlist(strsplit(refined, ","))), " Enumerations.")
        if(refined == "")
          refined = "No data available in table"
        
        input_selection <- NULL
        if(importFlags$ranges){
          input_selection <- importData[[paste0('newSelection', global_index)]]
          NULL #This makes sure nothing appears in the UI
        }
        
        fluidRow(
          column(2, h5(strong(var))),
          column(1, actionButton(paste0('applyOriginalSelection', global_index), 'Apply')),
          column(2, h5(original)),
          column(1, actionButton(paste0('applyRefinedSelection', global_index), 'Apply')),
          column(2, h5(refined)),
          column(4,
                 textInput(paste0('newSelection', global_index),
                           NULL,
                           placeholder = "Enter selection",
                           value = input_selection)
          ),
          hr()
        )
      }
    })
  })
  
  output$original_configuration_ranges <- renderUI({
    
    original <- paste0(petSelectedConfigurations, collapse=",")
    originalCount <- length(petSelectedConfigurations)
    if(originalCount > EnumerationMaxDisplay)
      original = paste0("List of ", originalCount, " Configurations.")
    
    refined <- paste0(unique(filterData()$CfgID), collapse=",")
    refinedCount <- length(unique(filterData()$CfgID))
    if(refinedCount > EnumerationMaxDisplay)
      refined = paste0("List of ", refinedCount, " Configurations.")
    if(refined == "")
      refined = "No configurations available."
    
    input_selection <- NULL
    # if(importFlags$ranges){
    #   input_selection <- importData[[paste0('newSelection', global_index
    #   NULL #This makes sure nothing appears in the UI
    # }
    
    fluidRow(
      column(2, h5(strong("Configuration Name(s)"))),
      column(1, actionButton('applyOriginalCfgIDs', 'Apply')),
      column(2, h5(original)),
      column(1, actionButton('applyRefinedCfgIDs', 'Apply')),
      column(2, h5(refined)),
      column(4,
             textInput('newCfgIDs',
                       NULL,
                       placeholder = "Enter selection",
                       value = input_selection)
      )
    )
  })
  
  observeEvent(input$applyOriginalCfgIDs, {
    original <- paste0(petSelectedConfigurations, collapse=",")
    updateTextInput(session, 'newCfgIDs', value = original)
  })
  
  observeEvent(input$applyRefinedCfgIDs, {
    refined <- paste0(unique(filterData()$CfgID), collapse=",")
    updateTextInput(session, 'newCfgIDs', value = refined)
  })
  
  # Pull values from petConfig.json File
  
  reactToApplyOriginalButtons <- observe({
    lapply(rownames(designVariables), function(row) {
      var = levels(droplevels(designVariables[row, "VarName"]))
      type = gsub("^\\s+|\\s+$", "", levels(droplevels(designVariables[row, "Type"])))
      global_i = which(varNames == var)
      if(type == "Numeric"){
        original = unlist(strsplit(gsub("^\\s+|\\s+$", "", levels(droplevels(designVariables[row, "Selection"]))), ","))
        observeEvent(input[[paste0('applyOriginalRange', global_i)]], {
          updateTextInput(session, paste0('newMin', global_i), value = as.numeric(original[1]))
          updateTextInput(session, paste0('newMax', global_i), value = as.numeric(original[2]))
        })
      }
      else if(type == "Enumeration"){
        original = gsub(",", ", ", levels(droplevels(designVariables[row, "Selection"])))
        observeEvent(input[[paste0('applyOriginalSelection', global_i)]], {
          updateTextInput(session, paste0('newSelection', global_i), value = original)
        })
      }
    })
  })
  
  observeEvent(input$applyAllOriginalNumeric, {
    lapply(rownames(designVariables), function(row) {
      var = levels(droplevels(designVariables[row, "VarName"]))
      type = gsub("^\\s+|\\s+$", "", levels(droplevels(designVariables[row, "Type"])))
      global_i = which(varNames == var)
      if(type == "Numeric"){
        original = unlist(strsplit(gsub("^\\s+|\\s+$", "", levels(droplevels(designVariables[row, "Selection"]))), ","))
        updateTextInput(session, paste0('newMin', global_i), value = as.numeric(original[1]))
        updateTextInput(session, paste0('newMax', global_i), value = as.numeric(original[2]))
      }
    })
  })
  
  observeEvent(input$applyAllOriginalEnum, {
    lapply(rownames(designVariables), function(row) {
      var = levels(droplevels(designVariables[row, "VarName"]))
      type = gsub("^\\s+|\\s+$", "", levels(droplevels(designVariables[row, "Type"])))
      global_i = which(varNames == var)
      if(type == "Enumeration"){
        original = gsub(",", ", ", levels(droplevels(designVariables[row, "Selection"])))
        updateTextInput(session, paste0('newSelection', global_i), value = original)
      }
    })
  })
  
  # Pull values from filterData
  
  reactToApplyRefinedButtons <- observe({
    lapply(varNum, function(var) {
      global_index = which(varNames == var)
      observeEvent(input[[paste0('applyRefinedRange', global_index)]], {
        updateTextInput(session, paste0('newMin', global_index), value = min(filterData()[var]))
        updateTextInput(session, paste0('newMax', global_index), value = max(filterData()[var]))
      })
    })
    lapply(varFac, function(var) {
      global_index = which(varNames == var)
      observeEvent(input[[paste0('applyRefinedSelection', global_index)]], {
        updateTextInput(session, paste0('newSelection', global_index), value = toString(unique(filterData()[var])[,1]))
      })
    })
  })
  
  observeEvent(input$applyAllRefinedNumeric, {
    lapply(varNum, function(var) {
      global_index = which(varNames == var)
      updateTextInput(session, paste0('newMin', global_index), value = min(filterData()[var]))
      updateTextInput(session, paste0('newMax', global_index), value = max(filterData()[var]))
    })
  })
  
  observeEvent(input$applyAllRefinedEnum, {
    lapply(varFac, function(var) {
      global_index = which(varNames == var)
      updateTextInput(session, paste0('newSelection', global_index), value = toString(unique(filterData()[var])[,1]))
    })
  })
  
  slowFactorRangeData <- eventReactive(input$updateRanges, {
    printFactorStatistics()
  })
  
  printFactorStatistics <- function(...){
    lapply(varRangeFac(), function(var) {
      all_ranges[[var]] <<- do.call(rbind, lapply(filterData()[var], summary))
      renderPrint({
        all_ranges[[var]]
      })
    })
  }
  
  output$factor_ranges <- renderUI({
    if(input$autoRange == TRUE){
      printFactorStatistics()
    }
    else {
      slowFactorRangeData()
    }
  })

  observeEvent(input$runRanges, {
    if (nzchar(Sys.getenv('DIG_INPUT_CSV'))) {
      resultsDirectory <- dirname(Sys.getenv('DIG_INPUT_CSV'))
      projectDirectory <- dirname(resultsDirectory)
      petConfigRefinedFilename <- file.path(resultsDirectory, "pet_config_refined.json")
      exportRangesFunction(petConfigRefinedFilename)
      system2("..\\Python27\\Scripts\\python.exe",
              args = c("..\\UpdatePETParameters.py",
                       "--pet-config",
                       petConfigRefinedFilename,
                       "--new-name",
                       paste0("\"",input$newPetName,"\"")),
              stdout = file.path(resultsDirectory, "UpdatePETParameters_stdout.log"),
              stderr = file.path(resultsDirectory, "UpdatePETParameters_stderr.log"),
              wait = FALSE)
    }
  })
  
  output$downloadRanges <- downloadHandler(
    filename = function() {paste('pet_config_refined_', Sys.Date(), '.json', sep='')},
    content = exportRangesFunction
  )
  
  exportRangesFunction <- function(file) { 
    petConfigRefined <- petConfig
    
    reassignDV <- function(dv, name) {
      global_i = which(varNames == addUnits(name))
      if("type" %in% names(dv) && dv$type == "enum") {
        selection <- strsplit(input[[paste0('newSelection', global_i)]], ",")
        if (length(unlist(selection)) > 1) {
          dv$items <- unlist(selection)
        } else {
          dv$items <- selection
        }
      } else {
        dv$RangeMin <- as.numeric(input[[paste0('newMin', global_i)]])
        dv$RangeMax <- as.numeric(input[[paste0('newMax', global_i)]])
      }
      dv
    }
    petConfigRefined$drivers[[1]]$designVariables <- Map(reassignDV, petConfigRefined$drivers[[1]]$designVariables, names(petConfigRefined$drivers[[1]]$designVariables))
    
    petConfigRefined$drivers[[1]]$details$Code <- paste0("num_samples=", input$petNumSamples)
    petConfigRefined$drivers[[1]]$details$DOEType <- input$petSamplingMethod
    
    selectedConfigurations <- strsplit(input$newCfgIDs, ",")
    if(length(unlist(selectedConfigurations)) > 1)
      selectedConfigurations <- unlist(selectedConfigurations)
    petConfigRefined$SelectedConfigurations <- selectedConfigurations
    
    
    write(toJSON(petConfigRefined, pretty = TRUE, auto_unbox = TRUE), file = file)
  }
  
  # UI Adjustments -----------------------------------------------------------
  
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

  updateColorSlider <- observeEvent(colSliderSettings(), {
    print("In updateColorSlider")
    if(input$colVarNum != ""){
      if(varClass[[toString(colSliderSettings()$variable)]] == "numeric") {
        updateSliderInput(session,
                          "colSlider",
                          step = colSliderSettings()$numericStep,
                          min = colSliderSettings()$numericMin,
                          max = colSliderSettings()$numericMax,
                          value = c(colSliderSettings()$lower, colSliderSettings()$upper))
      }
      else if(varClass[[toString(colSliderSettings()$variable)]] == "integer") {
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
      importFlags$ranges <- TRUE
      importFlags$bayesian <- TRUE
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
