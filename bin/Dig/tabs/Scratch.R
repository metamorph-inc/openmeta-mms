title <- "Scratch"
footer <- TRUE

ui <- function() {
  
  fluidPage(
    br(),
    verbatimTextOutput("text"),
    verbatimTextOutput("text2"),
    verbatimTextOutput("text3")
  )
  
}

server <- function(input, output, session, data) {
  
  output$text <- renderPrint(data$Filters())
  output$text2 <- renderPrint(apply(data$Filtered(), 2, max, na.rm=TRUE))
  
}