/////////////////////////////////////////////////////////////////////
////                                                             ////
////  FTDI FT232R Top Level Testbench                            ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////
////                                                             ////
////  USB 1.1 Top Level Test Bench                               ////
////                                                             ////
////  SystemC Version: usb_test.cpp                              ////
////  Author: Alfredo Luiz Foltran Fialho                        ////
////          alfoltran@ig.com.br                                ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
////                                                             ////
//// Verilog Version: test_bench_top.v + tests.v + tests_lib.v   ////
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

#include <stdlib.h>
#include <time.h>
#include <systemc.h>
#include "test_ft232r.h"


#define VCD_OUTPUT_ENABLE

int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_NS);
	sc_signal<bool>	rx_dp, rx_dn, tx_dp, tx_dn;
	sc_signal<bool> uart_txd, uart_rxd;
	sc_signal<bool> tx_oe_nc;

	ft232r			i_ft232r("FT232R");
	test_ft232r		i_test("TEST");

	i_ft232r.tx_dp(tx_dp);
	i_ft232r.tx_dn(tx_dn);
	i_ft232r.tx_oe(tx_oe_nc);
	i_ft232r.rx_dp(rx_dp);
	i_ft232r.rx_dn(rx_dn);
	i_ft232r.rx_d(rx_dp);
	i_ft232r.uart_txd(uart_txd);
	i_ft232r.uart_rxd(uart_rxd);

	i_test.txdp(rx_dp);
	i_test.txdn(rx_dn);
	i_test.rxdp(tx_dp);
	i_test.rxdn(tx_dn);
	i_test.uart_txd(uart_rxd);
	i_test.uart_rxd(uart_txd);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_FT232R");
	sc_trace(vcd_log, uart_txd, "UART_TXD");
	sc_trace(vcd_log, uart_rxd, "UART_RXD");

	sc_trace(vcd_log, tx_dp, "USB_TXDP");
	sc_trace(vcd_log, tx_dn, "USB_TXDN");
	sc_trace(vcd_log, rx_dp, "USB_RXDP");
	sc_trace(vcd_log, rx_dn, "USB_RXDN");
#endif

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif

	return i_test.error_cnt;
}

