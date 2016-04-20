/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple GPIO Test Bench								     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
#ifndef TEST_GPIO_H
#define TEST_GPIO_H

#include <systemc.h>
#include <GPIO/gpio.h>

SC_MODULE(test_gpio) {

	sc_in<bool>    clk;
	sc_out<bool>   rst;

	// DUT ports
	sc_inout<sc_lv<8>>	pin;

	sc_out<sc_uint<8>>  data_out;
	sc_out<bool>		data_out_wr;
	sc_out<sc_uint<8>>	oe;
	sc_out<bool>		oe_wr;
	sc_in<sc_uint<8>>	data_in;

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
	const sc_uint<8> out_test_x[] =  {0x00, 0x01, 0x55, 0xFF, 0x00, 0x01, 0x55, 0xFF};
	const sc_uint<8> out_test_oe[] = {0xFF, 0xFF, 0xFF, 0xFF, 0x0F, 0x0F, 0x0F, 0x0F};


	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** TEST CASE 1                                ***" << endl;
	cout << "**************************************************" << endl << endl;

	cout << endl;

	pin.write("ZZZZZZZZ");
	for (int i = 0; i < (sizeof(out_test_x)/sizeof(sc_uint<8>)); i++) {
		wait(clk.posedge_event());
		data_out.write(out_test_x[i]);
		oe.write(out_test_oe[i]);
		data_out_wr.write(true);
		oe_wr.write(true);
		wait(clk.posedge_event());
		data_out_wr.write(false);
		oe_wr.write(false);

		for (int i = 0; i < 10; i++) wait(clk.posedge_event());

	}
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

	SC_CTOR(test_gpio) {
		SC_THREAD(init);
		sensitive << clk.pos();
	}
};

#endif // TEST_GPIO_H
