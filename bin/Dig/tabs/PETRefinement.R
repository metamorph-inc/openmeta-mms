### This file does not currently match the new formatting standard

title <- "PET Refinement"

ui <- function() {
  fluidPage(

    wellPanel(
      h4("Driver Configuration"),
      fluidRow(
        column(6, h5(strong('Original Driver Settings: ')),
                  textOutput("originalDriverSettings"))
      ),
      br(),
      uiOutput("petDriverConfig"), 
      hr(),
      fluidRow(
        column(12,
          h4("Design Configurations"),
          h5(strong("Generated Configuration Model: ")),
          textOutput("generatedConfigurationModelText"),
          fluidRow(
            column(2),
            column(1, h5(strong('Original'))),
            column(2, h5(strong("Selection:"))),
            column(1, h5(strong('Refined'))),
            column(2, h5(strong("Selection:"))),
            column(4, h5(strong("New Selection:")))
            ), br(),
          uiOutput("original_configuration_ranges")
          )
      ), 
      hr(),
      conditionalPanel(
        condition = "output.numericDesignVariables == true",
        fluidRow(
          column(12,
            h4("Numeric Ranges"),
            fluidRow(
              column(2, h5(strong("Variable Name:"))),
              column(1, actionButton('applyAllOriginalNumeric', 'Original')),
              column(1, h5(strong("Minimum:"))),
              column(1, h5(strong("Maximum:"))),
              column(1, actionButton('applyAllRefinedNumeric', 'Refined')),
              column(1, h5(strong("Minimum:"))),
              column(1, h5(strong("Maximum:"))),
              column(2, h5(strong("New Minimum:"))),
              column(2, h5(strong("New Maximum:")))
            ), br(),
            uiOutput("original_numeric_ranges")
          )
        )
      ),
      conditionalPanel(
        condition = "output.enumerationDesignVariables == true",
        fluidRow(
          column(12,
            h4("Enumerated Ranges"),
            fluidRow(
              column(2, h5(strong("Variable Name:"))),
              column(1, actionButton('applyAllOriginalEnum', 'Original')),
              column(2, h5(strong("Selection:"))),
              column(1, actionButton('applyAllRefinedEnum', 'Refined')),
              column(2, h5(strong("Selection:"))),
              column(4, h5(strong("New Selection:")))
            ), 
            br(),
            uiOutput("original_enumeration_ranges")
          )
        )
      ),
      hr(),
      fluidRow(
        column(6,
          h4("PET Details"),
          uiOutput("petRename"),
          actionButton('runRanges', 'Execute New PET'), br()
        )
      )
    )

  )
}

enumerationMaxDisplay <- 3

server <- function(input, output, session, data) {
  raw_data <- data$raw
  raw_info <- data$meta
  pet  <- raw_info$pet

  FilterData <- function(...) {raw_data}
  
  varNames <- unlist(names(data$meta$variables))
  varClass <- sapply(data$raw,class)
  varNum <- varNames[varClass != "factor"]
  varFac <- varNames[varClass == "factor"]
  designVariables <- gsub(".type", "", 
                          names(unlist(data$meta$variables))[
                          unlist(data$meta$variables) == "Design Variable"])
  
  all_ranges <- list() #List of all ranges: 1 for all numerics and individual ones for each factor
  
  output$petDriverConfig <- renderUI({
    fluidRow(
      column(3, selectInput("petSamplingMethod", "New Sampling Method:", choices = c("Full Factorial", "Uniform", "Central Composite", "Opt Latin Hypercube"), selected = pet$sampling_method)),
      column(3, textInput("petNumSamples", "New Number of Samples:", value = pet$num_samples))
    )
  })
  
  output$petRename <- renderUI({
    fluidRow(
      column(12, h5(strong("Original MGA Filename: ")), textOutput("mgaFilenameText")),
      column(12, h5(strong("Original PET Name: ")), textOutput("currentPetNameText"), br()),
      column(12, textInput("newPetName", "New PET Name:", value = paste0(pet$name, "_Refined")))
    )
  })
  
  output$originalDriverSettings <- renderText(paste(pet$sampling_method," sampling with 'num_samples=", pet$num_samples,"' yielded ", nrow(raw_data), " points.", sep = ""))
  output$mgaFilenameText <- renderText(pet$mga_name)
  output$generatedConfigurationModelText <- renderText(pet$generated_configuration_model)
  output$currentPetNameText <- renderText(pet$name)
  
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
        
        
        refined <- FilterData()[var]
        if(dim(refined)[1] == 0){
          min_refined <- "No data available in table"
          max_refined <- "No data available in table"
        }
        else{
          min_refined <- sprintf("%.3e", min(FilterData()[var]))
          max_refined <- sprintf("%.3e", max(FilterData()[var]))
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
        if(length(unlist(strsplit(original, ","))) > enumerationMaxDisplay)
          original = paste0("List of ", length(unlist(strsplit(original, ","))), " Enumerations.")
        
        refined <- toString(unique(FilterData()[var])[,1])
        if(length(unlist(strsplit(refined, ","))) > enumerationMaxDisplay)
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
    
    original <- paste0(pet$selected_configurations, collapse=",")
    originalCount <- length(pet$selected_configurations)
    if(originalCount > enumerationMaxDisplay)
      original = paste0("List of ", originalCount, " Configurations.")
    
    refined <- paste0(unique(FilterData()$CfgID), collapse=",")
    refinedCount <- length(unique(FilterData()$CfgID))
    if(refinedCount > enumerationMaxDisplay)
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
    original <- paste0(pet$selected_configurations, collapse=",")
    updateTextInput(session, 'newCfgIDs', value = original)
  })
  
  observeEvent(input$applyRefinedCfgIDs, {
    refined <- paste0(unique(FilterData()$CfgID), collapse=",")
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
  
  # Pull values from FilterData
  
  reactToApplyRefinedButtons <- observe({
    lapply(varNum, function(var) {
      global_index = which(varNames == var)
      observeEvent(input[[paste0('applyRefinedRange', global_index)]], {
        updateTextInput(session, paste0('newMin', global_index), value = min(FilterData()[var]))
        updateTextInput(session, paste0('newMax', global_index), value = max(FilterData()[var]))
      })
    })
    lapply(varFac, function(var) {
      global_index = which(varNames == var)
      observeEvent(input[[paste0('applyRefinedSelection', global_index)]], {
        updateTextInput(session, paste0('newSelection', global_index), value = toString(unique(FilterData()[var])[,1]))
      })
    })
  })
  
  observeEvent(input$applyAllRefinedNumeric, {
    lapply(varNum, function(var) {
      global_index = which(varNames == var)
      updateTextInput(session, paste0('newMin', global_index), value = min(FilterData()[var]))
      updateTextInput(session, paste0('newMax', global_index), value = max(FilterData()[var]))
    })
  })
  
  observeEvent(input$applyAllRefinedEnum, {
    lapply(varFac, function(var) {
      global_index = which(varNames == var)
      updateTextInput(session, paste0('newSelection', global_index), value = toString(unique(FilterData()[var])[,1]))
    })
  })
  
  slowFactorRangeData <- eventReactive(input$updateRanges, {
    printFactorStatistics()
  })
  
  printFactorStatistics <- function(...){
    lapply(varRangeFac(), function(var) {
      all_ranges[[var]] <<- do.call(rbind, lapply(FilterData()[var], summary))
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
    petConfigRefined <- pet
    
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
    
    pet$selected_configurations <- strsplit(input$newCfgIDs, ",")
    if(length(unlist(pet$selected_configurations)) > 1)
      pet$selected_configurations <- unlist(pet$selected_configurations)
    petConfigRefined$selected_configurations <- pet$selected_configurations
    
    
    write(toJSON(petConfigRefined, pretty = TRUE, auto_unbox = TRUE), file = file)
  }
  
}