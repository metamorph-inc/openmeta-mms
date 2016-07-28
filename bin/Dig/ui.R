library(shiny)
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
              selectInput("colType", "Type:", choices = c("None", "Max/Min", "Discrete", "Highlighted"), selected = "None"),
              conditionalPanel(
                condition = "input.colType == 'Max/Min'",
                selectInput("colVarNum", "Colored Variable:", c()),
                radioButtons("radio", NULL, c("Maximize" = "max", "Minimize" = "min"), selected = "max"),
                sliderInput("colSlider", NULL, min=0, max=1, value=c(0.3,0.7), step=0.1)
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
          
          #h4(textOutput("filterVars"), align = "center")
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
            checkboxInput("removeMissing", "Remove Incomplete Rows", value = TRUE),
            checkboxInput("removeOutliers", "Remove Outliers", value = FALSE),
            conditionalPanel("input.removeOutliers == '1'",
                             sliderInput("numDevs", HTML("&sigma;:"), min = 1, max = 11, step = 0.1, value = 2)
            ),
            hr(),
            
            h4("Render Options"),
            checkboxInput("autoRender", "Automatically Rerender Plot", value = TRUE),
            checkboxInput("trendLines", "Overlay Trendline(s)", value = FALSE),
            checkboxInput("upperPanel", "Display Upper Panel", value = FALSE),
            strong("Data Point Style"),
            fluidRow(
              column(4, radioButtons("pointStyle", NULL, c("Normal" = 1,"Filled" = 19))),
              column(8, radioButtons("pointSize", NULL, c("Small" = 1, "Medium" = 1.5, "Large" = 2)))
            ),
            hr(),
            
            h4("Automatic Refresh"),
            checkboxInput("autoInfo", "Info Pane", value = TRUE),
            checkboxInput("autoData", "Data Table Tab", value = TRUE),
            checkboxInput("autoRange", "Ranges Tab", value = TRUE),
            hr(),

            h4("Color Options"),
            fluidRow(
              column(4, colourInput("normColor", "Normal", "black"))
            ),
            fluidRow(
              column(4, colourInput("maxColor", "Worst", "#E74C3C")),
              column(4, colourInput("midColor", "In Between", "#F1C40F")),
              column(4, colourInput("minColor", "Best", "#2ECC71"))
            ),
            #h5("Highlighted", align = "center"),
            fluidRow(
              column(4, colourInput("highlightColor", "Highlighted", "#377EB8"))
            ), hr(),


            actionButton("resetSettings", "Reset Settings"), br(),
            hr(),
            
            h4("Session Options"),
            strong("Save Session"),
            textInput("sessionName", NULL, placeholder = "Enter a filename..."),
            downloadButton("exportSession", "Download"),
            br(), br(),
            strong("Load Session"), br(),
            actionButton('importSession', 'Choose File'),
            hr(),
            
            h4("About"),
            p(strong("Version:"), "v1.2.5"),
            p(strong("Date:"), "7/28/2016"),
            p(strong("Developer:"), "Metamorph Software"),
            p(strong("Support:"), "tthomas@metamorphsoftware.com")
          )
        ),
      column(9))
    ),
  id = "inTabset"),
  h3("Filter Data:"),
  actionButton("resetSliders", "Reset Sliders"), br(), br(),
  uiOutput("enums"),
  uiOutput("sliders"),
  h3("Constants:"),
  uiOutput("constants")
  
  
  
)
)
