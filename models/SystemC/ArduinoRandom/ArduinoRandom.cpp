#include "ArduinoRandom.h"

ulong ArduinoRandom::Xn = 0;

ulong ArduinoRandom::lcg()
{
	Xn = (ARAND_A_CONST * Xn + ARAND_C_CONST) % ARAND_M_CONST;
	return Xn;
}

void  ArduinoRandom::randomSeed(long seed)
{
	Xn = (ulong) seed;
	for( int i = 0; i < 100; i++ )
	{
		lcg();
		Xn ^= seed;
	}
}


long ArduinoRandom::random(long min, long max)
{
	lcg();
	if( min > max)
	{
		long tmp = min;
		min = max;
		max = tmp;
	}
	ulong bins = (ulong)(max - min);
	// get a uniformly-distributed pseudo-random number in the range [0,1).
	double udprn = ((double) Xn) / ((double) ARAND_M_CONST);
	// quantize it to a bin number
	long bin = (long) ( udprn * bins );

	// Add the min offset
	return bin + min;
}
