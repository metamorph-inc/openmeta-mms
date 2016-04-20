/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple SPI Test Bench								     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef TEST_SPI_H
#define TEST_SPI_H

#include <systemc.h>
#include <GPIO/gpio.h>

SC_MODULE(test_spi) {

	sc_in<bool>    clk;
	sc_out<bool>   rst;

	// DUT ports

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

	cout << endl;

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

/////////////////////////////////////////////////////////////////////

	
	void init(void) {
		error_cnt = 0;

		rst.write(false);
		for (int i = 0; i < 10; i++) wait(clk.posedge_event());
		rst.write(true);

		test_case_1();

		for (int i = 0; i < 500; i++) wait(clk.posedge_event());
		sc_stop();
	}

	SC_CTOR(test_spi) {
		SC_THREAD(init);
		sensitive << clk.pos();
	}
};

#endif // TEST_SPI_H
