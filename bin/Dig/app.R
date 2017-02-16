# Copyright (c) 2016-2017, MetaMorph Software
# 
# Permission is hereby granted, free of charge, to any person obtaining
# a copy of this software and associated documentation files (the
# "Software"), to deal in the Software without restriction, including
# without limitation the rights to use, copy, modify, merge, publish,
# distribute, sublicense, and/or sell copies of the Software, and to
# permit persons to whom the Software is furnished to do so, subject to
# the following conditions:
# 
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
# LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
# OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
# WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#
# Script: app.R
#
# Author: Timothy Thomas [aut, cre],
#   Will Knight [aut]
#
# Maintainer: Timothy Thomas <tthomas@metamorphsoftware.com>
#
# Description: OpenMETA Visualizer
#
#   This framework allows for the assembly of selected tabs into a single
#   Visualizer session. This allows for the tabs to interact and share
#   selection sets, classification variables, and filter settings. It
#   also manages the persistence of the design data generated during
#   exploration.
#
# URL: www.metamorphsoftware.com/openmeta

library(shiny)
library(shinyjs)
library(jsonlite)
library(topsis)

# Load selected tabs.
custom_tab_files <- list.files('tabs', pattern = "*.R")
custom_tab_environments <- lapply(custom_tab_files, function(file_name) {
  env <- new.env()
  source(file.path('tabs',file_name), local = env)
  env
})

# Setup test input.
if (Sys.getenv('DIG_INPUT_CSV') == "") {
  Sys.setenv(DIG_INPUT_CSV=file.path('datasets',
                                     'WindTurbineForOptimization',
                                     'mergedPET.csv'))
  # Sys.setenv(DIG_INPUT_CSV=file.path('datasets',
  #                                    'WindTurbine',
  #                                    'mergedPET.csv'))
}

# Server ---------------------------------------------------------------------

Server <- function(input, output, session) {
  # Handles the processing of all UI interactions.
  #
  # Args:
  #   input: the Shiny list of all the UI input elements.
  #   output: the Shiny list of all the UI output elements.
  #   session: a handle for the Shiny session.

  # Dispose of this server when the UI is closed
  session$onSessionEnded(function() {
    stopApp()
  })
  
  # Read input files. 
  raw <- read.csv(Sys.getenv('DIG_INPUT_CSV'), fill=T)
  
  pet_config_present <- FALSE
  pet_config_file_name <- gsub("mergedPET.csv",
                              "pet_config.json",
                              Sys.getenv('DIG_INPUT_CSV'))
  if(file.exists(pet_config_file_name)){
    pet_config <- fromJSON(pet_config_file_name)
    pet_config_present <- TRUE
  } 
  
  # Process PET configuration tile ('pet_config_json').
  design_variable_names <- NULL
  numeric_design_variables <- FALSE
  enumerated_design_variables <- FALSE
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
  
  pet <- list()
  if(pet_config_present) {
    pet$dvs <- pet_config$drivers[[1]]$designVariables
    pet$design_variable_names <- names(pet$dvs)
    pet$numeric_design_variables <- lapply(pet$dvs,
                                       function(x) {
                                         "RangeMax" %in% names(x)
                                       })
    pet$enumerated_design_variables <- lapply(pet$dvs,
                                         function(x) {
                                           "type" %in% names(x)
                                         })
    pet$dv_types <- unlist(lapply(pet$numeric_design_variables, 
                              function(x) { 
                                if (x)
                                  "Numeric"
                                else
                                  "Enumeration"
                              }))
    pet$dv_selections <- unlist(lapply(pet$dvs, 
                                   function(x) {
                                     if("type" %in% names(x)
                                        && x$type == "enum") 
                                       paste0(unlist(x$items), collapse=",") 
                                     else 
                                       paste0(c(x$RangeMin, x$RangeMax),
                                              collapse=",")
                                   }))
    pet$design_variables <- data.frame(var_name=pet$design_variable_names, Type=pet$dv_types, Selection=pet$dv_selections)
    pet$objective_names <- names(pet_config$drivers[[1]]$objectives)
    pet$num_samples <- unlist(strsplit(as.character(pet_config$drivers[[1]]$details$Code),'='))[2]
    pet$sampling_method <- pet_config$drivers[[1]]$details$DOEType
    pet$generated_configuration_model <- pet_config$GeneratedConfigurationModel
    pet$selected_configurations <- pet_config$SelectedConfigurations
    pet$name <- pet_config$PETName
    pet$mga_name <- pet_config$MgaFilename
    
    # Generate units tables.
    units <- list()
    reverse_units <- list()
    # TODO(tthomas): Clean up the construction of the units list.
    for (i in 1:length(pet$design_variable_names))
    {
      unit <-pet_config$drivers[[1]]$designVariables[[pet$design_variable_names[i]]]$units
      if(is.null(unit)) {
        unit <- ""
        name_with_units <- pet$design_variable_names[[i]]
      }
      else
      {
        unit <- gsub("\\*\\*", "^", unit) #replace Python '**' with '^'
        unit <- gsub("inch", "in", unit)  #replace 'inch' with 'in' since 'in' is a Python reserved word
        unit <- gsub("yard", "yd", unit)  #replace 'yard' with 'yd' since 'yd' is an OpenMDAO reserved word
        name_with_units <- paste0(pet$design_variable_names[i]," (",unit,")")
      }
      units[[pet$design_variable_names[[i]]]] <- list("unit"=unit, "name_with_units"=name_with_units)
      reverse_units[[name_with_units]] <- pet$design_variable_names[[i]]
    }
    for (i in 1:length(pet$objective_names))
    {
      unit <- pet_config$drivers[[1]]$objectives[[pet$objective_names[i]]]$units
      if(is.null(unit)) {
        unit <- ""
        name_with_units <- pet$objective_names[[i]]
      }
      else
      {
        unit <- gsub("\\*\\*", "^", unit)
        unit <- gsub("inch", "in", unit)  #replace 'inch' with 'in' since 'in' is a Python reserved word
        unit <- gsub("yard", "yd", unit)  #replace 'yard' with 'yd' since 'yd' is an OpenMDAO reserved word
        name_with_units <- paste0(objective_names[i]," (",unit,")")
      }
      units[[pet$objective_names[[i]]]] <- list("unit"=unit, "name_with_units"=name_with_units)
      reverse_units[[name_with_units]] <- pet$objective_names[[i]]
    }
  }
  
  # COMMENT(tthomas): Why do we need this?
  # output$numeric_design_variables <- reactive({
  #   TRUE %in% numeric_design_variables
  # })
  # 
  # output$enumerationdesign_variables <- reactive({
  #   TRUE %in% enumerated_design_variables
  # })
  # 
  # outputOptions(output, "numeric_design_variables", suspendWhenHidden=FALSE)
  # outputOptions(output, "enumerated_design_variables", suspendWhenHidden=FALSE)
  
  # Build the 'data' list that is shared between all tabs.
  data <- list()
  data$raw <- raw
  
  # Build variables metadata list.
  variables <- lapply(names(raw), function(var_name) {
    if (var_name %in% pet$design_variable_names)
      type <- "Design Variable"
    else
      type <- "Objective"
    list(name = var_name,
         name_with_units = AddUnits(var_name),
         type = type
    )
  })
  names(variables) <- names(raw)
  
  # Build the 'meta' list.
  data$meta <- list(variables = variables, pet = pet)
  
  # Call individual tabs' Server() functions.
  lapply(custom_tab_environments, function(customEnv) {
    do.call(customEnv$server,
            list(input, output, session, data))
  })
}

# UI -------------------------------------------------------------------------

# Setup UI with requested tabs.
base_tabs <- NULL

print("Tabs:")
added_tabs <- lapply(custom_tab_environments, function(custom_env) {
  print(custom_env$title)
  tabPanel(custom_env$title, custom_env$ui())
})

tabset_arguments <- c(unname(base_tabs),
                      unname(added_tabs),
                      id = "inTabset")

# Defines the UI of the Visualizer.
ui <- fluidPage(
  titlePanel("Visualizer"),
  
  # Generates the master tabset from the user-defined tabs provided.
  do.call(tabsetPanel, tabset_arguments)
)

# Start the Shiny app.
shinyApp(ui = ui, server = Server)