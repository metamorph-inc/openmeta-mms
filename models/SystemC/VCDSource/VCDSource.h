/////////////////////////////////////////////////////////////////////
////                                                             ////
////  VCD Signal Source Model	(waveform playback)			     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef VCDSOURCE_H
#define VCDSOURCE_H

#include <systemc.h>
#include "Tokenizer.h"

SC_MODULE(VCDSource) {
	SC_HAS_PROCESS(VCDSource);
	VCDSource(sc_module_name mname, std::string const& fname, std::string const& sname);

  protected:
	 virtual void update_out(const std::string& v) = 0;
	 virtual bool check_type(int vtype) = 0;

  private:
	std::string fname;
	std::string sname;
	std::string sid;
	void player();
};

class VCDAnalogSource : public VCDSource {

public:
	typedef sc_uint<10>	analog_type; 

	SC_HAS_PROCESS(VCDAnalogSource);

	VCDAnalogSource(sc_module_name mname, std::string const& fname, std::string const& sname) 
		: VCDSource(mname, fname, sname)
	{
		analog_type		analog_tmp;
		analog_full_scale = ((1 << analog_tmp.length()) - 1);
	}

	// ports
	sc_out<analog_type>	out;

protected:
	virtual void update_out(const std::string& v) {
		double val;
		if (sscanf(v.c_str(), "%lg", &val) != 1) {
			std::cout << "Invalid real value: " << v << std::endl;
			return;
		}
		out.write(val * analog_full_scale);
	}

	virtual bool check_type(int vtype) {
		return vtype == VT_REAL;
	}

private:
	double analog_full_scale;
};

class VCDIntegerSource : public VCDSource {
	
public:
	typedef sc_int<32>	integer_type; 

	SC_HAS_PROCESS(VCDIntegerSource);

	VCDIntegerSource(sc_module_name mname, std::string const& fname, std::string const& sname) 
		: VCDSource(mname, fname, sname)
	{
	}

	// ports
	sc_out<integer_type>	out;

protected:
	virtual void update_out(const std::string& v)
	{
		int val = 0, pos = 0;
		std::string str(v);
		std::reverse(str.begin(), str.end());
		for(std::string::iterator it = str.begin(); it != str.end(); ++it) {
			val = (val << 1) + (*it == '1' ? 1 : 0);
			pos++; 
		}
		out.write(val);
	}

	virtual bool check_type(int vtype) {
		return vtype == VT_INTEGER;
	}
};

class VCDLogicSource : public VCDSource {

public:
	typedef sc_logic	logic_type; 

	SC_HAS_PROCESS(VCDLogicSource);

	VCDLogicSource(sc_module_name mname, std::string const& fname, std::string const& sname) 
		: VCDSource(mname, fname, sname)
	{
	}

	// ports
	sc_out<logic_type>	out;

protected:
	virtual void update_out(const std::string& v)
	{
		if (!v.length()) {
			return;
		}

		// Shortcut this...
		/*
		switch (v[0]) {
		case '1':
			out.write();
			break;

		case '0':
			break;

		case 'z':
		case 'Z':
			break;

		case 'x':
		case 'X':
			break;
		}
		*/
		sc_logic val(v[0]);
		out.write(val);
	}

	virtual bool check_type(int vtype) {
		return vtype == VT_WIRE;
	}
};

class VCDLogicVectorSource : public VCDSource {

public:
	typedef sc_lv<32>	logic_vector_type; 

	SC_HAS_PROCESS(VCDLogicVectorSource);

	VCDLogicVectorSource(sc_module_name mname, std::string const& fname, std::string const& sname) 
		: VCDSource(mname, fname, sname)
	{
	}

	// ports
	sc_out<logic_vector_type>	out;

protected:
	virtual void update_out(const std::string& v)
	{
		logic_vector_type val;
		val.range(v.length(), 0) = v.c_str();
		out.write(val);
	}

	virtual bool check_type(int vtype) {
		return vtype == VT_WIRE;
	}
};


#endif // VCDSOURCE_H