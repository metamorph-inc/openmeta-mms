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
            wellPanel(
              h4("Plot Options"),
              selectInput(ns("display"),
                          "Display Variables:",
                          c(),
                          # selected=si(ns("display"), NULL),
                          multiple=TRUE),
              conditionalPanel(
                condition = "input.auto_render == false",
                actionButton(ns("render_plot"), "Render Plot"),
                br()
              ),
              hr(),
              selectInput(ns("pairs_plot_marker"),
                          "Plot Markers:",
                          plot_markers,
                          selected=si(ns("pairs_plot_marker"), 1)),
              sliderInput(ns("pairs_plot_marker_size"), "Marker Size:",
                          min=0.5, max=2.5,
                          value=si(ns("pairs_plot_marker_size"),1),
                          step=0.025),
              hr(),
              h4("Info"),
              verbatimTextOutput(ns("pairs_stats"))#,
              # TODO(tthomas): Add this functionality back in.
              # h4("Download"),
              # downloadButton(ns('exportData'), 'Dataset'), 
              # paste("          "),
              # downloadButton(ns('exportPlot'), 'Plot'), hr(),
              # actionButton(ns("resetOptions"), "Reset to Default Options")
            )
          ),
          column(9,
              uiOutput(ns("pairs_display_error")),   
              uiOutput(ns("pairs_filter_error")), 
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
                checkboxInput(ns("add_pareto"), "Add Pareto Plot", si(ns("add_pareto"), FALSE)),
                style = "default")
            )
          ),
          column(9,
            plotOutput(ns("single_plot"), click = ns("plot_click"), brush = ns("plot_brush"), height=700)
          ),
          column(12,
            verbatimTextOutput(ns("single_info"))
          )
        )
      ),
      id = ns("explore_tabset"),
      selected = si(ns("explore_tabset"), NULL)
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
      si(ns("display"), NULL)
    } else if (all(saved %in% c(data$pre$var_range(), ""))) {
      selected <- si(ns("display"), NULL)
    }
    updateSelectInput(session,
                      "display",
                      choices = data$pre$var_range_list(),
                      selected = selected)
  })

  observe({
    selected <- isolate(input$x_input)
    if(is.null(selected) || selected == "") {
      selected <- data$pre$var_range()[1]
    }
    saved <- si_read(ns("x_input"))
    if (is.empty(saved)) {
      si(ns("x_input"), NULL)
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
      si(ns("y_input"), NULL)
    } else if (saved %in% c(data$pre$var_range(), "")) {
      selected <- si(ns("y_input"), NULL)
    }
    updateSelectInput(session,
                      "y_input",
                      choices = data$pre$var_range_list(),
                      selected = selected)
  })
     
  # Pairs Plot Tab -----------------------------------------------------------
  # TODO(tthomas): Restore the ability to have trendlines
  # pairsSetup <- function(...){
  #   if(input$pairs_upper_panel) {
  #     if(input$pairs_trendlines) {
  #       p <- pairs(pairs_data()[pairs_vars()], lower.panel = panel.smooth, upper.panel = panel.smooth, col = pairs_data()$color, pch = as.numeric(input$pointStyle), cex = as.numeric(input$pointSize))
  #     }
  #     else {
  #       p <- pairs(pairs_data()[pairs_vars()], col = pairs_data()$color, pch = as.numeric(input$pointStyle), cex = as.numeric(input$pointSize))
  #     }
  #   }
  #   else {
  #     if(input$trendLines) {
  #       p <- pairs(pairs_data()[pairs_vars()], lower.panel = panel.smooth, upper.panel = NULL, col = pairs_data()$color, pch = as.numeric(input$pointStyle), cex = as.numeric(input$pointSize))
  #     }
  #     else {
  #       p <- pairs(pairs_data()[pairs_vars()], upper.panel = NULL, col = pairs_data()$color, pch = as.numeric(input$pointStyle), cex = as.numeric(input$pointSize))
  #     }
  #   }
  #   p
  # }
  
  output$pairs_plot <- renderPlot({
    
    # Clear the error messages, if any.
    output$display_error <- renderText("")
    output$filter_error <- renderText("")
    
    if (length(input$display) >= 2 & nrow(data$Filtered()) > 0) {
      # pairs_setup()
      pairs(data$Colored()[vars_list()],
            upper.panel = NULL,
            col = data$Colored()$color,
            pch = as.numeric(input$pairs_plot_marker),
            cex = as.numeric(input$pairs_plot_marker_size))
    }
    else { 
      if (nrow(data$Colored()) == 0) {
        output$pairs_filter_error <- renderText(
            "<br/>No data points fit the current filtering scheme.")
      }
      if (length(input$display) < 2) {
        output$paris_display_error <- renderText(
          "<br/>Please select two or more Display Variables.")
      }
    }
  })
  
  vars_list <- reactive({
    idx <- NULL
    for(choice in 1:length(input$display)) {
      mm <- match(input$display[choice],data$pre$var_names())
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
        updateTabsetPanel(session, "explore_tabset", selected = "Single Plot")
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

  observeEvent(input$single_back_pairs, {
    updateTabsetPanel(session, "explore_tabset", selected = "Pairs Plot")
  })
  
  output$single_plot <- renderPlot(SinglePlot())
  
  SinglePlot <- reactive({
    req(input$x_input)
    if(data$pre$var_class()[input$x_input] == 'factor') {
      plot(data$Filtered()[[paste(input$x_input)]],
           data$Filtered()[[paste(input$y_input)]],
           xlab = paste(input$x_input),
           ylab = paste(input$y_input),
           pch = as.numeric(input$single_plot_marker),
           cex = as.numeric(input$single_plot_marker_size))#,
           # pch = as.numeric(input$pointStyle))
    } else {
      plot(data$Filtered()[[paste(input$x_input)]],
           data$Filtered()[[paste(input$y_input)]],
           xlab = paste(input$x_input),
           ylab = paste(input$y_input),
           col = data$Colored()$color,
           pch = as.numeric(input$single_plot_marker),
           cex = as.numeric(input$single_plot_marker_size))#,
           # pch = as.numeric(input$pointStyle))
    }
    if(input$add_pareto) {
      # lines()
      print("Added Pareto")
    }
  })
  
  output$single_info <- renderPrint({
    t(nearPoints(data$Filtered(),
                 input$plot_click,
                 xvar = input$x_input,
                 yvar = input$y_input,
                 maxpoints = 8))
  })
  
}