/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple UART Test Bench								     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////


#ifndef TEST_UART_H
#define TEST_UART_H

#include <systemc.h>
#include <UART/uart.h>

#define TEST_VEC_LENGTH	128
#define TEST_BURST_SIZE 64

SC_MODULE(test_uart) {

	sc_in<bool>   clk;
	sc_out<bool>   rst;

	sc_out<bool>	 txd;
	sc_in<bool>	 rxd;

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




void test_case_1(void) {
	sc_uint<8> magic = 76;

	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** TEST CASE 1                                ***" << endl;
	cout << "**************************************************" << endl << endl;

	cout << endl;

	if (!tx_empty.read()) {
		cout << "ERROR: UART TX register is not empty " <<
					" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	tx_data.write(magic);
	tx_data_wr.write(true);
	wait(clk.posedge_event());
	tx_data_wr.write(false);
	
	for (int i = 0; i < 10000; i++) wait(clk.posedge_event());

	if (!rx_avail.read()) {
		cout << "ERROR: UART RX register is empty " <<
					" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	if (rx_data.read() != magic) {
		cout << "ERROR: UART RX data mismatch, received: " << rx_data.read() <<
					" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	rx_data_rd.write(true);
	wait(clk.posedge_event());
	rx_data_rd.write(false);
	for (int i = 0; i < 100; i++) wait(clk.posedge_event());

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

/////////////////////////////////////////////////////////////////////

	void uart_loopback(void) {
		txd.write(rxd.read());
	}

	void init(void) {
		error_cnt = 0;

		rst.write(false);
		for (int i = 0; i < 10; i++) wait(clk.posedge_event());
		rst.write(true);

		test_case_1();

		for (int i = 0; i < 500; i++) wait(clk.posedge_event());
		sc_stop();
	}

	SC_CTOR(test_uart) {
		SC_METHOD(uart_loopback);
		sensitive << rxd;
		SC_THREAD(init);
		sensitive << clk.pos();
	}
};


#endif // TEST_UART_H