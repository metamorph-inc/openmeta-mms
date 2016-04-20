/////////////////////////////////////////////////////////////////////
////                                                             ////
////  FTDI FT232R Top Level Model	                             ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////
////                                                             ////
////  USB 1.1                                                    ////
////  Endpoints Config and FIFOs Instantiation                   ////
////                                                             ////
////  SystemC Version: usb.h                                     ////
////  Author: Alfredo Luiz Foltran Fialho                        ////
////          alfoltran@ig.com.br                                ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
////                                                             ////
//// Verilog Version: usb.v                                      ////
//// Copyright (C) 2000-2002 Rudolf Usselmann                    ////
////                         www.asics.ws                        ////
////                         rudi@asics.ws                       ////
////                                                             ////
//// This source file may be used and distributed without        ////
//// restriction provided that this copyright statement is not   ////
//// removed from the file and that any derivative work contains ////
//// the original copyright notice and the associated disclaimer.////
////                                                             ////
////     THIS SOFTWARE IS PROVIDED ``AS IS'' AND WITHOUT ANY     ////
//// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED   ////
//// TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS   ////
//// FOR A PARTICULAR PURPOSE. IN NO EVENT SHALL THE AUTHOR      ////
//// OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,         ////
//// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES    ////
//// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE   ////
//// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR        ////
//// BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF  ////
//// LIABILITY, WHETHER IN  CONTRACT, STRICT LIABILITY, OR TORT  ////
//// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT  ////
//// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE         ////
//// POSSIBILITY OF SUCH DAMAGE.                                 ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef FT232R_H
#define FT232R_H

#include <systemc.h>
#include "usb_defines.h"
#include "usb_core.h"
#include "usb_fifo512x8.h"
#include "usb_fifo128x8.h"
#include <UART/uart.h>
#include <POR/por.h>

SC_MODULE(ft232r) {
  public:

	// PHY Interface
	sc_out<bool>			tx_dp, tx_dn, tx_oe;
	sc_in<bool>				rx_dp, rx_dn, rx_d;

	// UART Interface
	sc_out<bool>			uart_txd;
	sc_in<bool>				uart_rxd;

  private:

	// Misc
	sc_clock				clk;

	sc_signal<bool>			rst, vcc, gnd;
	sc_signal<sc_uint<8> >	gnd8;
	sc_signal<sc_uint<14> >	cfg1, cfg2;
	sc_signal<bool>			ep1_we_nc, ep2_re_nc;
	sc_signal<sc_uint<8> >	ep1_dout_nc;
	sc_signal<bool>			usb_rst;

	// Interrupts
	sc_signal<bool>			crc16_err;

	// Vendor Features
	sc_signal<bool>			v_set_int;
	sc_signal<bool>			v_set_feature;
	sc_signal<sc_uint<16> >	wValue;
	sc_signal<sc_uint<16> >	wIndex;
	sc_signal<sc_uint<16> >	vendor_data;

	// USB Status
	sc_signal<bool>			usb_busy;
	sc_signal<sc_uint<4> >	ep_sel;

	// Endpoint Interface
	// EP1
	sc_signal<sc_uint<8> >	ep1_f_din;
	sc_signal<bool>			ep1_f_we;
	sc_signal<bool>			ep1_f_full;

	// EP2
	sc_signal<sc_uint<8> >	ep2_f_dout;
	sc_signal<bool>			ep2_f_re;
	sc_signal<bool>			ep2_f_empty;

	// EP1
	sc_signal<sc_uint<8> >	ep1_us_din;
	sc_signal<bool>			ep1_us_re;
	sc_signal<bool>			ep1_us_empty;

	// EP2
	sc_signal<sc_uint<8> >	ep2_us_dout;
	sc_signal<bool>			ep2_us_we;
	sc_signal<bool>			ep2_us_full;// Misc

	// UART
	sc_signal<bool>			uart_tx_data_wr;
	sc_signal<sc_uint<8>>	uart_tx_data;
	sc_signal<bool>			uart_tx_empty;

	sc_signal<sc_uint<8>>	uart_rx_data;
	sc_signal<bool>			uart_rx_avail;
	sc_signal<bool>			uart_rx_data_rd;


  public:
	usb_core				*i_core;			// CORE
	usb_fifo128x8			*i_ff_ep1;			// FIFO1
	usb_fifo128x8			*i_ff_ep2;			// FIFO2
	uart					*i_uart;			// UART
	por						*i_por;				// Power on Reset

	// Destructor
	~ft232r(void) {
		if (i_core)
			delete i_core;
		if (i_ff_ep1)
			delete i_ff_ep1;
		if (i_ff_ep2)
			delete i_ff_ep2;
		if (i_uart)
			delete i_uart;
		if (i_por)
			delete i_por;
	}

	SC_CTOR(ft232r) : clk("clock", 20.84, SC_NS) {
		vcc.write(true);
		gnd.write(false);
		gnd8.write(0);

		cfg1.write(EP_BULK  | EP_IN  | 64);
		cfg2.write(EP_BULK  | EP_OUT | 64);

		// CORE Instantiation and Binding
		i_core = new usb_core("CORE");
		i_core->clk_i(clk);
		i_core->rst_i(rst);
		i_core->tx_dp(tx_dp);
		i_core->tx_dn(tx_dn);
		i_core->tx_oe(tx_oe);
		i_core->rx_dp(rx_dp);
		i_core->rx_dn(rx_dn);
		i_core->rx_d(rx_d);
		i_core->phy_tx_mode(vcc);
		i_core->usb_rst(usb_rst);
		i_core->crc16_err(crc16_err);
		i_core->v_set_int(v_set_int);
		i_core->v_set_feature(v_set_feature);
		i_core->wValue(wValue);
		i_core->wIndex(wIndex);
		i_core->vendor_data(vendor_data);
		i_core->usb_busy(usb_busy);
		i_core->ep_sel(ep_sel);

		// EP1 -> BULK IN 128
		i_core->ep1_cfg(cfg1);
		i_core->ep1_din(ep1_us_din);
		i_core->ep1_dout(ep1_dout_nc);
		i_core->ep1_we(ep1_we_nc);
		i_core->ep1_re(ep1_us_re);
		i_core->ep1_empty(ep1_us_empty);
		i_core->ep1_full(gnd);

		// EP2 -> BULK OUT 128
		i_core->ep2_cfg(cfg2);
		i_core->ep2_din(gnd8);
		i_core->ep2_dout(ep2_us_dout);
		i_core->ep2_we(ep2_us_we);
		i_core->ep2_re(ep2_re_nc);
		i_core->ep2_empty(gnd);
		i_core->ep2_full(ep2_us_full);


		// RX FIFO Instantiation and Binding
		i_ff_ep1 = new usb_fifo128x8("RX_FIFO");
		i_ff_ep1->clk(clk);
		i_ff_ep1->rst(rst);
		i_ff_ep1->clr(gnd);
		i_ff_ep1->we(ep1_f_we);
		i_ff_ep1->din(ep1_f_din);
		i_ff_ep1->re(ep1_us_re);
		i_ff_ep1->dout(ep1_us_din);
		i_ff_ep1->empty(ep1_us_empty);
		i_ff_ep1->full(ep1_f_full);

		// TX FIFO Instantiation and Binding
		i_ff_ep2 = new usb_fifo128x8("TX_FIFO");
		i_ff_ep2->clk(clk);
		i_ff_ep2->rst(rst);
		i_ff_ep2->clr(gnd);
		i_ff_ep2->we(ep2_us_we);
		i_ff_ep2->din(ep2_us_dout);
		i_ff_ep2->re(ep2_f_re);
		i_ff_ep2->dout(ep2_f_dout);
		i_ff_ep2->empty(ep2_f_empty);
		i_ff_ep2->full(ep2_us_full);

		i_uart = new uart("UART");
		i_uart->clk(clk);
		i_uart->rst(rst);
		i_uart->txd(uart_txd);
		i_uart->rxd(uart_rxd);
		i_uart->tx_data_wr(uart_tx_data_wr);
		i_uart->tx_data(uart_tx_data);
		i_uart->tx_empty(uart_tx_empty);
		i_uart->rx_data_rd(uart_rx_data_rd);
		i_uart->rx_data(uart_rx_data);
		i_uart->rx_avail(uart_rx_avail);

		i_por = new por("POR");
		i_por->rst(rst);

		SC_THREAD(tx_fifo_push);
		SC_THREAD(rx_fifo_push);
	}

	void tx_fifo_push(void) {
		while (true) {
			while (ep2_f_empty.read() || (!uart_tx_empty.read())) wait(clk.posedge_event());
			ep2_f_re.write(true);
			wait(clk.posedge_event());
			ep2_f_re.write(false);
			uart_tx_data.write(ep2_f_dout.read());
			uart_tx_data_wr.write(true);
			cout << "FT232R uart tx write " << ep2_f_dout.read() << "@ " << sc_time_stamp() << endl;
			wait(clk.posedge_event());
			uart_tx_data_wr.write(false);
			wait(clk.posedge_event());
		}
	}

	void rx_fifo_push(void) {
		while (true) {
			while (ep1_f_full.read() || (!uart_rx_avail.read())) wait(clk.posedge_event());
			ep1_f_din.write(uart_rx_data.read());
			ep1_f_we.write(true);
			uart_rx_data_rd.write(true);
			cout << "FT232R uart rx read " << uart_rx_data.read() << "@ " << sc_time_stamp() << endl;
			wait(clk.posedge_event());
			ep1_f_we.write(false);
			uart_rx_data_rd.write(false);
			wait(clk.posedge_event());
			wait(clk.posedge_event());
		}
	}

};

#endif // FT232R

