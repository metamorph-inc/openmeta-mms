/////////////////////////////////////////////////////////////////////
////                                                             ////
////  USB ROM addresses (application specific)                   ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef USB_ROM_ADDR_H
#define USB_ROM_ADDR_H

// Device DescrIptor Length
#define	ROM_SIZE0	18
// Configuration Descriptor Length
#define	ROM_SIZE1	60
// Language ID Descriptor Start Length
#define	ROM_SIZE2A	8
// String 1 Descriptor Length
#define	ROM_SIZE2B	26
// String 2 Descriptor Length
#define	ROM_SIZE2C	28
// String 3 Descriptor Length
#define	ROM_SIZE2D	54

// Device Descriptor Start Address
#define	ROM_START0	0x00
// Configuration Descriptor Start Address
#define	ROM_START1	0x12
// Language ID Descriptor Start Address
#define	ROM_START2A	0x4e
// String 1 Descriptor Start Address
#define	ROM_START2B	0x56
// String 2 Descriptor Start Address
#define	ROM_START2C	0x70
// String 3 Descriptor Start Address
#define	ROM_START2D	0x8c

#endif // USB_ROM_ADDR_H