! Gamma function in double precision
!
      double precision function dgamma(x)
      implicit double precision (a - h, o - z)
      parameter (
     &    p0 = 0.999999999999999990d+00, 
     &    p1 = -0.422784335098466784d+00, 
     &    p2 = -0.233093736421782878d+00, 
     &    p3 = 0.191091101387638410d+00, 
     &    p4 = -0.024552490005641278d+00, 
     &    p5 = -0.017645244547851414d+00, 
     &    p6 = 0.008023273027855346d+00)
      parameter (
     &    p7 = -0.000804329819255744d+00, 
     &    p8 = -0.000360837876648255d+00, 
     &    p9 = 0.000145596568617526d+00, 
     &    p10 = -0.000017545539395205d+00, 
     &    p11 = -0.000002591225267689d+00, 
     &    p12 = 0.000001337767384067d+00, 
     &    p13 = -0.000000199542863674d+00)
      n = nint(x - 2)
      w = x - (n + 2)
      y = ((((((((((((p13 * w + p12) * w + p11) * w + p10) * 
     &    w + p9) * w + p8) * w + p7) * w + p6) * w + p5) * 
     &    w + p4) * w + p3) * w + p2) * w + p1) * w + p0
      if (n .gt. 0) then
          w = x - 1
          do k = 2, n
              w = w * (x - k)
          end do
      else
          w = 1
          do k = 0, -n - 1
              y = y * (x + k)
          end do
      end if
      dgamma = w / y
      end
!
