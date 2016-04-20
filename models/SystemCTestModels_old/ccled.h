///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////
////  LOCAL_CCLED -- Constant Current LED driver.
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

#ifndef LOCAL_CCLED_H
#define LOCAL_CCLED_H

#define REGISTER_SZ (8)							// Number of bits in the registers.
#define MAX_REGISTER_BIT_NO (REGISTER_SZ - 1)	// Maximum register bit number, starting with bit 0.

// Output logic: '0' == current sink ON, 'Z' == current sink OFF.
#define LOCAL_CCLED_ON SC_LOGIC_0
#define LOCAL_CCLED_OFF SC_LOGIC_Z

#include <systemc.h>

SC_MODULE(local_ccled) {

	// ports
	sc_in<sc_logic>   sdi_clk;	// clocks data into the shift register on the rising edge.
	sc_in<sc_logic>   sdi;		// serial data input
	sc_in<sc_logic>   le;		// latch enable, active high; latches data into the data register
	sc_in<sc_logic>   oeBar;	// output enable, active low; drives current sinks from the data register

	sc_out<sc_logic>  sdo;		// serial data output

	// The outputs are not combined into a single vector to make it easier to connect to
	// individual output signals in the GME/CyPhy component model.
	sc_out<sc_logic> out0;
	sc_out<sc_logic> out1;
	sc_out<sc_logic> out2;
	sc_out<sc_logic> out3;
	sc_out<sc_logic> out4;
	sc_out<sc_logic> out5;
	sc_out<sc_logic> out6;
	sc_out<sc_logic> out7;

	SC_CTOR(local_ccled){	

		SC_METHOD(toggleShift);
		dont_initialize();
		sensitive << sdi_clk.pos();

		SC_METHOD(latchData);
		dont_initialize();
		sensitive << le << srChanged;

		SC_METHOD(enableOutput);
		dont_initialize();
		sensitive << oeBar << drChanged;
	}

	//private methods:
	void toggleShift();
	void latchData();
	void enableOutput();

	// private registers:
	sc_uint<REGISTER_SZ> shiftRegister;
	sc_uint<REGISTER_SZ> dataRegister;

	// private events
	sc_event srChanged;
	sc_event drChanged;
};

#endif // LOCAL_CCLED_H
