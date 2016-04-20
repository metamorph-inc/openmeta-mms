/*
*  Authors:
*    Sandeep Neema <neemask@metamorphsoftware.com>
*
* This file includes the implementation of the Layout Solver class
* The solver class sets up the constraint problem including definition of constraint variables for each part,
*    post builtin (no-overlap, no edge cross) constraints, and post user-defined constraints coming in JSON
*/
#include "layout-solver.h"
#include <strstream>
#include <tuple>

/*
* @todo: 
*/

using namespace Gecode;

// Function to round doubles to strings with a given number of decimal places, for diagnostic messages.
std::string RoundedCast(double toCast, unsigned precision = 1u) {
	std::ostringstream result;
	result << std::fixed << std::setprecision(precision) << toCast;
	return result.str();
}


struct PackageSort : public std::binary_function<std::pair<double, int>, std::pair<double, int>, bool>
{
	Json::Value& packages;

	PackageSort(Json::Value& r)  : packages(r["packages"]) { }

	void examineConstraint(const Json::Value& con, 
		bool& exactPos, bool& rangePos, bool& relRange, int& relPkg)
	{
		if ( (con["type"].asString().compare("exact") == 0) &&
			!(con["x"].isNull() && con["y"].isNull()))
			exactPos = true;
		if ( (con["type"].asString().compare("relative-region") == 0) ||
			(con["type"].asString().compare("relative-pkg") == 0) )
		{
			relPkg = con["pkg_idx"].asInt();
			relRange = (con["type"].asString().compare("relative-range") == 0);
		}
		if ( (con["type"].asString().compare("range") == 0) ||
			(con["type"].asString().compare("in-region") == 0) || 
			(con["type"].asString().compare("in-region") == 0) 		)
			rangePos = true;

		return;
	}

	inline bool operator()(const std::pair<double, int>& p1, const std::pair<double, int>& p2)
	{
		// true means p1 > p2
		bool p1ExactPos = false, p1RangePos = false, p1RelRange = false;
		int p1RelPkg = -1;
		Json::Value& p1cons = packages[p1.second]["constraints"];
		for(int i=0; i<p1cons.size(); i++) 
			examineConstraint(p1cons[i], p1ExactPos, p1RangePos, p1RelRange, p1RelPkg);

		bool p2ExactPos = false, p2RangePos = false, p2RelRange = false;
		int p2RelPkg = -1;
		Json::Value& p2cons = packages[p2.second]["constraints"];
		for(int i=0; i<p2cons.size(); i++) 
			examineConstraint(p2cons[i], p2ExactPos, p2RangePos, p2RelRange, p2RelPkg);

		// (1) exact,  
		if (p1ExactPos) 
			return true;
		if (p2ExactPos) 
			return false;

		// default - bigger chip first
		if (p1.first > p2.first)
			return true;
		else if (p1.first < p2.first)
			return false;

		// (2) rel-package, rel-region,
		if (p1RelPkg >=0 && p1RelPkg == p2.second)		// p1 is relative to p2
			return false;
		if (p2RelPkg >= 0 && p2RelPkg == p1.second)		// p2 is relative to p1
			return true;

		if (p1RelPkg >= 0 && p2RelPkg >= 0)
		{	// if both are relative to package
			if (p1RelRange) return false;	// p1 is a range, where p2 is fixed
			return true;
		}

		// (3) range, in-region, ex-region
		if (p1RangePos)
			return true;
		if (p2RangePos)
			return false;

		return false;

	}
};

template < typename T > std::string to_string( const T& n )
{
    std::ostringstream stm ;
    stm << n ;
    return stm.str() ;
}


/////////////////////////////////////////////////////////////////
// Module Data
/////////////////////////////////////////////////////////////////

int *LayoutSolver::pkg_idx_map = 0;
size_t pkg_idx_map_sz = 0;
LayoutSolver::xBits_t * LayoutSolver::packages_exceptions_map = 0;
bool * LayoutSolver::exactXyrConstraintsExist = 0;

/////////////////////////////////////////////////////////////////
//
// Constructor for the LayoutSolver class
//
/////////////////////////////////////////////////////////////////

LayoutSolver::LayoutSolver(Json::Value& root) 
	: numChips(0), 
	boardW(resolution*root["boardWidth"].asDouble()),
	boardH(resolution*root["boardHeight"].asDouble()), 
	boardL(2),		// PART PLACEMENT is on top and bottom layer only
	interChipSpace(resolution*root["interChipSpace"].asDouble()), 
	boardEdgeSpace(resolution*root["boardEdgeSpace"].asDouble()),
	rootJson(root),
	packages(Json::arrayValue),
	packageMap(),
	exRects()
{
	packageMap.clear();
	exRects.clear();
	pkg_idx_map_sz = root["packages"].size();
	pkg_idx_map = new int[pkg_idx_map_sz]; // create a mapping pkg idx mapping table

	for (int i = 0; i < pkg_idx_map_sz; i++)
	{
		if (rootJson["packages"][i]["doNotPlace"] == true)
		{
			pkg_idx_map[i] = -1;		// pkg-idx skipped for placement
			continue;
		}
		pkg_idx_map[i] = numChips;
		packages[numChips++] = rootJson["packages"][i];
	}

	// Initialize the packages_exceptions_map and exactXyrConstraintsExist, MOT-728
	packages_exceptions_map = new LayoutSolver::xBits_t[numChips];
	exactXyrConstraintsExist = new bool[numChips];
	for (int i=0; i<numChips; i++)
	{
		packages_exceptions_map[ i ] = ToBits( NONE );	// MOT-728
		exactXyrConstraintsExist[ i ] = false;

		// Set the exception bits based on the found exceptions.
		const Json::Value& constraints = packages[i]["constraints"];
		int numConstraints = constraints.size();
		for (int j=0; j<numConstraints; j++)
		{
			const Json::Value& constraint = constraints[j];
			const std::string& cons_type = constraint["type"].asString();

			if (cons_type.compare("except") == 0)	// constraint exception found
			{
				const std::string& exception_name = constraint["name"].asString();

				if( ToString( BOARD_EDGE ) == exception_name )
				{
					packages_exceptions_map[i] |= ToBits( BOARD_EDGE );
					std::cout << "Found a BOARD_EDGE global-layout constraint exception in Package[ " << i << " ]" << std::endl;
				}

				if( ToString( INTER_CHIP ) == exception_name )
				{
					packages_exceptions_map[i] |= ToBits( INTER_CHIP );
					std::cout << "Found a INTER_CHIP global-layout constraint exception in Package[ " << i << " ]" << std::endl;
				}
			}
			else if (cons_type.compare("exact") == 0)	// exact constraint found
			{
				const Json::Value x = constraint["x"];
				if (!x.isNull() )
				{
					const Json::Value y = constraint["y"];
					if (!y.isNull() )
					{
						const Json::Value r = constraint["rotation"];

						if( !r.isNull() )
						{
							exactXyrConstraintsExist[ i ] = true;
						}
					}
				}
			}
		}
	}
}

void LayoutSolver::DeclareVariables()
{
	// Find the maximum width or height of any of the parts.
	// We use it to adjust the range of px and py, to allow
	// parts to partially overhang the board edge, if they
	// have the board-edge constraint exception, and
	// they are exactly placed. MOT-728.

	int maxWidthOrHeight = 0;
	for(int i=0; i<numChips; i++)
	{
		int w = ceil(packages[i]["width"].asDouble()*resolution);
		int h = ceil(packages[i]["height"].asDouble()*resolution);
		if( w > maxWidthOrHeight ) {
			maxWidthOrHeight = w;
		}
		if( h > maxWidthOrHeight ) {
			maxWidthOrHeight = h;
		}
	}

	// variable declaration
	px = IntVarArray(*this, numChips, -maxWidthOrHeight, boardW + maxWidthOrHeight);
	py = IntVarArray(*this, numChips, -maxWidthOrHeight, boardH + maxWidthOrHeight);
	pz = IntVarArray(*this, numChips, 0, boardL-1);	
	pr = IntVarArray(*this, numChips, 0, 3);	// rotation 0,1,2,3

	pw = IntSharedArray(numChips);
	ph = IntSharedArray(numChips);
	pl = IntSharedArray(numChips);
	kow = IntSharedArray(numChips);
	koh = IntSharedArray(numChips);
	ebmx = IntSharedArray(numChips);
	ebmy = IntSharedArray(numChips);
	rc = IntSharedArray(numChips);

	// package width and height constants
	for(int i=0; i<numChips; i++)
	{
		pw[i] = ceil(packages[i]["width"].asDouble()*resolution);
		ph[i] = ceil(packages[i]["height"].asDouble()*resolution);
		pl[i] = packages[i]["multiLayer"].asBool();
		packageMap[packages[i]["name"].asString()] = i;
		if (packages[i]["koWidth"].isNull())
			kow[i] = 0;
		else
		{
			kow[i] = ceil(packages[i]["koWidth"].asDouble()*resolution);
			kow[i] = ceil((kow[i] - pw[i])/2.0);
		}
		if (packages[i]["koHeight"].isNull())
			koh[i] = 0;
		else
		{
			koh[i] = ceil(packages[i]["koHeight"].asDouble()*resolution);
			koh[i] = ceil((koh[i] - ph[i])/2.0);
		}
		getEdgeGaps( ebmx[i], ebmy[i], i );	// get the board edge margins by package id, MOT-728
		rc[i] = -1;
	}

}

void LayoutSolver::PostOverlapConstraints()
{
	// non overlap constraints
	for(int i=0; i<numChips; i++)
	{
		BoolVar pri = expr(*this, (pr[i] == 1 || pr[i] == 3));
		BoolVar npri = expr(*this, (pr[i] == 0 || pr[i] == 2));
		for(int j=0; j<i; j++)
		{
			// int computedChipGap = interChipGap(i, j);	// Take into account the inter-chip-gap constraint exceptions; MOT-728.
			int computedChipGap = interChipSpace;	// Ignore the inter-chip-gap constraint exceptions; MOT-728.

			int nrx = MAX(MAX(kow[i],kow[j]),computedChipGap);
			int nry = MAX(MAX(koh[i],koh[j]),computedChipGap);
			int rix = MAX(MAX(koh[i],kow[j]),computedChipGap);
			int riy = MAX(MAX(kow[i],koh[j]),computedChipGap);

			// no overlap with no rotation
			BoolVar nolnr = expr(*this, (px[i] + pw[i] + nrx <= px[j]) || (px[j] + pw[j] + nrx <= px[i]) ||
				(py[i] + ph[i] + nry <= py[j]) || (py[j] + ph[j] + nry <= py[i]));
			// no overlap with rotation - flip w & h of [i]
			BoolVar nolri = expr(*this, (px[i] + ph[i] + rix <= px[j]) || (px[j] + pw[j] + rix <= px[i]) ||
				(py[i] + pw[i] + riy <= py[j]) || (py[j] + ph[j] + riy <= py[i]));
			// no overlap with rotation - flip w & h of [j]
			BoolVar nolrj = expr(*this, (px[i] + pw[i] + riy <= px[j]) || (px[j] + ph[j] + riy <= px[i]) ||
				(py[i] + ph[i] + rix <= py[j]) || (py[j] + pw[j] + rix <= py[i]));
			// no overlap with rotation - flip w & h of [i] & [j]
			BoolVar nolrij = expr(*this, (px[i] + ph[i] + nry <= px[j]) || (px[j] + ph[j] + nry <= px[i]) ||
				(py[i] + pw[i] + nrx <= py[j]) || (py[j] + pw[j] + nrx <= py[i]));
			// no overlap with or without rotation
			BoolVar prj = expr(*this, (pr[j] == 1 || pr[j] == 3));
			BoolVar nprj = expr(*this, (pr[j] == 0 || pr[j] == 2));
			BoolVar nol = expr(*this, 
				(pri && nprj && nolri) || 
				(npri && prj && nolrj) || 
				(pri && prj && nolrij) || 
				(npri && nprj && nolnr) );
			// same layers
			BoolVar sl = expr(*this, pz[i] == pz[j]);
			// if chip i OR chip j is multi-layer then there can't be an overlap
			if (pl[i] || pl[j])
				rel(*this, nol);
			else // else if they are on same-layer then there can't be an overalp
				rel(*this, sl >> nol);
		}
	}
}

//**************************************************************************
//
// LayoutSolver::getEdgeGaps
//
// Helper function to get the X and Y board edge gaps as a function
// of the package index, based on constraints and exceptions.
//
// See also: MOT-728
//**************************************************************************

void LayoutSolver::getEdgeGaps( int & ebx, int & eby, int pkg_idx )
{
	ebx = MAX(kow[ pkg_idx ], boardEdgeSpace);
	eby = MAX(koh[ pkg_idx ], boardEdgeSpace);

	// Handle the board-edge-constraint exception, MOT-728.
	if (exceptionExists( pkg_idx, BOARD_EDGE ) == true)
	{
		// Allow component to be placed close to the board's edge.
		ebx = 0;
		eby = 0;

		// For components with an exact placement constraint on X, Y, and R, allow the user to place
		// the package so it overhangs the board edge, as long as some of it is still on the board.
		if( exactXyrConstraintsExist[ pkg_idx ] )
		{
			ebx = -pw[ pkg_idx ];
			eby = -ph[ pkg_idx ];
		}
	}
}


//**************************************************************************
//
// LayoutSolver::PostEdgeConstraints
//
//**************************************************************************
void LayoutSolver::PostEdgeConstraints()
{
	for(int i=0; i<numChips; i++)
	{
		BoolVar pri = expr(*this, (pr[i] == 1 || pr[i] == 3));	// True if package is rotated 90 or 270 degrees CCW.
		BoolVar npri = expr(*this, (pr[i] == 0 || pr[i] == 2));	// True if package is rotate 0 or 180 degrees CCW.

		// board bound with edge gap with no rotation
		BoolVar nb = expr(*this, (px[i] + pw[i] + ebmx[i] <= boardW) && (py[i] + ph[i] + ebmy[i] <= boardH));
		// board bound with edge gap with rotation
		BoolVar rb  = expr(*this, (px[i] + ph[i] + ebmy[i] <= boardW) && (py[i] + pw[i] + ebmx[i] <= boardH));

		// reification
		rel(*this, (pri && rb) || (npri && nb));

		// board bound left-bottom edge
		BoolVar lb = expr(*this, (px[i] >= ebmx[i]) && (py[i] >= ebmy[i]));

		// board bound left-bottom edge with rotation
		BoolVar rlb = expr(*this, (px[i] >= ebmy[i]) && (py[i] >= ebmx[i]));

		// reification
		rel(*this, (pri && rlb) || (npri && lb));

		// min-max bottom-left bound
		int minb = MIN(ebmx[i], ebmy[i]);
		rel(*this, (px[i] >= minb) && (py[i] >= minb));
	}
}

void LayoutSolver::PostUserChipConstraints(int sel_pkg)
{
		// enforce specified placement constraints
		std::cout << "Applying User-defined Package Constraints ..." << std::endl;
		int i= (sel_pkg < 0) ? 0 : sel_pkg;
		int u= (sel_pkg < 0) ? numChips : (sel_pkg+1);
		for (; i<u; i++)
		{
			const Json::Value& constraints = packages[i]["constraints"];
			int numConstraints = constraints.size();
			for (int j=0; j<numConstraints; j++)
			{
				const Json::Value& constraint = constraints[j];
				processConstraint(constraint, i);
			}
		}
}

void LayoutSolver::PostUserGlobalConstraints(int sel_pkg)
{
		std::cout << "Applying User-defined Global/Group Constraints ..." << std::endl;
		{
			const Json::Value& constraints = rootJson["constraints"];
			int numConstraints = constraints.size();
			for (int j=0; j<numConstraints; j++)
			{
				const Json::Value& constraint = constraints[j];
				const Json::Value& cgroup = constraint["group"];
				if (cgroup.isNull())
				{
					int i= (sel_pkg < 0) ? 0 : sel_pkg;
					int u= (sel_pkg < 0) ? numChips : (sel_pkg+1);
					for (; i<u; i++)
						processConstraint(constraint, i);
					if (constraint["type"].asString().compare("ex-region") == 0)
					{
						std::pair<double, double> xRange = extractRange(constraint["x"].asString());
						std::pair<double, double> yRange = extractRange(constraint["y"].asString());
						int layer = atoi(constraint["layer"].asString().c_str());
						struct RZRect rzr;
						rzr.r = 0;
						rzr.z = layer;
						rzr.rect.x = (int)(xRange.first * resolution);
						rzr.rect.y = (int)(yRange.first * resolution);
						rzr.rect.width = (int)((xRange.second - xRange.first) * resolution);
						rzr.rect.height = (int)((yRange.second - yRange.first) * resolution);
						exRects.push_back(rzr);
					}
				}
				else
					for (int i=0; i<cgroup.size(); i++)
						processConstraint(constraint, cgroup[i].asInt());
			}
	}
}

void LayoutSolver::PostBrancher()
{
	// area-index list contains areas of chips along with their package index
	// we want to sort this list as follows
	// order 0 - exact constraints
	// order 1 - big chips
	// order 2 - relative-fixed constraints
	// order 3 - relative-range constraints
	// order 4 - abs-range constraint
	// order 5 - rest
	std::list<std::pair<double,int>> area_index_list;

	// At this point packages[] contains the packages that will be placed.
	// The number of packages is numchips.

	// Define a typedef for a tuple with ranking info for a package using
	// <int exactFactor, double area, int package index, std::string package name>
	typedef std::tuple<int, double, int, std::string> rank_t;

	std::vector<rank_t> rankVector;

	// Get the number of layers in the board design
	int numLayers = rootJson["numLayers"].asInt();

	// Get the ranking info for all the packages
	for(int i=0; i<numChips; i++)
	{
		// Check package for an "exact" constraint.
		bool exactX = false;
		bool exactY = false;
		bool exactLayer = false;


		Json::Value& consList = packages[i]["constraints"];
		for( int consIndex = 0; consIndex < consList.size(); consIndex++ )
		{
			if( consList[ consIndex ].isMember("type") && (consList[ consIndex ]["type"].asString().compare("exact") == 0) )
			{
				// There's some type of exact constraint.
				if( consList[ consIndex ].isMember("x") ) exactX = true;
				if( consList[ consIndex ].isMember("y") ) exactY = true;
				if( consList[ consIndex ].isMember("layer") ) exactLayer = true;
			}
		}

		// Check if if package uses both top and bottom of the board, like a through-hole component.
		bool isMultilayer = false;
		if( (numLayers > 1) && packages[i].isMember("multiLayer") && packages[i]["multiLayer"].asBool() )
		{
			isMultilayer = true;
		}

		// Compute a numerical exactFactor, that trumps area in package-placement rankings.
		int exactFactor = 0;
		if( exactX && exactY )
		{
			exactFactor = 1;
			if( (numLayers > 1) && (exactLayer || isMultilayer ) )
			{
				exactFactor = 2;
			}
		}

		// Compute the area of the board
		int computedChipGap = interChipSpace;
		double w = (ceil(packages[i]["width"].asDouble() * resolution) + computedChipGap)/resolution;
		double h = (ceil(packages[i]["height"].asDouble() * resolution) + computedChipGap)/resolution;
		double area = w * h;

		if( (numLayers > 1) && isMultilayer )
		{
			area *= 2;	// Count the bounding box on both the top and bottom of the board
		}
		
		std::string name = packages[i]["name"].asString();


		// Create a rank tuple for this package.
		rank_t rankInfo = make_tuple( exactFactor, area, i, name );

		// TODO: save it in a vector
		rankVector.push_back( rankInfo );

	}

	// Structure used for sorting the vector
	// See http://stackoverflow.com/questions/1380463/sorting-a-vector-of-custom-objects
	struct greater_than_key
	{
		inline bool operator() (const rank_t& rank1, const rank_t& rank2)
		{
			// Check the exact factor
			if( std::get<0>(rank1) != std::get<0>(rank2) )
			{
				return (std::get<0>(rank1) > std::get<0>(rank2));
			}
			// Check the area
			else if (std::get<1>(rank1) != std::get<1>(rank2) )
			{
				return (std::get<1>(rank1) > std::get<1>(rank2));
			}
			// Check the name string
			else 
			{
				return ( std::get<3>(rank1).compare( std::get<3>(rank2) ) < 0 );
			}
		}
	};

	// Sort the rank vector, with exactly-placed and large items first,
	// to minimize the time needed to search possible layouts.
	std::sort( rankVector.begin(), rankVector.end(), greater_than_key() );

	// Display the sorted vector and create the area_index_list from it.
	std::cout << "\nSorted Rank Vector (exactFactor, area, package index, package name ):" << std::endl;
	for(std::vector<rank_t>::iterator itRank = rankVector.begin(); itRank != rankVector.end(); ++itRank) 
	{
		std::cout << "\t(" << 
		std::get<0>(*itRank) << ",\t" <<
		std::get<1>(*itRank) << ",\t" <<
		std::get<2>(*itRank) << ",\t" <<
		std::get<3>(*itRank) << ")" << std::endl;

		int i = std::get<2>(*itRank);
		double area = std::get<1>(*itRank);
		area_index_list.push_back(std::pair<double,int>(area, i));
	}		

	//for(int i=0; i<numChips; i++)
	//{
	//	int computedChipGap = interChipSpace;	// Ignore the inter-chip-gap constraint exceptions; MOT-728.
	//	double w = (ceil(packages[i]["width"].asDouble() * resolution) + computedChipGap)/resolution;
	//	double h = (ceil(packages[i]["height"].asDouble() * resolution) + computedChipGap)/resolution;
	//	double area = w*h;

	//	area_index_list.push_back(std::pair<double,int>(area, i));
	//}

	//if (solverOptions.searchOrder == LayoutOptions::STATIC_BCF)	// if using a static search order - presort
	//	area_index_list.sort(PackageSort(rootJson));

	std::cout << "Placement Order" << std::endl;
	for(std::list<std::pair<double,int>>::iterator i=area_index_list.begin(); i!=area_index_list.end(); ++i)
	{
		std::cout << "Package [" << (*i).second << "], Name = " << packages[(*i).second]["name"].asString() << ", Area = " << (*i).first << std::endl;
	}

	// try custom brancher
	lobranch(*this, px, py, pz, pr, pw, ph, kow, koh, ebmx, ebmy, rc, boardW, boardH, boardL, interChipSpace, area_index_list, exRects);

}


void LayoutSolver::CalculateDensity()
{
	double eg = boardEdgeSpace / (double)resolution;
	double cg = interChipSpace/(double)resolution;
	double bw = boardW/(double)resolution;
	double bh = boardH/(double)resolution;

	double layer_area = (bw - 2*eg)*(bh - 2*eg);
	double board_area = layer_area * boardL;

	double total_chip_area_basic = 0.0;
	double total_chip_area = 0.0;
	double total_chip_area_halfcg = 0.0;
	for(int i=0; i<numChips; i++)
	{
		double w = ceil(packages[i]["width"].asDouble() * resolution)/resolution;
		double h = ceil(packages[i]["height"].asDouble() * resolution)/resolution;
		total_chip_area += ((w+cg)*(h+cg));
		total_chip_area_basic += (w*h);
		total_chip_area_halfcg += ((w+0.5*cg)*(h+0.5*cg));
	}
	std::cout << "Number of Chips: " << numChips << std::endl;
	std::cout << "Board Width: " << bw
		<< " Board Height: " << bh
		<< " Layers: " << boardL << std::endl;
	std::cout << "Effective Board Area (excluding edgegap): " << board_area << std::endl;
	std::cout << "Total Chip Area (Chips - no chipgap): " << total_chip_area_basic << std::endl;
	std::cout << "Total Chip Area (Chips with chipgap - all on edge): " << total_chip_area_halfcg << std::endl;
	std::cout << "Total Chip Area (Chips with chipgap - all internal): " << total_chip_area << std::endl;
	std::cout << "Layout Density (least) = " << total_chip_area_halfcg / board_area << std::endl;
	std::cout << "Layout Density (most) = " << total_chip_area / board_area << std::endl;
}


// search support
LayoutSolver::LayoutSolver(bool share, LayoutSolver& s) : Space(share, s), rootJson(s.rootJson) 
{
	px.update(*this, share, s.px);
	py.update(*this, share, s.py);
	pr.update(*this, share, s.pr);
	pz.update(*this, share, s.pz);
	pw.update(*this, share, s.pw);
	ph.update(*this, share, s.ph);
	kow.update(*this, share, s.kow);
	koh.update(*this, share, s.koh);
	ebmx.update(*this, share, s.ebmx);
	ebmy.update(*this, share, s.ebmy);
	pl.update(*this, share, s.pl);
}

// print solution
void LayoutSolver::print(std::ostream& os) const
{
	int numChips = px.size();
	os << "Package[nnn]{WWW,HHH}: X,Y,Z,R" << std::endl;
	FILE *out = fopen("partial_result.txt", "w");
	fprintf(out, "-skipped line\n");

	for(int i=0, n = 0; (n < numChips) && (i < pkg_idx_map_sz); i++, n++)
	{
		// Skip the do-not-place packages
		while( (i < pkg_idx_map_sz) && (-1 == pkg_idx_map[i]) ) i++;
		int j = pkg_idx_map[i];
		os  << "package[" << std::setw(3) << j << "]{" << std::setw(3) << pw[n] << "," << std::setw(3) << ph[n] << "}:" 
			<< px[n] << "," << py[n] << "," << pz[n] << "," <<  pr[n] << std::endl;
		if (px[n].assigned() && py[n].assigned() && pz[n].assigned() && pr[n].assigned())
		{
			fprintf(out, "%d  %f %f %f %f\n", j, px[n].val()/(float)resolution, py[n].val()/(float)resolution, 
				pw[n]/(float)resolution, ph[n]/(float)resolution);
		}
		else
		{
			fprintf(out, "%d  %f %f %f %f\n", j, -1.0, -1.0, 
				pw[n]/(float)resolution, ph[n]/(float)resolution);
		}
	}
	fclose(out);
}

void LayoutSolver::printLayout(std::ostream& os, Json::Value& root)
{
	Json::Value& packages = root["packages"];
	int numChips = packages.size();
	for(int i=0, n = 0; (n < numChips) && (i < pkg_idx_map_sz); i++, n++)
	{
		// Skip the do-not-place packages
		while( (i < pkg_idx_map_sz) && (-1 == pkg_idx_map[i]) ) i++;
		int j = pkg_idx_map[i];
		if (j < 0 || j >= px.size()) 
			continue;
		packages[i]["x"] = px[j].assigned() ? (double)(px[j].val()*1.0)/resolution : 0.0;
		packages[i]["y"] = py[j].assigned() ? (double)(py[j].val()*1.0)/resolution : 0.0;
		packages[i]["rotation"] = pr[j].assigned() ? Json::Value(pr[j].val()) : Json::Value(0);
		packages[i]["layer"] = pz[j].assigned() ? Json::Value(pz[j].val()) : Json::Value(0);
		packages[i]["constraints"] = Json::nullValue;
	}
	root["packages"] = packages;
	Json::StyledStreamWriter writer;
	writer.write(os, root);
}

void LayoutSolver::processConstraint(const Json::Value& constraint, int pkg_idx)
{
	/*
	* what do the users want?
	* place a part on a specific location on a specific layer
	* place a part (fixed-location) relative to another part
	* place a part (range-location) relative to another part
	* place a part within a region on a layer 
	* place a part on a layer - but outside a region
	* multiple constraints of a type?
	*/
	const std::string& type = constraint["type"].asString();
	if (type.compare("exact") == 0)	// exact constraint - could be on location, layer, or rotation
	{
		processExactConstraint(constraint, pkg_idx);
	}
	else if (type.compare("relative-pkg") == 0)
	{
		processRelativeConstraint(constraint, pkg_idx);
	}
	else if (type.compare("range") == 0)	// ranges must be specified as string
	{
		processRangeConstraint(constraint, pkg_idx);
	}
	else if (type.compare("in-region") == 0)
	{
		processRegionConstraint(constraint, pkg_idx);
	}
	else if (type.compare("ex-region") == 0)
	{
		processExRegionConstraint(constraint, pkg_idx);
	}
	else if (type.compare("relative-region") == 0)
	{
		processRelativeRegionConstraint(constraint, pkg_idx);
	}
	else if (type.compare("except") == 0)
	{
		// Do nothing here.
	}
	else
		std::cout << "WARNING: Ignoring unsupported constraint type: " << type << std::endl;
}


//**************************************************************************
//
// processExactConstraint
//
//**************************************************************************
void LayoutSolver::processExactConstraint(const Json::Value& constraint, int pkg_idx)
{
	
	// Get the name and index of the package for debug messages.
	std::ostringstream packageName;
	packageName << "package \"" << packages[pkg_idx]["name"].asString() << "\" (index = " << pkg_idx << ")";

	std::cout << "Applying an exact constraint on " << packageName.str() << "\"." << std::endl;

	// Set constraint's values into arrays px[], py[], pr[], pz[]; indexed by "pkg_id":
	const Json::Value x = constraint["x"];

	const std::string fca = "Found constraint attribute: ";
	if (!x.isNull())
	{
		rel(*this, px[pkg_idx] == (int)(x.asDouble() * resolution));
		std::cout << fca << packages[pkg_idx]["name"].asString() << ".x = " << x.asDouble() << std::endl;
	}
	const Json::Value y = constraint["y"];
	if (!y.isNull())
	{
		rel(*this, py[pkg_idx] == (int)(y.asDouble() * resolution));
		std::cout << fca << packages[pkg_idx]["name"].asString() << ".y = " << y.asDouble() << std::endl;
	}

	std::string rotation = "";
	const Json::Value r = constraint["rotation"];
	if (!r.isNull())
	{
		rel(*this, pr[pkg_idx] == r.asInt());
		std::cout << fca << packages[pkg_idx]["name"].asString() << ".rotation = " << r.asInt() << std::endl;
		rotation = to_string(90 * r.asInt());
	}
	const Json::Value z = constraint["layer"];
	if (!z.isNull())
	{
		rel(*this, pz[pkg_idx] == z.asInt());
		std::cout << fca << packages[pkg_idx]["name"].asString() << ".layer = " << z.asInt() << std::endl;
	}

	// check constraint - if r is specified OR if r is unspecified (then we choose min of width/height)
	//
	// We set the package's width and height based on whether there is a rotation constraint.

	// Set the width and height as if there is 0 or 180 rotation.
	int dw = pw[pkg_idx]; 
	int dh = ph[pkg_idx];

	// Likewise for the edge gaps.
	int ebx = ebmx[pkg_idx];
	int eby = ebmy[pkg_idx];

	// If rotation is unconstrained, we know that both width and height must be at least the minimum of width and height.
	if (r.isNull())
	{
		dh = dw = MIN(pw[pkg_idx], ph[pkg_idx]);

		// Likewise for edge gaps.
		eby = ebx = MIN(ebmx[pkg_idx], ebmy[pkg_idx]);
	}

	// If rotation is 90 or 270, then swap width and height.
	else if (r.asInt() == 1 || r.asInt() == 3)
	{
		dh = pw[pkg_idx]; dw = ph[pkg_idx];

		// Likewise for edge gaps.
		eby = ebmx[pkg_idx]; ebx = ebmy[pkg_idx];
	}

	// Now we start checking the exact constraint's parameters, to see if they seem plausible.

	// Check that the board layer is 0 or 1, if it was set.
	if (!z.isNull() && ((z.asInt() > 1) || (z.asInt() < 0)))
	{
		std::strstream str;
		str << "Unsolvable exact-layer constraint on " << packageName.str() << ", layer should be 0 (top) or 1 (bottom)" << std::endl;
		throw std::exception(str.str());
	}

	// Check that the board rotation is in the range [0::3] if it was set.
	if (!r.isNull() && ((r.asInt() > 3) || (r.asInt() < 0)))
	{
		std::strstream str;
		str << "Unsolvable exact-rotation constraint on " << packageName.str() << ", rotation should be between 0 (meaning 0 degrees) and 3 (meaning 270 degrees counterclockwise)" << std::endl;
		throw std::exception(str.str());
	}

	// Check the exact constraint's X value, if it was set.
	if (!x.isNull())	
	{
		//std::cout << "check X" << std::endl;
		checkOneDimension(
			(x.asDouble() * resolution + dw),	// right edge of component
			(x.asDouble() * resolution),		// left edge of component
			boardW,								// right edge of board
			0,									// left edge of board
			ebx,								// edge gap (board margin)
			"right side",						// message text for right
			"left side",						// message text for left
			packageName.str(),					// message text name of package
			"exact-X",							// message text constraint type
			exceptionExists(pkg_idx, BOARD_EDGE),
			exactXyrConstraintsExist[ pkg_idx ],
			rotation
			);
	} else {
		std::cout << "x.isNull()" << std::endl;
	}
	
	// Check the exact constraint's Y value, if it was set.
	if (!y.isNull())
	{
		// std::cout << "check Y" << std::endl;
		checkOneDimension(
			(y.asDouble() * resolution + dh),	// top edge of component
			(y.asDouble() * resolution),		// bottom edge of component
			boardH,								// top edge of board
			0,									// bottom edge of board
			eby,								// edge gap (board margin)
			"top",								// message text for top
			"bottom",							// message text for bottom
			packageName.str(),					// message text name of package
			"exact-Y",							// message text constraint type
			exceptionExists(pkg_idx, BOARD_EDGE),
			exactXyrConstraintsExist[ pkg_idx ],
			rotation
			);
	} else {
		std::cout << "y.isNull()" << std::endl;
	}
	
	// If we got this far, the exact constraint we just checked on this component wasn't obviously bad.
}


//**************************************************************************
//
// LayoutSolver::checkOneDimension
//
// Helper function to check either the X or Y exact constraint to see if it
// 'obviously' makes layout unsolvable.  If so, throws an exception.
//
// See also: MOT-728
//**************************************************************************

void LayoutSolver::checkOneDimension(
	double rightEdgeOfComponent,
	double leftEdgeOfComponent,
	double rightEdgeOfBoard,
	double leftEdgeOfBoard,
	double boardEdgeMargin,
	std::string rightSideText,
	std::string leftSideText,
	std::string packageText,
	std::string constraintName,
	bool boardEdgeException,
	bool exactXyrConstraintExists,
	std::string rotation )
{
	std::stringstream msg;	
	std::stringstream positionText;
	std::stringstream debugText;

	const std::string stars = "*************************************************************************************\n";

	positionText << std::fixed << std::setprecision(1) << rightSideText << " of component = " << rightEdgeOfComponent / resolution << "mm" <<
		", " << leftSideText << " of component = " << leftEdgeOfComponent / resolution << "mm" <<
		", " << rightSideText << " of board = " << rightEdgeOfBoard / resolution << "mm" <<
		", " << leftSideText << " of board = " << leftEdgeOfBoard / resolution << "mm\n";

	debugText << "\nDiagnostic values:\n" << 
		"\t boardEdgeMargin = " << abs( boardEdgeMargin ) / resolution << 
			(boardEdgeMargin > 0 ? " mm inside the board edges" : 
				( boardEdgeMargin < 0 ? " mm outside the board edges" : ", exactly at the board edges" )
			) << "\n";
	if (rotation.length() >=1){
		debugText << "\t rotation = '" << rotation << "'" << " (" << (std::stoi(rotation) * 90) << " degrees" << ( stoi(rotation) == 0 ? "" : " counter-clockwise" ) << ")\n";
	}
	debugText << "\t packageText = " << packageText << "\n" << 
		"\t constraintName = " << constraintName << "\n" << 
		"\t boardEdgeException = " << (boardEdgeException != 0) << "\n" << 
		"\t exactXyrConstraintExists = " << (exactXyrConstraintExists != 0) << "\n";

	// Check if component is completely off the board.  That's alway's bad.
	if( leftEdgeOfComponent > rightEdgeOfBoard )
	{
		// Component is completely off the right side of the board.
		msg << positionText.str();
		msg << stars;
		msg << "Unsolvable " << constraintName << " constraint on " << packageText << ", component is completely off the " << rightSideText<< " of the board." << std::endl;
		msg << stars;
		msg << debugText.str();
		throw std::exception(msg.str().c_str());
	}

	if( rightEdgeOfComponent < leftEdgeOfBoard )
	{
		// Component is completely off the left side of the board.
		msg << positionText.str();
		msg << stars;
		msg << "Unsolvable " << constraintName << " constraint on " << packageText << ", component is completely off the " << leftSideText<< " of the board." << std::endl;
		msg << stars;
		msg << debugText.str();
		throw std::exception(msg.str().c_str());
	}

	// Check if the component is partially off the board.
	// We consider that a warning for now.
	if( rightEdgeOfComponent > rightEdgeOfBoard )
	{
		// Component is partially off the right side of the board.
		if( exactXyrConstraintExists )
		{
			// The user should know what they are doing since they placed it exactly.
			std::cout << positionText.str() << "\nWarning: " << constraintName << " constraint on " << packageText << ", component is partially off the " << rightSideText<< " of the board.\n" << std::endl;
		}
		else
		{
			msg << positionText.str();
			msg << stars;
			msg << "Unsolvable " << constraintName << " constraint on " << packageText << ", component is partially off the " << rightSideText<< " of the board." << std::endl;
			msg << stars;
			msg << debugText.str();
			throw std::exception(msg.str().c_str());
		}
	}
	if( leftEdgeOfComponent < leftEdgeOfBoard )
	{
		// Component is partially off the left side of the board.
		if( exactXyrConstraintExists )
		{
			// The user should know what they are doing since they placed it exactly.
			std::cout << positionText.str() << "Warning: " << constraintName << " constraint on " << packageText << ", component is partially off the " << leftSideText<< " of the board.\n" << std::endl;
		}
		else
		{
			msg << positionText.str();
			msg << stars;
			msg << "Unsolvable " << constraintName << " constraint on " << packageText << ", component is partially off the " << leftSideText<< " of the board." << std::endl;
			msg << stars;
			msg << debugText.str();
			throw std::exception(msg.str().c_str());
		}
	}

	// Check if component exceeds the board margins, if not boardEdgeException.
	if( !boardEdgeException )
	{
		if( rightEdgeOfComponent > (rightEdgeOfBoard - boardEdgeMargin) )
		{
			// Component is past the right board margin.
			msg << positionText.str();
			msg << stars;
			msg << "Unsolvable " << constraintName << " constraint on " << packageText << ", component exceeds the " << rightSideText<< " board edge gap of " << boardEdgeMargin / resolution << " mm." << std::endl;
			msg << stars;
			msg << debugText.str();
			throw std::exception(msg.str().c_str());
		}
		if( leftEdgeOfComponent < leftEdgeOfBoard + boardEdgeMargin )
		{
			// Component is past the left board margin.
			msg << positionText.str();
			msg << stars;
			msg << "Unsolvable " << constraintName << " constraint on " << packageText << ", component exceeds the " << leftSideText<< " board edge gap of " << boardEdgeMargin / resolution << " mm." << std::endl;
			msg << stars;
			msg << debugText.str();
			throw std::exception(msg.str().c_str());
		}		
	}
}


void LayoutSolver::processRelativeConstraint(const Json::Value& constraint, int pkg_idx)
{
	int rel_pkg = constraint["pkg_idx"].asInt();	// index of other package
	if (rel_pkg < 0 && rel_pkg > rootJson["packages"].size())
	{
		std::cout << "Invalid pkg_idx value: " << rel_pkg << " in constraint applied to pkg: " << pkg_idx << std::endl;
		return;	
	}
	else
	{
		rel_pkg = pkg_idx_map[rel_pkg];		// map 
		if (packages[rel_pkg]["pkg_idx"] != constraint["pkg_idx"].asInt())
		{
			std::cout << "Invalid pkg_idx map: from: " << rel_pkg << " to: " << packages[rel_pkg]["pkg_idx"] << std::endl;
			return;
		}
		rc[pkg_idx] = rel_pkg;		// store in the rc variable to link relative constrained chips
	}

	// int computedChipGap = interChipGap(i, j);	// Take into account the inter-chip-gap constraint exceptions; MOT-728.
	int computedChipGap = interChipSpace;	// Ignore the inter-chip-gap constraint exceptions; MOT-728.

	// choices: a) null --> don't care, b) same layer, c) opposite layer
	const Json::Value l = constraint["layer"];
	int lD = -1;
	if (!l.isNull())
	{
		lD = l.asInt();
		if (lD == 0)	// same as the constrained
			rel(*this, pz[pkg_idx] == pz[rel_pkg]);
		else if (lD == 1)	// opposite of constrained
			rel(*this, pz[pkg_idx] != pz[rel_pkg]);
		else
			std::cout << "Layer constraint value of " << lD << " is not valid" << std::endl;
	}

	// relative constraint is source & target rotation dependent
	// the constraint spec (in json) supplies four possible offset values assuming 0 rotation of anchor (source), 
	// and 4 possible rotation values of target
	// these offsets should also hold under rotation of the source with the appropriate translation/rotation applied

	// the function relativeConstraint is parameterized with the delta-rotation of target relative to source
	// and returns a boolvar/expr that captures the four rotation of the source with the specified delta-rotation of target 

	BoolVar b0 = relativeConstraint(constraint, packages, pkg_idx, rel_pkg, lD, 0, computedChipGap);
	BoolVar b1 = relativeConstraint(constraint, packages, pkg_idx, rel_pkg, lD, 1, computedChipGap);
	BoolVar b2 = relativeConstraint(constraint, packages, pkg_idx, rel_pkg, lD, 2, computedChipGap);
	BoolVar b3 = relativeConstraint(constraint, packages, pkg_idx, rel_pkg, lD, 3, computedChipGap);

	// if the constraint has delta-rotation specified then we allow only that one, otherwise all 4
	if ( constraint["relativeRotation"].isNull() ) 
		// all four 
		rel(*this, b0 || b1 || b2 || b3);
	else 
	{
		int dr = constraint["relativeRotation"].asInt();
		switch(dr) {
		case 0:
			rel(*this, b0);
			break;
		case 1:
			rel(*this, b1);
			break;
		case 2:
			rel(*this, b2);
			break;
		case 3:
			rel(*this, b3);
			break;
		default:		// allow all delta rotations in case of a bad value
			rel(*this, b0 || b1 || b2 || b3);
		}
	}

	// TBD SKN  -- revisit the rotation preferences of relative constraints
	//const Json::Value r = constraint["rotation"];
	//if (!r.isNull())
	//	rel(*this, pr[pkg_idx] == r.asInt());			// if rotation is specified - force to that
	//else
	//	rel(*this, pr[pkg_idx] == pr[rel_pkg]);		// else orients the chip same as another one

}

BoolVar LayoutSolver::relativeConstraint(const Json::Value& constraint, const Json::Value& packages, int pkg_idx, int rel_pkg, int lD, int rot, int computedChipGap)
{
	char xVar[8] = "x";
	if (rot != 0)
		sprintf(xVar, "x%d", rot);
	const Json::Value x = constraint[xVar];
	double dx, dy;
	if (!x.isNull())
	{
		dx = x.asDouble();
		std::cout << "Applying Constraint: " << packages[pkg_idx]["name"].asString() << ".x = " 
			<< packages[rel_pkg]["name"].asString() << ".x + " << dx << std::endl;
	}
	else
	{
			std::strstream str;
			str << "Invalid relative constraint on package \"" << packages[pkg_idx]["name"].asString() << "\" index(" << pkg_idx << "), " << xVar << " value is null" << std::endl;
			throw std::exception(str.str());
	}
	char yVar[8] = "y";
	if (rot != 0)
		sprintf(yVar, "y%d", rot);
	const Json::Value y = constraint[yVar];
	if (!y.isNull())
	{
		dy = y.asDouble();
		std::cout << "Applying Constraint: " << packages[pkg_idx]["name"].asString() << ".y = " 
			<< packages[rel_pkg]["name"].asString() << ".y + " << dy << std::endl;
	}
	else
	{
		std::strstream str;
		str << "Invalid relative constraint on package \"" << packages[pkg_idx]["name"].asString() << "\" index(" << pkg_idx << "), " << yVar << " value is null" << std::endl;
		throw std::exception(str.str());
	}

	// check if both x&y are specified & same layer forced and not in conflict with interChipSpace
	if (!x.isNull() && !y.isNull() && lD == 0)	
	{
		rbp::Rect r1; r1.x=r1.y=0; r1.width=pw[rel_pkg]+interChipSpace; r1.height=ph[rel_pkg]+computedChipGap;
		rbp::Rect r2; r2.x=r2.y=0; r2.width=ph[rel_pkg]+interChipSpace; r2.height=pw[rel_pkg]+computedChipGap;
		rbp::Rect r3; r3.x=(int)(dx*resolution); r3.y=(int)(dy*resolution); r3.width=pw[pkg_idx]; r3.height=ph[pkg_idx];
		//std::cout << "[0, 0, " << r1.width << ", " << r1.height << "]" << std::endl;
		//std::cout << "[0, 0, " << r2.width << ", " << r2.height << "]" << std::endl;
		//std::cout << "[" << r3.x << ", " << r3.y << ", " << r3.width << ", " << r3.height << "]" << std::endl;

		bool r1r3d = rbp::DisjointRectCollection::Disjoint(r1, r3);
		bool r2r3d = rbp::DisjointRectCollection::Disjoint(r2, r3);
		if (!r1r3d && !r2r3d)	// both rotation not disjoint with offset
		{
			std::strstream str;
			str << "Unsolvable relative constraint on package \"" << packages[pkg_idx]["name"].asString() << "\" index(" << pkg_idx << "), offsets conflict with chipgap" << std::endl;
			throw std::exception(str.str());
		}
		else if (!r1r3d)
			std::cout << "WARNING: unsolvable relative constraint, offsets conflict with chipgap under no rotation" << std::endl;
		else if (!r2r3d)
			std::cout << "WARNING: unsolvable relative constraint, offsets conflict with chipgap under rotation" << std::endl;
	}
	BoolVar pc0 = twoPointConstraint(pkg_idx, rot, 0, rel_pkg, dx, dy);
	BoolVar dr0 = expr(*this, pr[rel_pkg] == 0 && pr[pkg_idx] == rot);
	BoolVar pc1 = twoPointConstraint(pkg_idx, rot, 1, rel_pkg, dx, dy);
	BoolVar dr1 = expr(*this, pr[rel_pkg] == 1 && pr[pkg_idx] == (1+rot)%4);
	BoolVar pc2 = twoPointConstraint(pkg_idx, rot, 2, rel_pkg, dx, dy);
	BoolVar dr2 = expr(*this, pr[rel_pkg] == 2 && pr[pkg_idx] == (2+rot)%4);
	BoolVar pc3 = twoPointConstraint(pkg_idx, rot, 3, rel_pkg, dx, dy);
	BoolVar dr3 = expr(*this, pr[rel_pkg] == 3 && pr[pkg_idx] == (3+rot)%4);
	BoolVar ret = expr(*this, (pc0 && dr0) || (pc1 && dr1) || (pc2 && dr2) || (pc3 && dr3));
	return ret;
}


void LayoutSolver::processRangeConstraint(const Json::Value& constraint, int pkg_idx)
{
	const Json::Value x = constraint["x"];
	if (!x.isNull() && x.isString())
	{
		processRangeConstraint(x.asString(), pkg_idx, px, packages[pkg_idx]["name"].asString(), "x");
	}
	const Json::Value y = constraint["y"];
	if (!y.isNull() && y.isString())
	{
		processRangeConstraint(y.asString(), pkg_idx, py, packages[pkg_idx]["name"].asString(), "y");
	}
	const Json::Value z = constraint["layer"];
	if (!z.isNull() && z.isString())
	{
		processRangeConstraint(z.asString(), pkg_idx, pz, packages[pkg_idx]["name"].asString(), "layer", 1.0);
	}
}

//*******************************************************************************************
//
//	LayoutSolver::processRangeConstraint
//
//	std::string val -- The string representation of the JSON constraint.
//	int vi -- The package index
//	IntVarArray& varArr -- A Gecode::IntVarArray, such as px, py, or pz, depending on if the constraint is x, y, or layer. 
//	std::string name -- The name of the package corresponding to the package index.
//	std::string varName -- Either "x", "y", or "layer".
//	double res -- Set to 1.0 if it's a "layer" constraint.  Defaults to "resolution" (=10.0) for x and y.
//
//*******************************************************************************************

void LayoutSolver::processRangeConstraint(std::string val, int vi, IntVarArray& varArr, std::string name, std::string varName, double res)
{
	std::pair<double, double> range = extractRange(val);	// Get the upper and lower bounds of the range.
	BoolVar lowerp = expr( *this, varArr[vi] >= (int)(range.first * res));
	BoolVar upperp = expr( *this, varArr[vi] <= (int)(range.second * res));

	rel( *this, upperp && lowerp );
	std::cout << "Applying Range Constraint on " << name <<
		":\nLower range limit (=" << RoundedCast(range.first) << 
		" mm) <= " << varName << 
		" <= upper range limit (=" << RoundedCast(range.second) << 
		" mm)." << std::endl;

	// check constraint for x & y a) upper > lower, b) upper > edgegap, c) lower < (boardW|H - edgegap)
	if (range.second < range.first)
	{
		std::strstream str;
		str << "Unsolvable range constraint on package \"" << name << "\" index(" << vi << "), upper < lower" << std::endl;
		throw std::exception(str.str());
	}

	if (varName.compare("x") == 0 || varName.compare("y") == 0)
	{

		// Set the edge gap to the package's X-or-Y edge-of-board margin.  MOT-728.
		int eg = ebmx[vi];
		if( varName.compare("y") == 0 )
		{
			eg = ebmy[vi];
		}

		// Convert the edge gap to millimeters
		double leftEdgeGapInMm = (double) eg / res;
		double rightEdgeGapInMm = (double) (boardW - eg) / res;
		double topEdgeGapInMm = (double) (boardH - eg) / res;

		if (range.second < leftEdgeGapInMm)
		{
			std::strstream str;
			str << "Unsolvable range constraint on package \"" << name << 
				"\" index(" << vi << 
				"):\nUpper limit of the range constraint (=" << RoundedCast( range.second ) << 
				" mm) is less than the board's edge spacing (=" << RoundedCast( leftEdgeGapInMm ) 
				<< " mm)." << std::endl;
			throw std::exception(str.str());
		}
		if ((varName.compare("x") == 0) && (range.first > rightEdgeGapInMm))
		{
			std::strstream str;
			str << "Unsolvable range constraint on package \"" << name << 
				"\" index(" << vi << 
				"):\nLower limit of range constraint (=" << RoundedCast( range.first ) << 
				" mm) > right edge of board minus board-edge space =(" << RoundedCast( rightEdgeGapInMm ) << 
				" mm)." << std::endl;
			throw std::exception(str.str());
		}
		if ((varName.compare("y") == 0) && (range.first > topEdgeGapInMm))
		{
			std::strstream str;
			str << "Unsolvable range constraint on package \"" << name <<
				"\" index(" << vi << 
				"):\nLower limit of range constraint (=" << RoundedCast( range.first ) <<
				" mm) > top of board minus board-edge spacing (=" << RoundedCast( topEdgeGapInMm ) << 
				" mm)." <<std::endl;
			throw std::exception(str.str());
		}
	}
}

void LayoutSolver::processRegionConstraint(const Json::Value& constraint, int pkg_idx)
{
	BoolVar prb = expr(*this, pr[pkg_idx] == 0 || pr[pkg_idx] == 2);
	std::cout << "Applying In Region Constraint: x[";
	int dx,dy;
	BoolVar xCons;
	{
		const Json::Value& x = constraint["x"];
		std::pair<double, double> xRange = extractRange(x.asString());
		std::cout << xRange.first << "-" << xRange.second << "] y[";
		BoolVar lower = expr( *this, px[pkg_idx] >= (int)(xRange.first * resolution));
		BoolVar upperw = expr( *this, px[pkg_idx] <= (int)(xRange.second * resolution - pw[pkg_idx]));
		BoolVar upperl = expr( *this, px[pkg_idx] <= (int)(xRange.second * resolution - ph[pkg_idx]));
		BoolVar upper = expr( *this, (prb >> upperw) && ((!prb) >> upperl));
		xCons = expr( *this, lower && upper);
		dx = (int)((xRange.second - xRange.first)*resolution);
	}
	BoolVar yCons;
	{
		const Json::Value& y = constraint["y"];
		std::pair<double, double> yRange = extractRange(y.asString());
		std::cout << yRange.first << "-" << yRange.second << "] layer[";
		BoolVar lower = expr( *this, py[pkg_idx] >= (int)(yRange.first * resolution));
		BoolVar upperw = expr( *this, py[pkg_idx] <= (int)(yRange.second * resolution - pw[pkg_idx]));
		BoolVar upperl = expr( *this, py[pkg_idx] <= (int)(yRange.second * resolution - ph[pkg_idx]));
		BoolVar upper = expr( *this, (prb >> upperl) && ((!prb) >> upperw));
		yCons = expr( *this, lower && upper);
		dy = (int)((yRange.second - yRange.first)*resolution);
	}
	BoolVar zCons;
	{
		const Json::Value& z = constraint["layer"];
		int zVal = atoi(z.asString().c_str());
		std::cout << zVal << "]" << std::endl;
		zCons = expr( *this, pz[pkg_idx] == zVal );
	}
	rel( *this, xCons && yCons && zCons);
	// check constraint
	if ( abs(dx * dy) < (pw[pkg_idx]*ph[pkg_idx]) )
	{
		// throw a fit
		std::strstream str;
		str << "Unsolvable region constraint on package \"" << packages[pkg_idx]["name"].asString() << "\" index(" << pkg_idx << "), region smaller than pkg size" << std::endl;
		throw std::exception(str.str());
	}
}


void LayoutSolver::processExRegionConstraint(const Json::Value& constraint, int pkg_idx)
{
	std::cout << "Applying Ex Region Constraint: x[";
	BoolVar prb = expr( *this, pr[pkg_idx] == 0 || pr[pkg_idx] == 2);
	BoolVar xCons;
	{
		const Json::Value& x = constraint["x"];
		std::pair<double, double> xRange = extractRange(x.asString());
		std::cout << xRange.first << "-" << xRange.second << "] y[";
		BoolVar upper = expr( *this, px[pkg_idx] >= (int)(xRange.second * resolution));
		BoolVar lowerw = expr( *this, px[pkg_idx] <= (int)(xRange.first * resolution - pw[pkg_idx]));
		BoolVar lowerh = expr( *this, px[pkg_idx] <= (int)(xRange.first * resolution - ph[pkg_idx]));
		BoolVar lower = expr( *this, (prb && lowerw) || ((!prb) && lowerh));
		xCons = expr( *this, lower || upper);
	}
	BoolVar yCons;
	{
		const Json::Value& y = constraint["y"];
		std::pair<double, double> yRange = extractRange(y.asString());
		std::cout << yRange.first << "-" << yRange.second << "] layer[";
		BoolVar upper = expr( *this, py[pkg_idx] >= (int)(yRange.second * resolution));
		BoolVar lowerw = expr( *this, py[pkg_idx] <= (int)(yRange.first * resolution - pw[pkg_idx]));
		BoolVar lowerh = expr( *this, py[pkg_idx] <= (int)(yRange.first * resolution - ph[pkg_idx]));
		BoolVar lower = expr( *this, (prb && lowerh) || ((!prb) && lowerw));
		yCons = expr( *this, lower || upper);
	}
	BoolVar zCons;
	{
		const Json::Value& z = constraint["layer"];
		int zVal = atoi(z.asString().c_str());
		std::cout << zVal << "]" << std::endl;
		zCons = expr( *this, pz[pkg_idx] != zVal );
	}
	rel( *this, xCons || yCons || zCons);
}

void LayoutSolver::processRelativeRegionConstraint(const Json::Value& constraint, int pkg_idx)
{
	int rel_pkg = constraint["pkg_idx"].asInt();	// index of other package
	if (rel_pkg < 0 && rel_pkg > rootJson["packages"].size())
	{
		std::cout << "Invalid pkg_idx value: " << rel_pkg << " in constraint applied to pkg: " << pkg_idx << std::endl;
		return;	
	}
	else
	{
		rel_pkg = pkg_idx_map[rel_pkg];		// map 
		if (packages[rel_pkg]["pkg_idx"] != constraint["pkg_idx"].asInt())
		{
			std::cout << "Invalid pkg_idx map: from: " << rel_pkg << " to: " << packages[rel_pkg]["pkg_idx"] << std::endl;
			return;
		}
	}
	// box range
	const Json::Value& x = constraint["x"];
	std::pair<double, double> xRange = extractRange(x.asString());
	const Json::Value& y = constraint["y"];
	std::pair<double, double> yRange = extractRange(y.asString());

	double x1 = xRange.first, x2 = xRange.second;
	double y1 = yRange.first, y2 = yRange.second;
	std::cout << "Applying Relative Region Constraint: " << x1 << " <= " << packages[pkg_idx]["name"].asString() << "{" << packages[rel_pkg]["name"].asString() << "}.x" << " <= " << x2 << std::endl; 
	std::cout << "Applying Relative Region Constraint: " << y1 << " <= " << packages[pkg_idx]["name"].asString() << "{" << packages[rel_pkg]["name"].asString() << "}.y" << " <= " << y2 << std::endl; 

	// if i consider my anchor chip as being at the origin, then I can got four transformations
	// of the target  box
	// and then I have to create a box constraint expr for each rotation of anchor chip
	BoolVar rot0 = boxConstraint(pkg_idx, 0, rel_pkg, x1, y1, x2, y2);
	BoolVar rot1 = boxConstraint(pkg_idx, 1, rel_pkg, x1, y1, x2, y2);
	BoolVar rot2 = boxConstraint(pkg_idx, 2, rel_pkg, x1, y1, x2, y2);
	BoolVar rot3 = boxConstraint(pkg_idx, 3, rel_pkg, x1, y1, x2, y2);

	rel(*this, rot0 || rot1 || rot2 || rot3);

	// TBD SKN - revisit rotation preference of relative constraints
	//const Json::Value r = constraint["rotation"];
	//if (!r.isNull())
	//	rel(*this, pr[pkg_idx] == r.asInt());			// if rotation is specified - force to that
	//else
	//	rel(*this, pr[pkg_idx] == pr[rel_pkg]);		// else orients the chip same as another one

	// choices: a) null --> don't care, b) same layer, c) opposite layer
	const Json::Value l = constraint["layer"];
	if (!l.isNull())
	{
		int d = l.asInt();
		if (d == 0)	// same as the constrained
			rel(*this, pz[pkg_idx] == pz[rel_pkg]);
		else if (d == 1)	// opposite of constrained
			rel(*this, pz[pkg_idx] != pz[rel_pkg]);
		else
			std::cout << "Layer constraint value of " << d << " is not valid" << std::endl;
	}

	// check constraint
	int dx,dy;
	dx = (int)(resolution*(xRange.second - xRange.first));
	dy = (int)(resolution*(yRange.second - yRange.first));
	if ( abs(dx * dy) < (pw[pkg_idx]*ph[pkg_idx]) )
	{
		// throw a fit
		std::strstream str;
		str << "Unsolvable relative region constraint on package \"" << packages[pkg_idx]["name"].asString() << "\" index(" << pkg_idx << "), region smaller than pkg size" << std::endl;
		throw std::exception(str.str());
	}
}

BoolVar LayoutSolver::twoPointConstraint(int pi, int drot, int refrot, int refi, double x, double y)
{
	int cost[] = { 1, 0, -1, 0 };
	int sint[] = { 0, 1, 0, -1 };
	int  rx = (int)(resolution * (x * cost[refrot] - y * sint[refrot]));
	int ry = (int)(resolution * (x * sint[refrot] + y * cost[refrot]));

	if (refrot == 1) 
		rx += (ph[refi] - ((drot % 2 == 0) ? ph[pi] : pw[pi])); 
	else if (refrot == 2) { 
		rx += (pw[refi] - ((drot % 2 == 0) ? pw[pi] : ph[pi])); 
		ry += (ph[refi] - ((drot % 2 == 0) ? ph[pi] : pw[pi])); 
	} else if (refrot == 3)
		ry += (pw[refi] - ((drot % 2 == 0) ? pw[pi] : ph[pi]));

	BoolVar xCons = expr( *this, px[pi] == px[refi] + rx);
	BoolVar yCons = expr( *this, py[pi] == py[refi] + ry);

	return expr(*this, xCons && yCons);
}


BoolVar LayoutSolver::boxConstraint(int pi, int refrot, int refi, double x1, double y1, double x2, double y2)
{
	std::pair<int, int> p1 = rotatePoint(refrot, refi, x1, y1);
	std::pair<int, int> p2 = rotatePoint(refrot, refi, x2, y2);
	int lx = MIN(p1.first, p2.first);
	int ly = MIN(p1.second, p2.second);
	int ux = MAX(p1.first, p2.first);
	int uy = MAX(p1.second, p2.second);

	//std::cout << "Range Rotated [" << refrot << "] " << lx << " <= x <= " << ux << std::endl;
	//std::cout << "Range Rotated [" << refrot << "] " << ly << " <= y <= " << uy << std::endl;

	IntVar p1x = expr(*this, px[refi] + lx);
	IntVar p1y = expr(*this, py[refi] + ly);
	IntVar p2x = expr(*this, px[refi] + ux);
	IntVar p2y = expr(*this, py[refi] + uy);

	BoolVar range = rangeConstraint(pi, p1x, p1y, p2x, p2y);
	BoolVar prefRot = expr(*this, pr[refi] == refrot);

	return expr(*this, prefRot && range);
}

BoolVar LayoutSolver::rangeConstraint(int pi, IntVar& p1x, IntVar& p1y, IntVar& p2x, IntVar& p2y)
{
	BoolVar prb = expr(*this, pr[pi] == 0 || pr[pi] == 2);
	BoolVar xCons;
	{
		BoolVar lower = expr( *this, px[pi] >= p1x);
		BoolVar upperw = expr( *this, px[pi] <= (p2x - pw[pi]));
		BoolVar upperl = expr( *this, px[pi] <= (p2x - ph[pi]));
		BoolVar upper = expr( *this, (prb && upperw) || ((!prb) && upperl));
		xCons = expr(*this, lower && upper);
	}
	BoolVar yCons;
	{
		BoolVar lower = expr( *this, py[pi] >= p1y);
		BoolVar upperw = expr( *this, py[pi] <= (p2y - pw[pi]));
		BoolVar upperl = expr( *this, py[pi] <= (p2y - ph[pi]));
		BoolVar upper = expr( *this, (prb && upperl) || ((!prb) && upperw));
		yCons = expr( *this, lower && upper);
	}
	return expr( *this, xCons && yCons );
}

std::pair<int, int> LayoutSolver::rotatePoint(int rotation, int refi, double x, double y)
{
	int cost[] = { 1, 0, -1, 0 };
	int sint[] = { 0, 1, 0, -1 };
	int  rx = (int)(resolution * (x * cost[rotation] - y * sint[rotation]));
	int ry = (int)(resolution * (x * sint[rotation] + y * cost[rotation]));

	if (rotation == 1) rx += ph[refi];
	else if (rotation == 2) { rx += pw[refi]; ry += ph[refi]; }
	else if (rotation == 3) ry += pw[refi];

	return std::pair<int, int>(rx, ry);
}

std::pair<double, double> LayoutSolver::extractRange(const std::string& val)
{
	// range is a-b, or a:b, or a
	int seppos = val.find_first_of(":");
	double begin=0, end=0;
	if (seppos != std::string::npos)
	{
		std::string bs = val.substr(0, seppos);
		std::string es = val.substr(seppos+1);
		begin = (bs != "") ? atof(bs.c_str()) : 0;
		end = (es !="") ? atof(es.c_str()) : 9999;
	}
	else
		begin = end = atof(val.c_str());

	return std::pair<double,double>(begin, end);
}



