/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Pulse Oxymeter Application (uC modul + software) testbench ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef TEST_PULSEOXYAPP_H
#define TEST_PULSEOXYAPP_H

#include <systemc.h>
#include <PulseOxyApp/PulseOxyApp.h>
#include <UART/uart.h>
#include <POR/por.h>


SC_MODULE(test_PulseOxyApp) {

	sc_clock    clk;

	// DUT ports
	sc_out<bool>  txd;
	sc_in<bool>	  rxd;

	// Signals
	sc_signal<bool>	   rst;
	sc_signal<bool>   tx_data_wr;
	sc_signal<sc_uint<8>> tx_data;
	sc_signal<bool>  tx_empty;

	sc_signal<bool>   rx_data_rd;
	sc_signal<sc_uint<8>> rx_data;
	sc_signal<bool>  rx_avail;

	// Local vars
	int error_cnt;

	uart	i_uart;
	por		i_por;
	
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

	tx_data.write(0x74);
	tx_data_wr.write(true);
	wait(clk.posedge_event());
	tx_data_wr.write(false);

	wait(10, SC_MS);
	
	if (rx_avail.read()) {
		cout << "Result of IR measurement: " << rx_data.read() << endl;
		rx_data_rd.write(true); wait(clk.posedge_event()); rx_data_rd.write(false); 
	}
	else {
		cout << "No response received for IR measurement request." << endl;
		error_cnt++;
	}

	tx_data.write(0x76);
	tx_data_wr.write(true);
	wait(clk.posedge_event());
	tx_data_wr.write(false);

	wait(10, SC_MS);
	
	if (rx_avail.read()) {
		cout << "Result of RED measurement: " << rx_data.read() << endl;
		rx_data_rd.write(true); wait(clk.posedge_event()); rx_data_rd.write(false); 
	}
	else {
		cout << "No response received for RED measurement request." << endl;
		error_cnt++;
	}

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

/////////////////////////////////////////////////////////////////////

	
	void init(void) {
		error_cnt = 0;

		while (!rst.read()) wait(rst.value_changed_event());
		for (int i = 0; i < 100; i++) wait(clk.posedge_event());

		test_case_1();

		//for (int i = 0; i < 10; i++) wait(clk.posedge_event());
		sc_stop();
	}

	SC_CTOR(test_PulseOxyApp) : i_uart("UART"), i_por("POR"), clk("clk", 20.84, SC_NS) {

		i_por.rst(rst);

		i_uart.clk(clk);
		i_uart.rst(rst);
		i_uart.txd(txd);
		i_uart.rxd(rxd);
		i_uart.tx_data_wr(tx_data_wr);
		i_uart.tx_data(tx_data);
		i_uart.tx_empty(tx_empty);

		i_uart.rx_data_rd(rx_data_rd);
		i_uart.rx_data(rx_data);
		i_uart.rx_avail(rx_avail);

	
		SC_THREAD(init);
			sensitive << clk.posedge_event();
	}
};


#endif //TEST_PULSEOXYAPP_H