
title <- "d3 Visualization - Parallel Axis Plot"

ui <- function() {

  fluidPage(
    br(),
    wellPanel(
      
      # tags$div(
      #   HTML("
      #       <head>
      #       <link rel='stylesheet' type='text/css' href='shared/shiny.css'/>
      #       <script src=\"http://d3js.org/d3.v3.min.js\" charset=\"utf-8\"></script>
      #       <link rel='stylesheet' href='style.css'/>
      #       </head>
      #       <body>
      #       <script type='text/javascript' src='script.js'></script>
      #       </body>
      #   ")
      # )


      #to style to d3 output pull in css
      tags$head(tags$link(rel = "stylesheet", type = "text/css", href = "style.css")),
      #load D3JS library
      tags$script(src="https://d3js.org/d3.v3.min.js"),
      #load javascript
      tags$script(src="script.js"),
      #create div referring to div in the d3script
      tags$div(id="div_tree")
      #create div referring to div in the d3script
    )
  )
}

server <- function(input, output, session, raw_data, raw_info) {

  output$text <- renderText("raw_data")
  
  df <- raw_data
  row.names(df) <- NULL
  d3df <- apply(df, 1, function(row) as.list(row[!is.na(row)]))
  

  csv_path <- normalizePath(Sys.getenv("DIG_INPUT_CSV"))
  
  print(csv_path)
  
  session$sendCustomMessage(type="csvdata", d3df)

  varNames <- names(raw_data)
  varClass <- sapply(raw_data,class)
  varNums <- varNames[varClass != "factor"]
  varFacs <- varNames[varClass == "factor"]
  rawAbsMin <- apply(raw_data[varNums], 2, min, na.rm=TRUE)
  rawAbsMax <- apply(raw_data[varNums], 2, max, na.rm=TRUE)
  

}