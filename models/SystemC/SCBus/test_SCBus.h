/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Test bench for SCBus									     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////


#ifndef TEST_SCBUS_H
#define TEST_SCBUS_H

#include <SCBus/SCBus.h>

SC_MODULE(test_SCBus) {

	sc_in<bool>   clk;
	sc_out<bool>   rst;

	sc_out<bool>   tx_data_wr;
	sc_out<sc_uint<8>> tx_data;
	sc_in<bool>  tx_empty;

	sc_out<bool>   rx_data_rd;
	sc_in<sc_uint<8>> rx_data;
	sc_in<bool>  rx_avail;

	// Signals
	sc_signal<sc_uint<32> >	wd_cnt;

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




void loopback_test(void) {

	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** LOOPBACK TEST                              ***" << endl;
	cout << "**************************************************" << endl << endl;

	cout << endl;

	while (true) {
		wait(clk.posedge_event());
		if ( rx_avail.read()) {
			char ch = char(rx_data.read());
			cout << "RX: " << ch << endl;
			rx_data_rd.write(true);
			wait(clk.posedge_event());
			rx_data_rd.write(false);
			wait(clk.posedge_event());

			tx_data.write(ch);
			tx_data_wr.write(true);
			wait(clk.posedge_event());
			tx_data_wr.write(false);
			wait(clk.posedge_event());
		}

	}

	for (int i = 0; i < 100; i++) wait(clk.posedge_event());

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

		loopback_test();

		for (int i = 0; i < 500; i++) wait(clk.posedge_event());
		sc_stop();
	}

	SC_CTOR(test_SCBus) {
		SC_THREAD(init);
		sensitive << clk.pos();
	}
};


#endif // TEST_SCBUS_H