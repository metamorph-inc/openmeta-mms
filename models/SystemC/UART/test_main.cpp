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

#include <stdlib.h>
#include <time.h>
#include <systemc.h>
#include <UART/uart.h>
#include "test_uart.h"

#define VCD_OUTPUT_ENABLE

int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_NS);

	sc_clock clk("clock", 20.84, SC_NS);
	sc_signal<bool>	rst;

	sc_signal<bool>	txd, rxd;
	sc_signal<bool>   tx_data_wr;
	sc_signal<sc_uint<8>> tx_data;
	sc_signal<bool>  tx_empty;

	sc_signal<bool>   rx_data_rd;
	sc_signal<sc_uint<8>> rx_data;
	sc_signal<bool>  rx_avail;

	uart			i_uart("UART");
	test_uart			i_test("TEST");

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

	i_test.clk(clk);
	i_test.rst(rst);
	i_test.txd(rxd);	
	i_test.rxd(txd);	
	i_test.tx_data_wr(tx_data_wr);
	i_test.tx_data(tx_data);
	i_test.tx_empty(tx_empty);
	i_test.rx_data_rd(rx_data_rd);
	i_test.rx_data(rx_data);
	i_test.rx_avail(rx_avail);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_UART");
	sc_trace(vcd_log, clk, "Clk");
	sc_trace(vcd_log, rst, "Reset_n");
	sc_trace(vcd_log, txd, "TXD");
	sc_trace(vcd_log, rxd, "RXD");

	sc_trace(vcd_log, tx_data_wr, "tx_data_wr");
	sc_trace(vcd_log, tx_data, "tx_data");
	sc_trace(vcd_log, tx_empty, "tx_empty");

	sc_trace(vcd_log, rx_data_rd, "rx_data_rd");
	sc_trace(vcd_log, rx_data, "rx_data");
	sc_trace(vcd_log, rx_avail, "rx_avail");
#endif

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif

	return i_test.error_cnt;
}

