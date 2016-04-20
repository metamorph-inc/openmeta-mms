/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple UART Model	                             ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef UART_H
#define UART_H

#include <systemc.h>

SC_MODULE(uart) {

	// ports
	sc_in<bool>   clk;
	sc_in<bool>   rst;

	sc_out<bool>  txd;
	sc_in<bool>	  rxd;

	sc_in<bool>   tx_data_wr;
	sc_in<sc_uint<8>> tx_data;
	sc_out<bool>  tx_empty;

	sc_in<bool>   rx_data_rd;
	sc_out<sc_uint<8>> rx_data;
	sc_out<bool>  rx_avail;

	SC_CTOR(uart) : divisor(10) {
		
		SC_METHOD(common_clk_gen);
		dont_initialize();
		sensitive << clk.pos() << rst;

		SC_METHOD(rx_clk_gen);
		dont_initialize();
		sensitive << clk.pos() << rst;

		SC_METHOD(tx_clk_gen);
		dont_initialize();
		sensitive << clk.pos() << rst;

		SC_METHOD(transmit_data);
		dont_initialize();
		sensitive << clk.pos() << rst;

		SC_METHOD(receive_data);
		dont_initialize();
		sensitive << clk.pos() << rst;
	}

//private:
	void common_clk_gen();
	void rx_clk_gen();
	void tx_clk_gen();
	void transmit_data();
	void receive_data();

	const sc_uint<16>	divisor;

	sc_signal<bool> common_clk;
	sc_uint<16> common_clk_div;

	sc_signal<bool> tx_clk;
	sc_uint<16> tx_clk_div;

	sc_signal<bool> rx_clk;
	sc_uint<16> rx_clk_div;

	sc_signal<bool> rx_clear_clk_div;

	sc_uint<10> tx_reg;
	sc_uint<8> tx_reg_in;
	sc_uint<16> tx_bit_cnt;
	enum tx_state_t { IDLE_TX, LOAD_TX, SHIFT_TX, STOP_TX };
	tx_state_t tx_FSM;

	sc_uint<8> rx_reg;
	sc_uint<16> rx_bit_cnt;
	enum rx_state_t { IDLE_RX, START_RX, EDGE_RX, SHIFT_RX, STOP_RX, DONE_RX, RX_OVF};
	rx_state_t rx_FSM;
};

#endif // UART_H