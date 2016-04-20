#include "layout-exception.h"

//////////////////////////////////////////////////////////////
//
// layout-exception.cpp
//
// Adds methods to the layout-solver class, to handle
// global-layout-constraint exceptions.
//
// See also MOT-728.
//
//////////////////////////////////////////////////////////////

// Gets a bit-field pattern from an exception-type enum.
LayoutSolver::xBits_t LayoutSolver::ToBits( LayoutSolver::glcx_t exceptionType )
{
	switch ( exceptionType )
	{
		case NONE:			return 0;
		case BOARD_EDGE:	return (1<<0);
		case INTER_CHIP:	return (1<<1);
		default:			return 0;
	}
}

// Gets an exception-name string from an exception-type enum.
// The valid names need to match the ones used for exceptions in the layout-input.json file. 
const char* LayoutSolver::ToString( LayoutSolver::glcx_t exceptionType )
{
	switch ( exceptionType )
	{
		case NONE:			return "";
		case BOARD_EDGE:	return "Board_Edge_Spacing";
		default:			return "?";
	}
}

// Check if a package has an exception, MOT-728
bool LayoutSolver::exceptionExists( int pkg_idx, LayoutSolver::glcx_t exception_type )
{
	if( packages_exceptions_map[ pkg_idx ] & ToBits( exception_type ) )
		return true;
	return false;
}


// Returns the chip gap between two packages, taking the inter-chip-gap exception into consideration. MOT-728.
int LayoutSolver::interChipGap( int pkg_idx1, int pkg_idx2)
{
	if( exceptionExists( pkg_idx1, INTER_CHIP ) || exceptionExists( pkg_idx2, INTER_CHIP ) )
		return 0;
	return interChipSpace;
}

