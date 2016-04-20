/////////////////////////////////////////////////////////////////////
////                                                             ////
////  USB DEFINES                                                ////
////                                                             ////
////  SystemC Version: usb_defines.h                             ////
////  Author: Alfredo Luiz Foltran Fialho                        ////
////          alfoltran@ig.com.br                                ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
////                                                             ////
//// Verilog Version: usb1_defines.v                             ////
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

#ifndef USB_DEFINES_H
#define USB_DEFINES_H

// Do not use quotes for this include (app specific version needs to be used)
#include <usb_rom_addr.h>


//#define USBF_VERBOSE_DEBUG

// Endpoint Configuration Constants
#define EP_IN		0x0200
#define EP_OUT		0x0400
#define EP_CTRL		0x2800
#define EP_ISO		0x1000
#define EP_BULK		0x2000
#define EP_INT		0x0000

// PID Encodings
#define USBF_T_PID_OUT		0x1
#define USBF_T_PID_IN		0x9
#define USBF_T_PID_SOF		0x5
#define USBF_T_PID_SETUP	0xd
#define USBF_T_PID_DATA0	0x3
#define USBF_T_PID_DATA1	0xb
#define USBF_T_PID_DATA2	0x7
#define USBF_T_PID_MDATA	0xf
#define USBF_T_PID_ACK		0x2
#define USBF_T_PID_NACK		0xa
#define USBF_T_PID_STALL	0xe
#define USBF_T_PID_NYET		0x6
#define USBF_T_PID_PRE		0xc
#define USBF_T_PID_ERR		0xc
#define USBF_T_PID_SPLIT	0x8
#define USBF_T_PID_PING		0x4
#define USBF_T_PID_RES		0x0

// The HMS_DEL is a constant for the "Half Micro Second"
// Clock pulse generator. This constant specifies how many
// Phy clocks there are between two hms_clock pulses. This
// constant plus 2 represents the actual delay.
// Example: For a 60 Mhz (16.667 nS period) Phy Clock, the
// delay must be 30 phy clock: 500ns / 16.667nS = 30 clocks
#define USBF_HMS_DEL		0x16

// After sending Data in response to an IN token from host, the
// host must reply with an ack. The host has 622nS in Full Speed
// mode and 400nS in High Speed mode to reply. RX_ACK_TO_VAL_FS
// and RX_ACK_TO_VAL_HS are the numbers of UTMI clock cycles
// minus 2 for Full and High Speed modes.
// #define USBF_RX_ACK_TO_VAL_FS	36
#define USBF_RX_ACK_TO_VAL_FS	200

// After sending a OUT token the host must send a data packet.
// The host has 622nS in Full Speed mode and 400nS in High Speed
// mode to send the data packet.
// TX_DATA_TO_VAL_FS and TX_DATA_TO_VAL_HS are is the numbers of
// UTMI clock cycles minus 2.
// #define USBF_TX_DATA_TO_VAL_FS	36
#define USBF_TX_DATA_TO_VAL_FS	200

#endif

