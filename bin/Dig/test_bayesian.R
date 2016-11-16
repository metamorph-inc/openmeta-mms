debugSource("bayesian_utils.R")
debugSource("uq.R")

# Load data
data_all = read.csv("WindTurbineSim.csv")

# Get subset of data for training
data = data_all[1:4500, ]

numberOfVariables = ncol(data)

dataDirections = list("modelica.jturbine"="Input",
                      "modelica.ratio"="Input",
                      "modelica.rho"="Input",
                      "tl_peakPowerOutput.output"="Output",
                      "tl_integratedEnergy.output"="Output"
                      )

# Set up distribution types and parameters
distributionTypes = list("modelica.jturbine"="norm",
                         "modelica.ratio"="unif",
                         "modelica.rho"="norm")

distributionParams = list("modelica.jturbine"=list(mean=mean(data[,"modelica.jturbine"]),
                                                   stdDev=0.2*abs(mean(data[,"modelica.jturbine"]))),
                          "modelica.ratio"=list(min=min(data[,"modelica.ratio"]),
                                                max=max(data[,"modelica.ratio"])),
                          "modelica.rho"=list(mean=mean(data[,"modelica.rho"]),
                                              stdDev=0.1*abs(mean(data[,"modelica.rho"]))))

resampledList = resampleData(data, dataDirections, distributionTypes, distributionParams)
result = resampledList$dist
resampledData = resampledList$resampledData

for(i in 1:numberOfVariables) {
  plot(result[[i]][["xOrig"]], result[[i]][["yOrig"]], type='l', col="darkslateblue", main=names(data)[[i]], ylim=range(result[[i]][["yOrig"]], result[[i]][["yResampled"]]))
  lines(result[[i]][["xResampled"]], result[[i]][["yResampled"]], col="darkgreen")
}

rho = buildGuassianCopula(resampledData)

obsIndex = c(1,2,3)
observations = data[nrow(data), 1:3]

result = forwardUq(data, resampledData, rho, observations, obsIndex)

