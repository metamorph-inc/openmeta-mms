title <- "Scratch"
footer <- TRUE

ui <- function(id) {
  ns <- NS(id)
  
  fluidPage(
    br(),
    verbatimTextOutput(ns("text")),
    verbatimTextOutput(ns("text2")),
    verbatimTextOutput(ns("text3"))
  )
  
}

server <- function(input, output, session, data) {
  
  output$text <- renderPrint(data$Filters())
  output$text2 <- renderPrint(apply(data$Filtered(), 2, max, na.rm=TRUE))
  
}