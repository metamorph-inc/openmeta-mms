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
#         Will Knight [aut]
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
library(shinyBS)
library(jsonlite)
library(topsis)

# Defined Constants ----------------------------------------------------------

abbreviate_length <- 25

# Custom Functions -----------------------------------------------------------

RemoveItemNumber <- function(factor) {sub("[0-9]+. ", "", factor)}

units <- NULL
reverse_units <- NULL

AddUnits <- function(name) {
  if(is.null(units) || !(name %in% names(units)))
    name
  else
    units[[name]]$name_with_units
}

RemoveUnits <- function(name_with_units) {
  if(is.null(reverse_units) || !(name_with_units %in% names(reverse_units)))
    name_with_units
  else
    reverse_units[[name_with_units]]
}

# Resolve Dataset Configuration ----------------------------------------------

pet_config_present <- FALSE
if (Sys.getenv('DIG_INPUT_CSV') == "") {
  # Visualizer 2.0 style input dataset
  if (Sys.getenv('DIG_DATASET_CONFIG') == "") {
    # Setup one of the test datasets if no input dataset
    Sys.setenv(DIG_DATASET_CONFIG=file.path('datasets',
                                            'WindTurbineForOptimization',
                                            'visualizer_config.json'))
    # Sys.setenv(DIG_DATASET_FOLDER=file.path('datasets',
    #                                         'WindTurbine',
    #                                         'visualizer_config.json'))
    # Sys.setenv(DIG_DATASET_FOLDER=file.path('datasets',
    #                                         'TestPETRefinement',
    #                                         'visualizer_config.json'))
  }
  config_filename <- gsub("\\\\", "/", Sys.getenv('DIG_DATASET_CONFIG'))
  visualizer_config <- fromJSON(config_filename)
  tab_requests <- visualizer_config$tabs
  launch_dir <- dirname(config_filename)
  raw_data_filename <- file.path(launch_dir, visualizer_config$raw_data)
  pet_config_value <- visualizer_config$pet_config
  if (!is.null(pet_config_value) && pet_config_value != "") {
    pet_config_filename <- file.path(launch_dir, pet_config_value)
    if (file.exists(pet_config_filename)) {
      pet_config_present <- TRUE
    }
  }
} else {
  # Visualizer legacy dataset format
  raw_data_filename <- Sys.getenv('DIG_INPUT_CSV')
  pet_config_filename <- gsub("mergedPET.csv",
                              "pet_config.json",
                              Sys.getenv('DIG_INPUT_CSV'))
  if (file.exists(pet_config_filename)) {
    pet_config_present <- TRUE
  }
  tab_requests <- c("Explore.R",
                    "DataTable.R",
                    "Histogram.R",
                    "PETRefinement.R",
                    "Scratch.R",
                    "ParallelAxisPlot.R",
                    "UncertaintyQuantification.R")
}

# Load Tabs and Data ---------------------------------------------------------

# Read input dataset file
raw <- read.csv(raw_data_filename, fill=T)

# Read tab files
tab_files <- list()
for (i in 1:length(tab_requests)) {
  request <- tab_requests[i]
  if (file.exists(file.path(launch_dir, request))) {
    tab_files <- c(tab_files, file.path(launch_dir, request))
  } else if (file.exists(file.path('tabs', request))) {
    tab_files <- c(tab_files, file.path('tabs', request))
  }
}

tab_environments <- lapply(tab_files, function(file_name) {
  env <- new.env()
  source(file_name, local = env)
  # debugSource(file_name, local = env)
  env
})

# Process PET Configuration File ('pet_config_json') -----------------------

pet <- NULL
variables <- NULL

if(pet_config_present) {
  pet_config <- fromJSON(pet_config_filename)
  dvs <- pet_config$drivers[[1]]$designVariables
  design_variable_names <- names(dvs)
  design_variables <- Map(function(item, name) {
    new_item <- list()
    new_item$name <- name
    if ("RangeMax" %in% names(item)) {
      new_item$type <- "Numeric"
    } else {
      new_item$type <- "Enumeration"
    }
    if("type" %in% names(item) && item$type == "enum") {
      new_item$selection <- paste0(unlist(item$items), collapse=",")
    } else {
      new_item$selection <- paste0(c(item$RangeMin, item$RangeMax),
                                   collapse=",")
    }
    new_item
  }, dvs, names(dvs))
  objective_names <- names(pet_config$drivers[[1]]$objectives)
  num_samples <- unlist(strsplit(as.character(pet_config$drivers[[1]]$details$Code),'='))[2]
  sampling_method <- pet_config$drivers[[1]]$details$DOEType
  generated_configuration_model <- pet_config$GeneratedConfigurationModel
  selected_configurations <- pet_config$SelectedConfigurations
  pet_name <- pet_config$PETName
  mga_name <- pet_config$MgaFilename
  
  pet <- list(sampling_method=sampling_method,
              num_samples=num_samples,
              pet_name=pet_name,
              mga_name=mga_name,
              generated_configuration_model=generated_configuration_model,
              selected_configurations=selected_configurations,
              design_variable_names=design_variable_names,
              design_variables=design_variables,
              pet_config=pet_config)
  
  # TODO(tthomas): Clean up the construction of the units list.
  # Generate units tables.
  units <- list()
  reverse_units <- list()
  for (i in 1:length(design_variable_names))
  {
    unit <- pet_config$drivers[[1]]$designVariables[[design_variable_names[i]]]$units
    if(is.null(unit) || unit == "") {
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
    unit <- pet_config$drivers[[1]]$objectives[[objective_names[i]]]$units
    if(is.null(unit)) {
      unit <- ""
      name_with_units <- objective_names[[i]]
    }
    else
    {
      unit <- gsub("\\*\\*", "^", unit)
      unit <- gsub("inch", "in", unit)  #replace 'inch' with 'in' since 'in' is a Python reserved word
      unit <- gsub("yard", "yd", unit)  #replace 'yard' with 'yd' since 'yd' is an OpenMDAO reserved word
      name_with_units <- paste0(objective_names[i]," (",unit,")")
    }
    units[[objective_names[[i]]]] <- list("unit"=unit, "name_with_units"=name_with_units)
    reverse_units[[name_with_units]] <- objective_names[[i]]
  }
  
  # Build variables metadata list.
  variables <- lapply(names(raw), function(var_name) {
    if (var_name %in% design_variable_names)
      type <- "Design Variable"
    else
      type <- "Objective"
    list(name = var_name,
         name_with_units = AddUnits(var_name),
         type = type
    )
  })
  names(variables) <- names(raw)
}

# Pre-Processing ----------------------------------------------------------

var_names <- names(raw)
var_class <- sapply(raw, class)
var_facs <- var_names[var_class == "factor"]
var_ints <- var_names[var_class == "integer"]
var_nums <- var_names[var_class == "numeric"]
var_nums_and_ints <- var_names[var_class == "integer" |
                               var_class == "numeric"]
abs_max <- apply(raw[var_nums_and_ints], 2, max, na.rm=TRUE)
abs_min <- apply(raw[var_nums_and_ints], 2, min, na.rm=TRUE)
var_range_nums_and_ints <- var_nums_and_ints[(abs_min != abs_max) &
                                             (abs_min != Inf)]
var_range_facs <- var_facs[apply(raw[var_facs], 2, function(var_fac) {
                                   length(names(table(var_fac))) > 1
                                 })]
var_range <- c(var_facs, var_nums_and_ints)
var_constants <- subset(var_names, !(var_names %in% var_range))

preprocessing <- list(var_names=var_names,
                      var_class=var_class,
                      var_facs=var_facs,
                      var_ints=var_ints,
                      var_nums=var_nums,
                      var_nums_and_ints=var_nums_and_ints,
                      abs_min=abs_min,
                      abs_max=abs_max,
                      AbsMax=NULL,
                      AbsMin=NULL,
                      var_range_nums_and_ints=var_range_nums_and_ints,
                      var_range_facs=var_range_facs,
                      var_range=var_range,
                      var_constants=var_constants)

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
  
  # Filters (Enumerations, Sliders) and Constants ----------------------------
  
  # Lets the UI know if a tab has requested the 'Filters' footer.  
  footer_preferences <- lapply(tab_environments,
                               function(tab_env) {tab_env$footer})
  names(footer_preferences) <- lapply(tab_environments,
                                      function(tab_env) {tab_env$title})
  output$display_footer <- reactive({
    display <- (footer_preferences[[input$master_tabset]])
  })
  outputOptions(output, "display_footer", suspendWhenHidden=FALSE)
  
  # Generates the sliders and select boxes.
  output$filters <- renderUI({
    var_selects <- var_range_facs
    var_sliders <- var_range_nums_and_ints
    
    div(
      fluidRow(
        lapply(var_selects, function(var_select) {
          GenerateEnumUI(var_select)
        })
      ),
      fluidRow(
        lapply(var_sliders, function(var_slider) {
          GenerateSliderUI(var_slider, AbbreviateLabel(var_slider))
        })
      )
    )
  })

  # Slider abbreviation function based off sliderWidth
  AbbreviateLabel <- function(name) {
    if(!is.null(input$sliderWidth)){
      abbreviate_length <<- input$sliderWidth/8
    }
    abbreviate(name, abbreviate_length)
  }
  
  # Process slider pixel width when opening filters
  observeEvent(input$footer_collapse, {
    session$onFlushed(function() {
      session$sendCustomMessage("update_widths", message = 1);
    })
  })
  
  GenerateEnumUI <- function(current) {
    items <- names(table(raw[[current]]))
    
    for(i in 1:length(items)){
      items[i] <- paste0(i, '. ', items[i])
    }
    
    selected_value <- input[[paste0('filter_', current)]]
    if(is.null(selected_value))
      selected_value <- items
    
    column(2, selectInput(inputId = paste0('filter_', current),
                          label = current,
                          multiple = TRUE,
                          selectize = FALSE,
                          choices = items,
                          selected = selected_value)
    )
  }
  
  GenerateSliderUI <- function(current, label) {
    
    slider_value <- input[[paste0('filter_', current)]]
      
    if(current %in% var_nums){
      min <- as.numeric(abs_min[current])
      max <- as.numeric(abs_max[current])
      step <- signif(max((max-min)*0.01, abs(min)*0.001, abs(max)*0.001),
                     digits = 4)
      slider_min <- signif((min - step*10), digits = 4)
      slider_max <- signif((max + step*10), digits = 4)
    }
    else{
      step <- 0
      slider_min <- as.numeric(abs_min[current])
      slider_max <- as.numeric(abs_max[current])
    }
    
    if(is.null(slider_value)){
      # TODO(tthomas): Why are we using 'step' around the already 'stepped' numerics?
      slider_value <- c(signif(slider_min-step*10, digits = 4),
                        signif(slider_max+step*10, digits = 4))
      # slider_value <- c(slider_min, slider_max)
    }
    
    
      column(2,
             # Hidden well panel for slider tooltip
             wellPanel(id = paste0("slider_tooltip_", current),
                       style = "position: absolute; z-index: 65; box-shadow: 10px 10px 15px grey; width: 20vw; left: 1vw; top: -275%; display: none;",
                       h4(label),
                       textInput(paste0("min_input_", current), "Min:"),
                       textInput(paste0("max_input_", current), "Max:"),
                       actionButton(paste0("submit_", current), "Apply","success")
             ),
             # The slider itself
             sliderInput(paste0('filter_', current),
                         label,
                         step = step,
                         min = slider_min,
                         max = slider_max,
                         value = slider_value)
      )
  }
  
  # Custom action button for exact entry. This makes a green button
  # and can also be accessed to produced different themed buttons
  actionButton <- function(inputId, label, btn.style = "" , css.class = "") {
    if ( btn.style %in% c("primary","info","success","warning","danger","inverse","link"))
      btn.css.class <- paste("btn",btn.style,sep="-")
    else btn.css.class = ""
    tags$button(id=inputId, type="button", class=paste("btn action-button",btn.css.class,css.class,collapse=" "), label)
  }
  
  openSliderToolTip <- function(current) {
    # This function calls hide on all slider exact entry windows on a
    # 'double click' and then calls 'show' on the opened one.
    for(i in 1:length(var_range_nums_and_ints)) {
      hide(paste0("slider_tooltip_", var_range_nums_and_ints[i]))
    }
    shinyjs::show(paste0("slider_tooltip_", current))
  }
  
  lapply(var_range_nums_and_ints, function(current) {
    # This handles the processing of exact entry back into the slider.
    # It reacts to either the submit button OR the enter key
    observe({
      input[[paste0("submit_", current)]]
      input$last_key_pressed
      
      isolate({
        slider_value = input[[paste0('filter_', current)]]
        new_min = input[[paste0("min_input_", current)]]
        new_max = input[[paste0("max_input_", current)]]
        updateTextInput(session, paste0("min_input_", current), value = "")
        updateTextInput(session, paste0("max_input_", current), value = "")
        suppressWarnings({ #Suppress warnings from non-numeric inputs
          if(!is.null(new_min) && new_min != "" && !is.na(as.numeric(new_min)))
            slider_value = as.numeric(c(new_min, slider_value[2]))
          if(!is.null(new_max) && new_max != "" && !is.na(as.numeric(new_max)))
            slider_value = as.numeric(c(slider_value[1], new_max))
        })
        updateSliderInput(session, paste0('filter_', current), value = slider_value)
        hide(paste0("slider_tooltip_", current))
      })
    })
  })
  
  # This function adds a double click handler to each slider
  observe({
    lapply(var_range_nums_and_ints, function(current) {
      onevent("dblclick", paste0("filter_", current), openSliderToolTip(current))
      inlineCSS(list(.style = "overflow: hidden"))
    })
  })
  
  output$constants <- renderUI({
    fluidRow(
      lapply(var_constants, function(var_constant) {
        column(2,
          p(strong(paste0(var_constant,":")), unname(raw[1,var_constant]))
        )
      })
    )
  })
  
  output$constants_present <- reactive({
    length(var_constants) > 0
  })
  
  outputOptions(output, "constants_present", suspendWhenHidden=FALSE)
  
  observeEvent(input$reset_sliders, {
    for(column in 1:length(var_names)){
      name <- var_names[column]
      switch(var_class[column],
        "numeric" =
        {
          max <- as.numeric(unname(data$meta$preprocessing$AbsMax()[var_names[column]]))
          min <- as.numeric(unname(data$meta$preprocessing$AbsMin()[var_names[column]]))
          diff <- (max-min)
          if (diff != 0) {
            step <- max(diff*0.01, abs(min)*0.001, abs(max)*0.001)
            updateSliderInput(session, paste0('filter_', name), value = c(signif(min-step*10, digits = 4), signif(max+step*10, digits = 4)))
          }
        },
        "integer" =
        {
          max <- as.integer(unname(data$meta$preprocessing$AbsMax()[var_names[column]]))
          min <- as.integer(unname(data$meta$preprocessing$AbsMin()[var_names[column]]))
          if(min != max) {
            updateSliderInput(session, paste0('filter_', name), value = c(min, max))
          }
        },
        "factor"  = updateSelectInput(session, paste0('filter_', name), selected = names(table(data$raw$df[var_names[column]])))
      )
    }
  })
  
  # Data processing ----------------------------------------------------------
    
  FilteredData <- reactive({
    # This reactive holds the full dataset that has been filtered using the
    # values of the sliders.
    data_filtered <- data$raw$df
    for(index in 1:length(var_names)) {
      name <- var_names[index]
      input_name <- paste("filter_", name, sep="")
      selection <- input[[input_name]]
      if(length(selection) != 0) {
        if(name %in% var_nums_and_ints) {
          isolate({
            above <- (data_filtered[[name]] >= selection[1])
            below <- (data_filtered[[name]] <= selection[2])
            in_range <- above & below
          })
        }
        else if (name %in% var_facs) {
            selection <- unlist(lapply(selection, function(factor){
                                                    RemoveItemNumber(factor)
                                                  }))
            in_range <- (data_filtered[[name]] %in% selection)
        }
        
        # Don't filter based on missing values.
        in_range <- in_range | is.na(data_filtered[[name]])
        
        data_filtered <- subset(data_filtered, in_range)
      }
      # print(nrow(data_filtered))
    }
    # print("Data Filtered.")
    data_filtered
  })
  
  Filters <- reactive({
    # This reactive returns a list of all the filter values so a tab can use
    # the information for filtering the raw dataset itself.
    #
    # Each of the variables will have a "type" that is simply the var_class for
    # that variable and either "selection" or "min" and "max", e.g.:
    # > Filters()$Engine$type
    #   "factor"
    # > Filters()$Engine$selection
    #   "V6" "V8"
    # > Filters()$TopSpeed$type
    #   "numeric"
    # > Filters()$TopSpeed$min
    #   130
    # > Filters()$TopSpeed$max
    #   210
    
    filters <- list()
    for(index in 1:length(var_names)) {
      name <- var_names[index]
      input_name <- paste("filter_", name, sep="")
      selection <- input[[input_name]]
      filters[[name]] <- list()
      filters[[name]]$type <- var_class[[name]]
      if(var_class[[name]] == "factor") {
        filters[[name]]$selection <- unname(sapply(selection,
                                                   RemoveItemNumber))
      }
      else {
        filters[[name]]$min <- selection[1]
        filters[[name]]$max <- selection[2]
      }
    }
    filters
  })
  
  ColoredData <- reactive({
    data_colored <- FilteredData()
    data_colored$color <- character(nrow(data_colored))
    data_colored$color <- "black"  #input$normColor
    if (input$coloring_source != "None") {
      if (input$coloring_source == "Live") {
        type <- input$live_coloring_type
      }
      else {
        type <- coloring$items[[input$coloring_source]]$type
      }
      isolate({
        coloring$current <- list()
        coloring$current$name <- input$coloring_source
        coloring$current$type <- type
      })
      switch(type,
        "Max/Min" = 
        {
          if (input$coloring_source == "Live") {
            var <- input$live_coloring_variable_numeric
            goal <- input$live_coloring_max_min
          }
          else {
            scheme <- coloring$items[[input$coloring_source]]
            var <- scheme$var
            goal <- scheme$goal
          }
          bins <- 30
          divisor <- (data$meta$preprocessing$AbsMax()[[var]] - data$meta$preprocessing$AbsMin()[[var]]) / bins
          minimum <- data$meta$preprocessing$AbsMin()[[var]]
          maximum <- data$meta$preprocessing$AbsMax()[[var]]
          cols <- rainbow(bins, 1, 0.875, start = 0, end = 0.325)
          if (goal == "Maximize") {
            data_colored$color <- unlist(sapply(data_colored[[var]], function(value) {
                            cols[max(1, ceiling((value - minimum) / divisor))]}))
          } 
          else {
            data_colored$color <- unlist(sapply(data_colored[[var]], function(value) {
                            cols[max(1, ceiling((maximum - value) / divisor))]}))
          }
          isolate({
            coloring$current$var <- var
            coloring$current$goal <- goal
          })
        },
        "Discrete" = 
        {
          if (input$coloring_source == "Live") {
            var <- input$live_color_variable_factor
            palette_selection <- input$live_color_palette
            if (palette_selection == "Rainbow") {
              s_value <- input$live_color_rainbow_s
              v_value <- input$live_color_rainbow_v
            }
          }
          else {
            scheme <- coloring$items[[input$coloring_source]]
            var <- scheme$var
            palette_selection <- scheme$palette
            if (palette_selection == "Rainbow") {
              s_value <- scheme$rainbow_s
              v_value <- scheme$rainbow_v
            }
          }
          variables_list = names(table(data_colored[[var]]))
          switch(palette_selection,
                 "Rainbow"={cols <- rainbow(length(variables_list),
                                            s_value,
                                            v_value)},
                 "Heat"={cols <- heat.colors(length(variables_list))},
                 "Terrain"={cols <- terrain.colors(length(variables_list))},
                 "Topo"={cols <- topo.colors(length(variables_list))},
                 "Cm"={cols <- cm.colors(length(variables_list))})
          for(i in 1:length(variables_list)){
            data_colored$color[(data_colored[[var]] == variables_list[i])] <- cols[i]
          }
          isolate({
            coloring$current$var <- var
            coloring$current$colors <- cols
          })
        }
      )
    }
    # print("Data Colored")
    # TODO(tthomas): Move adding units code out into the Explore.R tab.
    # names(data_colored) <- lapply(names(data_colored), AddUnits)
    data_colored
  })
  
  # Coloring -----------------------------------------------------------------
  
  updateSelectInput(session, "live_coloring_variable_numeric",
                    choices = var_range_nums_and_ints)
  updateSelectInput(session, "live_color_variable_factor",
                    choices = var_range_facs)
  
  observe({
    if (length(data$added$classifications) == 0) {
      numeric_choices <- as.list(var_range_nums_and_ints)
    } else {
      numeric_choices <- list(Variables=as.list(var_range_nums_and_ints),
                              Classifications=as.list(names(data$added$classifications)))
    }
    updateSelectInput(session, "live_coloring_variable_numeric",
                      choices = numeric_choices)
  })
  
  coloring_items <- list()
  current_coloring <- list()
  coloring <- reactiveValues(items=coloring_items,
                             current=current_coloring)
  
  output$coloring_table <- renderTable({
    names <- unlist(lapply(coloring$items,
                           function(current) {current$name}))
    descriptions <- unlist(lapply(coloring$items, function(current) {
      switch(current$type,
        "Max/Min"={paste0(current$goal, " ", current$var)},
        "Discrete"={paste0(current$var, " with ",
                           current$palette, " palette.")}
      )
    }))
    table <- data.frame(Names=names, Descriptions=descriptions)
  })
  
  observeEvent(input$live_coloring_add_classification, {
    isolate({
      name <- input$live_coloring_name
      if (name != "" && !(name %in% names(coloring$items))) {
        switch(input$live_coloring_type,
          "Max/Min"={
            coloring$items[[name]] <- list()
            coloring$items[[name]]$name <- name
            coloring$items[[name]]$type <- "Max/Min"
            coloring$items[[name]]$var <- input$live_coloring_variable_numeric
            coloring$items[[name]]$goal <- input$live_coloring_max_min
            coloring$items[[name]]$slider <- input$col_slider
          },
          "Discrete"={
            coloring$items[[name]] <- list()
            coloring$items[[name]]$name <- name
            coloring$items[[name]]$type <- "Discrete"
            coloring$items[[name]]$var <- input$live_color_variable_factor
            coloring$items[[name]]$palette <- input$live_color_palette
            coloring$items[[name]]$rainbow_s <- input$live_color_rainbow_s
            coloring$items[[name]]$rainbow_v <- input$live_color_rainbow_v
          }
        )
      }
    })
  })
  
  observe({
    isolate({
      selected <- input$coloring_source
    })
    new_choices <- c("None", "Live", names(coloring$items))
    updateSelectInput(session,
                      "coloring_source",
                      choices = new_choices,
                      selected = selected)
  })
  
  output$coloring_legend <- renderUI({
    req(coloring$current$type)
    if (coloring$current$type == "Discrete") {
      names <- names(table(data$raw[coloring$current$var]))
      raw_label <- ""
      for(i in 1:length(coloring$current$colors)){
        raw_label <- HTML(paste(raw_label, "<font color=",
                                sub("FF$", "", coloring$current$colors[i]),
                                "<b>", "&#9632", " ",
                                names[i], '<br/>'))
      }
      raw_label
    }
  })
  
  # Classifications and Sets -------------------------------------------------
  
  classification_items <- list()
  set_items <- list()
  added <- reactiveValues(classifications=classification_items,
                          sets=set_items)
  output$no_classifications <- renderText(NoClassifications())
  NoClassifications <- reactive({
    if (length(data$added$classifications) == 0) {
      "No Classifications Available."
    }
  })
  output$classification_table_output <- renderTable(ClassificationsTable())
  
  ClassificationsTable <- reactive({
    new_table <- do.call(rbind, data$added$classifications)
    new_table
  })
  
  # Final Processing ---------------------------------------------------------
  
  # Build the 'data' list that is shared between all tabs.
  data <- list()
  data$raw <- reactiveValues(df = raw)
  data$Filtered <- FilteredData
  data$Colored <- ColoredData
  data$Filters <- Filters
  data$added <- added
  
  # Build the 'meta' list.
  data$meta <- list(variables=variables,
                    coloring=coloring,
                    pet=pet,
                    preprocessing=preprocessing)
  
  data$meta$preprocessing$AbsMax <- reactive({
    classification_names <- names(data$added$classifications)
    vars <- c(var_nums_and_ints, classification_names)
    apply(data$raw$df[vars], 2, max, na.rm=TRUE)
  })
  
  data$meta$preprocessing$AbsMin <- reactive({
    vars <- c(var_nums_and_ints, names(data$added$classifications))
    apply(data$raw$df[vars], 2, min, na.rm=TRUE)
  })
  
  
  # data$experimental <- list()
  
  # Call individual tabs' Server() functions.
  lapply(tab_environments, function(custom_env) {
    do.call(custom_env$server,
            list(input, output, session, data))
  })
}

# UI -------------------------------------------------------------------------

# Setup UI with requested tabs.
base_tabs <- NULL

print("Tabs:")
added_tabs <- lapply(tab_environments, function(tab_env) {
  print(tab_env$title)
  tabPanel(tab_env$title, tab_env$ui())
})

tabset_arguments <- c(unname(base_tabs),
                      unname(added_tabs),
                      id = "master_tabset")

# Defines the UI of the Visualizer.
ui <- fluidPage(
  useShinyjs(),
  tags$script(src = "main.js"),
  titlePanel("Visualizer"),
  
  # Generates the master tabset from the user-defined tabs provided.
  do.call(tabsetPanel, tabset_arguments),
  
  # Optional Footer.
  conditionalPanel("output.display_footer",
    hr(),
    bsCollapse(id = "footer_collapse", open = NULL,
      bsCollapsePanel("Filters", 
        # tags$div(title = "Activate to show filters for all dataset variables.",
        #          checkboxInput("viewAllFilters", "View All Filters", value = TRUE)),
        tags$div(title = "Return visible sliders to default state.",
                 actionButton("reset_sliders", "Reset Visible Filters")),
        hr(),
        
        uiOutput("filters"),
        conditionalPanel("output.constants_present",
          h3("Constants:"),
          uiOutput("constants")
        ),
        style = "default"
      ),
      bsCollapsePanel("Coloring",
        column(3,
          h4("Coloring Source"),
          selectInput("coloring_source", "Source", choices = c("None", "Live"), selected = "None"),
          htmlOutput("coloring_legend")
        ),
        column(3,
          h4("Live"),
          selectInput("live_coloring_type", "Type:", choices = c("Max/Min", "Discrete"), selected = "Max/Min"),  #, "Highlighted", "Ranked"), selected = "None")
          conditionalPanel(
            condition = "input.live_coloring_type == 'Max/Min'",
            selectInput("live_coloring_variable_numeric", "Colored Variable:", c()),
            radioButtons("live_coloring_max_min", NULL, c("Maximize" = "Maximize", "Minimize" = "Minimize"), selected = "Maximize")
          ),
          conditionalPanel(
            condition = "input.live_coloring_type == 'Discrete'",
            selectInput("live_color_variable_factor",
                        "Colored Variable:",
                        c()),
            selectInput("live_color_palette",
                        "Color Palette:",
                        c("Rainbow", "Heat", "Terrain", "Topo", "Cm")),
            conditionalPanel(
              condition = "input.live_color_palette == 'Rainbow'",
              sliderInput("live_color_rainbow_s", "Saturation:",
                          min=0, max=1,
                          value=1, step=0.025),
              sliderInput("live_color_rainbow_v", "Value/Brightness:",
                          min=0, max=1,
                          value=1, step=0.025)
            )
          )
        ),
        column(6,
          h4("Saved"),
          textInput("live_coloring_name", "Name"),
          actionButton("live_coloring_add_classification", "Add Current 'Live' Coloring"),
          br(), tableOutput("coloring_table")
        ),
        style = "default"
      ),
      bsCollapsePanel("Classifications",
        fluidRow(
          column(12,
            textOutput('no_classifications'),
            tableOutput('classification_table_output')
          )
        ),
        style = "default"
      )
    )
  )
)

# Start the Shiny app.
shinyApp(ui = ui, server = Server)
