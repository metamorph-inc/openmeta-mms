# Dependency: 'copula' from CRAN
#   Depends on the external 'gsl' library, which must be installed separately
#   (available in Homebrew as 'gsl' and Ubuntu as 'libgsl2' and 'libgsl-dev')
library("copula")

#' Builds the correlation matrix/gaussian copula for the provided data.
#'
#' @param resampledData The data to compute the correlation matrix for
#' 
#' @return The correlation matrix (gaussian copula)
buildGuassianCopula = function(resampledData) {
  xnew = x2u(resampledData, resampledData)
  rho = cor(xnew)
  
  return(rho)
}

#' Performs forward uncertainty quantification
forwardUq = function(originalData, resampledData, rho, observations, observationsIndex) {
  xStandard = x2u(data[observationsIndex])
  
  yPred = forwardUqImpl(originalData, xStandard, rho, observationsIndex)
  
  result = list()
  
  postIndexes = 1:nrow(rho)
  postIndexes = postIndexes[! postIndexes %in% observationsIndex]
  
  uPoints = seq(0.001, 0.999, 0.001)
  for (i in length(observations)) {
    observationResult = list()
    for (j in length(postIndexes)) {
      tempResult = list()
      originalIndex = postIndexes[j]
      originalName = names(originalData)[[originalIndex]]
      
      condMu = yPred[i, j, 1]
      condSigma = yPred[i, j, 2]
      tempResult$postPoints = densityInversecdf(data[originalIndex], uPoints)
      pdfPostPoints = dnorm(qnorm(uPoints, mean=condMu, sd=condSigma), mean=condMu, sd=condSigma)
      postFunction = approxfun(tempResult$postPoints, pdfPostPoints, yleft=0, yright=0)
      area = integrate(postFunction, min(tempResult$postPoints), max(tempResult$postPoints))$value
      tempResult$postPdf = pdfPostPoints / area
      observationResult[originalName] = tempResult
    }
    result[i] = observationResult
  }
  
  
  return(result)
}

backwardUq = function(originalData, resampledData, rho, observations, observationsIndex) {
  nObs = rows(observations)
  nCali = numberOfVariables - length(observationsIndex)
  
  yObsU = makeEmptyDataFrame(rows(observations))
  for (i in observationsIndex) {
    yObsU[names(originalData)[i]] = densityCdf(originalData[[i]], observations[[i]])
  }
  yObsStd = x2u(data[observationsIndex], observations[observationsIndex])
  
  result = backwardUqImpl(nCali, originalData, nObs, yObsU, yObsStd, rho, obsIndex)
}

# -- Internal implementations of forwards/backwards UQ (these correspond to the
#    forward/backwards UQ methods from Maha's Matlab code)

forwardUqImpl = function(x, yObs, rho, obsIndex) {
  m = nrow(yObs)
  nr = nrow(rho)
  
  obsIndexInver = 1:nr
  obsIndexInver = obsIndexInver[! obsIndexInver %in% obsIndex]
  
  result = array(NA, c(m, length(obsIndexInver), 2)) # Result is a 3-dimensional array: first index is observation number, second is unknown number, third is mean/std dev
  
  for (observation in 1:m) {
    thisObs = unlist(yObs[observation,])
    thisObs = matrix(thisObs, nrow=1, ncol=length(thisObs))
    Cxx = rho[obsIndexInver, obsIndexInver] # Covariance matrix of unobserved variables
    Cxy = rho[obsIndexInver, obsIndex] # Covariance matrix of unobserved and observed variables
    Cyy = rho[obsIndex, obsIndex] # Covariance matrix of observed variables
    CondCovariance = Cxx - Cxy%*%(solve(Cyy))%*%t(Cxy)
    CondMean = Cxy %*% (solve(Cyy))%*%t(thisObs)
    CondSigma = sqrt(abs(diag(CondCovariance)))
    CondSigma[CondSigma == 0] = 0.1 # Avoid 0 Sigma
    
    
    
    for (i in 1:length(obsIndexInver)) {
      xTemp = c(CondMean[[i]], CondMean[[i]] + CondSigma[[i]])
      uPoints = pnorm(xTemp, mean=0, sd=1)
      xOriginal = densityInverseCdf(x[[obsIndexInver[[i]]]], uPoints)
      result[observation, i,] = c(xOriginal[[1]], xOriginal[[2]] - xOriginal[[1]]) # mean, std deviation
    }
  }
  
  return(result)
}

# Backwards UQ using particle filter method
backwardUqImpl = function(nc, x, nObs, yObsU, yObsStandard, rho, obsIndex) {
  nr = nrow(rho)
  
  obsIndexInver = 1:nr
  obsIndexInver = obsIndexInver[! obsIndexInver %in% obsIndex] 
  
  yObsOrg = makeEmptyDataFrame(nrow(yObsU))
  yObsPdf = makeEmptyDataFrame(nrow(yObsU))
  
  for (i in 1:length(obsIndex)) {
    variableIndex = obsIndex[i]
    yObsOrg = densityInverseCdf(x[[variableIndex]], yObsU[[i]]) # Original matlab code keeps this around in a 2D array for each variable, but doesn't use it
    yObsPdf[names(x)[variableIndex]] = densityPdf(x[[variableIndex]], yObsOrg)
  }
  
  numberOfParticles = 1e5
  #Original MATLAB code appears to seed the RNG here; do we want to?
  uSampleVector = runif(nc * numberOfParticles)
  uSample = matrix(uSampleVector, nrow=numberOfParticles, ncol=nc) # This is transposed relative to matlab code, to preserve columns as variables, rows as individual samples
  
  pdfMargin = rep(1, numberOfParticles)
  for (i in 1:nc) {
    xSample = densityInverseCdf(x[[i]], uSample[[i]])
    pdfMarginTemp = densityPdf(x[[i]], xSample)
    pdfMargin = pdfMargin * pdfMarginTemp
  }
  
  likelihood = pdfMargin
  copula = normalCopula(P2p(rho), nr, dispstr="un")
  for (i in 1:nObs) {
    for (j in 1:length(obsIndex)) {
      likelihood = likelihood * yObsPdf[[i, j]]
    }
    secondMatrix = matrix(1, numberOfParticles, 1)%*%data.matrix(yObsU[i,])
    uSampleNew = cbind(uSample, secondMatrix)
    likelihoodTemp = dCopula(uSampleNew, copula)
    likelihood = likelihood * likelihoodTemp
  }
  
  weight = likelihood / sum(likelihood)
  posteriorIndices = genDist(weight, numberOfParticles)
  posteriorUSamples = uSampleNew[posteriorIndices, ]
  
  uPoints = seq(0.001, 0.999, 0.001)
  for (i in 1:nc) {
    priorDensity = density(x[[i]], from=min(x[[i]]), to=max(x[[i]])) # Are defaults adequate here?
    priorPdf = priorDensity$y
    xPoints = priorDensity$x
    
    uPdf = densityPdf(posteriorUSamples[,i], uPoints) # Different bandwidth hardcoded here
    xPost = densityInverseCdf(x[[i]], uPoints)
    
    postFunction = approxfun(xPost, uPdf, yleft=0, yright=0)
    area = integrate(postFunction, min(xPost), max(xPost))$value
    postPdf = uPdf / area
    
    plot(xPost, postPdf, type="l", col="red")
    lines(xPoints, priorPdf, col="green")
  }
}