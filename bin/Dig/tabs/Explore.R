require(jpeg)
require(png)

title <- "Explore"
footer <- TRUE

plot_markers <- c("Square"=0,
                  "Circle"=1,
                  "Triangle Point Up"=2,
                  "Plus"=3,
                  "Cross"=4,
                  "Diamond"=5,
                  "Triangle Point Down"=6,
                  "Square Cross"=7,
                  "Star"=8,
                  "Diamond Plus"=9,
                  "Circle Plus"=10,
                  "Triangles Up And Down"=11,
                  "Square Plus"=12,
                  "Circle Cross"=13,
                  "Square And Triangle Down"=14,
                  "Filled Square"=15,
                  "Filled Circle"=16,
                  "Filled Triangle Point Up"=17,
                  "Filled Diamond"=18,
                  "Solid Circle"=19,
                  "Bullet (Smaller Circle)"=20)  #,
                  # "Filled Circle Red"=21,
                  # "Filled Square Red"=22,
                  # "Filled Diamond Red"=23,
                  # "Filled Triangle Point Up Red"=24,
                  # "Filled Triangle Point Down Red"=25)

ui <- function(id) {
  ns <- NS(id)
  
  fluidPage(
  	br(),
    tabsetPanel(
      tabPanel("Pairs Plot",
        fluidRow(
          column(3,
            br(),
            # TODO(tthomas): Fix restore.. OPENMETA-
            # bsCollapse(id = ns("pairs_plot_collapse"), open = si(ns("pairs_plot_collapse"), "Variables"),
            bsCollapse(id = ns("pairs_plot_collapse"), open = "Variables",
              bsCollapsePanel("Variables",
                selectInput(ns("display"), "Display Variables:", c(),
                            multiple=TRUE),
                conditionalPanel(
                  condition = paste0('input["', ns('auto_render'), '"] == false'),
                  actionButton(ns("render_plot"), "Render Plot"))
              ),
              # TODO(tthomas): Add this functionality back in.
              # h4("Download"),
              # downloadButton(ns('exportData'), 'Dataset'), 
              # paste("          "),
              # downloadButton(ns('exportPlot'), 'Plot'), hr(),
              # actionButton(ns("resetOptions"), "Reset to Default Options")
              bsCollapsePanel("Plot Options",
                checkboxInput(ns("auto_render"), "Render Automatically",
                              value = si(ns("auto_render"), TRUE)),
                checkboxInput(ns("pairs_upper_panel"), "Display Upper Panel",
                              value = si(ns("pairs_upper_panel"), FALSE)),
                checkboxInput(ns("pairs_trendlines"), "Add Trendlines",
                              value = si(ns("pairs_trendlines"), FALSE)),
                checkboxInput(ns("pairs_units"), "Display Units",
                              value = si(ns("pairs_units"), TRUE))
              ),
              bsCollapsePanel("Markers",
                selectInput(ns("pairs_plot_marker"),
                            "Plot Markers:",
                            plot_markers,
                            selected=si(ns("pairs_plot_marker"), 1)),
                sliderInput(ns("pairs_plot_marker_size"), "Marker Size:",
                            min=0.5, max=2.5,
                            value=si(ns("pairs_plot_marker_size"),1),
                            step=0.025)
              )
            ),
            hr(),
            h4("Info"),
            verbatimTextOutput(ns("pairs_stats"))#,
          ),
          column(9,
              htmlOutput(ns("pairs_display_error")),   
              htmlOutput(ns("pairs_filter_error")), 
              plotOutput(ns("pairs_plot"), dblclick = ns("pairs_click"), height = 700)
          )
        )
      ), 
      tabPanel("Single Plot",
        fluidRow(
          column(3,
            br(),
            # actionButton(ns("single_back_pairs"), "Back"),
            # br(), br(),
            bsCollapse(id = ns("single_plot_collapse"), open = si(ns("single_plot_collapse"), "Variables"),
              bsCollapsePanel("Variables", 
                selectInput(ns("x_input"), "X-axis", c(), selected=NULL),
                selectInput(ns("y_input"), "Y-Axis", c(), selected=NULL),
                style = "default"),
              bsCollapsePanel("Markers",
                selectInput(ns("single_plot_marker"),
                            "Plot Markers:",
                            plot_markers,
                            selected = si(ns("single_plot_marker"), 1)),
                sliderInput(ns("single_plot_marker_size"), "Marker Size:",
                            min=0.5, max=2.5, value=si(ns("single_plot_marker_size"), 1), step=0.025),
                style = "default"),
              bsCollapsePanel("Filter", 
                p(strong("Adjust Sliders to Selection")),
                actionButton(ns("update_x"), "X"),
                actionButton(ns("update_y"), "Y"),
                actionButton(ns("update_both"), "Both"),
                style = "default"),
              # TODO(wknight): Restore this functionality.
              # br(), br(),
              # p(strong("Highlight Selection")),
              # bootstrapPage(
              #   actionButton(ns("highlightData"), "Highlight Selection", class = "btn btn-primary")
              # )
              bsCollapsePanel("Overlays",
                checkboxInput(ns("add_regression"), "Add Regression", si(ns("add_regression"), FALSE)),
                selectInput(ns("regression_type"), "Regression Type", c("Linear", "Quadratic", "Exponential"), selected=si(ns("regression_type"), "Linear")),
                checkboxInput(ns("add_contour"), "Add Contour Plot", si(ns("add_contour"), FALSE)),
                selectInput(ns("contour_var"), "Contour Variable", c(), selected=NULL),
                checkboxInput(ns("add_pareto"), "Add Pareto Plot", si(ns("add_pareto"), FALSE)),
                style = "default")
            )
          ),
          column(9,
            htmlOutput(ns("single_filter_error")),
            plotOutput(ns("single_plot"), dblclick = ns("plot_dblclick"), click = ns("plot_click"), brush = ns("plot_brush"), height=700)
          ),
          column(12,
            verbatimTextOutput(ns("single_info"))
          )
        )
      ),
      tabPanel("Point Details",
        htmlOutput(ns("guids_error")), 
        conditionalPanel(
          condition = paste0('output["', ns('guids_present'), '"] == true'),
          fluidRow(
            column(12,
              br(),
              selectInput(ns("details_guid"), label = "GUID", choices = c(), NULL),
              verbatimTextOutput(ns("point_details"))
            )
          )
        ),
        conditionalPanel(
          condition = paste0('output["', ns('found_images'), '"] == true'),
          hr(),
          fluidRow(
            column(3, h4("Images"),
              selectInput(ns("file_images"), NULL, c(), NULL),
              wellPanel(
                uiOutput(ns("image_info")),
                p("Click on the left or right of the displayed image to cycle through available images.")
              )
            ),
            column(9, br(),
              tags$div(imageOutput(ns("image"), click = ns("image_click")), style="text-align: center;")
            )
          )
        ),
        conditionalPanel(
          condition = paste0('output["', ns('found_simdis'), '"] == true'),
          hr(),
          fluidRow(
            column(12, h4("SIMDIS")),
            column(3, selectInput(ns("file_simdis"), NULL, c(), NULL)),
            column(3, actionButton(ns("launch_simdis"), "Launch in SIMDIS"))
          )
        )
      ),
      id = ns("tabset"),
      selected = if (!is.null(si_read(ns("tabset"))) && si_read(ns("tabset")) == "Point Details") "Single Plot" else si(ns("tabset"), NULL) #COMMENT(tthomas): Avoid bug with 'Point Details' tab being selected on launch.
    )
  )
}

server <- function(input, output, session, data) {
  ns <- session$ns
  
  observe({
    selected <- isolate(input$display)
    if(is.null(selected)) {
      selected <- data$pre$var_range()[c(1,2)]
    }
    saved <- si_read(ns("display"))
    if (is.empty(saved)) {
      si_clear(ns("display"))
    } else if (all(saved %in% c(data$pre$var_range(), ""))) {
      selected <- si(ns("display"), NULL)
    }
    updateSelectInput(session,
                      "display",
                      choices = data$pre$var_range_list(),
                      selected = selected)
  })

  output$pairs_plot <- renderPlot({
    
    if (length(input$display) >= 2 & nrow(data$Filtered()) > 0) {
      # Clear the error messages, if any.
      output$pairs_display_error <- renderUI(tagList(""))
      output$pairs_filter_error <- renderUI(tagList(""))
      
      # pairs_setup()
      if(input$pairs_upper_panel) {
        if(input$pairs_trendlines) {
          params <- list(upper.panel=panel.smooth, lower.panel=panel.smooth)
        }
        else {
          params <- list()
        }
      }
      else {
        if(input$pairs_trendlines) {
          params <- list(lower.panel = panel.smooth, upper.panel = NULL)
        }
        else {
          params <- list(upper.panel = NULL)
        }
      }
      pairs_data <- data$Colored()[vars_list()]
      if(input$pairs_units) {
        names(pairs_data) <- sapply(names(pairs_data), function(name) {
          data$meta$variables[[name]]$name_with_units
        })
      }
      params <- c(params,
                  list(x = pairs_data,
                       col = data$Colored()$color,
                       pch = as.numeric(input$pairs_plot_marker),
                       cex = as.numeric(input$pairs_plot_marker_size)))
      do.call(pairs, params)
    }
    else {
      if (length(input$display) < 2) {
        output$pairs_display_error <- renderUI(
          tagList(br(), "Please select two or more Display Variables.")
        )
      }
      if (nrow(data$Filtered()) == 0) {
        output$pairs_filter_error <- renderUI(
          tagList(br(), "No data points fit the current filtering scheme.")
        )
      }
    }
  })
  
  vars_list <- reactive({
    idx <- NULL
    for(choice in 1:length(input$display)) {
      mm <- match(input$display[choice],names(data$raw$df))
      if(mm > 0) { idx <- c(idx,mm) }
    }
    idx
  })
  
  output$pairs_stats <- renderText({
    # print("In render stats")
    if(nrow(data$Filtered()) > 0) {
      table <- paste0("Total Points: ", nrow(data$raw$df),
                      "\nCurrent Points: ", nrow(data$Filtered()))
    }
    else {
      table <- "No data points fit the filtering scheme."
    }
    table
  })
  
  # TODO(wknight): Can we make this a little less hardcoded? Are there any
  #   libraries that already do this selection from a plot?
  # Change to single plot when user clicks a plot on pairs matrix.
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
        updateSelectInput(session, "x_input", selected = x_var)
        updateSelectInput(session, "y_input", selected = y_var)
        updateTabsetPanel(session, "tabset", selected = "Single Plot")
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
  
  # Single Plot Tab ----------------------------------------------------------

  observe({
    selected <- isolate(input$x_input)
    if(is.null(selected) || selected == "") {
      selected <- data$pre$var_range()[1]
    }
    saved <- si_read(ns("x_input"))
    if (is.empty(saved)) {
      si_clear(ns("x_input"))
    } else if (saved %in% c(data$pre$var_range(), "")) {
      selected <- si(ns("x_input"), NULL)
    }
    updateSelectInput(session,
                      "x_input",
                      choices = data$pre$var_range_list(),
                      selected = selected)
  })

  observe({
    selected <- isolate(input$y_input)
    if(is.null(selected) || selected == "") {
      selected <- data$pre$var_range()[2]
    }
    saved <- si_read(ns("y_input"))
    if (is.empty(saved)) {
      si_clear(ns("y_input"))
    } else if (saved %in% c(data$pre$var_range(), "")) {
      selected <- si(ns("y_input"), NULL)
    }
    updateSelectInput(session,
                      "y_input",
                      choices = data$pre$var_range_list(),
                      selected = selected)
  })
     
  observe({
    selected <- isolate(input$contour_var)
    if(is.null(selected) || selected == "") {
      selected <- data$pre$var_range_nums_and_ints()[1]
    }
    saved <- si_read(ns("contour_var"))
    if (is.empty(saved)) {
      si_clear(ns("contour_var"))
    } else if (saved %in% c(data$pre$var_range_nums_and_ints(), "")) {
      selected <- si(ns("contour_var"), NULL)
    }
    updateSelectInput(session,
                      "contour_var",
                      choices = data$pre$var_range_nums_and_ints_list(),
                      selected = selected)
  })
     
  observeEvent(input$single_back_pairs, {
    updateTabsetPanel(session, "tabset", selected = "Pairs Plot")
  })
  
  output$single_plot <- renderPlot(SinglePlot())
  
  SinglePlot <- reactive({
    req(input$x_input, input$y_input)
    if(nrow(data$Filtered()) == 0) {
      output$single_filter_error <- renderUI(
        tagList(br(), "No data points fit the current filtering scheme.")
      )
      NULL
    } else
    {
      output$single_filter_error <- renderUI(tagList(""))
      
      x_data <- data$Filtered()[[paste(input$x_input)]]
      y_data <- data$Filtered()[[paste(input$y_input)]]
      params <- list(x = x_data,
                     y = y_data,
                     xlab = paste(data$meta$variables[[input$x_input]]$name_with_units),
                     ylab = paste(data$meta$variables[[input$y_input]]$name_with_units),
                     pch = as.numeric(input$single_plot_marker),
                     cex = as.numeric(input$single_plot_marker_size))#,
                     # pch = as.numeric(input$pointStyle))
      if(data$pre$var_class()[input$x_input] != 'factor') {
        params <- c(params, list(col = data$Colored()$color))
      }
      do.call(plot, params)
      
      if(input$add_pareto) {
        # lines()
        print("Added Pareto")
      }
      if(input$add_regression) {
        fit_data <- data$Filtered()
        switch(input$regression_type,
          "Linear" = {
            print("Added Linear Regression")
            fit <- lm(formula = paste(input$y_input, "~", input$x_input), data=fit_data)
            print(cor(fit_data[[input$x_input]], fit_data[[input$y_input]]))
            abline(fit, col="darkblue")
            function_text <- paste0(input$y_input, " = ",
                                    format(fit$coefficients[[input$x_input]], digits=4), "*", input$x_input,
                                    " + ", format(fit$coefficients[["(Intercept)"]], digits=4))
          },
          "Quadratic" = {
            fit_data[["Square"]] <- fit_data[[input$x_input]]^2
            fit <- lm(formula = paste0(input$y_input, "~", input$x_input, "+Square"), data=fit_data)
            print(summary(fit))
            x_vals <- seq(min(fit_data[[input$x_input]]),max(fit_data[[input$x_input]]),length.out=100)
            predict_input <- list()
            predict_input[[input$x_input]] <- x_vals
            predict_input[["Square"]] <- x_vals^2
            y_vals <- predict(fit, predict_input)
            lines(x_vals, y_vals, col="darkblue")
            function_text <- paste0(input$y_input, " = ",
                                    format(fit$coefficients[["Square"]], digits=4), "*", input$x_input, "^2", " + ",
                                    format(fit$coefficients[[input$x_input]], digits=4), "*", input$x_input, " + ",
                                    format(fit$coefficients[["(Intercept)"]], digits=4))
          },
          "Exponential" = {
            x_vals <- seq(min(fit_data[[input$x_input]]),max(fit_data[[input$x_input]]),length.out=100)
            fit_data <- fit_data[fit_data[[input$y_input]] > 0, ]
            fit <- lm(formula = paste0("log(", input$y_input, ")~", input$x_input), data=fit_data)
            print(summary(fit))
            predict_input <- list()
            predict_input[[input$x_input]] <- x_vals
            y_vals <- exp(predict(fit, predict_input))
            lines(x_vals, y_vals, col="darkblue")
            function_text <- paste0(input$y_input, " = e^(",
                                    format(fit$coefficients[[input$x_input]], digits=4), "*", input$x_input, " + ",
                                    format(fit$coefficients[["(Intercept)"]], digits=4), ")")
          }
        )
        legend("topleft", bty="n", legend=c(paste(input$regression_type, "Regression"),
                                            paste("Adjusted R-squared:",format(summary(fit)$adj.r.squared, digits=4)),
                                            paste("Function:", function_text)))
      }
      if(input$add_contour &&
         !(input$contour_var %in% c(input$x_input, input$y_input))) {
        data.loess <- loess(paste0(input$contour_var, "~",
                                   input$x_input, "*",
                                   input$y_input),
                            data = data$Filtered())
        x_grid <- seq(min(x_data),
                      max(x_data),
        			        (max(x_data)-min(x_data))/50)
        y_grid <- seq(min(y_data),
                      max(y_data),
        			        (max(y_data)-min(y_data))/50)
        data.fit <- expand.grid(x = x_grid, y = y_grid)
        colnames(data.fit) <- c(paste(input$x_input), paste(input$y_input))
        my.matrix <- predict(data.loess, newdata = data.fit)
        # filled.contour(x = x_grid, y = y_grid, z = my.matrix, add = TRUE, color.palette = terrain.colors)
        contour(x = x_grid, y = y_grid, z = my.matrix, add = TRUE,
                col="darkblue", labcex=1.35, lwd = 1.5, method="edge")
      }
    }
  })
  
  output$single_info <- renderPrint({
    near_points <- nearPoints(data$Filtered(),
                              input$plot_click,
                              xvar = input$x_input,
                              yvar = input$y_input,
                              maxpoints = 8)
    names(near_points) <- sapply(names(near_points),
      function(name) {data$meta$variables[[name]]$name_with_units})
    t(near_points)
  })
  
  # Point Details -----------------------------------------------------

  observe({
    selected <- isolate(input$details_guid)
    choices <- as.character(data$raw$df$GUID)
    if(is.null(selected) || selected == "") {
      selected <- choices[1]
    }
    saved <- si_read(ns("details_guid"))
    if (is.empty(saved)) {
      si_clear(ns("details_guid"))
    } else if (saved %in% c(choices, "")) {
      selected <- si(ns("details_guid"), NULL)
    }
    updateSelectInput(session,
                      "details_guid",
                      choices = choices,
                      selected = selected)
  })
  
  observe({
    pts <- nearPoints(data$Filtered(),
                      input$plot_dblclick,
                      xvar = input$x_input,
                      yvar = input$y_input,
                      maxpoints = 1)
    if(nrow(pts) != 0) {
      guid <- as.character(unlist(pts[["GUID"]]))
      updateTabsetPanel(session, "tabset",
                        selected = "Point Details")
      updateSelectInput(session, "details_guid", selected = guid)
    }
  })
  
  output$point_details <- renderPrint({
    req(input$details_guid)
    single_point <- data$raw$df[data$raw$df$GUID == input$details_guid, ]
    row.names(single_point) <- ""
    names(single_point) <- sapply(names(single_point), function(name) {
      data$meta$variables[[name]]$name_with_units
    })
    t(single_point[!(names(single_point) == "GUID")])
  })
  
  output$guids_present <- reactive({
    "GUID" %in% names(data$raw$df) && length(data$raw$df$GUID) > 0
  })
  outputOptions(output, "guids_present", suspendWhenHidden=FALSE)
  
  
  observe({
    if ("GUID" %in% names(data$raw$df) && length(data$raw$df$GUID) > 0) {
      output$guids_error <- renderUI(tagList(""))
    } else {
      output$guids_error <- renderUI(
        tagList(br(), "No GUIDs found in this dataset.")
      )
    }
  })
  
  output$found_simdis <- reactive({
    guid_folder <- guid_folders[[input$details_guid]]
    if(!is.null(guid_folder) &&
       "simdis.zip" %in% tolower(list.files(guid_folder))) {
      choices <- unzip(file.path(guid_folder, "simdis.zip"), list = TRUE)$Name
      choices <- choices[grepl(".asi$", choices) | grepl(".spy$", choices)]
      selected <- isolate(input$file_simdis)
      if(!(selected %in% choices)) {
        selected <- choices[1]
      }
      updateSelectInput(session, "file_simdis",
                        choices = choices,
                        selected = selected)
      TRUE
    } else {
      FALSE
    }
  })
  outputOptions(output, "found_simdis", suspendWhenHidden=FALSE)
  
  observeEvent(input$launch_simdis, {
    print(paste0("Launching Simdis on ", input$details_guid, "..."))
    
    # Extract Zip
    unzip(file.path(guid_folders[[input$details_guid]], "SIMDIS.zip"),
          exdir = tempdir())
    
    # Execute SIMDIS
    asi_filename <- file.path(tempdir(), input$file_simdis, fsep="\\")
    print(paste0("Calling 'simdis ", asi_filename, "'..."))
    system2("simdis",
            args = c(paste0("\"",asi_filename,"\"")),
            stdout = file.path(launch_dir,
                               "VisualizerRunSimdis_stdout.log"),
            stderr = file.path(launch_dir,
                               "VisualizerRunSimdis_stderr.log"),
            wait = FALSE)
  })
  
  output$found_images <- reactive({
    guid_folder <- guid_folders[[input$details_guid]]
    if(!is.null(guid_folder) &&
       "images.zip" %in% tolower(list.files(guid_folder))) {
      choices <- unzip(file.path(guid_folder, "images.zip"), list = TRUE)$Name
      choices <- choices[grepl(".png$", choices) | grepl(".jpg$", choices)]
      selected <- isolate(input$file_images)
      if(!(selected %in% choices)) {
        selected <- choices[1]
      }
      unzip(file.path(guid_folders[[input$details_guid]], "images.zip"),
            exdir = tempdir(), overwrite = TRUE)
      updateSelectInput(session, "file_images",
                        choices = choices,
                        selected = selected)
      TRUE
    } else {
      FALSE
    }
  })
  outputOptions(output, "found_images", suspendWhenHidden=FALSE)
  
  output$image <- renderImage({
    req(input$file_images, input$details_guid)
    path <- file.path(tempdir(), input$file_images, fsep="\\")
    max_width  <- session$clientData[[paste0('output_', ns("image"), "_width")]]
    max_height <- session$clientData[[paste0('output_', ns("image"), "_height")]]
    if (grepl(".png$", input$file_images)) {
      type <- "image/png"
      dims <- dim(png::readPNG(path))
    } else {
      type <- "image/jpg"
      dims <- dim(jpeg::readJPEG(path))
    }
    # print(paste("AspectRatioFrame:", max_width/max_height, "ARSource:", dims[2]/dims[1]))
    if(max_width/max_height>dims[2]/dims[1]) {
      list(src = path,
           contentType = "image/png",
           height = max_height,
           align = "center")
    } else {
      list(src = path,
           contentType = "image/png",
           width = max_width)
    }
    
  }, deleteFile = FALSE)
  
  observe({
    req(input$file_images, guid_folders, input$details_guid)
    choices <- unzip(file.path(guid_folders[[input$details_guid]], "images.zip"), list = TRUE)$Name
    message <- paste0("Image ", match(input$file_images, choices),
                      " of ", length(choices))
    output$image_info <- renderUI({tagList(p(message), br())})
  })
  
  observeEvent(input$image_click, {
    req(input$image_click, input$details_guid)
    guid_folder <- guid_folders[[input$details_guid]]
    choices <- unzip(file.path(guid_folder, "images.zip"), list = TRUE)$Name
    num_selected <- match(isolate(input$file_images), choices)
    center  <- session$clientData[[paste0('output_', ns("image"), "_width")]]/2
    if (input$image_click$x < center) {
      num_selected <- num_selected - 1
    } else {
      num_selected <- num_selected + 1
    }
    num_selected <- ((num_selected + length(choices) - 1) %% length(choices)) + 1
    updateSelectInput(session, "file_images",
                      choices = choices,
                      selected = choices[num_selected])
  })
}