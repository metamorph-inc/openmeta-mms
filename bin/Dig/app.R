library(shiny)
library(shinyjs)
library(jsonlite)

### BASE VISUALIZER ###

## Access Individual Tabs ##

custom_tab_files <- list.files('tabs', pattern = "*.R")

custom_tab_environments <- lapply(custom_tab_files, function(file_name) {
  env <- new.env()
  source(file.path('tabs',file_name), local = env)
  env
})

##

## Setup Test Input ##

if (Sys.getenv('DIG_INPUT_CSV') == "") {
  Sys.setenv(DIG_INPUT_CSV=file_path('datasets','WindTurbineForOptimization','mergedPET.csv'))
  # Sys.setenv(DIG_INPUT_CSV=file_path('datasets','WindTurbine','mergedPET.csv'))
}

##

server <- function(input, output, session) {

  session$onSessionEnded(function() {
    stopApp()
  })
  
# Read Input Files
  
  raw = read.csv(Sys.getenv('DIG_INPUT_CSV'), fill=T)
  
  pet_config_present <- FALSE
  pet_config_file_name = gsub("mergedPET.csv", "pet_config.json", Sys.getenv('DIG_INPUT_CSV'))
  if(file.exists(pet_config_file_name)){
    pet_config <- fromJSON(pet_config_file_name)
    pet_config_present <- TRUE
  } 
  
  
# Process PET Configuration File ('pet_config_json')
  
  design_variable_names <- NULL
  numeric_design_variables <- FALSE
  enumerate_design_variables <- FALSE
  design_variables <- NULL
  objective_names <- NULL
  units <- NULL
  reverse_units <- NULL
  
  AddUnits <- function(name) {
    if(is.null(units) | !(name %in% names(units)))
      name
    else
      units[[name]]$name_with_units
  }
  
  RemoveUnits <- function(name_with_units) {
    if(is.null(reverse_units) | !(name_with_units %in% names(reverse_units)))
      name_with_units
    else
      reverse_units[[name_with_units]]
  }
  
  if(pet_config_present) {
    design_variable_names <- names(pet_config$drivers[[1]]$designVariables)
    print(design_variable_names)
    numeric_design_variables <- lapply(pet_config$drivers[[1]]$designVariables, 
                                       function(x) {
                                         "RangeMax" %in% names(x)
                                       })
    enumerate_design_variables <- lapply(pet_config$drivers[[1]]$designVariables, 
                                         function(x) {
                                           "type" %in% names(x)
                                         })
    dv_types <- unlist(lapply(numeric_design_variables, 
                              function(x) { 
                                if (x) "Numeric" else "Enumeration"
                              })
                       )
    dv_selections <- unlist(lapply(pet_config$drivers[[1]]$designVariables, 
                                   function(x) {
                                     if("type" %in% names(x) && x$type == "enum") 
                                       paste0(unlist(x$items), collapse=",") 
                                     else 
                                       paste0(c(x$RangeMin, x$RangeMax), collapse=",")
                                   })
                            )
    design_variables <- data.frame(var_name=design_variable_names, Type=dv_types, Selection=dv_selections)
    objective_names <- names(pet_config$drivers[[1]]$objectives)
    pet_config_num_samples <- unlist(strsplit(as.character(pet_config$drivers[[1]]$details$Code),'='))[2]
    pet_config_sampling_method <- pet_config$drivers[[1]]$details$DOEType
    pet_generated_configuration_model <- pet_config$GeneratedConfigurationModel
    pet_selected_configurations <- pet_config$SelectedConfigurations
    pet_name <- pet_config$pet_name
    pet_mga_name <- pet_config$mga_file_name
    
    # Generate Units Tables
    units <- list()
    reverse_units <- list()
    print(design_variable_names)
    for (i in 1:length(design_variable_names))
    {
      unit <-pet_config$driver[[1]]$design_variables[[design_variable_names[i]]]$units
      if(is.null(unit)) {
        unit <- ""
        name_with_units <- design_variable_names[[i]]
      }
      else
      {
        unit <- gsub("\\*\\*", "^", unit) #replace Python '**' with '^'
        unit <- gsub("inch", "in", unit)  #replace 'inch' with 'in' since 'in' is a Python reserved word
        unit <- gsub("yard", "yd", unit)  #replace 'yard' with 'yd' since 'yd' is an OpenMDAO reserved word
        name_with_units <- paste0(design_variable_names[i]," (",unit,")")
      }
      units[[design_variable_names[[i]]]] <- list("unit"=unit, "name_with_units"=name_with_units)
      reverse_units[[name_with_units]] <- design_variable_names[[i]]
    }
    for (i in 1:length(objective_names))
    {
      unit <-pet_config$driver[[1]]$objectives[[objective_names[i]]]$units
      if(is.null(unit)) {
        unit <- ""
        name_with_units <- objective_names[[i]]
      }
      else
      {
        unit <- gsub("\\*\\*", "^", unit)
        name_with_units <- paste0(objective_names[i]," (",unit,")")
      }
      units[[objective_names[[i]]]] <- list("unit"=unit, "name_with_units"=name_with_units)
      reverse_units[[name_with_units]] <- objective_names[[i]]
    }
  }
  
  output$pet_config_present <- reactive({
    print(paste("pet_config_present:",pet_config_present))
    pet_config_present
  })
  
  output$numeric_design_variables <- reactive({
    TRUE %in% numeric_design_variables
  })
  
  output$enumerationdesign_variables <- reactive({
    TRUE %in% enumerate_design_variables
  })
  
  outputOptions(output, "pet_config_present", suspendWhenHidden=FALSE)
  outputOptions(output, "numeric_design_variables", suspendWhenHidden=FALSE)
  outputOptions(output, "enumerationdesign_variables", suspendWhenHidden=FALSE)
  
  output$no_pet_config_message <- renderText(paste("No pet_config_json file was found_"))
  
  # Build info data frame
  variables <- lapply(names(raw), function(var_name) {
    list(name = var_name,
         name_with_units = AddUnits(var_name),
         type = if (var_name %in% design_variable_names) "Design Variable" else "Objective"
    )
  })
  names(variables) <- names(raw)
  
  info <- list(variables = variables)
  
# Call Individual Tabs' Server Function
  
  lapply(custom_tab_environments, function(customEnv) {
    do.call(customEnv$server,
            list(input, output, session, raw, info))
  })

}

## Setup UI for Additional Tabs ##

added_tabs <- lapply(custom_tab_environments, function(custom_env) {
  # tabUI <- do.call(UI, list(), envir = customEnv)
  print(custom_env$title)
  tabPanel(custom_env$title, custom_env$ui())
})

base_tabs <- NULL

tabsetArguments <- c(unname(base_tabs),
                     unname(added_tabs),
                     id = "inTabset")

##

ui <- fluidPage(
  #  Application title
  titlePanel("Visualizer"),
  do.call(tabsetPanel, tabsetArguments)
)


shinyApp(ui = ui, server = server)