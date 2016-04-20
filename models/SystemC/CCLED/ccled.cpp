///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////
////  CCLED -- Constant Current LED driver.
////
////  This SystemC model is based on the STMicroelectronics STP08DP05.  It has 8 constant current sinks that
////  are controlled by an 8-bit output register.  Data is latched into the output register from an 8-bit
////  shift register. One data signal is clocked into the shift register.
////
////  The STP08DP05 data sheet can be found here:
////  http://www.st.com/st-web-ui/static/active/en/resource/technical/document/datasheet/CD00156241.pdf 
////
////  See also MOT-502, "Create SystemC model and running test bench for LED Driver component"
////   
////  Author: Henry Forson, MetaMorph, Inc.
////          hforson@metamorphsoftware.com
////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CCLED.h"

void ccled::toggleShift()
{
	// Shift the shift register
	int tmp = shiftRegister.to_int() << 1;

	if( sdi.read() == SC_LOGIC_1 )
	{
		tmp |= 1;
	}
	shiftRegister = tmp;
	sdo.write( sc_logic( (tmp >> MAX_REGISTER_BIT_NO) & 1));
	srChanged.notify();
}


void ccled::latchData()
{
	if( le.read() == SC_LOGIC_1 )
	{
		int tmp = shiftRegister;
		dataRegister = tmp;
		drChanged.notify();
	}
}



void ccled::enableOutput()
{
	if( oeBar.read() == SC_LOGIC_0 )
	{
		// Enable outputs based on the data register.
		int tmp = dataRegister.to_int();
		int bitNo = 0;
		out0 = ( tmp & (1 << (bitNo++)) ) ? CCLED_ON : CCLED_OFF;
		out1 = ( tmp & (1 << (bitNo++)) ) ? CCLED_ON : CCLED_OFF;
		out2 = ( tmp & (1 << (bitNo++)) ) ? CCLED_ON : CCLED_OFF;
		out3 = ( tmp & (1 << (bitNo++)) ) ? CCLED_ON : CCLED_OFF;
		out4 = ( tmp & (1 << (bitNo++)) ) ? CCLED_ON : CCLED_OFF;
		out5 = ( tmp & (1 << (bitNo++)) ) ? CCLED_ON : CCLED_OFF;
		out6 = ( tmp & (1 << (bitNo++)) ) ? CCLED_ON : CCLED_OFF;
		out7 = ( tmp & (1 << (bitNo++)) ) ? CCLED_ON : CCLED_OFF;
	}	
	else
	{
		// Set output to high impedance.
		out0 = CCLED_OFF;
		out1 = CCLED_OFF;
		out2 = CCLED_OFF;
		out3 = CCLED_OFF;
		out4 = CCLED_OFF;
		out5 = CCLED_OFF;
		out6 = CCLED_OFF;
		out7 = CCLED_OFF;
	}
}


