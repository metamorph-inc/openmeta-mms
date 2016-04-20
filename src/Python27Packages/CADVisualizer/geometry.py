import math
import logging

def find_circle_center(x1, y1, x2, y2, angle):
	"""
		This works, but requires a consistent definition of a positive
		angle from a fixed x-axis, which the EAGLE brd file does not have.

		To recreate an arc given data in an EAGLE brd file, use
		find_circle_center_3pt_arc().
	"""

	logger = logging.getLogger()
	slope = (y2-y1)/(x2-x1)
	perp_slope = -1/slope
	xm = (x1 + x2)/2
	ym = (y1 + y2)/2
	dist_chord = math.sqrt((x2-x1)**2 + (y2-y1)**2)
	dist_perp = dist_chord/(2*math.tan(angle/2))

	# (y - ym) = perp_slope*(x-xm)
	# (yc - ym)^2 + (xc - xm)^2 = dist_perp^2
	# Substitute: (perp_slope^2 + 1)*(xc - xm)^2 = dist_perp

	xc = dist_perp / math.sqrt(perp_slope**2 + 1) + xm
	yc = perp_slope*(xc - xm) + ym

	return xc, yc


def find_circle_center_3pt_arc(x1, y1, x2, y2, angle):
	""" Link: http://stackoverflow.com/questions/4103405 
		Finds the center of a circle given 3 points on the circle.
	"""

	(xm, ym) = arc_midpoint(x1, y1, x2, y2, angle)

	mid12 = [0.5*(x1+x2), 0.5*(y1+y2)]
	mid2m = [0.5*(x2+xm), 0.5*(y2+ym)]

	s1inf = False
	s2inf = False

	yd1 = y2 - y1
	yd2 = ym - y2

	try:
		s1 = (y2-y1)/(x2-x1)
	except ZeroDivisionError:
		s1inf = True  # s1 = infinity
	try:	
		s2 = (ym-y2)/(xm-x2)
	except ZeroDivisionError:
		s2inf = True  # s2 = infinity

	if yd1 == 0:  # s1 = 0
		xc = mid12[0]
		yc = mid2m[1] if s2inf else mid2m[1] + (mid2m[0]-xc)/s2
	elif yd2 == 0:  # s2 = 0
		xc = mid2m[0]
		yc = mid12[1] if s1inf else mid12[1] + (mid12[0]-xc)/s1
	elif s1inf:
		yc = mid12[1]
		xc = s2*(mid2m[1]-yc) + mid2m[0]
	elif s2inf:
		yc = mid2m[1]
		xc = s1*(mid12[1]-yc) + mid12[0]
	else:
		xc = (s1*s2*(mid12[1]-mid2m[1]) - s1*mid2m[0] + s2*mid12[0]) / (s2 - s1)
		yc = mid12[1] - (xc - mid12[0]) / s1

	return xc, yc

def arc_midpoint(x1, y1, x2, y2, angle):
	"""
		Grab point on arc that is the midpoint between points 1 and 2.
	"""
	theta = angle / 2
	tan2theta = math.atan2(y2-y1, x2-x1) - math.pi/2
	d = math.sqrt((x2-x1)**2 + (y2-y1)**2)
	delta = (1 - math.cos(theta)) * 0.5*d / math.sin(theta)

	xm = 0.5*(x2+x1) + delta * math.cos(tan2theta)
	ym = 0.5*(y2+y1) + delta * math.sin(tan2theta)

	return xm, ym

def rotate_and_translate(refPkg, x, y):
	''' 
		Rotates and translates point. Eg:
		input: lower-left-hand corner of rectangle
		rotation: 90 degrees
		output: point is at lower-right-hand corner of rectangle
	'''
	# Point-wise translate center of part corresponding to frame rotation
	theta = refPkg.rotation * math.pi / 2.0
	rx = x * math.cos(theta) - y * math.sin(theta)
	ry = x * math.sin(theta) + y * math.cos(theta)

	if refPkg.rotation == 1:
		rx += refPkg.height
	elif refPkg.rotation == 2:
		rx += refPkg.width
		ry += refPkg.height
	elif refPkg.rotation == 3:
		ry += refPkg.width

	# Now translate
	rx += refPkg.x
	ry += refPkg.y

	return [rx, ry]


def rotate_vector(x, y, rot):
    ''' Rotates <X,Y> coordinate vector in multiples of 90 degrees. '''
    rot = rot % 4

    for i in xrange(rot):
        rotX = -y
        y = x
        x = rotX

    return [x, y]