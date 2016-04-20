#pragma once

// See also: http://en.wikipedia.org/wiki/Linear_congruential_generator

typedef unsigned long long ulong;

#define ARAND_M_CONST (25214903917ULL)
#define ARAND_A_CONST (11)
#define ARAND_C_CONST (281474976710656ULL)

class ArduinoRandom
{
private:
	static ulong Xn;
	static ulong lcg();
public:
	static void randomSeed(long seed);
	static long random(long min, long max);
};

