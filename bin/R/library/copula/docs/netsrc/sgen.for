
c
c	Permission is given to use this code for any noncommercial,
c	nonprofit use.  All other uses must be approved by the
c	author.
c
c     Written by:   John Nolan        (July 1996, version history below)
c                   Math/Stat Department
c                   American University
c                   jpnolan@american.edu
c
c     Different parameterizations are used for stable distributions,
c     the variable iparam is used throughout to denote what
c     parameterization to use:
c
c         iparam = 0 for S0 parameterization of Nolan,
c             a variation of Zolotarev's (M) parameterization
c         iparam = 1 for S=S1 parameterization of Samorodnitsky and Taqqu
c             a variation of Zolotarev's (A) parameterization
c         iparam = 2 for S*=S2 parameterization of Nolan
c
c **********************************************************************

      subroutine sgen(n,x,alpha,beta,gamma,delta,n2,unif,iparam,ierr)
cDEC$	ATTRIBUTES DLLEXPORT :: SGEN
C
C     Generate stable random variables with the specified parameters.
C     The method of Chambers, Mallow and Stuck is used for the basic
C     work, we just convert to the desired form.
C
C     Written by:
C       John P. Nolan           July 1996
C       Math/Stat Dept.
C       American University
C       Washington, DC 20016
C       e-mail: jpnolan@american.edu
C
C     output: x(1),...,x(n) are realizations of a
C         stable random variable with the given parameters.
C
C     input values:
C       alpha  = index of stability of the distribution (0 < alpha < 2)
C       beta   = skewness parameter (-1 <= beta <= 1)
C       delta     = shift parameter
C       gamma  = scale parameter (gamma > 0)
C       iparam = parameterization type:
C                  0 is S0 parameterization Nolan - just a scale multiple
C                     and a shift of a standardized r.v. in Zolotarev's
C                     (M) parameterization
C                  1 is S parameterization of Samorodnitsky and Taqqu
C                  2 is S* parameterization of Nolan
C       unif   = array of n2 = 2*n uniform random variables on the
C                interval (0,1).  NOTE: the array must not contain
C                values of 0 or 1; this will cause a floating point
C                error below.  Many random number generators will
C                return values of 0 and 1 in a long stream of random
C                variates, so the caller must eliminate them before
C                calling this routine.
C       ierr   = return code, ierr=0 means ok
C
      IMPLICIT NONE
      include 'stable.fd'

      INTEGER*4 N, N2, IPARAM, ierr
      REAL*8 ALPHA, delta, gamma, BETA, X(N), UNIF(N2)

      INTEGER*4 I,J
      REAL*8 gamma0, delta0, U, EXPON,RSTAB

c     scale and shift corrections to S0 parameterization
c     (RSTAB returns an S0(alpha,beta,1,0) r.v.)
      call scheck( 1,alpha,beta,gamma,delta,iparam,
     1      gamma0,delta0,ierr )
      if ((ierr .ne. noerr).and.(ierr.ne.warnrnd)) goto 100

C     call RSTAB in a loop with a uniform and an exponential r.v.
      J = 1
      DO I = 1,N
        EXPON = -DLOG(UNIF(J))
        U = UNIF(J+1)
        X(I) = gamma0 * RSTAB( ALPHA, beta, U, EXPON ) + delta0
        J = J + 2
      ENDDO

100	continue
      RETURN
      END


c **********************************************************************
C
C  rstab  --  random stable standardized form
C  Adapted from a file supplied by Chambers, et al on 5/18/92.
C  John Nolan (jpnolan@american.edu) has:
C  1. converted original from RATFOR to FORTRAN and all variables
C     and functions have been explicitly typed (5/18/92)
C  2. converted rstab to double precision (11/7/97)
C  3. Prevented division by 0 that sometimes occurs (11/7/97)
C
      real*8 function rstab(alpha,bprime,u,w)
C arguments ..
C       alpha : characteristic exponent
C      bprime : skewness in revised parameterization
C           u : uniform variate on (0,1)
C           w : exponentially distributed variate
      real*8 alpha,bprime,u,w,phiby2,a,eps,piby2,b,bb,tau
     * ,a2,a2p,b2,b2p,alogz,tan2,z,d2,d,denom
c      double precision da,db
      data piby2/1.570796326794896d0 /
c      real*8 thr1
c      data thr1/0.99d0/

      eps = 1.0d0-alpha
C compute some tangents
      phiby2 = piby2*(u-0.5d0)
      a = phiby2*tan2(phiby2)
      bb = tan2(eps*phiby2)
      b = eps*phiby2*bb
      if (eps.gt.(-0.99d0)) then
        tau = bprime/(tan2(eps*piby2)*piby2)
      else
        tau = bprime*piby2*eps*(1.0d0-eps)*tan2((1.0d0-eps)*piby2)
      endif

C compute some necessary subexpressions
c
c Modification (J. Nolan): comment out redundant double precision code.
C if phi near pi by 2, use double precision throughout
c      if (a.gt.thr1) then
C double precision
c        da = dble(a)**2
c        db = dble(b)**2
c        a2 = 1.d0-da
c        a2p = 1.d0+da
c        b2 = 1.d0-db
c        b2p = 1.d0+db
c      else
C single precision
        a2 = a**2
        a2p = 1.0d0+a2
        a2 = 1.0d0-a2
        b2 = b**2
        b2p = 1.0d0+b2
        b2 = 1.0d0-b2
c      endif


c     Modification (J. Nolan): prevent division by 0
      denom = w*a2*b2p
      if (denom .eq. 0.0d0) then
c       This is a kludge, but will prevent the rare case of division
c       by zero that occurs if a2=0 (which occurs if u is close to 0
c       or u is close to 1), or w = 0.  In theory, these possibilities
c	  have probability 0, but in practice they can occur with random
c	  number generators that return uniform values in [0,1] instead
c	  of (0,1).  Return the mode of the corr. distrib. to try to
c	  be as inconspicuous as possible.
        call smode0( alpha, bprime, rstab )
      else
C compute coefficient
        z = a2p*(b2+2.0d0*phiby2*bb*tau)/denom
C compute the exponential-type expression
        alogz = dlog(z)
        d = d2(eps*alogz/(1.0d0-eps))*(alogz/(1.0d0-eps))
C compute stable
        rstab = (1.0d0+eps*d)*2.0d0*((a-b)*(1.0d0+a*b)
     1     -phiby2*tau*bb*(b*a2-2.0d0*a))/(a2*b2p)+tau*d
      endif

      return
      end


c **********************************************************************

C d2         evaluate (exp(x)-1)/x
      real*8 function d2(z)
      real*8 z,p1,p2,q1,q2,q3,pv,zz
C
C     corrections made to values of q1 and q2 - J. P. Nolan
C     See JASA Vol 82, pg. 704, June 1987 (still one error)
C     and Hart, et al approximation 1801, pg 213.
C
C     corrected following data statement from Chambers, Mallows, Stuck routine
      data p1,p2,q1,q2,q3/.84006 68525 36483 239 d3,
     *  .20001 11415 89964 569 d2,
     *  .16801 33705 07296 648 d4,
     *  .18001 33704 07390 023 d3,1.d0/
C the approximation 1801 from hart et al (1968, p. 213)

      if (dabs(z).gt.0.1) then
        d2 = (dexp(z)-1.0d0)/z
      else
        zz = z*z
        pv = p1+zz*p2
        d2 = 2.0d0*pv/(q1+zz*(q2+zz*q3)-z*pv)
      endif

      return
      end


c **********************************************************************
C mytan        tangent function
      real*8 function mytan(xarg)
      logical neg,inv
      real*8 p0,p1,p2,q0,q1,q2,piby4,piby2,pi,x,xarg,xx
      data  p0,p1,p2,q0,q1,q2/.129221035d+3,-.887662377d+1,
     * .528644456d-1,.164529332d+3,-.45 1320561d+2,1.0/
C the approximation 4283 from hart et al(1968, p. 251)
      data pi    /3.141592653589793d0/,piby2/1.570796326794896d0/,
     1     piby4 / .78539 81633 97448 3d0/

      inv = .false.
      x = xarg
      neg = (x.lt.0.0)
      x = dabs(x)
C perform range reduction if necessary
      if (x.gt.piby4) then
        x = dmod(x,pi)
        if (x.gt.piby2) then
          neg = .not. neg
          x = pi-x
        endif
        if (x.gt.piby4) then
          inv = .true.
          x = piby2-x
        endif
      endif
      x = x/piby4
C convert to range of rational
      xx = x*x
      mytan = x*(p0+xx*(p1+xx*p2))/(q0+xx*(q1+xx*q2))
      if (neg) mytan = -mytan
      if (inv) mytan = 1.0d0/mytan

      return
      end


c **********************************************************************
C  tan2       compute tan(x)/x
C     function defined only for abs(xarg).le.pi by 4
C     for other arguments returns tan(x)/x, computed directly
      real*8 function tan2(xarg)
      real*8 mytan,xarg,p0,p1,p2,q0,q1,q2,piby4,x,xx
      data  p0,p1,p2,q0,q1,q2/.129221035d+3,-.887662377d+1,
     * .528644456d-1,.164529332d+3,-.45 1320561d+2,1.0/
C the approximation 4283 from hart et al(1968, p. 251)
      data piby4 / .78539 81633 97448 3d0/

      x = dabs(xarg)
      if (x.gt.piby4) then
        tan2 = mytan(xarg)/xarg
      else
        x = x/piby4
C convert to range of rational approx.
        xx = x*x
        tan2 = (p0+xx*(p1+xx*p2))/(piby4*(q0+xx*(q1+xx*q2)))
      endif

      return
      end

c **********************************************************************

      subroutine scnvrt( iparam, thetai, jparam, thetaj )
cDEC$	ATTRIBUTES DLLEXPORT :: SCNVRT

c     Utility routine to convert between parameterizations
c     Convert from parameterization iparam to parameterization jparam
c	This routine does no error checking, it could blow up if parameter
c	values are invalid (e.g. alpha < 0, etc.)
c
      implicit none
      include 'stable.fd'
      real*8 thetai(4),thetaj(4)
      integer*4 iparam, jparam
      real*8 alpha, beta, gammai, deltai, gamma0, delta0,
     1    gammaj, deltaj, xmode, galpha
	external galpha

c	unpack parameters from input array
	alpha  = thetai(1)
	beta   = thetai(2)
	gammai = thetai(3)
	deltai = thetai(4)

c	convert from iparam to S0 parameterization
	if ((iparam .le. 0) .or. (iparam .gt. 3)) then
	  gamma0 = gammai
	  delta0 = deltai
	elseif (iparam .eq. 1) then
	  gamma0 = gammai
        if (alpha .eq. 1.0d0) then
          delta0 = deltai + beta*gammai*dlog(gammai)/piby2
        else
          delta0= deltai+beta*dtan(piby2*alpha)*gammai
        endif
	elseif (iparam .eq. 2) then
	  gamma0 = gammai * alpha**(-1.0d0/alpha)
        call smode0( alpha, beta, xmode )
        delta0 = deltai - gamma0*xmode
	elseif (iparam .eq. 3) then
        gamma0 = gammai*alpha**(-1.0d0/alpha)
        delta0 = deltai - gamma0*beta*galpha(alpha)
	endif

c	convert from the S0 to jparam parameterization
	if ((jparam .le. 0) .or. (jparam .gt. 3)) then
        gammaj = gamma0
	  deltaj = delta0
	elseif (jparam .eq. 1) then
        gammaj = gamma0
        if (alpha .eq. 1.0d0) then
          deltaj = delta0 - beta*gamma0*dlog(gamma0)/piby2
        else
          deltaj = delta0 - beta*gamma0*dtan(piby2*alpha)
        endif
      elseif (jparam .eq. 2) then
        gammaj = gamma0*(alpha**(1.0d0/alpha))
        call smode0( alpha, beta, xmode )
        deltaj = delta0 + gamma0*xmode
	elseif (jparam .eq. 3) then
	  gammaj = gamma0*(alpha**(1.0d0/alpha))
	  deltaj = delta0 + gamma0*beta*galpha(alpha)
	endif

c	pack converted parameter results into output array
	thetaj(1) = alpha
	thetaj(2) = beta
	thetaj(3) = gammaj
	thetaj(4) = deltaj

      return
      end

c **********************************************************************

      real*8 function galpha( alpha )
c	compute
c g(alpha) = tan(pi*alpha/2)*(gamma(1+(2/alpha))/gamma(3/alpha)-1)  alpha .ne. 1
c		 = (2*EulerGamma-3)/pi	      alpha=1
c	This function is used in converting between iparam=3
c     parameterization.

      implicit none
	include 'stable.fd'
      real*8 alpha, dgamma, euler, g1

	euler = 0.57721 56649 01532 86060d0
	g1 = (2.0d0*euler - 3.0d0)/pi
      if (dabs(alpha-1.0d0) .lt. 0.0001d0) then
        galpha = g1
      else
        galpha = dtan( piby2 * alpha) *
     1         (dgamma(1.0d0+2.0d0/alpha)/dgamma(3.0d0/alpha) - 1.0d0)
      endif

      return
      end
c **********************************************************************

      blockdata stableblkdata
      implicit none
      include 'stable.fd'

c     tolerances/thresholds for computations
c     tol(1) for relative error in pdf numerical integration
c     tol(2) for relative error in cdf numerical integration
c     tol(3) for relative error in quantile calculation
c     tol(4) for comparing values of alpha and beta
c     tol(5) for testing values of x
c     tol(6) for exponential evaluation
c     tol(7) for finding location of the peak of the pdf integrand
c			   and limiting the strim search
c     tol(8) for strim
c	tol(9) for minimum alpha
c	tol(10) min value of xtol allowed
c     tol(11) threshold for quantile search

      data tol / 0.12d-13, 0.12d-13, 0.12d-13, 0.01d0, 0.005d0,
     1  200.0d0, 1.0d-14, 1.0d-51, 0.1d0, 1.0d-13, 1.0d-10  /

      data debug/.false./

      end

c
c *********************************************************************
c
      subroutine scheck( n, alpha, beta, gamma, delta, iparam, 
     1  gamma0, delta0, ierr ) 
c
c     check input parameters and convert scale and shift to S0
c     parameterization
c     
      implicit none      
      include 'stable.fd'
      integer*4 n, iparam, ierr
      real*8 alpha,beta,gamma,delta,gamma0,delta0,thetai(4),thetaj(4) 
      integer*4 jparam
      
c     error checking on input parameters
      if( (n .lt. 1) .or. (alpha .lt. tol(9)) .or. (alpha .gt. 2.0d0) 
     1      .or. (beta .lt. -1.0d0) .or. (beta .gt. 1.0d0)
     2      .or. (gamma .le. 0.0d0) .or. (iparam .lt. 0) 
     3      .or. (iparam .gt. 3) ) then
        ierr = errpar
        return
      endif     
      ierr = noerr
      
c     store some quantities in global variables
      alpha0 = alpha
      beta0 = beta

c	tolerance for testing closeness of x to zeta
      xtol = dmax1(tol(5)*alpha0**(1.0d0/alpha0),tol(10))

c     check for (alpha,beta) at or near a special case:
c		(alpha=*, beta=+1 or -1)  totally skewed case
c		(alpha=2, beta=*)  Gauss case
c		(alpha=.5, beta=+1 or -1) Levy case
c		(alpha=1, beta= 0) Cauchy case
c     determine if a one-sided density
      onesid = (alpha0 .lt. 1.0d0) .and. 
     1   ( (beta0 .gt. 1.0d0-tol(4)) .or. (beta0 .lt. -1.0d0+tol(4)))      
      gauss = (alpha .eq. 2.0d0)
      levy = onesid .and. (dabs(alpha-0.5d0) .lt. tol(4))
      anear1 = (dabs(alpha-1.0d0) .lt. tol(4))
      cauchy = anear1 .and.(dabs(beta).lt.tol(4))
c	if near, but not exactly at a special case, set ierr to a 
c	rounding warning
      if ( (onesid .and. (dabs(beta) .ne. 1.0d0))
	1    .or. (levy .and. (alpha .ne. 0.5d0)) 
	1    .or. (anear1 .and. (alpha .ne. 1.0d0))
	1    .or. (cauchy .and. (beta .ne. 0.0d0) ) ) ierr = warnrnd

c     compute shift and scale in S0 parameterization  
      if (anear1) then
	  thetai(1) = 1.0d0
      else
	  thetai(1) = alpha
	endif
	thetai(2) = beta
	thetai(3) = gamma
	thetai(4) = delta
      jparam = 0                                                                      
      call scnvrt( iparam, thetai, jparam, thetaj ) 
      gamma0 = thetaj(3)
	delta0 = thetaj(4)
      
      return
      end


c **********************************************************************
       
      subroutine smode0( alpha, beta, x )  	             
c	 
cDEC$	ATTRIBUTES DLLEXPORT :: SMODE0
c                        
c     Compute the mode of a S0(alpha,beta,1,0) random variable. 
c     This routine currently uses a table of mode locations
c     that was numerically derived.  A linear interpolation is
c     done on those values.  This should be replaced with an
c     equation when (if?) one is found.
c
      implicit none      
      include 'stable.fd'
      real*8 alpha, beta, x
                        
      integer*4 nalpha,nbeta,i,j,k
      parameter (nalpha=41,nbeta=11)                        
      real*8 xmode(nalpha,nbeta), da,db,x1,x2,x3,x4,y1,y2,c1,c2, babs


       data (xmode( 1,k),k=1,11) /
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00 /
       data (xmode( 2,k),k=1,11) /
     1     0.00000000000000D+00,    -0.78701706824618D-02,
     1    -0.15740341364924D-01,    -0.23610512047385D-01,
     1    -0.31480682729847D-01,    -0.39350853412309D-01,
     1    -0.47221024094771D-01,    -0.55091194777233D-01,
     1    -0.62961365459694D-01,    -0.70831536142156D-01,
     1    -0.78701706824617D-01 /
       data (xmode( 3,k),k=1,11) /
     1     0.00000000000000D+00,    -0.15838444032436D-01,
     1    -0.31676888064815D-01,    -0.47515332097042D-01,
     1    -0.63353776128923D-01,    -0.79192220160088D-01,
     1    -0.95030664189862D-01,    -0.11086910821710D+00,
     1    -0.12670755223991D+00,    -0.14254599625529D+00,
     1    -0.15838444025863D+00 /
       data (xmode( 4,k),k=1,11) /
     1     0.00000000000000D+00,    -0.24007870213409D-01,
     1    -0.48015731305156D-01,    -0.72023576284931D-01,
     1    -0.96031394786046D-01,    -0.12003917209192D+00,
     1    -0.14404688799664D+00,    -0.16805451549300D+00,
     1    -0.19206201928172D+00,    -0.21606935409486D+00,
     1    -0.24007646282278D+00 /
       data (xmode( 5,k),k=1,11) /
     1     0.00000000000000D+00,    -0.32490031951239D-01,
     1    -0.64978235344327D-01,    -0.97463769153978D-01,
     1    -0.12994561220702D+00,    -0.16242252475352D+00,
     1    -0.19489302844866D+00,    -0.22735538640435D+00,
     1    -0.25980758316011D+00,    -0.29224730454968D+00,
     1    -0.32467191754344D+00 /
       data (xmode( 6,k),k=1,11) /
     1     0.00000000000000D+00,    -0.41369773835088D-01,
     1    -0.82708192336175D-01,    -0.12400498834559D+00,
     1    -0.16524991095223D+00,    -0.20643173779536D+00,
     1    -0.24753821536721D+00,    -0.28855605073778D+00,
     1    -0.32947091070662D+00,    -0.37026742167848D+00,
     1    -0.41092917054220D+00 /
       data (xmode( 7,k),k=1,11) /
     1     0.00000000000000D+00,    -0.50538917992431D-01,
     1    -0.10090819050852D+00,    -0.15106129392046D+00,
     1    -0.20096204336259D+00,    -0.25057439383180D+00,
     1    -0.29986151544203D+00,    -0.34878564702431D+00,
     1    -0.39730808980758D+00,    -0.44538923286988D+00,
     1    -0.49298858672495D+00 /
       data (xmode( 8,k),k=1,11) /
     1     0.00000000000000D+00,    -0.59550700255715D-01,
     1    -0.11861028929190D+00,    -0.17704864106880D+00,
     1    -0.23479071914006D+00,    -0.29177045071043D+00,
     1    -0.34792471024206D+00,    -0.40319184858372D+00,
     1    -0.45751118742547D+00,    -0.51082280061839D+00,
     1    -0.56306739493316D+00 /
       data (xmode( 9,k),k=1,11) /
     1     0.00000000000000D+00,    -0.67749352607713D-01,
     1    -0.13452009593341D+00,    -0.20003545133244D+00,
     1    -0.26417613406424D+00,    -0.32685537193291D+00,
     1    -0.38799804929159D+00,    -0.44753504958923D+00,
     1    -0.50540115779917D+00,    -0.56153408737739D+00,
     1    -0.61587393926204D+00 /
       data (xmode(10,k),k=1,11) /
     1     0.00000000000000D+00,    -0.74537426063130D-01,
     1    -0.14753927249644D+00,    -0.21851689870656D+00,
     1    -0.28730023894388D+00,    -0.35379576508109D+00,
     1    -0.41793656767383D+00,    -0.47966794317336D+00,
     1    -0.53894205265550D+00,    -0.59571552616930D+00,
     1    -0.64994823010711D+00 /
       data (xmode(11,k),k=1,11) /
     1     0.00000000000000D+00,    -0.79547042797407D-01,
     1    -0.15704230498123D+00,    -0.23173754754555D+00,
     1    -0.30339731632321D+00,    -0.37192875421011D+00,
     1    -0.43728825597722D+00,    -0.49945276331040D+00,
     1    -0.55840897002348D+00,    -0.61414870609616D+00,
     1    -0.66666666499783D+00 /
       data (xmode(12,k),k=1,11) /
     1     0.00000000000000D+00,    -0.82667069237720D-01,
     1    -0.16288378737483D+00,    -0.23962721719107D+00,
     1    -0.31257541646351D+00,    -0.38163248475171D+00,
     1    -0.44678289767866D+00,    -0.50804306040481D+00,
     1    -0.56544275489429D+00,    -0.61901719708384D+00,
     1    -0.66880337596465D+00 /
       data (xmode(13,k),k=1,11) /
     1     0.00000000000000D+00,    -0.83986693307232D-01,
     1    -0.16527405471459D+00,    -0.24258552165425D+00,
     1    -0.31549236949046D+00,    -0.38388251563382D+00,
     1    -0.44776292940920D+00,    -0.50718784587590D+00,
     1    -0.56223015123297D+00,    -0.61296901648758D+00,
     1    -0.65948428365971D+00 /
       data (xmode(14,k),k=1,11) /
     1     0.00000000000000D+00,    -0.83717475064861D-01,
     1    -0.16463301656603D+00,    -0.24126651208840D+00,
     1    -0.31307054584030D+00,    -0.37989923616360D+00,
     1    -0.44176913887760D+00,    -0.49876336916146D+00,
     1    -0.55099136619630D+00,    -0.59857106039121D+00,
     1    -0.64162065044370D+00 /
       data (xmode(15,k),k=1,11) /
     1     0.00000000000000D+00,    -0.82126382171975D-01,
     1    -0.16147155504421D+00,    -0.23641835049800D+00,
     1    -0.30630149012313D+00,    -0.37092462121278D+00,
     1    -0.43029751852875D+00,    -0.48451885187131D+00,
     1    -0.53372389372667D+00,    -0.57806043560317D+00,
     1    -0.61767742771610D+00 /
       data (xmode(16,k),k=1,11) /
     1     0.00000000000000D+00,    -0.79489463207065D-01,
     1    -0.15631143507548D+00,    -0.22878208340058D+00,
     1    -0.29613207297710D+00,    -0.35810298654388D+00,
     1    -0.41468163581702D+00,    -0.46596729218972D+00,
     1    -0.51210830952884D+00,    -0.55327160310859D+00,
     1    -0.58962760179645D+00 /
       data (xmode(17,k),k=1,11) /
     1     0.00000000000000D+00,    -0.76064482302016D-01,
     1    -0.14963845366909D+00,    -0.21903622234362D+00,
     1    -0.28340831766990D+00,    -0.34243001920638D+00,
     1    -0.39605182363061D+00,    -0.44435931842270D+00,
     1    -0.48750098877404D+00,    -0.52565153613835D+00,
     1    -0.55899313300071D+00 /
       data (xmode(18,k),k=1,11) /
     1     0.00000000000000D+00,    -0.72077552495445D-01,
     1    -0.14187946885294D+00,    -0.20777189976057D+00,
     1    -0.26885502781022D+00,    -0.32474112609522D+00,
     1    -0.37533570353553D+00,    -0.42069861903991D+00,
     1    -0.46096653610256D+00,    -0.49631116671345D+00,
     1    -0.52691699645605D+00 /
       data (xmode(19,k),k=1,11) /
     1     0.00000000000000D+00,    -0.67718705272770D-01,
     1    -0.13339474382181D+00,    -0.19548660622244D+00,
     1    -0.25307535336716D+00,    -0.30572008509666D+00,
     1    -0.35327791006112D+00,    -0.39577421924630D+00,
     1    -0.43332387073501D+00,    -0.46608603971739D+00,
     1    -0.49423883569454D+00 /
       data (xmode(20,k),k=1,11) /
     1     0.00000000000000D+00,    -0.63142672403887D-01,
     1    -0.12447924019659D+00,    -0.18258809674382D+00,
     1    -0.23656038057166D+00,    -0.28591646393569D+00,
     1    -0.33046666866246D+00,    -0.37019699546588D+00,
     1    -0.40519308053053D+00,    -0.43559361537281D+00,
     1    -0.46156291180663D+00 /
       data (xmode(21,k),k=1,11) /
     1     0.00000000000000D+00,    -0.58472238382292D-01,
     1    -0.11536852857902D+00,    -0.16940338700104D+00,
     1    -0.21970295804413D+00,    -0.26576556884867D+00,
     1    -0.30736077241822D+00,    -0.34443405847033D+00,
     1    -0.37703720446053D+00,    -0.40528256567405D+00,
     1    -0.42931452982196D+00 /
       data (xmode(22,k),k=1,11) /
     1     0.00000000000000D+00,    -0.53802660599274D-01,
     1    -0.10624664202686D+00,    -0.15618961607811D+00,
     1    -0.20281219418208D+00,    -0.24560772862362D+00,
     1    -0.28431408849311D+00,    -0.31883860641565D+00,
     1    -0.34919745406649D+00,    -0.37547290909288D+00,
     1    -0.39778493554965D+00 /
       data (xmode(23,k),k=1,11) /
     1     0.00000000000000D+00,    -0.49206191735777D-01,
     1    -0.97254287765039D-01,    -0.14314507830768D+00,
     1    -0.18612745238528D+00,    -0.22570555358958D+00,
     1    -0.26159652966195D+00,    -0.29367472899590D+00,
     1    -0.32192167374174D+00,    -0.34638793056251D+00,
     1    -0.36716625529671D+00 /
       data (xmode(24,k),k=1,11) /
     1     0.00000000000000D+00,    -0.44736402187762D-01,
     1    -0.88496591591784D-01,    -0.13041948527213D+00,
     1    -0.16983075592696D+00,    -0.20625877555087D+00,
     1    -0.23941144307529D+00,    -0.26913739663626D+00,
     1    -0.29538674662811D+00,    -0.31817889260719D+00,
     1    -0.33757821806207D+00 /
       data (xmode(25,k),k=1,11) /
     1     0.00000000000000D+00,    -0.40431790724405D-01,
     1    -0.80049960322315D-01,    -0.11812311815886D+00,
     1    -0.15405760132746D+00,    -0.18741653900789D+00,
     1    -0.21790970162639D+00,    -0.24536830505830D+00,
     1    -0.26971625859397D+00,    -0.29094412987022D+00,
     1    -0.30908839700684D+00 /
       data (xmode(26,k),k=1,11) /
     1     0.00000000000000D+00,    -0.36319053125339D-01,
     1    -0.71968036286702D-01,    -0.10633463541732D+00,
     1    -0.13890609468236D+00,    -0.16928773746386D+00,
     1    -0.19720096541985D+00,    -0.22246832109140D+00,
     1    -0.24499390226619D+00,    -0.26474345517821D+00,
     1    -0.28172745296993D+00 /
       data (xmode(27,k),k=1,11) /
     1     0.00000000000000D+00,    -0.32415648687274D-01,
     1    -0.64286518246116D-01,    -0.95107683021512D-01,
     1    -0.12444459537699D+00,    -0.15194927664915D+00,
     1    -0.17736268081647D+00,    -0.20050721262439D+00,
     1    -0.22127405896060D+00,    -0.23960928085127D+00,
     1    -0.25550070486316D+00 /
       data (xmode(28,k),k=1,11) /
     1     0.00000000000000D+00,    -0.28731946186733D-01,
     1    -0.57027294213907D-01,    -0.84476363886071D-01,
     1    -0.11071802982080D+00,    -0.13545287285404D+00,
     1    -0.15844734470149D+00,    -0.17953108502411D+00,
     1    -0.19858955646276D+00,    -0.21555495810498D+00,
     1    -0.23039679838508D+00 /
       data (xmode(29,k),k=1,11) /
     1     0.00000000000000D+00,    -0.25272957172097D-01,
     1    -0.50201652467960D-01,    -0.74459672759155D-01,
     1    -0.97753071798057D-01,    -0.11983057064368D+00,
     1    -0.14048804068433D+00,    -0.15956839092918D+00,
     1    -0.17695796699406D+00,    -0.19258118632086D+00,
     1    -0.20639429849741D+00 /
       data (xmode(30,k),k=1,11) /
     1     0.00000000000000D+00,    -0.22039657435900D-01,
     1    -0.43813034914927D-01,    -0.65065173184101D-01,
     1    -0.85562382300248D-01,    -0.10509932212056D+00,
     1    -0.12350308444799D+00,    -0.14063436528796D+00,
     1    -0.15638613503199D+00,    -0.17068088634082D+00,
     1    -0.18346684963574D+00 /
       data (xmode(31,k),k=1,11) /
     1     0.00000000000000D+00,    -0.19030131740902D-01,
     1    -0.37858875321204D-01,    -0.56291888079491D-01,
     1    -0.74148081318601D-01,    -0.91264549404356D-01,
     1    -0.10749969037167D+00,    -0.12273483180042D+00,
     1    -0.13687400942478D+00,    -0.14984300227558D+00,
     1    -0.16158726574250D+00 /
       data (xmode(32,k),k=1,11) /
     1     0.00000000000000D+00,    -0.16240365819905D-01,
     1    -0.32332572972309D-01,    -0.48132633012162D-01,
     1    -0.63504493110937D-01,    -0.78323158963012D-01,
     1    -0.92476996546772D-01,    -0.10586901911791D+00,
     1    -0.11841751470875D+00,    -0.13005566225718D+00,
     1    -0.14073080511108D+00 /
       data (xmode(33,k),k=1,11) /
     1     0.00000000000000D+00,    -0.13664955821477D-01,
     1    -0.27224589170102D-01,    -0.40575941704000D-01,
     1    -0.53620486127638D-01,    -0.66266098911419D-01,
     1    -0.78428442028390D-01,    -0.90032073881987D-01,
     1    -0.10101094924119D+00,    -0.11130863738515D+00,
     1    -0.12087814688161D+00 /
       data (xmode(34,k),k=1,11) /
     1     0.00000000000000D+00,    -0.11297624858059D-01,
     1    -0.22523608527759D-01,    -0.33607514229327D-01,
     1    -0.44481314499041D-01,    -0.55080299219731D-01,
     1    -0.65344033435447D-01,    -0.75216990792550D-01,
     1    -0.84649025575916D-01,    -0.93595640230264D-01,
     1    -0.10201808041218D+00 /
       data (xmode(35,k),k=1,11) /
     1     0.00000000000000D+00,    -0.91316272769341D-02,
     1    -0.18217263445048D-01,    -0.27211546317132D-01,
     1    -0.36070178133065D-01,    -0.44750566956848D-01,
     1    -0.53212097463590D-01,    -0.61416691135684D-01,
     1    -0.69328990219443D-01,    -0.76916608401456D-01,
     1    -0.84150275786627D-01 /
       data (xmode(36,k),k=1,11) /
     1     0.00000000000000D+00,    -0.71600653371371D-02,
     1    -0.14292939018006D-01,    -0.21371615897691D-01,
     1    -0.28369587791403D-01,    -0.35261039520119D-01,
     1    -0.42021054613031D-01,    -0.48625730089845D-01,
     1    -0.55052445983032D-01,    -0.61279880265732D-01,
     1    -0.67288161670600D-01 /
       data (xmode(37,k),k=1,11) /
     1     0.00000000000000D+00,    -0.53762618924156D-02,
     1    -0.10738206442702D-01,    -0.16071596803371D-01,
     1    -0.21362381431136D-01,    -0.26596695478767D-01,
     1    -0.31761027117342D-01,    -0.36842258425788D-01,
     1    -0.41827615569277D-01,    -0.46704886003214D-01,
     1    -0.51462431603813D-01 /
       data (xmode(38,k),k=1,11) /
     1     0.00000000000000D+00,    -0.37737630643902D-02,
     1    -0.75413360126101D-02,    -0.11296451886988D-01,
     1    -0.15032944514410D-01,    -0.18744699225404D-01,
     1    -0.22425690495267D-01,    -0.26069998184986D-01,
     1    -0.29671733882094D-01,    -0.33225284714905D-01,
     1    -0.36725016935515D-01 /
       data (xmode(39,k),k=1,11) /
     1     0.00000000000000D+00,    -0.23468279377938D-02,
     1    -0.46917344797082D-02,    -0.70328058541212D-02,
     1    -0.93681736281560D-02,    -0.11695885202805D-01,
     1    -0.14014073430291D-01,    -0.16320860452078D-01,
     1    -0.18614404526695D-01,    -0.20892855307622D-01,
     1    -0.23154378270759D-01 /
       data (xmode(40,k),k=1,11) /
     1     0.00000000000000D+00,    -0.10903113044483D-02,
     1    -0.21803311717577D-02,    -0.32698633573531D-02,
     1    -0.43586537585473D-02,    -0.54464509942061D-02,
     1    -0.65329871783311D-02,    -0.76180538996956D-02,
     1    -0.87013644584800D-02,    -0.97826953222992D-02,
     1    -0.10861761978370D-01 /
       data (xmode(41,k),k=1,11) /
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00,     0.00000000000000D+00,
     1     0.00000000000000D+00 /


c     error checking (also takes care of alpha=2 case)      
      if ((alpha .le. 0.0d0) .or. (alpha .ge. 2.0d0) .or. 
     1    (beta .lt. -1.0d0) .or. (beta .gt. 1.0) ) then 
        x = 0.0d0
        return
      endif  

c     the array xmode contains numerically derived values of the mode.  
c        alpha grid: 0.0 to 2.0, steps of size da
c        beta grid:  0.0 to 1.0, steps of size db
c        xmode(i,j) = location of the mode for an 
c                     S0(alpha(i),beta(j),1,0) distribution.  
c     The algorthim is to find the flanking alpha and beta values
c     and do a linear interpolation on the tabulated values.
      da = 0.05d0
      db = 0.10d0
      i = 1+IDINT(alpha/da)
      i = min(nalpha-1,max(i,1))
      babs = dabs(beta)
      j = 1+IDINT(babs/db)
      j = min(nbeta-1,max(j,1))

      x1 = xmode(i,j)
      x2 = xmode(i+1,j)
      x3 = xmode(i,j+1)
      x4 = xmode(i+1,j+1)
      c1 = (alpha-da*(i-1))/da
      c2 = (babs-db*(j-1))/db
      y1 = (1.0d0-c1)*x1+c1*x2
      y2 = (1.0d0-c1)*x3+c1*x4

      x = (1.0d0-c2)*y1+c2*y2
      
      if (beta .lt. 0.0d0) then
        x = -x
      endif
                 
      return
      end
      
