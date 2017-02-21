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

# Defined Constants
DEFAULT_NAME_LENGTH <- 25

# Load selected tabs.
custom_tab_files <- list.files('tabs', pattern = "*.R")
custom_tab_environments <- lapply(custom_tab_files, function(file_name) {
  env <- new.env()
  # source(file.path('tabs',file_name), local = env)
  debugSource(file.path('tabs',file_name), local = env)
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

# Custom Functions -----------------------------------------------------------

RemoveItemNumber <- function(factor) {sub("[0-9]+. ","", factor)}

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
  
  # Process PET Configuration File ('pet_config_json') -----------------------
  
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
  
  if(pet_config_present) {
    dvs <- pet_config$drivers[[1]]$designVariables
    design_variable_names <- names(dvs)
    numeric_design_variables <- lapply(dvs,
                                       function(x) {
                                         "RangeMax" %in% names(x)
                                       })
    enumerated_design_variables <- lapply(dvs,
                                          function(x) {
                                            "type" %in% names(x)
                                          })
    dv_types <- unlist(lapply(numeric_design_variables, 
                              function(x) { 
                                if (x)
                                  "Numeric"
                                else
                                  "Enumeration"
                              }))
    dv_selections <- unlist(lapply(dvs, 
                                   function(x) {
                                     if("type" %in% names(x)
                                        && x$type == "enum") 
                                       paste0(unlist(x$items), collapse=",") 
                                     else 
                                       paste0(c(x$RangeMin, x$RangeMax),
                                              collapse=",")
                                   }))
    design_variables <- data.frame(var_name=design_variable_names, Type=dv_types, Selection=dv_selections)
    objective_names <- names(pet_config$drivers[[1]]$objectives)
    num_samples <- unlist(strsplit(as.character(pet_config$drivers[[1]]$details$Code),'='))[2]
    sampling_method <- pet_config$drivers[[1]]$details$DOEType
    generated_configuration_model <- pet_config$GeneratedConfigurationModel
    selected_configurations <- pet_config$SelectedConfigurations
    name <- pet_config$PETName
    mga_name <- pet_config$MgaFilename
    
    # Generate units tables.
    units <- list()
    reverse_units <- list()
    # TODO(tthomas): Clean up the construction of the units list.
    for (i in 1:length(design_variable_names))
    {
      unit <-pet_config$drivers[[1]]$designVariables[[design_variable_names[i]]]$units
      if(is.null(unit) | unit == "") {
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
  
  # Pre-Processing ----------------------------------------------------------
  
  var_names <- names(raw)
  var_class <- sapply(raw, class)
  var_facs <- var_names[var_class == "factor"]
  var_ints <- var_names[var_class == "integer"]
  var_nums <- var_names[var_class == "numeric"]
  var_nums_and_ints <- var_names[var_class == "integer" |
                                 var_class == "numeric"]
  abs_min <- apply(raw[var_nums_and_ints], 2, min, na.rm=TRUE)
  abs_max <- apply(raw[var_nums_and_ints], 2, max, na.rm=TRUE)
  var_range_nums_and_ints <- var_nums_and_ints[(abs_min != abs_max) &
                                               (abs_min != Inf)]
  var_range_facs <- var_facs[apply(raw[var_facs], 2, function(var_fac) {
                                     length(names(table(var_fac))) > 1
                                   })]
  var_range <- c(var_facs, var_nums_and_ints)
  var_constants <- subset(var_names, !(var_names %in% var_range))
  
  AbbreviatedNames <- reactive({
    # TODO(wknight): Write a clear description of this function.
    req(input$dimension)
    abbreviation_length <- as.integer(input$dimension/6/10)
    # print(paste(abbreviation_length))
    # TODO(wknight): Find a way to understand when the window is going to
    # to transition from 12-wide to 4-wide, so we handle the 4-wide case
    # more elegantly. 
    if (abbreviation_length < 10)		
      abbreviation_length <- DEFAULT_NAME_LENGTH		
    
    #print(paste0(windowWidth, ":", abbrevLength))		
    abbreviate(var_names, abbreviation_length)		
  })
  
  # Filters (Enumerations, Sliders) and Constants ----------------------------
  
  # Lets the UI know if a tab has requested the 'Filters' footer.  
  footer_preferences <- lapply(custom_tab_environments,
                               function(custom_env) {custom_env$footer})
  names(footer_preferences) <- lapply(custom_tab_environments,
                                      function(custom_env) {custom_env$title})
  output$display_filters <- reactive({
    display <- (footer_preferences[[input$master_tabset]])
  })
  outputOptions(output, "display_filters", suspendWhenHidden=FALSE)
  
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
          GenerateSliderUI(var_slider, AbbreviatedNames()[var_slider])
        })
      )
    )
  })
  
  # setupToolTip <- function(...){
  #   varsList <- match(varRangeNum(), var_names)
  #   openToolTip <<- openToolTip[1:length(varsList),]
  #   row.names(openToolTip) <<- unlist(strsplit(toString(varsList), ", "))
  #   openToolTip$display <<- F
  #   openToolTip$valApply <<- 0
  # }
  
  # actionButton <- function(inputId, label, btn.style = "" , css.class = "") {
  #   if ( btn.style %in% c("primary","info","success","warning","danger","inverse","link"))
  #     btn.css.class <- paste("btn",btn.style,sep="-")
  #   else btn.css.class = ""
  #   tags$button(id=inputId, type="button", class=paste("btn action-button",btn.css.class,css.class,collapse=" "), label)
  # }

  GenerateEnumUI <- function(current) {
    items <- names(table(raw[[current]]))
    
    for(i in 1:length(items)){
      items[i] <- paste0(i, '. ', items[i])
    }
    
    selected_value <- input[[paste0('filter_', current)]]
    # COMMENT(tthomas): I think sticky filters should be the only option.
    if(is.null(selected_value)) # | !input$stickyFilters)
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
    
    # COMMENT(tthomas): I think sticky filters should be the only option.
    if(is.null(slider_value)) # | !input$stickyFilters)
      # TODO(tthomas): Why are we using 'step' around the already 'stepped' numerics?
      slider_value <- c(signif(slider_min-step*10, digits = 4),
                        signif(slider_max+step*10, digits = 4))
    
    column(2,
           # useShinyjs(),
           # wellPanel(id = paste0("slider_tooltip_", current), 
           #           style = "position: absolute; z-index: 65; box-shadow: 10px 10px 15px grey; width: 20vw; left: 1vw; top: -275%; display: none;",
           #           h4(label),
           #           textInput(paste0("min_input_", current), "Min:"),
           #           textInput(paste0("max_input_", current), "Max:"),
           #           actionButton(paste0("submit_", current), "Apply",
           #                        "success")),
           sliderInput(paste0('filter_', current),
                       label,
                       step = step,
                       min = slider_min,
                       max = slider_max,
                       value = slider_value)
    )
  }
  
  # openSliderToolTip <- function(current) {
  #   toggle(paste0("slider_tooltip_", current))
  #   openToolTip[toString(current), "display"] <<- !openToolTip[toString(current), "display"]
  #   for(i in 1:length(openToolTip[,"display"])){
  #     row = row.names(openToolTip)[i]
  #     if(row != current && openToolTip[row,"display"]){
  #       toggle(paste0("slider_tooltip", row))
  #       openToolTip[row,"display"] <<- F
  #     }
  #   }
  # }
  
  # Slider tooltip handler.
  # observe({
  #   lapply(var_names, function(name) {
  #     if(name %in% varRangeNum()){
  #       current = match(name, var_names)
  #       
  #       onevent("dblclick", paste0("filter_", current), openSliderToolTip(current))
  #       observe({
  #         input$lastkeypresscode
  #         input[[paste0("submit", current)]] 
  #         
  #         isolate({
  #           currentValOfApply <- openToolTip[toString(current), "valApply"]
  #           if(((!is.null(input$lastkeypresscode) && input$lastkeypresscode == 13) || input[[paste0("submit", current)]] != currentValOfApply) && openToolTip[toString(current), "display"]){
  #             if(input[[paste0("submit", current)]] != currentValOfApply) 
  #               openToolTip[toString(current), "valApply"] <<- input[[paste0("submit", current)]]
  #             slider_value = input[[paste0('filter_', current)]]
  #             newMin = input[[paste0("min_inp", current)]]
  #             newMax = input[[paste0("max_inp", current)]]
  #             updateTextInput(session, paste0("min_inp", current), value = "")
  #             updateTextInput(session, paste0("max_inp", current), value = "")
  #             suppressWarnings({ #Suppress warnings from non-numeric inputs
  #               if(!is.null(newMin) && newMin != "" && !is.na(as.numeric(newMin)))
  #                 slider_value = as.numeric(c(newMin, slider_value[2]))
  #               if(!is.null(newMax) && newMax != "" && !is.na(as.numeric(newMax)))
  #                 slider_value = as.numeric(c(slider_value[1], newMax))
  #             })
  #             updateSliderInput(session, paste0('filter_', current), value = slider_value)
  #             toggle(paste0("slider_tooltip", current))
  #             openToolTip[toString(current), "display"] <<- F
  #           }
  #         })
  #       })
  #     }
  #   })
  # })
  
  output$constants <- renderUI({
    # print("In render constants")
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
  
  # observeEvent(input$resetSliders, {
  #   # print("In resetDefaultSliders()")
  #   for(column in 1:length(var_names)){
  #     switch(varClass[column],
  #            "numeric" = 
  #            {
  #              max <- as.numeric(unname(rawAbsMax()[var_names[column]]))
  #              min <- as.numeric(unname(rawAbsMin()[var_names[column]]))
  #              diff <- (max-min)
  #              if (diff != 0) {
  #                step <- max(diff*0.01, abs(min)*0.001, abs(max)*0.001)
  #                updateSliderInput(session, paste0('filter_', column), value = c(signif(min-step*10, digits = 4), signif(max+step*10, digits = 4)))
  #              }
  #            },
  #            "integer" = 
  #            {
  #              max <- as.integer(unname(rawAbsMax()[var_names[column]]))
  #              min <- as.integer(unname(rawAbsMin()[var_names[column]]))
  #              if(min != max) {
  #                updateSliderInput(session, paste0('filter_', column), value = c(min, max))
  #              }
  #            },
  #            "factor"  = updateSelectInput(session, paste0('filter_', column), selected = names(table(raw_plus()[var_names[column]])))
  #     )
  #   }
  # })
  
  # Data processing ----------------------------------------------------------
    
  FilteredData <- reactive({
    # This reactive holds the full dataset that has been filtered using the
    # values of the sliders.
    data <- raw
    for(index in 1:length(var_names)) {
      name <- var_names[index]
      input_name <- paste("filter_", name, sep="")
      selection <- input[[input_name]]
      if(length(selection) != 0) {
        if(name %in% var_nums_and_ints) {
          isolate({
            above <- (data[[name]] >= selection[1])
            below <- (data[[name]] <= selection[2])
            in_range <- above & below
          })
        }
        else if (name %in% var_facs) {
            selection <- unlist(lapply(selection, function(factor){
                                                    RemoveItemNumber(factor)
                                                  }))
            inRange <- (data[[name]] %in% selection)
        }
        
        # Don't filter based on missing values.
        inRange <- inRange | is.na(data[[name]])
        
        data <- subset(data, inRange)
      }
    }
    print("Data Filtered.")
    data
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
  
  # Final Processing ---------------------------------------------------------
  
  # Build the 'data' list that is shared between all tabs.
  data <- list()
  data$raw <- raw
  data$Filtered <- FilteredData
  data$Filters <- Filters
  
  # Build variables metadata list.
  variables <- lapply(var_names, function(var_name) {
    if (var_name %in% design_variable_names)
      type <- "Design Variable"
    else
      type <- "Objective"
    list(name = var_name,
         name_with_units = AddUnits(var_name),
         type = type
    )
  })
  names(variables) <- var_names
  
  pet <- list(sampling_method=sampling_method,
              num_samples=num_samples,
              name=name,
              mga_name=mga_name,
              generated_configuration_model=generated_configuration_model,
              selected_configurations=selected_configurations,
              design_variable_names=design_variable_names)
  
  preprocessing <- list(var_names=var_names,
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
                        var_constants=var_constants)
  
  # Build the 'meta' list.
  data$meta <- list(variables=variables,
                    pet=pet,
                    preprocessing=preprocessing)
  
  data$experimental <- list()
  
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
                      id = "master_tabset")

# Defines the UI of the Visualizer.
ui <- fluidPage(
  tags$head(
    # COMMENT(tthomas): Can we tighten this down?
    tags$script(    
      'var dimension = 0;    
      $(document).on("shiny:connected", function(e) {   
      dimension = window.innerWidth;   
      Shiny.onInputChange("dimension", dimension);    
      });   
      $(window).resize(function(e) {    
      dimension = window.innerWidth;   
      Shiny.onInputChange("dimension", dimension);    
      });
    ')  
  ),
  
  titlePanel("Visualizer"),
  
  # Generates the master tabset from the user-defined tabs provided.
  do.call(tabsetPanel, tabset_arguments),
  
  # Optional Filters footer.
  conditionalPanel("output.display_filters",
    hr(),
    h3("Filter Data:"),
    wellPanel(
      tags$div(title = "Activate to show filters for all dataset variables.",
               checkboxInput("viewAllFilters", "View All Filters", value = TRUE)),
      tags$div(title = "Return visible sliders to default state.",
               actionButton("resetSliders", "Reset Visible Filters")),
      hr(),
      uiOutput("filters")
    ),
    conditionalPanel("output.constants_present",
      # bootstrapPage(tags$script('
      #   $(document).on("keydown", function (e) {
      #   Shiny.onInputChange("lastkeypresscode", e.keyCode);
      #   });
      # ')),
      h3("Constants:"),
      uiOutput("constants")
    )
  )
)

# Start the Shiny app.
shinyApp(ui = ui, server = Server)