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
source("utils.R")

# Defined Constants ----------------------------------------------------------

ABBREVIATION_LENGTH <- 25
SAVE_DIG_INPUT_CSV <- TRUE
FILTER_WIDTH_IN_COLUMNS <- 2

# Resolve Dataset Configuration ----------------------------------------------

pet_config_present <- FALSE
design_tree_present <- FALSE
saved_inputs <- NULL
visualizer_config <- NULL

dig_input_csv <- Sys.getenv('DIG_INPUT_CSV')
dig_dataset_config <- Sys.getenv('DIG_DATASET_CONFIG')
if (dig_dataset_config == "") {
  if(dig_input_csv == "") {
    # Setup one of the test datasets if no input dataset
    config_filename=file.path('datasets',
                              'boxpacking',
                              'visualizer_config.json',
                              fsep = "\\\\")
    # config_filename=file.path('datasets',
    #                           'WindTurbineForOptimization',
    #                           'visualizer_config.json',
    #                           fsep = "\\\\")
    # config_filename=file.path('datasets',
    #                           'WindTurbine',
    #                           'visualizer_config.json',
    #                           fsep = "\\\\")
    # config_filename=file.path('datasets',
    #                           'TestPETRefinement',
    #                           'visualizer_config.json',
    #                           fsep = "\\\\")
  } else {
    # Visualizer legacy launch format
    csv_dir <- dirname(dig_input_csv)
    config_filename <- file.path(csv_dir,
                                 sub(".csv",
                                     "_viz_config.json",
                                     basename(dig_input_csv)),
                                 fsep = "\\\\")
    
  }
} else {
  config_filename <- gsub("\\\\", "/", dig_dataset_config)
}

if(file.exists(config_filename)) {
  visualizer_config <- fromJSON(config_filename, simplifyDataFrame=FALSE)
} else {
  visualizer_config <- list()
  visualizer_config$raw_data <- basename(dig_input_csv)
  visualizer_config$pet_config <- "pet_config.json"
  visualizer_config$tabs <- c("Explore.R",
                              "DataTable.R",
                              "Histogram.R",
                              "PETRefinement.R",
                              "Scratch.R",
                              "ParallelAxisPlot.R",
                              "UncertaintyQuantification.R")
}

tab_requests <- visualizer_config$tabs
saved_inputs <- visualizer_config$inputs
launch_dir <- dirname(config_filename)

pet_config_filename <- visualizer_config$pet_config
if (!is.null(pet_config_filename) && pet_config_filename != "") {
  pet_config_filename <- file.path(launch_dir, pet_config_filename)
  if (file.exists(pet_config_filename)) {
    pet_config_present <- TRUE
  }
}

design_tree_filename <- file.path(launch_dir, "design_tree.json")
if (file.exists(design_tree_filename)) {
  design_tree_present <- TRUE
  FILTER_WIDTH_IN_COLUMNS <- 3
}

# Saved Input Functions ------------------------------------------------------

si <- function(id, default) {
  # Retrieves saved input state from the previous session for a UI element
  # with a given id. This function will most often be called in a 'ui(id)'
  # definition when creating a UI input element.
  # 
  # This function deletes the saved value after it is accessed, so each tab
  # should take care to persist the value through any regeneration of UI
  # elements.
  #
  # Args:
  #   id: the 'id' to look up in the saved_inputs list
  #   default: the value to return if the 'id' isn't found

  if(!is.null(saved_inputs) && !is.null(saved_inputs[[id]])) {
    value <- saved_inputs[[id]]
    saved_inputs[[id]] <<- NULL
    value
  } else {
    default
  }
}

si_read <- function(id) {
  # Retrieves saved input state from the previous session for a UI element
  # with a given id but does not consider it 'applied.'
  saved_inputs[[id]]
}

si_clear <- function(id) {
  # Clears the saved input state from the previous session for a UI element
  # with a given id.
  if(!is.null(saved_inputs[[id]])) {
    saved_inputs[[id]] <<- NULL
  }
}

# Load Tabs and Data ---------------------------------------------------------

# Locate tab files
tab_files <- list()
tab_ids <- list()
for (i in 1:length(tab_requests)) {
  request <- tab_requests[i]
  if (file.exists(file.path(launch_dir, request))) {
    tab_files <- c(tab_files, file.path(launch_dir, request))
    tab_ids <- c(tab_ids, sub("\\.R$", "", request))
  } else if (file.exists(file.path('tabs', request))) {
    tab_files <- c(tab_files, file.path('tabs', request))
    tab_ids <- c(tab_ids, sub("\\.R$", "", request))
  }
}

# Source tab files
print("Sourcing Tabs:")
tab_environments <- mapply(function(file_name, id) {
    env <- new.env()
    if(!is.null(visualizer_config$tab_data)) {
      env$tab_data <- visualizer_config$tab_data[[id]]
    } else {
      env$tab_data <- NULL
    }
    source(file_name, local = env)
    # debugSource(file_name, local = env)
    print(paste0(env$title, " (", file_name, ")"))
    env
  },
  file_name=tab_files,
  id=tab_ids,
  SIMPLIFY = FALSE
)

# Read input dataset file
raw <- read.csv(file.path(launch_dir, visualizer_config$raw_data), fill=T)
if(!is.null(visualizer_config$augmented_data)) {
  augmented_filename <- file.path(launch_dir,
                                  visualizer_config$augmented_data)
  augmented <- read.csv(augmented_filename, fill=T)
  extra <- raw[!(raw$GUID %in% augmented$GUID),]
  raw <- rbind(augmented, extra)
}

# Locate Artifacts
print("Locating Artifacts:")
guid_folders <- NULL
if(file.exists(file.path(launch_dir, 'metadata.json'))) {
  config_folders <- GetConfigFolders(launch_dir)
  print(paste("Config Folders:", length(config_folders)))
  results_dir <- file.path(launch_dir,"..","..","results")
  guid_folders <- FindGUIDFolders(results_dir, config_folders)
  print(paste("GUID Folders:", length(guid_folders)))
} else {
  print("No Artifacts Found.")
}

# Process PET Configuration File ('pet_config.json') -------------------------
pet <- NULL
if (!is.null(visualizer_config[["pet"]])) {
  pet <- visualizer_config$pet
} else if(pet_config_present) {
  pet <- BuildPet(pet_config_filename)
}

# Process Variables ----------------------------------------------------------

variables <- NULL
if (!is.null(visualizer_config[["variables"]])) {
  variables <- visualizer_config$variables
} else {
  variables <- BuildVariables(pet, names(raw))
}

# Server ---------------------------------------------------------------------

Server <- function(input, output, session) {
  # Handles the processing of all UI interactions.
  #
  # Args:
  #   input: the Shiny list of all the UI input elements.
  #   output: the Shiny list of all the UI output elements.
  #   session: a handle for the Shiny session.
  
  # Data Pre-Processing --------------------------------------------------------

  var_class <- reactive({
    df_class <- sapply(data$raw$df, class)
    df_class[-which(names(df_class) %in% c("GUID", "CfgID"))]
  })
  var_names <- reactive({names(var_class())})
  var_facs <- reactive({
    if (any(var_class() == "factor")) {
      var_names()[var_class() == "factor"]
    } else {
      NULL
    }
  })
  var_ints <- reactive({
    if (any(var_class() == "integer")) {
      var_names()[var_class() == "integer"]
    } else {
      NULL
    }
  })
  var_nums <- reactive({
    if (any(var_class() == "numeric")) {
      var_names()[var_class() == "numeric"]
    } else {
      NULL
    }
  })
  var_nums_and_ints <- reactive({
    selected <- var_class() == "integer" | var_class() == "numeric"
    if (any(selected)) {
      var_names()[selected]
    } else {
      NULL
    }
  })
  abs_max <- reactive({
    if (is.null(var_nums_and_ints())) {
      NULL
    } else {
      apply(data$raw$df[var_nums_and_ints()], 2, max, na.rm=TRUE)
    }
  })
  abs_min <- reactive({
    if (is.null(var_nums_and_ints())) {
      NULL
    } else {
      apply(data$raw$df[var_nums_and_ints()], 2, min, na.rm=TRUE)
    }
  })
  var_range_nums_and_ints <- reactive({
    if (is.null(var_nums_and_ints()) ||
        is.null(abs_min()) ||
        is.null(abs_max())) {
      NULL
    } else {
      tentative_vars <- var_nums_and_ints()[(abs_min() != abs_max()) &
                                            (abs_min() != Inf)]
      if (length(tentative_vars) == 0) {
        NULL
      } else {
        tentative_vars
      }
    }
  })
  var_range_facs <- reactive({
    if(is.null(var_facs())) {
      NULL
    } else {
      tentative_vars <- var_facs()[apply(data$raw$df[var_facs()], 2,
                                         function(var_fac) {
                                           length(names(table(var_fac))) > 1
                                         })]
      if (length(tentative_vars) == 0) {
        NULL
      } else {
        tentative_vars
      }
    }
  })
  var_range <- reactive({c(var_range_facs(), var_range_nums_and_ints())})
  var_range_nums_and_ints_list <- reactive({
    if (is.null(var_range_nums_and_ints())) {
      NULL
    } else {
      AddCategories(data$meta$variables[var_range_nums_and_ints()])
    }
  })
  var_range_facs_list <- reactive({
    if (is.null(var_range_facs())) {
      NULL
    } else {
      AddCategories(data$meta$variables[var_range_facs()])
    }
  })
  var_range_list <- reactive({
    if (is.null(var_range())) {
      NULL
    } else {
      AddCategories(data$meta$variables[var_range()])
    }
  })
  var_constants <- reactive({
    if(is.null(var_range())) {
      var_names()
    } else {
      subset(var_names(), !(var_names() %in% var_range()))
    }
  })
  
  pre <- list(var_names=var_names,
              var_class=var_class,
              var_facs=var_facs,
              var_ints=var_ints,
              var_nums=var_nums,
              var_nums_and_ints=var_nums_and_ints,
              abs_min=abs_min,
              abs_max=abs_max,
              var_range_nums_and_ints=var_range_nums_and_ints,
              var_range_facs=var_range_facs,
              var_range=var_range,
              var_range_nums_and_ints_list=var_range_nums_and_ints_list,
              var_range_facs_list=var_range_facs_list,
              var_range_list=var_range_list,
              var_constants=var_constants)
  
  # Design Configs for Filters -----------------------------------------------
  
  SelectAllComponents <- function(node) {
    if(node[['Type']] == "Component") {
      node[['Selected']] <- TRUE
    } else {
      node[['Children']] <- lapply(node[['Children']], SelectAllComponents)
    }
    node
  }
  
  observe({
    if(design_tree_present) {
      if(is.empty(visualizer_config$config_tree)) {
        names <- names(DesignConfigs())
        config_tree <- SelectAllComponents(DesignConfigs()[[names[1]]])
      } else {
        config_tree <- visualizer_config$config_tree
      }
      session$sendCustomMessage(type = "setup_design_configurations", config_tree)
    }
  })
  
  DesignConfigs <- reactive({
    if(design_tree_present) {
      design_configs <- fromJSON(design_tree_filename, simplifyDataFrame = FALSE)
    } else {
      NULL
    }
  })
  
  # observe(print(DesignConfigs()))
  
  # observe({print(paste("SDC:",paste(SelectedDesignConfigs(),collapse=",")))})

  SelectedDesignConfigs <- reactive({
    # print(input$filter_design_config_tree)
    if(!is.null(input$filter_design_config_tree)) {
      names <- names(DesignConfigs())
      passing <- sapply(names, function(name) {
        filter_tree <- input$filter_design_config_tree
        current_tree <- DesignConfigs()[[name]]
        compare_node(current_tree, filter_tree)
      })
      if(any(passing)) {
        names[passing]
      } else {
        NULL
      }
    } else {
      if ("CfgID" %in% names(data$raw$df) && is.null(DesignConfigs())) {
        if(pet_config_present) {
          pet$selected_configurations
        } else {
          unique(data$raw$df[['CfgID']])
        }
      } else {
        NULL
      }
    }
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
  
  # Special observe to cover 'footer_collapse'
  observe({
    if(!is.null(si_read("footer_collapse"))) {
      open <- si("footer_collapse")
      if(is.empty(open)) {
        updateCollapse(session, "footer_collapse", close = "Filters")
      } else {
        updateCollapse(session, "footer_collapse", open = open)
      }
    }
  })
  
  # Generates the sliders and select boxes.
  output$filters <- renderUI({
    var_selects <- pre$var_range_facs()
    var_sliders <- pre$var_range_nums_and_ints()
    
    div(
      fluidRow(
        lapply(var_selects, function(var_select) {
          GenerateEnumUI(var_select)
        })
      ),
      fluidRow(
        lapply(var_sliders, function(var_slider) {
          GenerateSliderUI(var_slider)
        })
      )
    )
  })
  
  # Slider abbreviation function based off slider_width
  abbreviation_length <- ABBREVIATION_LENGTH
  AbbreviateLabel <- function(name) {
    if(!is.null(input$slider_width)){
      abbreviation_length <<- input$slider_width/8
    }
    abbreviate(name, abbreviation_length)
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
      # selected_value <- items
      selected_value <- si(paste0('filter_', current), items)
    
    column(FILTER_WIDTH_IN_COLUMNS,
           selectInput(inputId = paste0('filter_', current),
                       label = current,
                       multiple = TRUE,
                       selectize = FALSE,
                       choices = items,
                       selected = selected_value)
    )
  }
  
  GenerateSliderUI <- function(current) {
    
    if(current %in% pre$var_nums()){
      min <- as.numeric(pre$abs_min()[current])
      max <- as.numeric(pre$abs_max()[current])
      step <- signif(max((max-min)*0.01, abs(min)*0.001, abs(max)*0.001),
                     digits = 4)
      slider_min <- signif((min - step*10), digits = 4)
      slider_max <- signif((max + step*10), digits = 4)
    }
    else{
      step <- 0
      slider_min <- as.numeric(pre$abs_min()[current])
      slider_max <- as.numeric(pre$abs_max()[current])
    }
    
    slider_value <- input[[paste0('filter_', current)]]
    if(is.null(slider_value)){
      # TODO(tthomas): Why are we using 'step' around the already 'stepped' numerics?
      # slider_value <- c(signif(slider_min-step*10, digits = 4),
      #                   signif(slider_max+step*10, digits = 4))
      slider_value <- si(paste0('filter_', current),
                         c(signif(slider_min-step*10, digits = 4),
                           signif(slider_max+step*10, digits = 4)))
      # slider_value <- c(slider_min, slider_max)
    }
    
    
      column(FILTER_WIDTH_IN_COLUMNS,
             # Hidden well panel for slider tooltip
             wellPanel(id = paste0("slider_tooltip_", current),
                       style = "position: absolute; z-index: 65; box-shadow: 10px 10px 15px grey; width: 20vw; left: 1vw; top: -275%; display: none;",
                       h4(data$meta$variables[[current]]$name_with_units),
                       textInput(paste0("tooltip_min_", current), "Min:"),
                       textInput(paste0("tooltip_max_", current), "Max:"),
                       actionButton(paste0("submit_", current), "Apply","success")
             ),
             # The slider itself
             sliderInput(paste0('filter_', current),
                         AbbreviateLabel(current),
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
    for(i in 1:length(pre$var_range_nums_and_ints())) {
      hide(paste0("slider_tooltip_", pre$var_range_nums_and_ints()[i]))
    }
    shinyjs::show(paste0("slider_tooltip_", current))
  }
  
  observe({
    lapply(pre$var_range_nums_and_ints(), function(current) {
      # This handles the processing of exact entry back into the slider.
      # It reacts to either the submit button OR the enter key
      input[[paste0("submit_", current)]]
      input$last_key_pressed
      
      isolate({
        slider_value = input[[paste0('filter_', current)]]
        new_min = input[[paste0("tooltip_min_", current)]]
        new_max = input[[paste0("tooltip_max_", current)]]
        updateTextInput(session, paste0("tooltip_min_", current), value = "")
        updateTextInput(session, paste0("tooltip_max_", current), value = "")
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
    lapply(pre$var_range_nums_and_ints(), function(current) {
      onevent("dblclick", paste0("filter_", current), openSliderToolTip(current))
      inlineCSS(list(.style = "overflow: hidden"))
    })
  })
  
  output$constants <- renderUI({
    fluidRow(
      lapply(pre$var_constants(), function(var_constant) {
        column(2,
          p(strong(paste0(var_constant,":")), unname(raw[1,var_constant]))
        )
      })
    )
  })
  
  output$constants_present <- reactive({
    length(pre$var_constants()) > 0
  })
  
  outputOptions(output, "constants_present", suspendWhenHidden=FALSE)
  
  observeEvent(input$reset_sliders, {
    session$sendCustomMessage(type = "select_all_design_configurations", "")
    for(column in 1:length(pre$var_names())){
      name <- pre$var_names()[column]
      switch(pre$var_class()[column],
        "numeric" =
        {
          max <- as.numeric(unname(data$pre$abs_max()[pre$var_names()[column]]))
          min <- as.numeric(unname(data$pre$abs_min()[pre$var_names()[column]]))
          diff <- (max-min)
          if (diff != 0) {
            step <- max(diff*0.01, abs(min)*0.001, abs(max)*0.001)
            updateSliderInput(session, paste0('filter_', name), value = c(signif(min-step*10, digits = 4), signif(max+step*10, digits = 4)))
          }
        },
        "integer" =
        {
          max <- as.integer(unname(data$pre$abs_max()[pre$var_names()[column]]))
          min <- as.integer(unname(data$pre$abs_min()[pre$var_names()[column]]))
          if(min != max) {
            updateSliderInput(session, paste0('filter_', name), value = c(min, max))
          }
        },
        "factor"  = updateSelectInput(session, paste0('filter_', name), selected = names(table(data$raw$df[pre$var_names()[column]])))
      )
    }
  })
  
  # Data processing ----------------------------------------------------------
    
  FilteredData <- reactive({
    # This reactive holds the full dataset that has been filtered using the
    # values of the sliders.
    data_filtered <- data$raw$df
    if(input$remove_missing) {
      data_filtered <- data_filtered[complete.cases(data_filtered), ]
    }
    if(input$remove_outliers) {
      #Filter out rows by standard deviation
      for(column in 1:length(data$pre$var_range_nums_and_ints())) {
        a <- sapply(data_filtered[data$pre$var_range_nums_and_ints()[column]],
          function(x) {
            m <- mean(x, na.rm = TRUE)
            s <- sd(x, na.rm = TRUE)
            x >= m - input$num_sd*s &
            x <= m + input$num_sd*s
          }
        )
        data_filtered <- subset(data_filtered, a)
      }
    }
    if("CfgID" %in% names(data_filtered)) {
      data_filtered <- subset(data_filtered, data_filtered$CfgID %in% SelectedDesignConfigs())
    }
    for(index in 1:length(pre$var_names())) {
      name <- pre$var_names()[index]
      input_name <- paste("filter_", name, sep="")
      selection <- input[[input_name]]
      if(length(selection) != 0) {
        if(name %in% pre$var_nums_and_ints()) {
          isolate({
            above <- (data_filtered[[name]] >= selection[1])
            below <- (data_filtered[[name]] <= selection[2])
            in_range <- above & below
          })
        }
        else if (name %in% pre$var_facs()) {
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
    # Each of the variables will have a "type" that is simply the 
    # pre$var_class() for that variable and either "selection" or "min" and
    # "max", e.g.:
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
    for(index in 1:length(pre$var_names())) {
      name <- pre$var_names()[index]
      input_name <- paste("filter_", name, sep="")
      selection <- input[[input_name]]
      filters[[name]] <- list()
      filters[[name]]$type <- pre$var_class()[[name]]
      if(pre$var_class()[[name]] == "factor") {
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
        type <- data$meta$colorings[[input$coloring_source]]$type
      }
      isolate({
        data$colorings$current <- list()
        data$colorings$current$name <- input$coloring_source
        data$colorings$current$type <- type
      })
      switch(type,
        "Max/Min" = 
        {
          if (input$coloring_source == "Live") {
            var <- input$live_coloring_variable_numeric
            goal <- input$live_coloring_max_min
          }
          else {
            scheme <- data$meta$colorings[[input$coloring_source]]
            var <- scheme$var
            goal <- scheme$goal
          }
          bins <- 30
          req(var)
          divisor <- (data$pre$abs_max()[[var]] - data$pre$abs_min()[[var]]) / bins
          minimum <- data$pre$abs_min()[[var]]
          maximum <- data$pre$abs_max()[[var]]
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
            data$colorings$current$var <- var
            data$colorings$current$goal <- goal
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
            scheme <- data$meta$colorings[[input$coloring_source]]
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
            data$colorings$current$var <- var
            data$colorings$current$colors <- cols
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
  
  if(is.null(visualizer_config$coloring)) {
    colorings <- list()
  } else {
    colorings <- visualizer_config$coloring
  }
  
  output$coloring_table <- renderTable({
    names <- unlist(lapply(data$meta$colorings,
                           function(current) {current$name}))
    descriptions <- unlist(lapply(data$meta$colorings, function(current) {
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
      if (!(name %in% c(names(data$meta$colorings), "", "current"))) {
        switch(input$live_coloring_type,
          "Max/Min"={
            data$meta$colorings[[name]] <- list()
            data$meta$colorings[[name]]$name <- name
            data$meta$colorings[[name]]$type <- "Max/Min"
            data$meta$colorings[[name]]$var <- input$live_coloring_variable_numeric
            data$meta$colorings[[name]]$goal <- input$live_coloring_max_min
            data$meta$colorings[[name]]$slider <- input$col_slider
          },
          "Discrete"={
            data$meta$colorings[[name]] <- list()
            data$meta$colorings[[name]]$name <- name
            data$meta$colorings[[name]]$type <- "Discrete"
            data$meta$colorings[[name]]$var <- input$live_color_variable_factor
            data$meta$colorings[[name]]$palette <- input$live_color_palette
            data$meta$colorings[[name]]$rainbow_s <- input$live_color_rainbow_s
            data$meta$colorings[[name]]$rainbow_v <- input$live_color_rainbow_v
          }
        )
        updateTextInput(session, "live_coloring_name", value = "")
      }
    })
  })
  
  output$coloring_legend <- renderUI({
    req(data$colorings$current$type)
    req(data$colorings$current$var)
    req(data$raw$df)
    if (data$colorings$current$type == "Discrete") {
      names <- names(table(data$raw$df[data$colorings$current$var]))
      raw_label <- ""
      for(i in 1:length(data$colorings$current$colors)){
        raw_label <- HTML(paste(raw_label, "<font color=",
                                sub("FF$", "", data$colorings$current$colors[i]),
                                "<b>", "&#9632", " ",
                                names[i], '<br/>'))
      }
      raw_label
    }
  })
  
  observe({
    isolate({
      selected <- input$coloring_source
    })
    new_choices <- c("None", "Live", names(data$meta$colorings))
    if (!is.null(si_read("coloring_source")) &&
        si_read("coloring_source") %in% new_choices) {
      selected <- si("coloring_source", NULL)
    }
    updateSelectInput(session,
                      "coloring_source",
                      choices = new_choices,
                      selected = selected)
  })
  
  observe({
    selected <- isolate(input$live_color_variable_factor)
    if(is.null(selected) || selected == "") {
      selected <- data$pre$var_range_facs()[1]
    }
    saved <- si_read("live_color_variable_factor")
    if (is.empty(saved)) {
      si("live_color_variable_factor", NULL)
    } else if (saved %in% c(data$pre$var_range_facs(), "")) {
      selected <- si("live_color_variable_factor", NULL)
    }
    updateSelectInput(session, "live_color_variable_factor",
                      choices = data$pre$var_range_facs_list(),
                      selected = selected)
  })
  
  observe({
    selected <- isolate(input$live_coloring_variable_numeric)
    if(is.null(selected) || selected == "") {
      selected <- data$pre$var_range_nums_and_ints()[1]
    }
    saved <- si_read("live_coloring_variable_numeric")
    if (is.empty(saved)) {
      si("live_coloring_variable_numeric", NULL)
    } else if (saved %in% c(data$pre$var_range_nums_and_ints(), "")) {
      selected <- si("live_coloring_variable_numeric", NULL)
    }
    updateSelectInput(session, "live_coloring_variable_numeric",
                      choices = data$pre$var_range_nums_and_ints_list(),
                      selected = selected)
  })
  
  # Sets ---------------------------------------------------------------------
  
  # Blank or saved data
  if(is.null(visualizer_config$sets)) {
    sets <- list()
  } else {
    sets <- visualizer_config$sets
  }
  
  # Classifications table
  output$no_classifications <- renderText(NoClassifications())
  NoClassifications <- reactive({
    if (!any(sapply(data$meta$variables,
                    function(var) var$type == "Classification"))) {
      "No Classifications Available."
    }
  })
  
  output$classification_table_output <- renderTable(ClassificationsTable())
  
  ClassificationsTable <- reactive({
    new_table <- do.call(rbind, classifications())
    if (!is.null(new_table)) {
      new_table <- cbind(name=names(classifications()), new_table)
    }
  })
  
  classifications <- reactive({
    data$meta$variables[sapply(data$meta$variables,
                        function(x) x$type == "Classification")]
  })
  
  # Final Processing ---------------------------------------------------------
  
  # Build the 'data' list that is shared between all tabs.
  data <- list()
  data$raw <- reactiveValues(df = raw)
  data$Filtered <- FilteredData
  data$Colored <- ColoredData
  data$Filters <- Filters
  
  # Build the 'meta' list.
  data$meta <- reactiveValues(variables=variables,
                              colorings=colorings,
                              pet=pet,
                              sets=sets)
  data$pre <- pre
  
  # Call each tab's server() function ----------------------------------------
  
  mapply(function(tab_env, id) {
      # do.call(tab_env$server,
      #         list(input, output, session, data))
      callModule(tab_env$server, paste(id), data)
    },
    tab_env=tab_environments,
    id=tab_ids,
    SIMPLIFY = FALSE
  )
  
  # Session Save/Restore -----------------------------------------------------

  # The save/restore functionality relies upon three main mechanisms: 
  #  1. It saves the values of all the inputs to the visualizer config file
  #     on close by supplying the necessary callback function to
  #     session$onSessionEnded().
  #  2. It provides a function, si(id, default) to the tabs that allows
  #     them to check for a saved value for a input when creating inputs
  #     in their 'ui'.
  #  3. Lastly, there are a number of inputs that can't be restored using the
  #     standard si() function. These special inputs are restored using 
  #     observe(), isolate(), si_read(), and si() functions and can be found
  #     throughout the rest of the body of the 'app.R' file.
  
  # Dispose of this server when the UI is closed
  session$onSessionEnded(function() {
    
    # Save the updated raw data
    if(is.null(visualizer_config$augmented_data)) {
      visualizer_config$augmented_data <- sub(".json", "_data.csv",
                                              basename(config_filename))
    }
    write.csv(isolate(data$raw$df),
              file=file.path(launch_dir, visualizer_config$augmented_data),
              row.names = FALSE)
    
    # Prepare metadata for saving to visualizer config file
    meta <- isolate(reactiveValuesToList(data$meta))
    visualizer_config$variables <- meta$variables
    meta$colorings$current <- NULL
    visualizer_config$colorings <- meta$colorings
    # visualizer_config$pet <- meta$pet
    visualizer_config$sets <- meta$sets
    # visualizer_config$comments <- meta$comments
    visualizer_config$config_tree <- isolate(input$filter_design_config_tree)
    
    # Prepare inputs for saving to visualizer config file
    current_inputs <- isolate(reactiveValuesToList(input))
    current_inputs[["window_width"]] <- NULL
    current_inputs[unlist(lapply(names(current_inputs), function (name) {
      "shinyActionButtonValue" %in% class(current_inputs[[name]]) ||
      # grepl("click", name) ||
      grepl("^tooltip_", name)
    }))] <- NULL
    combined_inputs <- c(saved_inputs,
                         current_inputs[setdiff(names(current_inputs),
                                                names(saved_inputs))])
    combined_inputs <- combined_inputs[order(names(combined_inputs))]
    visualizer_config$inputs <- combined_inputs
    
    # Retrive tab data to save
    tab_data <- lapply(tab_environments, function(tab_env) {
      if(!is.null(tab_env$TabData)) {
        tab_env$TabData()
      }
    })
    names(tab_data) <- tab_ids
    visualizer_config$tab_data <- tab_data
    
    # Save visualizer config file
    if(SAVE_DIG_INPUT_CSV || dig_input_csv == "") {
      write(toJSON(visualizer_config, pretty = TRUE, auto_unbox = TRUE),
            file=config_filename)
      print("Session saved.")
    }
    
    # Clear environment variables (necessary only for development)
    Sys.setenv(DIG_INPUT_CSV="")
    Sys.setenv(DIG_DATASET_CONFIG="")
    stopApp()
  })
  
  
}

# UI -------------------------------------------------------------------------

# Setup UI with requested tabs.
base_tabs <- NULL

added_tabs <- mapply(function(tab_env, id) {
    tabPanel(tab_env$title, tab_env$ui(paste(id)))
  },
  tab_env=tab_environments,
  id=tab_ids,
  SIMPLIFY = FALSE
)

tabset_arguments <- c(unname(base_tabs),
                      unname(added_tabs),
                      id = "master_tabset",
                      selected = si("master_tabset", NULL))

# Defines the UI of the Visualizer.
ui <- fluidPage(
  useShinyjs(),
  tags$script(src = "main.js"),
  
  tags$head(tags$link(rel = "stylesheet", type = "text/css", href = "DesignConfig.css")),
  tags$script(src = "d3.v3.min.js"),
  tags$script(src = "design_config_selector.js"),
  
  titlePanel("Visualizer"),
  
  # Generates the master tabset from the user-defined tabs provided.
  do.call(tabsetPanel, tabset_arguments),
  
  # Optional Footer.
  conditionalPanel("output.display_footer",
    hr(),
    bsCollapse(id = "footer_collapse", open = "Filters",  # COMMENT(tthomas): Filters need to open to initialize properly, observe() in server covers saved input.
      bsCollapsePanel("Filters", 
        # tags$div(title = "Activate to show filters for all dataset variables.",
        #          checkboxInput("viewAllFilters", "View All Filters", value = TRUE)),
        tags$div(title = "Return visible sliders to default state.",
                 actionButton("reset_sliders", "Reset Visible Filters")),
        hr(),
        
        if(design_tree_present) {
          fluidRow(
            column(3, tags$label("Design Configuration Tree"), tags$div(id="design_configurations")),
            column(9, uiOutput("filters"))
          )
        } else {
          fluidRow(
            column(12, uiOutput("filters"))
          )
        },
        conditionalPanel("output.constants_present",
          h3("Constants:"),
          uiOutput("constants")
        ),
        style = "default"
      ),
      bsCollapsePanel("Coloring",
        column(3,
          h4("Coloring Source"),
          selectInput("coloring_source", "Source", choices = c("None", "Live")),  #, selected = si("coloring_source", "None")),
          htmlOutput("coloring_legend")
        ),
        column(3,
          h4("Live"),
          selectInput("live_coloring_type", "Type:", choices = c("Max/Min", "Discrete"), selected = si("live_coloring_type", "Max/Min")),  #, "Highlighted", "Ranked"), selected = "None")
          conditionalPanel(
            condition = "input.live_coloring_type == 'Max/Min'",
            selectInput("live_coloring_variable_numeric", "Colored Variable:", c()),
            radioButtons(inputId = "live_coloring_max_min",
                         label = NULL,
                         choices = c("Maximize" = "Maximize", "Minimize" = "Minimize"),
                         selected = si("live_coloring_max_min", "Maximize"))
          ),
          conditionalPanel(
            condition = "input.live_coloring_type == 'Discrete'",
            selectInput("live_color_variable_factor",
                        "Colored Variable:",
                        c()),
            selectInput("live_color_palette",
                        "Color Palette:",
                        c("Rainbow", "Heat", "Terrain", "Topo", "Cm"),
                        si("live_color_palette", "Rainbow")),
            conditionalPanel(
              condition = "input.live_color_palette == 'Rainbow'",
              sliderInput("live_color_rainbow_s", "Saturation:",
                          min=0, max=1,
                          value=si("live_color_rainbow_s", 1),
                          step=0.025),
              sliderInput("live_color_rainbow_v", "Value/Brightness:",
                          min=0, max=1,
                          value=si("live_color_rainbow_v", 1),
                          step=0.025)
            )
          )
        ),
        column(6,
          h4("Saved"),
          textInput("live_coloring_name", "Name", si("live_coloring_name", "")),
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
      ),
      bsCollapsePanel("Configuration",
        fluidRow(
          column(3,
            h4("Data Processing"),
            checkboxInput("remove_missing", "Remove Missing",
                          si("remove_missing", FALSE)),
            checkboxInput("remove_outliers", "Remove Outlier",
                          si("remove_outliers", FALSE)),
            sliderInput("num_sd", HTML("&sigma;:"), min = 1, max = 11, step = 0.1,
                        value = si("num_sd", 6))
          ),
          column(3,
            h4("About"),
            p(strong("Version:"), "v2.0.0"),
            p(strong("Date:"), "5/1/2017"),
            p(strong("Developer:"), "Metamorph Software"),
            p(strong("Support:"), "tthomas@metamorphsoftware.com")
          )
        ),
        style = "default"
      )
    )
  )
)

# Start the Shiny app.
shinyApp(ui = ui, server = Server)
