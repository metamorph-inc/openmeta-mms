/////////////////////////////////////////////////////////////////////
////                                                             ////
////  VCD Signal Source Model	(waveform playback)			     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
#include "VCDSource.h"

#include <iostream>
#include <fstream>
#include <list>
#include <io.h>


//
// VCD Format Description:
// http://lost-contact.mit.edu/afs/rose-hulman.edu/cadence-0910/INCISIV121/doc/vlogref/chap20.html
//
// Supported variable types:
//  integer [<size>](default: 32, but size will be ignored) -> sc_int<32>
//	real [<size>]	(default: 64, but size will be ignored -> analog_type (sc_uint<10>) (0.0 - 1.0 will be scaled to full scale)
//	wire [<size>]	(default: 1, error for bus/vector)  -> sc_logic
//

void VCDSource::player()
{
	std::list<string> scope;
	
	sc_time_unit	timeunit = SC_US;
	int				timescale = 1;
	
	if (_access_s(fname.c_str(), 0 ) != 0 ) {
		std::cout << "Cannot find file: " << fname << std::endl;
		return;
	}

	CTokenizer t(fname.c_str());

	int tok;

	// Parse the header, keep only important stuff
	while ( (tok = t.next_token()) != TT_ENDDEFINITIONS ) {
		if (tok == TT_EOF) {
			std::cout << "Unexpected end of file." << std::endl;
			return;
		}
		if (tok == TT_SCOPE) {
			// we need two more tokens: <scope type> <identifier>
			if ((tok = t.next_scope_token()) != ST_MODULE) {
				std::cout << "Unsupported scope type: " << t.get_word() << std::endl;
				t.to_end(); continue;
			}
			tok = t.next_str_token();
			scope.push_back(t.get_word());
			t.to_end();
		}
		else if (tok == TT_UPSCOPE) {
			scope.pop_back();
			t.to_end();
		}
		else if (tok == TT_VAR) {
			// we need four more tokens: <var_type> <size> <identifier_code> <reference>
			int vsize;
			string vid;
			string vref;
			int vtype = t.next_var_token(true);

			tok = t.next_str_token();
			if (sscanf(t.get_word(), "%d", &vsize) != 1) {
				std::cout << "Invalid variable size: " << t.get_word() << std::endl;
				t.to_end(); continue;
			}

			tok = t.next_str_token();
			vid = t.get_word();
			tok = t.next_str_token();
			vref = t.get_word();
			
			string vref_full;
			for (list<string>::const_iterator ci = scope.cbegin(); ci != scope.cend(); ++ci) {
				vref_full.append(*ci);
				vref_full.append(".");
			}
			vref_full.append(vref);
			if (vref == sname) {
				// std::cout << "VCDSource found variable: " << vref_full.c_str() << " [" << vid << "]" << std::endl;
				sid = vid;
				if (!check_type(vtype)) {
					std::cout << "Unsupported/mismatched variable type for: " << t.get_word() << std::endl;
					t.to_end(); continue;
				}
				
			}
			t.to_end();
		}
		else if (tok == TT_TIMESCALE) {
			// we need two more tokens: <number> <time_dimension>
			tok = t.next_tscale_token();
			if (tok == TST_ONE) {
				timescale = 1;
			}
			else if (tok == TST_TEN) {
				timescale = 10;
			}
			else if (tok == TST_HUND) {
				timescale = 100;
			}
			else {
				std::cout << "Unexpected timescale number token: " << t.get_word() << std::endl;
				t.to_end(); continue;
			}

			tok = t.next_tscale_token();
			if (tok == TST_SEC) {
				timeunit = SC_SEC;
			}
			else if (tok == TST_M) {
				timeunit = SC_MS;
			}
			else if (tok == TST_U) {
				timeunit = SC_US;
			}
			else if (tok == TST_N) {
				timeunit = SC_NS;
			}
			else if (tok == TST_P) {
				timeunit = SC_PS;
			}
			else if (tok == TST_F) {
				timeunit = SC_FS;
			}
			else {
				std::cout << "Unexpected timescale unit token: " << t.get_word() << std::endl;
				t.to_end(); continue;
			}
			t.to_end();
		}
		else {
			t.to_end();
		}
	}

	if (!sid.length()) {
		std::cout << "Could not find variable in VCD source file: " << sname << std::endl;
		return;
	}

	// End of header, read till the end
	unsigned __int64 ctime = 0;
	while ( (tok = t.next_token()) != TT_EOF ) {

		if (tok == TT_DUMPVARS || 
			tok == TT_DUMPALL || 
			tok == TT_DUMPON || 
			tok == TT_DUMPOFF ||
			tok == TT_COMMENT ||
			tok == TT_END) {
			//skip these
		}
		else if (tok == TT_STRING) {
			const std::string w = t.get_word();
			if (w[0] == '#') {
				unsigned __int64 ts;
				if (sscanf(w.substr(1).c_str(), "%I64u", &ts) != 1) {
					std::cout << "Invalid timestamp: " << w << std::endl;
					continue;
				}
				// Wait until this event, continue processing after
				wait((ts-ctime) * timescale, timeunit);
				ctime = ts;
			}
			else if (w[0] == 'r' || w[0] == 'R' || w[0] == 'b' || w[0] == 'B') {
				// we need one more token: <val> <id>
				tok = t.next_token();
				if (tok != TT_STRING) {
					std::cout << "Unexpected token in value change line: " << t.get_word() << std::endl;
				}
				if (sid == t.get_word()) {
					update_out(w.substr(1));
				}
			}
			else if (w[0] == '0' || w[0] == '1' || w[0] == 'x' || w[0] == 'X' || w[0] == 'z' || w[0] == 'Z') {
				// the is in this token: <val><id>, at least two chars...
				string tmp = t.get_word();
				if (tmp.length() > 1) {
					if (sid == tmp.substr(1)) {
						update_out(w.substr(0,1));
					}
				}
			}
			else {
				std::cout << "Unexpected string in value dump section: " << t.get_word() << std::endl;
			}

		}
		else {
			std::cout << "Unexpected token in value dump section: " << t.get_word() << std::endl;
		}
	}
}


VCDSource::VCDSource(sc_module_name mname, std::string const& fname, std::string const& sname) : 
	sc_module(mname), fname(fname), sname(sname)
{
		SC_THREAD(player);
}
