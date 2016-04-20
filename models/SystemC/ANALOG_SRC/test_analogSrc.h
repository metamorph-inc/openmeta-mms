/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple PWM as DAC Model Test Bench					     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef TEST_ANALOGSRC_H
#define TEST_ANALOGSRC_H

#include <systemc.h>
#include <ANALOG_SRC/analogSrc.h>

SC_MODULE(test_analogSrc) {

	sc_in<bool>    clk;

	// DUT ports
	//sc_out<sc_logic>    pwm;
	sc_in<sc_uint<10>>	analog;
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
	sc_uint<10> ana_val;
	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** TEST CASE 1                                ***" << endl;
	cout << "**************************************************" << endl << endl;
	for (int i = 0; i < 100; i++)
	{
		wait(100,SC_MS);
		ana_val = analog.read();
		cout << "Analog output : " << ana_val << " at: " << sc_time_stamp() << endl;
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

	SC_CTOR(test_analogSrc) {
		SC_THREAD(init);
		sensitive << clk.pos();
	}
};

#endif // TEST_ANALOGSRC_H
