
title <- "Parallel Coordinates Plot"

ui <- function() {

  fluidPage(
    br(),
    
    wellPanel(
      
      h3("Parallel Coordinates Plot"),
      actionButton("refresh", "Refresh"),
      br(),
      
      ############## D3 ###############
      #to style to d3 output pull in css
      tags$head(tags$link(rel = "stylesheet", type = "text/css", href = "style.css")),
      #load D3JS library
      tags$script(src="https://d3js.org/d3.v3.min.js"),
      #load javascript
      tags$script(src="script.js"),
      #create div referring to div in the d3script
      tags$div(id="div_parallel_coords")
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
    input$dimension
    input$refresh
    isolate(session$sendCustomMessage(type="dataframe", d3df))
  })

  varNames <- names(raw_data)
  varClass <- sapply(raw_data,class)
  varNums <- varNames[varClass != "factor"]
  varFacs <- varNames[varClass == "factor"]
  rawAbsMin <- apply(raw_data[varNums], 2, min, na.rm=TRUE)
  rawAbsMax <- apply(raw_data[varNums], 2, max, na.rm=TRUE)
  
}