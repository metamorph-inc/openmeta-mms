title <- "Histogram"
footer <- TRUE

ui <- function(id) {
  ns <- NS(id)
  
  fluidPage(
    br(),
    column(3,
      selectInput(ns("sandboxVar"), "Histogram Variable:", c())
    ),
    column(9,
      plotOutput(ns("sandboxPlot"))
    )
  )
  
}

server <- function(input, output, session, data) {
  
  vars <- data$meta$preprocessing$var_range_nums_and_ints
  
  updateSelectInput(session,
                    "sandboxVar",
                    choices = vars,
                    selected = vars[1])
  
  output$sandboxPlot <- renderPlot({
    if(input$sandboxVar != "") {
      hist(data$Filtered()[[input$sandboxVar]],
           main = paste("Histogram of" , paste(input$sandboxVar)),
           xlab = paste(input$sandboxVar))
    }
  })
  
}