debugSource("bayesian_utils.R")
debugSource("uq.R")

# Load data
data_all = read.csv("WindTurbineBladeDoEforOptimizationUnderUncertainty_mergedPET.csv")

# Get subset of data for training
data = data_all[data_all$CfgID == "32-16",-1]

numberOfVariables = ncol(data)

dataDirections = list("IN_ElemCount"="Input",
                      "IN_E11"="Input",
                      "IN_E22"="Input",
                      "IN_Root_AvgCapMaterialThickness"="Input",
                      "IN_Tip_AvgCapMaterialThickness"="Input",
                      "OUT_Blade_Cost_Total"="Output",
                      "OUT_Blade_Tip_Deflection"="Output"
                      )

# Set up distribution types and parameters
distributionTypes = list("IN_ElemCount"="norm",
                         "IN_E11"="norm",
                         "IN_E22"="norm",
                         "IN_Root_AvgCapMaterialThickness"="norm",
                         "IN_Tip_AvgCapMaterialThickness"="norm"
                         )

distributionParams = list("IN_ElemCount"=list(mean=mean(data[,"IN_ElemCount"]),
                                                   stdDev=0.2*abs(mean(data[,"IN_ElemCount"]))),
                          "IN_E11"=list(mean=mean(data[,"IN_E11"]),
                                                   stdDev=0.2*abs(mean(data[,"IN_E11"]))),
                          "IN_E22"=list(mean=mean(data[,"IN_E22"]),
                                                   stdDev=0.2*abs(mean(data[,"IN_E22"]))),
                          "IN_Root_AvgCapMaterialThickness"=list(mean=mean(data[,"IN_Root_AvgCapMaterialThickness"]),
                                                   stdDev=0.2*abs(mean(data[,"IN_Root_AvgCapMaterialThickness"]))),
                          "IN_Tip_AvgCapMaterialThickness"=list(mean=mean(data[,"IN_Tip_AvgCapMaterialThickness"]),
                                                   stdDev=0.2*abs(mean(data[,"IN_Tip_AvgCapMaterialThickness"]))))

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

