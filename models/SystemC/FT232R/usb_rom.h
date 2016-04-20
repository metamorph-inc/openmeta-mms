/////////////////////////////////////////////////////////////////////
////                                                             ////
////  USB Descriptor ROM                                         ////
////                                                             ////
////  SystemC Version: usb_rom.h                                 ////
////  Author: Alfredo Luiz Foltran Fialho                        ////
////          alfoltran@ig.com.br                                ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
////                                                             ////
//// Verilog Version: usb1_rom1.v                                ////
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

#ifndef USB_ROM_H
#define USB_ROM_H

#include "systemc.h"


SC_MODULE(usb_rom) {

  public:

	sc_in<bool>			clk;
	sc_in<sc_uint<8> >	adr;
	sc_out<sc_uint<8> >	dout;

	void dout_update(void) {
		switch (adr.read()) {// synopsys full_case parallel_case
			// ====================================
			// =====    DEVICE Descriptor     =====
			// ====================================
			case 0x00:	dout.write(  18); break;	// this descriptor length
			case 0x01:	dout.write(0x01); break;	// descriptor type
			case 0x02:	dout.write(0x00); break;	// USB version low byte
			case 0x03:	dout.write(0x02); break;	// USB version high byte
			case 0x04:	dout.write(0x00); break;	// device class
			case 0x05:	dout.write(0x00); break;	// device sub class
			case 0x06:	dout.write(0x00); break;	// device protocol
			case 0x07:	dout.write(   8); break;	// max packet size
			case 0x08:	dout.write(0x03); break;	// vendor ID low byte
			case 0x09:	dout.write(0x04); break;	// vendor ID high byte
			case 0x0a:	dout.write(0x01); break;	// product ID low byte
			case 0x0b:	dout.write(0x60); break;	// product ID high byte
			case 0x0c:	dout.write(0x00); break;	// device rel. number low byte
			case 0x0d:	dout.write(0x06); break;	// device rel. number high byte
			case 0x0e:	dout.write(0x01); break;	// Manufacturer String Index
			case 0x0f:	dout.write(0x02); break;	// Product Descr. String Index
			case 0x10:	dout.write(0x03); break;	// S/N String Index
			case 0x11:	dout.write(0x01); break;	// Number of possible config.

			// ====================================
			// ===== Configuration Descriptor =====
			// ====================================
			case 0x12:	dout.write(0x09); break;	// this descriptor length
			case 0x13:	dout.write(0x02); break;	// descriptor type
			case 0x14:	dout.write(0x20); break;	// total data length low byte
			case 0x15:	dout.write(   0); break;	// total data length high byte
			case 0x16:	dout.write(0x01); break;	// number of interfaces
			case 0x17:	dout.write(0x01); break;	// number of configurations
			case 0x18:	dout.write(0x00); break;	// Conf. String Index
			case 0x19:	dout.write(0xA0); break;	// Config. Characteristics
			case 0x1a:	dout.write(0x2D); break;	// Max. Power Consumption

			// ====================================
			// =====   Interface Descriptor   =====
			// ====================================
			case 0x1b:	dout.write(0x09); break;	// this descriptor length
			case 0x1c:	dout.write(0x04); break;	// descriptor type
			case 0x1d:	dout.write(0x00); break;	// interface number
			case 0x1e:	dout.write(0x00); break;	// alternate setting
			case 0x1f:	dout.write(0x02); break;	// number of endpoints
			case 0x20:	dout.write(0xff); break;	// interface class
			case 0x21:	dout.write(0xff); break;	// interface sub class
			case 0x22:	dout.write(0xff); break;	// interface protocol
			case 0x23:	dout.write(0x02); break;	// interface string index

			// ====================================
			// =====   Endpoint 1 Descriptor  =====
			// ====================================
			case 0x24:	dout.write(0x07); break;	// this descriptor length
			case 0x25:	dout.write(0x05); break;	// descriptor type
			case 0x26:	dout.write(0x81); break;	// endpoint address
			case 0x27:	dout.write(0x02); break;	// endpoint attributes
			case 0x28:	dout.write(0x40); break;	// max packet size low byte
			case 0x29:	dout.write(0x00); break;	// max packet size high byte
			case 0x2a:	dout.write(0x00); break;	// polling interval

			// ====================================
			// =====   Endpoint 2 Descriptor  =====
			// ====================================
			case 0x2b:	dout.write(0x07); break;	// this descriptor length
			case 0x2c:	dout.write(0x05); break;	// descriptor type
			case 0x2d:	dout.write(0x02); break;	// endpoint address
			case 0x2e:	dout.write(0x02); break;	// endpoint attributes
			case 0x2f:	dout.write(0x40); break;	// max packet size low byte
			case 0x30:	dout.write(0x00); break;	// max packet size high byte
			case 0x31:	dout.write(0x00); break;	// polling interval

			// ====================================
			// ===== String Descriptor Lang ID=====
			// ====================================
			case 0x4e:	dout.write(0x04); break;	// this descriptor length
			case 0x4f:	dout.write(0x03); break;	// descriptor type

						// English - U.S.
			case 0x50:	dout.write(0x09); break;	// Language ID 0 low byte
			case 0x51:	dout.write(0x04); break;	// Language ID 0 high byte

			// ====================================
			// =====   String Descriptor 1    =====
			// ====================================
			case 0x56:	dout.write(  10); break;	// this descriptor length
			case 0x57:	dout.write(0x03); break;	// descriptor type

						// "FTDI"
			case 0x58:	dout.write(   0); break;
			case 0x59:	dout.write( 'F'); break;
			case 0x5a:	dout.write(   0); break;
			case 0x5b:	dout.write( 'T'); break;
			case 0x5c:	dout.write(   0); break;
			case 0x5d:	dout.write( 'D'); break;
			case 0x5e:	dout.write(   0); break;
			case 0x5f:	dout.write( 'I'); break;

			// ====================================
			// =====   String Descriptor 2    =====
			// ====================================
			case 0x60:	dout.write(  32); break;	// this descriptor length
			case 0x61:	dout.write(0x03); break;	// descriptor type

						// "FT232R USB UART"
			case 0x62:	dout.write(   0); break;
			case 0x63:	dout.write( 'F'); break;
			case 0x64:	dout.write(   0); break;
			case 0x65:	dout.write( 'T'); break;
			case 0x66:	dout.write(   0); break;
			case 0x67:	dout.write( '2'); break;
			case 0x68:	dout.write(   0); break;
			case 0x69:	dout.write( '3'); break;	//e-circumflex
			case 0x6a:	dout.write(   0); break;
			case 0x6b:	dout.write( '2'); break;
			case 0x6c:	dout.write(   0); break;
			case 0x6d:	dout.write( 'R'); break;
			case 0x6e:	dout.write(   0); break;
			case 0x6f:	dout.write( ' '); break;
			case 0x70:	dout.write(   0); break;
			case 0x71:	dout.write( 'U'); break;
			case 0x72:	dout.write(   0); break;
			case 0x73:	dout.write( 'S'); break;
			case 0x74:	dout.write(   0); break;
			case 0x75:	dout.write( 'B'); break;
			case 0x76:	dout.write(   0); break;
			case 0x77:	dout.write( ' '); break;
			case 0x78:	dout.write(   0); break;
			case 0x79:	dout.write( 'U'); break;
			case 0x7a:	dout.write(   0); break;
			case 0x7b:	dout.write( 'A'); break;
			case 0x7c:	dout.write(   0); break;
			case 0x7d:	dout.write( 'R'); break;
			case 0x7e:	dout.write(   0); break;
			case 0x7f:	dout.write( 'T'); break;

			// ====================================
			// =====   String Descriptor 3    =====
			// ====================================
			case 0x8c:	dout.write(  20); break;	// this descriptor length
			case 0x8d:	dout.write(0x03); break;	// descriptor type

						// "MetaMorph"
			case 0x8e:	dout.write(   0); break;
			case 0x8f:	dout.write( 'M'); break;
			case 0x90:	dout.write(   0); break;
			case 0x91:	dout.write( 'e'); break;
			case 0x92:	dout.write(   0); break;
			case 0x93:	dout.write( 't'); break;
			case 0x94:	dout.write(   0); break;
			case 0x95:	dout.write( 'a'); break;
			case 0x96:	dout.write(   0); break;
			case 0x97:	dout.write( 'M'); break;
			case 0x98:	dout.write(   0); break;
			case 0x99:	dout.write( 'o'); break;
			case 0x9a:	dout.write(   0); break;
			case 0x9b:	dout.write( 'r'); break;
			case 0x9c:	dout.write(   0); break;
			case 0x9d:	dout.write( 'p'); break;
			case 0x9e:	dout.write(   0); break;
			case 0x9f:	dout.write( 'h'); break;

			default:	dout.write(0x00); break;
		}
	}

	SC_CTOR(usb_rom) {
		SC_METHOD(dout_update);
		sensitive << clk.pos();
	}

};

#endif

