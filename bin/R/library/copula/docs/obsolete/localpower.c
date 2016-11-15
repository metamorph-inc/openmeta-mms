/*#################################################################################
##
##   R package Copula by Jun Yan and Ivan Kojadinovic Copyright (C) 2008, 2009
##
##   This file is part of the R package copula.
##
##   The R package copula is free software: you can redistribute it and/or modify
##   it under the terms of the GNU General Public License as published by
##   the Free Software Foundation, either version 3 of the License, or
##   (at your option) any later version.
##
##   The R package copula is distributed in the hope that it will be useful,
##   but WITHOUT ANY WARRANTY; without even the implied warranty of
##   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
##   GNU General Public License for more details.
##
##   You should have received a copy of the GNU General Public License
##   along with the R package copula. If not, see <http://www.gnu.org/licenses/>.
##
#################################################################################*/


/***********************************************************************
  
 Testing wheter a copula belongs to the Extreme-Value class
 
***********************************************************************/

#include <R.h>
#include <Rmath.h>
#include <R_ext/Applic.h>
#include "Anfun.h"

/***********************************************************************
  
 Extreme-Value Test (Local Power)
 
***********************************************************************/

void evtest_LP(double *U, int *n, int *p, double *g, int *m, 
	       int *N, double *tg,  int *nt, double *s0,
	       double *pcopt, double *der, double *dert, 
	       double *delta_term, double *step, double *ndelta)
{
  double *influ = Calloc((*n) * (*m) * (*nt), double);
  double *random = Calloc(*n, double);
  double *process = Calloc((*m) * (*nt), double);
 
  double *u = Calloc(*p, double);
  double *ut = Calloc(*p, double);
 
  double s, t, ecterm, proc, mean, ind, indt, d, dt, invsqrtn = 1.0/sqrt(*n);

  int a, i, j, k, l, c;

  for (c = 0; c < *nt; c++) 
    {
      t = tg[c];
      
      /* for each point of the grid */
      for (j = 0; j < *m; j++) 
	{
      
	  /* temporary arrays */
	  for (k = 0; k < *p; k++)
	    {
	      u[k] = g[j + k * (*m)];
	      ut[k] =  R_pow(u[k], t);
	    }
	  
	  ecterm = R_pow(pcopt[j + c * (*m)], (1 - t)/t) / t; 

	  /* for each pseudo-obs */
	  for (i = 0; i < *n; i++) 
	    { 
	      ind = 1.0;
	      indt = 1.0;
	      d = 0.0;
	      dt = 0.0;
	      for (k = 0; k < *p; k++)
		{
		  ind *= (U[i + k * (*n)] <= u[k]);
		  indt *= (U[i + k * (*n)] <= ut[k]);
		  d += der[j + k * (*m)] * (U[i + k * (*n)] <= u[k]);
		  dt += dert[j + k * (*m) + c * (*m) * (*p)] * (U[i + k * (*n)] <= ut[k]);
		}
	      influ[i + j * (*n) + c * (*n) * (*m)] 
		= (ecterm * (indt - dt) - (ind - d)) * invsqrtn;
	    }
	}
      
    }
  
  GetRNGstate();
  
  /* generate N approximate realizations */
  for (l = 0; l < *N; l++)
    { 
      /* generate n variates */
      mean = 0.0;
      for (i=0;i<*n;i++)
	{
	  random[i] = norm_rand(); /*(unif_rand() < 0.5) ? -1.0 : 1.0 ;*/
	  mean += random[i];
	}
      mean /= *n;

      /* realization number l */
      /* random part only */
      for (c = 0; c < *nt; c++) 
	for (j = 0; j < *m; j++)
	  { 
	    process[j + c * (*m)] = 0.0;
	    for (i = 0; i < *n; i++)
	      process[j + c * (*m)] 
		+= (random[i] - mean) * influ[i + j * (*n) + c * (*n) * (*m)];
	  }
	
      /* for every delta */
      for (a = 0; a < *ndelta; a++)
	{
	  s0[l + a * (*N)] = 0.0;

	  /* realization number l: random + delta */
	  for (c = 0; c < *nt; c++) 
	    {
	      s = 0.0;
	      for (j = 0; j < *m; j++)
		{ 
		  /* add delta term to random part */
		  proc = process[j + c * (*m)] + a * (*step) * delta_term[j + c * (*m)];
		  s += proc * proc;
		}
	      s /= *m;
	      s0[l + a * (*N)] += s;
	    }
	}
    }

  PutRNGstate();

  Free(influ);
  Free(process);
  Free(random);
  Free(u);
  Free(ut);
}

/***********************************************************************
  
 Extreme-Value Test based on An
 Derivatives based on Cn

***********************************************************************/

void evtestA_LP(double *U, double *V, int *n, double *u, double *v, 
		int *m, int *N, double *s0, double *Afun, 
		double *der1, double *der2, double *delta_term,
		double *step, double *ndelta)
{
  double *influ = Calloc((*n) * (*m), double);

  double *S = Calloc(*n, double);
  double *Sp = Calloc(*n, double);
  double *Sm = Calloc(*n, double);
  double *T = Calloc(*n, double); 
  double *Tp = Calloc(*n, double); 
  double *Tm = Calloc(*n, double); 
 
  double *random = Calloc(*n, double);
  double *process = Calloc(*m, double);

  double pu, pv, sum, proc, mean, 
    invsqrtn = 1.0 / sqrt(*n), minTkSi,  minSkTi,
    lb = 1.0 / (*n + 1.0), ub = *n / (*n + 1.0),
    loguv, loguvA, Aterm;

  int a, i, j, k, l; 

  /* temporary arrays */
  for (i = 0; i < *n; i++)
    {
      S[i] = - log(U[i]);
      T[i] = - log(V[i]);
      Sp[i] = - log(MIN(U[i] + invsqrtn, ub));
      Tp[i] = - log(MIN(V[i] + invsqrtn, ub));
      Sm[i] = - log(MAX(U[i] - invsqrtn, lb));
      Tm[i] = - log(MAX(V[i] - invsqrtn, lb));
    }

  /* for each point of the grid */
  for (j = 0; j < *m; j++) 
    {
      loguv = log(u[j] * v[j]);

      pu = loguv / log(u[j]);
      pv = loguv / log(v[j]);

      loguvA = loguv * Afun[j];
      Aterm = exp(loguvA) * loguvA;

      for (i = 0; i < *n; i++) /* for each pseudo-obs */
	{
	  sum = 0.0;
	  for (k = 0; k < *n; k++)
	    {
	      minTkSi = MIN(pv * T[k], pu * S[i]);
	      minSkTi = MIN(pu * S[k], pv * T[i]);
	      sum += -log(MIN(pu * Sm[k], minTkSi)) 
		+ log(MIN(pu * Sp[k], minTkSi))
		- log(MIN(pv * Tm[k], minSkTi)) 
		+ log(MIN(pv * Tp[k], minSkTi));
	    }
	  sum *= invsqrtn / 2.0;
	  
	  
	  influ[i + j * (*n)] = (U[i] <= u[j]) * (V[i] <= v[j]) 
	    - der1[j] * (U[i] <= u[j]) - der2[j] * (V[i] <= v[j]) 
	    - Aterm * (-log(MIN(pu * S[i], pv * T[i])) - sum);
	  
	  influ[i + j * (*n)] *= invsqrtn;
	}
    }

  GetRNGstate();

  /* generate N approximate realizations */
  for (l = 0; l < *N; l++)
    {
      /* generate n variates */
      mean = 0.0;
      for (i=0;i<*n;i++)
	{
	  random[i] = norm_rand(); /*(unif_rand() < 0.5) ? -1.0 : 1.0 ;*/
	  mean += random[i];
	}
      mean /= *n;
      
      /* realization number l */
      /* random part only */
      for (j = 0; j < *m; j++)
	{ 
	  process[j] = 0.0;
	  for (i = 0; i < *n; i++)
	    process[j] += (random[i] - mean) * influ[i + j * (*n)];
	}

      /* for every delta */
      for (a = 0; a < *ndelta; a++)
	{
	  /* realization number l: random + delta */
	  s0[l + a * (*N)] = 0.0;
	  for (j = 0; j < *m; j++)
	    { 
	      proc = process[j] + a * (*step) * delta_term[j];
	      s0[l + a * (*N)] += proc * proc;
	    }
	  s0[l + a * (*N)] /= *m; 
	}
    }
  
  PutRNGstate();
  
  
  Free(influ);
  Free(random);
  Free(process);
  
  Free(S);
  Free(T);
  Free(Sp);
  Free(Tp);
  Free(Sm);
  Free(Tm);
}



