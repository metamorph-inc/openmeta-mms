title <- "PET Refinement"
footer <- TRUE

ui <- function(id) {
  ns <- NS(id)
  
  fluidPage(
    br(),
    conditionalPanel(condition = paste0("output['", ns("pet_config_present"), "'] == true"),
      wellPanel(
        h4("Driver Configuration"),
        fluidRow(
          column(6, h5(strong('Original Driver Settings: ')),
                    textOutput(ns("original_driver_settings")))
        ),
        br(),
        uiOutput(ns("pet_driver_config")), 
        hr(),
        fluidRow(
          column(12,
            h4("Design Configurations"),
            h5(strong("Generated Configuration Model: ")),
            textOutput(ns("generated_configuration_model_text")),
            fluidRow(
              column(2),
              column(1, h5(strong('Original'))),
              column(2, h5(strong("Selection:"))),
              column(1, h5(strong('Refined'))),
              column(2, h5(strong("Selection:"))),
              column(4, h5(strong("New Selection:")))
              ), br(),
            uiOutput(ns("original_configuration_ranges"))
            )
        ), 
        hr(),
        conditionalPanel(
          condition = paste0("output['", ns("numeric_design_variables_present"), "'] == true"),
          fluidRow(
            column(12,
              h4("Numeric Ranges"),
              fluidRow(
                column(2, h5(strong("Variable Name:"))),
                column(1, actionButton(ns('apply_all_original_numeric'), 'Original')),
                column(1, h5(strong("Minimum:"))),
                column(1, h5(strong("Maximum:"))),
                column(1, actionButton(ns('apply_all_refined_numeric'), 'Refined')),
                column(1, h5(strong("Minimum:"))),
                column(1, h5(strong("Maximum:"))),
                column(2, h5(strong("New Minimum:"))),
                column(2, h5(strong("New Maximum:")))
              ), br(),
              uiOutput(ns("original_numeric_ranges"))
            )
          )
        ),
        conditionalPanel(
          condition = paste0("output['", ns("enumerated_design_variables_present"), "'] == true"),
          fluidRow(
            column(12,
              h4("Enumerated Ranges"),
              fluidRow(
                column(2, h5(strong("Variable Name:"))),
                column(1, actionButton(ns('apply_all_original_enum'), 'Original')),
                column(2, h5(strong("Selection:"))),
                column(1, actionButton(ns('apply_all_refined_enum'), 'Refined')),
                column(2, h5(strong("Selection:"))),
                column(4, h5(strong("New Selection:")))
              ), 
              br(),
              uiOutput(ns("original_enumeration_ranges"))
            )
          )
        ),
        hr(),
        fluidRow(
          column(6,
            h4("PET Details"),
            uiOutput(ns("pet_rename")),
            actionButton(ns('run_ranges'), 'Execute New PET'), br()
          )
        )
      )
    )
  )
}

max_enums_display <- 3

server <- function(input, output, session, data) {
  
  ns <- session$ns

  FilterData <- data$Filtered
  var_nums <- isolate(data$pre$var_nums())
  var_facs <- isolate(data$pre$var_facs())
  
  pet <- isolate(data$meta$pet)
  numeric_dvs <- unlist(lapply(pet$design_variable_names,
                               function (var) {var %in% var_nums}))
  numeric_design_variables <- pet$design_variable_names[numeric_dvs]
  enumerated_dvs <- unlist(lapply(pet$design_variable_names,
                                  function (var) {var %in% var_facs}))
  enumerated_design_variables <- pet$design_variable_names[enumerated_dvs]
  
  design_variables <- pet$design_variables
  
  output$pet_config_present <- reactive({
    !is.null(pet$pet_name)
  })
  
  output$numeric_design_variables_present <- reactive({
    length(numeric_design_variables) > 0
  })
  
  output$enumerated_design_variables_present <- reactive({
    length(enumerated_design_variables) > 0
  })
  
  outputOptions(output, "pet_config_present", suspendWhenHidden=FALSE)
  outputOptions(output, "numeric_design_variables_present", suspendWhenHidden=FALSE)
  outputOptions(output, "enumerated_design_variables_present", suspendWhenHidden=FALSE)
  
  output$pet_driver_config <- renderUI({
    fluidRow(
      column(3, selectInput(ns("pet_sampling_method"),
                            "New Sampling Method:",
                            choices = c("Full Factorial",
                                        "Uniform",
                                        "Central Composite",
                                        "Opt Latin Hypercube"),
                            selected = si(ns("pet_sampling_method"), pet$sampling_method))),
      column(3, textInput(ns("pet_num_samples"),
                          "New Number of Samples:",
                          value = si(ns("pet_num_samples"), pet$num_samples)))
    )
  })
  
  output$pet_rename <- renderUI({
    fluidRow(
      column(12, h5(strong("Original MGA Filename: ")), textOutput(ns("mga_filename_text"))),
      column(12, h5(strong("Original PET Name: ")), textOutput(ns("current_pet_name_text")), br()),
      column(12, textInput(ns("newPetName"),
                           "New PET Name:",
                           value = si(ns("newPetName"), paste0(pet$pet_name, "_Refined"))))
    )
  })
  
  output$original_driver_settings <- renderText(paste(pet$sampling_method," sampling with 'num_samples=", pet$num_samples,"' yielded ", nrow(data$raw$df), " points.", sep = ""))
  output$generated_configuration_model_text <- renderText(pet$generated_configuration_model)
  output$mga_filename_text <- renderText(pet$mga_name)
  output$current_pet_name_text <- renderText(pet$pet_name)
  
  output$original_configuration_ranges <- renderUI({
    
    original <- toString(pet$selected_configurations)
    original_count <- length(pet$selected_configurations)
    if(original_count > max_enums_display)
      original = paste0("List of ", original_count, " Configurations.")
    
    refined <- toString(unique(FilterData()$CfgID))
    refined_count <- length(unique(FilterData()$CfgID))
    if(refined_count > max_enums_display)
      refined = paste0("List of ", refined_count, " Configurations.")
    if(refined == "")
      refined = "No configurations available."
    isolate({
      selected <- input$new_cfg_ids
    })
    
    fluidRow(
      column(2, h5(strong("Configuration Name(s)"))),
      column(1, actionButton(ns('apply_original_cfg__ids'), 'Apply')),
      column(2, h5(original)),
      column(1, actionButton(ns('apply_refined_cfg__ids'), 'Apply')),
      column(2, h5(refined)),
      column(4,
             textInput(ns('new_cfg_ids'),
                       NULL,
                       placeholder = "Enter selection",
                       value = si(ns('new_cfg_ids'), selected))
      )
    )
  })
  
  observeEvent(input$apply_original_cfg__ids, {
    original <- toString(pet$selected_configurations)
    updateTextInput(session, 'new_cfg_ids', value = original)
  })
  
  observeEvent(input$apply_refined_cfg__ids, {
    refined <- toString(unique(FilterData()$CfgID))
    updateTextInput(session, 'new_cfg_ids', value = refined)
  })
  
  AbbreviateNumber <- function(value) {
    value <- as.character(value)
    if (nchar(value) > 8)
      value <- sprintf("%.3e", as.numeric(value))
    value
  }
  
  output$original_numeric_ranges <- renderUI({
    lapply(design_variables, function(var){
      if(var$type == "Numeric") {
        selection <- unlist(strsplit(var$selection, "\\,"))
        original_min <- AbbreviateNumber(selection[1])
        original_max <- AbbreviateNumber(selection[2])
        refined_min <- AbbreviateNumber(min(FilterData()[var$name]))
        refined_max <-AbbreviateNumber(max(FilterData()[var$name]))
        isolate({
          selected_min <- input[[paste0('new_min_', var$name)]]
          selected_max <- input[[paste0('new_max_', var$name)]]
        })
        fluidRow(
          column(2, h5(strong(var$name))),
          column(1, actionButton(ns(paste0('apply_original_range_', var$name)), 'Apply')),
          column(1, h5(original_min)),
          column(1, h5(original_max)),
          column(1, actionButton(ns(paste0('apply_refined_range_', var$name)), 'Apply')),
          column(1, h5(refined_min)),
          column(1, h5(refined_max)),
          column(2,
                 textInput(ns(paste0('new_min_', var$name)),
                           NULL,
                           placeholder = "Enter min",
                           value = si(ns(paste0('new_min_', var$name)), selected_min))
          ),
          column(2,
                 textInput(ns(paste0('new_max_', var$name)),
                           NULL,
                           placeholder = "Enter max",
                           value = si(ns(paste0('new_max_', var$name)), selected_max))
          ),
          hr()
        )
      }
    })
  })
  
  observeEvent(input$apply_all_original_numeric, {
    lapply(design_variables, function(var){
      if(var$type == "Numeric") {
        original <- unlist(strsplit(var$selection, "\\,"))
        updateTextInput(session, paste0('new_min_', var$name), value = original[1])
        updateTextInput(session, paste0('new_max_', var$name), value = original[2])
      }
    })
  })
  
  observeEvent(input$apply_all_refined_numeric, {
    lapply(design_variables, function(var){
      if(var$type == "Numeric") {
        updateTextInput(session, paste0('new_min_', var$name),
                        value = min(FilterData()[var$name]))
        updateTextInput(session, paste0('new_max_', var$name),
                        value = max(FilterData()[var$name]))
      }
    })
  })
  
  output$original_enumeration_ranges <- renderUI({
    lapply(design_variables, function(var){
      if(var$type == "Enumeration"){
        
        original <- toString(unlist(strsplit(var$selection, ",")))
        original_count <- length(unlist(strsplit(original, ",")))
        if(original_count > max_enums_display)
          original = paste0("List of ", original_count, " Enumerations.")
        
        refined <- toString(unique(FilterData()[[var$name]]))
        refined_count <- length(unlist(strsplit(refined, ",")))
        if(refined_count > max_enums_display)
          refined = paste0("List of ", refined_count, " Enumerations.")
        
        isolate({
          selected <- input[[paste0('new_selection_', var$name)]]
        })
        
        fluidRow(
          column(2, h5(strong(var$name))),
          column(1, actionButton(ns(paste0('apply_original_selection_', var$name)), 'Apply')),
          column(2, h5(original)),
          column(1, actionButton(ns(paste0('apply_refined_selection_', var$name)), 'Apply')),
          column(2, h5(refined)),
          column(4,
                 textInput(ns(paste0('new_selection_', var$name)),
                           NULL,
                           placeholder = "Enter selection",
                           value = si(ns(paste0('new_selection_', var$name)), selected))
          ),
          hr()
        )
      }
    })
  })
  
  observeEvent(input$apply_all_original_enum, {
    lapply(design_variables, function(var){
      if(var$type == "Enumeration"){
        original <- toString(unlist(strsplit(var$selection, ",")))
        updateTextInput(session, paste0('new_selection_', var$name),
                          value=original)
      }
    })
  })

  observeEvent(input$apply_all_refined_enum, {
    lapply(design_variables, function(var){
      if(var$type == "Enumeration"){
        refined <- toString(unique(FilterData()[[var$name]]))
        updateTextInput(session, paste0('new_selection_', var$name),
                        value=refined)
      }
    })
  })
  
  ReactToSingleApplyButtons <- observe({
    lapply(design_variables, function(var) {
      if(var$type == "Numeric"){
        original <- unlist(strsplit(var$selection, "\\,"))
        observeEvent(input[[paste0('apply_original_range_', var$name)]], {
          updateTextInput(session, paste0('new_min_', var$name), value = original[1])
          updateTextInput(session, paste0('new_max_', var$name), value = original[2])
        })
        observeEvent(input[[paste0('apply_refined_range_', var$name)]], {
          updateTextInput(session, paste0('new_min_', var$name),
                          value = min(FilterData()[var$name]))
          updateTextInput(session, paste0('new_max_', var$name),
                          value = max(FilterData()[var$name]))
        })
      }
      else if (var$type == "Enumeration"){
        observeEvent(input[[paste0('apply_original_selection_', var$name)]], {
          original <- toString(unlist(strsplit(var$selection, ",")))
          updateTextInput(session, paste0('new_selection_', var$name),
                          value=original)
        })
        observeEvent(input[[paste0('apply_refined_selection_', var$name)]], {
          refined <- toString(unique(FilterData()[[var$name]]))
          updateTextInput(session, paste0('new_selection_', var$name),
                          value=refined)
        })
      }
    })
  })
  
  observeEvent(input$run_ranges, {
    if (!is.null(pet$pet_config_filename)) {
      results_directory <- dirname(pet_config_filename)
      project_directory <- dirname(results_directory)
      pet_refined_filename <- file.path(results_directory, "pet_config_refined.json")
      ExportRangesFunction(pet_refined_filename)
      system2("..\\Python27\\Scripts\\python.exe",
              args = c("..\\UpdatePETParameters.py",
                       "--pet-config",
                       paste0("\"",pet_refined_filename,"\""),
                       "--new-name",
                       paste0("\"",input$newPetName,"\"")),
              stdout = file.path(results_directory, "UpdatePETParameters_stdout.log"),
              stderr = file.path(results_directory, "UpdatePETParameters_stderr.log"),
              wait = FALSE)
    }
  })
  
  output$downloadRanges <- downloadHandler(
    filename = function() {paste('pet_config_refined_', Sys.Date(), '.json', sep='')},
    content = ExportRangesFunction
  )
  
  ExportRangesFunction <- function(file) { 
    pet_config_refined <- pet$pet_config
    
    ReassignDV <- function(dv, name) {
      if("type" %in% names(dv) && dv$type == "enum") {
        dv$items <- unlist(lapply(strsplit(input[[paste0('new_selection_', name)]], ","),
                                  trimws))
        # To retain array type in .json file
        if (length(dv$items) == 1) {
          dv$items <- list(dv$items)
        }
      } else {
        dv$RangeMin <- as.numeric(input[[paste0('new_min_', name)]])
        dv$RangeMax <- as.numeric(input[[paste0('new_max_', name)]])
      }
      dv
    }

    new_dvs <- Map(ReassignDV,
                   pet_config_refined$drivers[[1]]$designVariables,
                   names(pet_config_refined$drivers[[1]]$designVariables))
    pet_config_refined$drivers[[1]]$designVariables <- new_dvs
    pet_config_refined$drivers[[1]]$details$Code <- paste0("num_samples=", input$pet_num_samples)
    pet_config_refined$drivers[[1]]$details$DOEType <- input$pet_sampling_method
    
    selected_configurations <- unlist(lapply(strsplit(input$new_cfg_ids, ","), trimws))
    # To retain array type in .json file
    if(length(selected_configurations) == 1)
      selected_configurations <- list(selected_configurations)
    pet_config_refined$SelectedConfigurations <- selected_configurations
    
    write(toJSON(pet_config_refined, pretty = TRUE, auto_unbox = TRUE), file = file)
  }
}
