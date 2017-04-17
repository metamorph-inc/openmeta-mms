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
  ns <- session$ns
  
  vars <- data$pre$var_range_nums_and_ints
  vars_list <- data$pre$var_range_nums_and_ints_list
  
  observe({
    sandboxVar <- isolate(input$sandboxVar)
    if (!is.null(si_read(ns("sandboxVar")))
        && si_read(ns("sandboxVar")) %in% c(data$pre$var_range(), "")) {
      sandboxVar <- si(ns("sandboxVar"), NULL)
    }
    updateSelectInput(session,
                      "sandboxVar",
                      choices = vars_list(),
                      selected = sandboxVar)
  })
  
  output$sandboxPlot <- renderPlot({
    if(input$sandboxVar != "") {
      hist(data$Filtered()[[input$sandboxVar]],
           main = paste("Histogram of" , paste(input$sandboxVar)),
           xlab = paste(input$sandboxVar))
    }
  })
  
}