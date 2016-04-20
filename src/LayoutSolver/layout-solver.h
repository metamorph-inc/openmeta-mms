#ifndef LAYOUT_SOLVER_H
#define LAYOUT_SOLVER_H

#include "layout-defines.h"

class LayoutSolver : public Gecode::Space 
{
public:
	// Public typedefs
	typedef enum glcx_e { NONE, BOARD_EDGE, INTER_CHIP } glcx_t;	// The types of global-layout-constraint exceptions, MOT-728

	// Public methods
	LayoutSolver(Json::Value& root); 
	void DeclareVariables();
	void PostOverlapConstraints();
	void PostEdgeConstraints();
	void PostUserChipConstraints(int sel_pkg=-1);		// apply constraints only to the selected package
	void PostUserGlobalConstraints(int sel_pkg=-1);
	void PostBrancher();
	void CalculateDensity();

		// search support
	LayoutSolver(bool share, LayoutSolver& s);
	virtual Space* copy(bool share) {
			return new LayoutSolver(share,*this);
	}
		// print solution
	void print(std::ostream& os = std::cout) const;
	void printLayout(std::ostream& os, Json::Value& root);

	// Public data
	static const int resolution = 10;	// (1/10) = 0.1 mm
	unsigned int numChips;		// number of chips to place

protected:
	// Protected data
	static int *pkg_idx_map;

	// variables related to initialization of layout problem, singleton
	std::map<std::string, int> packageMap;		// index of package in variable list
	Json::Value& rootJson;
	Json::Value packages;						// local copy of packages
	std::list<RZRect> exRects;					// rectangles provided through global exclusion constraints

	// search problem configuration 
	int boardW;			// width (post multiplication with resolution)
	int boardH;			// height (post multiplication with resolution)
	int boardL;			// layers -- forced to 2
	int interChipSpace;	// The default inflation around each package to add margins, unless there is an inter-chip gap exception.  See also interChipGap() and MOT-728. 
	int boardEdgeSpace;	// board edge margin

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// search variables duplicated across LayoutSolver Space copies
	Gecode::IntVarArray px;		// x-position of packages
	Gecode::IntVarArray py;		// y-position of packages
	Gecode::IntVarArray pz;		// z-position of packages (layers)
	Gecode::IntVarArray pr;		// rotation of packages (0 - as is, 1 - width, height flipped)
	Gecode::IntSharedArray pw;	// package width
	Gecode::IntSharedArray ph;	// package height
	Gecode::IntSharedArray pl;	// package layer
	Gecode::IntSharedArray kow;	// keep-out width
	Gecode::IntSharedArray koh;	// keep-out height
	Gecode::IntSharedArray ebmx;	// edge of board margin, horizontal
	Gecode::IntSharedArray ebmy;	// edge of board margin, vertical
	Gecode::IntSharedArray rc;


private:
	// Private typedefs
	typedef int xBits_t;	// bit field for types of exceptions, MOT-728

	// Private methods
	void processConstraint(const Json::Value& constraint, int pkg_idx);
	void processExactConstraint(const Json::Value& constraint, int pkg_idx);
	void processRelativeConstraint(const Json::Value& constraint, int pkg_idx);
	void processRangeConstraint(const Json::Value& constraint, int pkg_idx);
	void processRegionConstraint(const Json::Value& constraint, int pkg_idx);
	void processExRegionConstraint(const Json::Value& constraint, int pkg_idx);
	void processRelativeRegionConstraint(const Json::Value& constraint, int pkg_idx);
	void processGlobalLayoutConstraintException(const Json::Value& constraint, int pkg_idx);
	void processRangeConstraint(std::string val, int vi, Gecode::IntVarArray& varArr, std::string name, std::string varName, double res=resolution);
	Gecode::BoolVar twoPointConstraint(int pi, int rot, int refrot, int refi, double x1, double y1);
	Gecode::BoolVar boxConstraint(int pi, int refrot, int refi, double x1, double y1, double x2, double y2);
	Gecode::BoolVar rangeConstraint(int pi, Gecode::IntVar& p1x, Gecode::IntVar& p1y, Gecode::IntVar& p2x, Gecode::IntVar& p2y);
	std::pair<int, int> rotatePoint(int rotation, int refi, double x, double y);
	std::pair<double, double> extractRange(const std::string& val);
	Gecode::BoolVar relativeConstraint(const Json::Value& constraint, const Json::Value& packages, int pkg_idx, int rel_pkg, int lD, int rot, int computedChipGap);
	bool exceptionExists( int pkg_idx, glcx_t exception_type );	// Check if a package has an exception, MOT-728
	const char* ToString( glcx_t exceptionType );
	xBits_t ToBits( glcx_t exceptionType );
	int interChipGap( int pkg_idx1, int pkg_idx2);	// Returns the chip gap between two packages, taking the inter-chip-gap exception into consideration. MOT-728.
	void checkOneDimension(	// Helper function to check exact constraint X or Y
		double rightEdgeOfComponent,
		double leftEdgeOfComponent,
		double rightEdgeOfBoard,
		double leftEdgeOfBoard,
		double boardEdgeMargin,
		std::string rightSideText,
		std::string leftSideText,
		std::string debugPackageName,
		std::string constraintName,
		bool boardEdgeException,
		bool exactXyrConstraintExists,
		std::string rotation);
	void LayoutSolver::getEdgeGaps( int & egx, int & egy, int pkg_idx );
	
	// Private data
	static xBits_t *packages_exceptions_map;
	static bool *exactXyrConstraintsExist;
};

#endif //LAYOUT_SOLVER_H