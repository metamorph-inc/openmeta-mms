library(shiny)
library(plotly)
require(shinyjs)

# Define UI for PET Design Space Browser application
shinyUI(fluidPage(
  #  Application title
  titlePanel("PET Design Space Browser"),
  #verbatimTextOutput("debug"),
  tabsetPanel(
    tabPanel("Pairs Plot",
      fluidRow(
        column(3,
          br(),
          wellPanel(
            h4("Plot Options"),
            selectInput("display",
                        "Display Variables:",
                        c(),
                        multiple = TRUE),
            conditionalPanel(
              condition = "input.autoRender == false",
              actionButton("renderPlot", "Render Plot"),
              br()
            ), hr(),
              h4("Data Coloring"),
              selectInput("colType", "Type:", choices = c("None", "Max/Min", "Discrete", "Highlighted", "Ranked"), selected = "None"),
              conditionalPanel(
                condition = "input.colType == 'Max/Min'",
                selectInput("colVarNum", "Colored Variable:", c()),
                radioButtons("radio", NULL, c("Maximize" = "max", "Minimize" = "min"), selected = "max"),
                sliderInput("colSlider", NULL, min=0, max=1, value = c(0.3, 0.7), step=0.1)
              ),
              conditionalPanel(
                condition = "input.colType == 'Discrete'",
                selectInput("colVarFactor", "Colored Variable:", c()),
                htmlOutput("colorLegend")
              )
              
            ,  hr(),
            h4("Info"), #br(),
            verbatimTextOutput("stats"),
            conditionalPanel(condition = "input.autoInfo == false",
                            actionButton("updateStats", "Update"),
                            br()),  hr(),
            h4("Download"),
            downloadButton('exportData', 'Dataset'), 
            paste("          "),
            downloadButton('exportPlot', 'Plot'), hr(),
            actionButton("resetOptions", "Reset to Default Options")
          )
        ),
        column(9,
            uiOutput("displayError"),   
            uiOutput("filterError"),
            uiOutput("pairsDisplay")
        )
      )
    ),
    tabPanel("Single Plot",
      fluidRow(
        column(3,
          br(),
          wellPanel(
            selectInput("xInput", "X-axis", c()),
            selectInput("yInput", "Y-Axis", c()),
            br(),
            p(strong("Adjust Sliders to Selection")),
            actionButton("updateX", "X"),
            actionButton("updateY", "Y"),
            actionButton("updateBoth", "Both"),
            br(), br(),
            #p(strong("Highlight Selection")),
            bootstrapPage(
              actionButton("highlightData", "Highlight Selection", class = "btn btn-primary")
            )
          )
        ),
        column(9,
          plotOutput("singlePlot", click = "plot_click", brush = "plot_brush", height=700)
        ),
        column(12,
          verbatimTextOutput("info")
        )
      )
    ),
    tabPanel("Data Table",
      wellPanel(
        fluidRow(
          conditionalPanel(condition = "input.autoData == false", 
                           actionButton("updateDataTable", "Update Data Table")
                           , br(), br())
        ),
        fluidRow(
          dataTableOutput(outputId="table")
        )
      )
    ),
    tabPanel("Data Ranking",
             wellPanel(
               fluidRow(
                 column(4, selectInput("weightMetrics",
                             "Select Weighted Metrics:",
                             c(),
                             multiple = TRUE),
                        actionButton("clearMetrics", "Clear Metrics")),
                 column(8, plotlyOutput("rankPieChart")),
                 br(), br(), br()
               ),
               uiOutput("rankings"), 
               downloadButton("exportPoints", "Export Selected Points"), 
               actionButton("colorRanked", "Color by Selected Rows"), 
               br(), hr(),
               h4(strong("Ranked Data"), align = "center"),
               DT::dataTableOutput("rankTable")
             )
    ),
    tabPanel("Ranges",
     wellPanel(
        fluidRow(
          column(6, conditionalPanel(condition = "input.autoRange == false",
                           actionButton("updateRanges", "Update Ranges"), br(), br()),
          downloadButton('exportRanges', 'Download Ranges'), br(), br())
        ),
        fluidRow(
          column(12,
                 verbatimTextOutput("ranges")
          )
        )
      )
    ),
    tabPanel("Options",
      fluidRow(
        column(6,
          br(),
          wellPanel(
            h4("Data Processing Options"),
            tags$div(title = "Removes data points that have missing attributes.",
                     checkboxInput("removeMissing", "Remove Incomplete Rows", value = TRUE)), 
            fluidRow(
              column(4, 
                tags$div(title = "Removes data points outside of a set number of standard deviations from the mean.", 
                  checkboxInput("removeOutliers", "Remove Outliers", value = FALSE))),
              conditionalPanel("input.removeOutliers == '1'", 
                column(8, 
                  tags$div(title = "Number of standard deviations to filter data by.", 
                    sliderInput("numDevs", HTML("&sigma;:"), min = 1, max = 11, step = 0.1, value = 6))))
            ),
            tags$div(title = "Sticky Filters try to preserve their settings when removing/adding outliers or missing data rows.", 
                     checkboxInput("stickyFilters", "Sticky Filters", value = TRUE)),
            hr(),
            
            h4("Render Options"),
            tags$div(title = "Pairs plot will automatically update.",
                     checkboxInput("autoRender", "Automatically Rerender Plot", value = TRUE)),
            tags$div(title = "Allow trendline to be inserted into plot.",
                     checkboxInput("trendLines", "Overlay Trendline(s)", value = FALSE)),
            tags$div(title = "Shows the upper panel in the Pairs Plot.",
                     checkboxInput("upperPanel", "Display Upper Panel", value = FALSE)),
            strong("Data Point Style"),
            fluidRow(
              column(4, tags$div(title = "Normal: cheerios, Filled: dots.", 
                                 radioButtons("pointStyle", NULL, c("Normal" = 1,"Filled" = 19)))),
              column(8, tags$div(title = "Size of data points.",
                                 radioButtons("pointSize", NULL, c("Small" = 1, "Medium" = 1.5, "Large" = 2))))
            ),
            hr(),
            
            h4("Automatic Refresh"),
            tags$div(title = "Automatically updates info pane on pairs plot tab.",
                     checkboxInput("autoInfo", "Info Pane", value = TRUE)),
            tags$div(title = "Automatically updates Data Table tab.",
                     checkboxInput("autoData", "Data Table Tab", value = TRUE)),
            tags$div(title = "Automatically updates Ranges Tab.",
                     checkboxInput("autoRange", "Ranges Tab", value = TRUE)),
            hr(),

            h4("Color Options"),
            fluidRow(
              column(4, tags$div(title = "Default color of data points.",
                                 colourInput("normColor", "Normal", "black")))
            ),
            fluidRow(
              column(4, tags$div(title = "Color of 'worst' data points.", 
                                 colourInput("maxColor", "Worst", "#E74C3C"))),
              column(4, tags$div(title = "Color of 'in between' data points.", 
                                 colourInput("midColor", "In Between", "#F1C40F"))),
              column(4, tags$div(title = "Color of 'best' data points.", 
                                 colourInput("minColor", "Best", "#2ECC71")))
            ),
            fluidRow(
              column(4, tags$div(title = "Color of highlighted data points.",
                                 colourInput("highlightColor", "Highlighted", "#377EB8")))
            ),
            fluidRow(
              column(4, tags$div(title = "Color of ranked data points.",
                                 colourInput("rankColor", "Ranked", "#D13ABA")))
            ),hr(),


            tags$div(title = "Return to default settings.",
                     actionButton("resetSettings", "Reset Settings")), 
            br(),
            hr(),
            
            h4("Session Options"),
            strong("Save Session"),
            textInput("sessionName", NULL, placeholder = "Enter a filename..."),
            tags$div(title = "Download current state of visualizer.",
                     downloadButton("exportSession", "Download")),
            br(), br(),
            strong("Load Session"), br(),
            tags$div(title = "Load a saved session.",
                     actionButton('importSession', 'Choose File')),
            hr(),
            
            h4("About"),
            p(strong("Version:"), "v1.2.10"),
            p(strong("Date:"), "9/2/2016"),
            p(strong("Developer:"), "Metamorph Software"),
            p(strong("Support:"), "tthomas@metamorphsoftware.com")
          )
        ),
      column(9))
    ),
  id = "inTabset"),
  h3("Filter Data:"),
  fluidRow(
    column(3,
      tags$div(title = "Activate to show filters for all dataset variables.",
               checkboxInput("viewAllFilters", "View All Filters", value = FALSE)),
      tags$div(title = "Return visible sliders to default state.",
               actionButton("resetSliders", "Reset Visible Filters"))
    ),
    br(), br(), br(), br(), br()
  ),
  uiOutput("filters"),
  h3("Constants:"),
  uiOutput("constants")
  
  
  
)
)
