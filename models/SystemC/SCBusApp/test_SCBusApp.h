/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Android Application for SCBus Testing (uC modul + software)////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef TEST_SCBUSAPP_H
#define TEST_SCBUSAPP_H

#include <math.h>
#include <SCBusApp/SCBusApp.h>
#include <POR/por.h>

#define PI 3.14159265

SC_MODULE(test_SCBusApp) {

	typedef sc_uint<10>	analog_type; 

	sc_clock    clk;

	// DUT ports
	sc_out<analog_type>	analog0;
	sc_out<analog_type>	analog1;

	// Signals
	sc_signal<bool>	   rst;

	por		i_por;
	double phase0, dp0;
	double phase1, dp1;

	void init(void) {
		while (!rst.read()) wait(rst.value_changed_event());
		for (int i = 0; i < 100; i++) wait(clk.posedge_event());

		//test_case_1();

		//for (int i = 0; i < 10; i++) wait(clk.posedge_event());
		//sc_stop();
	}

	void analog_gen()
	{
		analog0.write(512.0 * (1.0 + sin(phase0))); phase0 += dp0;
		analog1.write(512.0 * (1.0 + sin(phase1))); phase1 += dp1;
	}

	SC_CTOR(test_SCBusApp) : 
		i_por("POR"), clk("clk", 100, SC_US),
		phase0(0.0), dp0(PI/100), phase1(PI), dp1(PI/40)
	{

		i_por.rst(rst);

		SC_THREAD(init);
			sensitive << clk.posedge_event();
		SC_METHOD(analog_gen);
			dont_initialize();
			sensitive << clk.posedge_event();
	}
};


#endif //TEST_SCBUSAPP_H