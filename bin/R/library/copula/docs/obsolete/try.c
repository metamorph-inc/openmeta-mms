#include "Rinternals.h"
#include "R_ext/Applic.h"

double stepfn(double x, double a, double b) {
  return a * pow(x, 3.0) + b;
} 

void vec_stepfn(double *x, int n, void *ex) {
  int i;
  double *ptr = ex;
  double At = ptr[0], t = ptr[1];
  for (i = 0; i < n; i++) x[i] = stepfn(x[i], At, t);
  return;
}

void int_stepfn(double *lower, double *upper, double *ex) {
  double result, abserr;
  int last, neval, ier;
  int lenw;
  int *iwork;
  double *work;
  int limit=100;
  double reltol=0.00001;
  double abstol=0.00001;
  
  lenw = 4 * limit;
  iwork =   (int *) R_alloc(limit, sizeof(int));
  work = (double *) R_alloc(lenw,  sizeof(double));

  Rdqags(vec_stepfn, (void *)ex, lower, upper,
	 &abstol,  &reltol,
	 &result,  &abserr,  &neval,  &ier,
	 &limit,  &lenw, &last,
	 iwork, work);

  printf("%f %f %d\n", result, abserr, ier);
  return;
} 

