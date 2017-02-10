
title <- "Parallel Axis Plot"

ui <- function() {

  fluidPage(
    br(),
    
    wellPanel(
      
      h3("Parallel Coordinates Plot"),
      actionButton("refresh", "Refresh"),
      br(),
      
      ############## D3 ###############
      #to style to d3 output pull in css
      tags$head(tags$link(rel = "stylesheet", type = "text/css", href = "parallelAxisPlotStyle.css")),
      #load D3JS library
      tags$script(src="https://d3js.org/d3.v3.min.js"),
      #load javascript
      tags$script(src="parallelAxisPlotScript.js"),
      #create div referring to div in the d3script
      tags$div(id="div_parallel_axis_plot")
      #create div referring to div in the d3script
      ##################################
    )
  )
}

server <- function(input, output, session, raw_data, raw_info) {

  output$text <- renderText("raw_data")

  row.names(raw_data) <- NULL
  d3df <- apply(raw_data, 1, function(row) as.list(row[!is.na(row)]))
  
  observe({
    input$dimension #Causes d3 object to reflect current window size
    input$refresh   #Causes d3 object to re-render when button is clicked
    
    #This line sends the current raw_data to the d3 process.
    isolate(session$sendCustomMessage(type="dataframe", d3df))
  })
  
}