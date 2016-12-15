## Copyright (C) 2012 Marius Hofert, Ivan Kojadinovic, Martin Maechler, and Jun Yan
##
## This program is free software; you can redistribute it and/or modify it under
## the terms of the GNU General Public License as published by the Free Software
## Foundation; either version 3 of the License, or (at your option) any later
## version.
##
## This program is distributed in the hope that it will be useful, but WITHOUT
## ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
## FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
## details.
##
## You should have received a copy of the GNU General Public License along with
## this program; if not, see <http://www.gnu.org/licenses/>.

### This demo shows how the HAC package can be used for estimationg NACs

require(HAC)
require(copula)

## Build an 'nacopula' object (nested Archimedean copula (NAC))
theta <- 2:5
copG <- onacopulaL("Gumbel", list(theta[1], NULL, list(list(theta[2], c(2,1)),
                                                       list(theta[4], c(5,6)),
                                                       list(theta[3], c(4,3)))))
## Sample from copG
set.seed(271)
U <- pobs(rnacopula(1000, copula=copG))

## fitCopula(copG, U) does not provide fitting capabilities for HACs/NACs yet
## but we can convert copG to a 'hac' object
hacG <- nacopula2hac(copG)
plot(hacG) # plot method

## Parameters of the nested Gumbel copula can either be estimated
## based on a fixed structure...
colnames(U) <- paste(1:ncol(U))
hac.fixed <- estimate.copula(U, hac=hacG) # defaults: type = 1 (Gumbel), method = 1 (MPLE)
## ... or the structure of the Gumbel copula can be estimated as well:
hac.flex <- estimate.copula(U, type=hacG$type)
## Note:
## estimate.copula(, hac = ...) calls .QML() internally which proceeds as follows:
## 1) Compute matrix (tau_{ij}) of pairwise maximum (log-)likelihood estimators (via tau)
## 2) Determine the pair (i,j) ('pair') with maximal tau_{ij} and convert it to theta_{ij}
##    (see tau2theta())
## 3) Replace U[,i] by delta(max{U_i,U_j}) (~ U(0,1)) for delta(u) = phi(2 * phi^{-1}(u))
##    and remove U[,j] (see .cop.T())
## 4) Repeat this process until there is an associated estimate of theta for the whole path
##    of variables. They then determine the estimates for all parameters in the given
##    nested structure (see .union() and .compare.one() therein)

## Show the estimates
plot(hac.fixed)
plot(hac.flex)

## Last but not least, the estimation results can be re-converted
## into 'nacopula'-objects again
cop.fixed <- hac2nacopula(hac.fixed)
cop.flex <- hac2nacopula(hac.flex)
