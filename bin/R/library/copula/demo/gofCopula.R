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

##>>> NOTA BENE must contain exactly the \dontrun{} part of
## ../man/gofCopula.Rd
## ===================

## A two-dimensional data example ----------------------------------
x <- rCopula(200, claytonCopula(3))

## Does the Gumbel family seem to be a good choice (statistic "Sn")?
gofCopula(gumbelCopula(), x)
## With "SnC", really s..l..o..w.. --- with "SnB", *EVEN* slower
gofCopula(gumbelCopula(), x, method = "SnC", trafo.method = "cCopula")
## What about the Clayton family?
gofCopula(claytonCopula(), x)

## Similar with a different estimation method
gofCopula(gumbelCopula (), x, estim.method="itau")
gofCopula(claytonCopula(), x, estim.method="itau")


## A three-dimensional example  ------------------------------------
x <- rCopula(200, tCopula(c(0.5, 0.6, 0.7), dim = 3, dispstr = "un"))

## Does the Gumbel family seem to be a good choice?
g.copula <- gumbelCopula(dim = 3)
gofCopula(g.copula, x)
## What about the t copula?
t.copula <- tCopula(dim = 3, dispstr = "un", df.fixed = TRUE)
if(FALSE) ## this is *VERY* slow currently
  gofCopula(t.copula, x)

## The same with a different estimation method
gofCopula(g.copula, x, estim.method="itau")
if(FALSE) # still really slow
  gofCopula(t.copula, x, estim.method="itau")

## The same using the multiplier approach
gofCopula(g.copula, x, simulation="mult")
gofCopula(t.copula, x, simulation="mult")
if(FALSE) # no yet possible
    gofCopula(t.copula, x, simulation="mult", estim.method="itau")
