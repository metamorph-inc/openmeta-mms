/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple UART Model										     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#include "uart.h"

void uart::common_clk_gen()
{
	 if (!rst.read()) {
		 common_clk.write(false);
		 common_clk_div = 0;
	 }
	 else {
		 common_clk.write(false);
		 if (common_clk_div == divisor) {
			 common_clk.write(true);
			 common_clk_div = 0;
		 }
		 else {
			 common_clk_div += 1;
		 }
	 }
}

void uart::tx_clk_gen()
{
	if (!rst.read()) {
		 tx_clk.write(false);
		 tx_clk_div = 0;
	 }
	 else {
		 tx_clk.write(false);
		 if (common_clk.read()) {
			 if (tx_clk_div == 15) {
				 tx_clk.write(true);
				 tx_clk_div = 0;
			 }
			 else {
				 tx_clk_div += 1;
			 }
		 }
	 }
}

void uart::rx_clk_gen()
{
	if (!rst.read()) {
		 rx_clk.write(false);
		 rx_clk_div = 0;
	 }
	 else {
		 rx_clk.write(false);
		 if (rx_clear_clk_div.read()) {
			 rx_clk_div = 0;
		 }
		 else if (common_clk.read()) {
			 if (rx_clk_div == 7) {
				 rx_clk.write(true);
				 rx_clk_div = 0;
			 }
			 else {
				 rx_clk_div += 1;
			 }
		 }
	 }
}

void uart::transmit_data()
{
	if (!rst.read()) {
		tx_reg = 0xFF;
		txd.write(tx_reg.bit(0));
		tx_bit_cnt = 0;
		tx_FSM = IDLE_TX;
		tx_empty.write(true);
		tx_reg_in = 0;
	}
	else {
		tx_empty.write(false);	// unless otherwise
		switch (tx_FSM) {
		case IDLE_TX:
			if (tx_data_wr.read()) {
				tx_reg_in = tx_data.read();
				tx_FSM = LOAD_TX;
			}
			else {
				tx_empty.write(true);
			}
			break;
		case LOAD_TX:
			if (tx_clk.read()) {
				tx_bit_cnt = 8 + 1;
				tx_reg = (sc_uint<1>(1), tx_reg_in, sc_uint<1>(0));
				txd.write(tx_reg.bit(0));
				tx_FSM = SHIFT_TX;
			}
			break;
		case SHIFT_TX:
			if (tx_clk.read()) {
				tx_reg = (sc_uint<1>(1), tx_reg.range(9, 1));
				txd.write(tx_reg.bit(0));
				if (tx_bit_cnt-- == 1) {
					tx_FSM = STOP_TX;
				}
			}
			break;
		case STOP_TX:
			if (tx_clk.read()) {
				tx_FSM = IDLE_TX;
			}
			break;
		default:
			tx_FSM = IDLE_TX;
		}
	}
}

void uart::receive_data()
{
	if (!rst.read()) {
		rx_reg = 0;
		rx_data.write(rx_reg);
		rx_avail.write(false);
		rx_bit_cnt = 0;
		rx_FSM = IDLE_RX;
		rx_clear_clk_div.write(false);
	}
	else {
		rx_clear_clk_div.write(false);	// unless otherwise
		rx_avail.write(false);

		switch (rx_FSM) {
		case IDLE_RX:
			rx_bit_cnt = 0;
			if (common_clk.read()) {
				if (!rxd.read()) {
					rx_FSM = START_RX;
					rx_clear_clk_div.write(1);	// bit sync
				}
			}
			break;
		case START_RX:
			if (rx_clk.read()) {
				if (rxd.read()) {	// framing error
					cout << "uart: start bit / framing error (" << sc_time_stamp() << ")" << endl;
					rx_FSM = RX_OVF;
				}
				else {
					rx_FSM = EDGE_RX;
				}
			}
			break;
		case EDGE_RX:
			if (rx_clk.read()) {
				if (rx_bit_cnt == 8) {
					rx_FSM = STOP_RX;
				}
				else {
					rx_FSM = SHIFT_RX;
				}
			}
			break;
		case SHIFT_RX:
			if (rx_clk.read()) {
				rx_bit_cnt++;
				rx_reg = (rxd.read(), rx_reg.range(7, 1));
				rx_FSM = EDGE_RX;
			}
			break;
		case STOP_RX:
			if (rx_clk.read()) {
				rx_FSM = DONE_RX;
			}
			break;
		case DONE_RX:
			rx_data.write(rx_reg);
			rx_avail.write(true);
			if (rx_data_rd.read()) {
				rx_FSM = IDLE_RX;
			}
			break;
		case RX_OVF:
			if (rxd.read()) {
				rx_FSM = IDLE_RX;
			}
			break;
		default:
			rx_FSM = IDLE_RX;
		}
	}
}

