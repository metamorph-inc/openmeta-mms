/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple NJL5501R (+patient) Test Bench					     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
#ifndef TEST_NJL5501R_H
#define TEST_NJL5501R_H

#include <systemc.h>
#include <NJL5501R/njl5501r.h>

SC_MODULE(test_njl5501r) {

	sc_in<bool>    clk;

	// DUT ports
	sc_out<sc_uint<10>>	red_led;
	sc_out<sc_uint<10>>	ir_led;

	// Signals

	// Local Vars
	int error_cnt;

/////////////////////////////////////////////////////////////////////
////                                                             ////
////              Test Bench Library                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

void show_errors(void) {
	cout << "+----------------------+" << endl;
	cout << "| TOTAL ERRORS: " << error_cnt << endl;
	cout << "+----------------------+" << endl << endl;
}



/////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////
////                                                             ////
////              Test Case Collection                           ////
////                                                             ////
/////////////////////////////////////////////////////////////////////




void test_case_1(void) {
	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** TEST CASE 1                                ***" << endl;
	cout << "**************************************************" << endl << endl;

	red_led.write(0);
	ir_led.write(2047);
	for (int j = 0; j < 10; j++) {
		for (int i = 0; i < 4; i++) wait(clk.posedge_event());
		red_led.write(ir_led.read()/2);
		ir_led.write(red_led.read()/2);
	}
	cout << endl;
	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

/////////////////////////////////////////////////////////////////////

	
	void init(void) {
		error_cnt = 0;

		//for (int i = 0; i < 10; i++) wait(clk.posedge_event());

		test_case_1();

		//for (int i = 0; i < 10; i++) wait(clk.posedge_event());
		sc_stop();
	}

	SC_CTOR(test_njl5501r) {
		SC_THREAD(init);
		sensitive << clk.pos();
	}
};

#endif // TEST_NJL5501R_H