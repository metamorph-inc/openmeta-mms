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

#ifndef TEST_PWMDAC_H
#define TEST_PWMDAC_H

#include <systemc.h>
#include <PWMDAC/pwmdac.h>

SC_MODULE(test_pwmdac) {

	sc_in<bool>    clk;

	// DUT ports
	sc_out<sc_logic>    pwm;
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
	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** TEST CASE 1                                ***" << endl;
	cout << "**************************************************" << endl << endl;

	for (double duty = 0.0; duty < 1.0; duty += 0.01) {
		int period = 256;
		int ontime = period * duty;
		sc_uint<10> expected = 1023 * ontime / period;
		for (int n = 0; n < 10; n++) {
			int c = 0;
            pwm.write(sc_dt::Log_1);
			for (; c < ontime; c++) wait(clk.posedge_event());
			pwm.write(sc_dt::Log_0);
			for (; c < period; c++) wait(clk.posedge_event());
		}
		if (expected != analog.read()) {
			cout << "Analog output mismatch, expected: " << expected << " got: " << analog.read() << endl;
			error_cnt++;
		}
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

	SC_CTOR(test_pwmdac) {
		SC_THREAD(init);
		sensitive << clk.pos();
	}
};

#endif // TEST_PWMDAC_H
