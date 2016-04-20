#ifndef LAYOUT_DEFINES_H
#define LAYOUT_DEFINES_H

#include <gecode/int.hh>
#include <gecode/search.hh>
#include <gecode/minimodel.hh>
#include <gecode/gist.hh>
#include <json.h>
#include <fstream>
#include <iostream>
#include <iomanip>
#include <algorithm>
#include <map>
#include <unordered_map>
#include <list>
#include <utility>
#include <time.h>
#include <sys/timeb.h>
#include <tuple>

#define _USE_MATH_DEFINES
#include <math.h>

#include <MaxRectsBinPack.h>

#define MAX_LAYERS 4			// for part placement - really only 2
#define MAX(a, b) ((a)>(b) ? (a) : (b))
#define MIN(a, b) ((a)<(b) ? (a) : (b))
#define MAX_OPTS   128

#define CENTER_THRESHOLD 1.0		// size of placement span compared to chip size when to consider a center placement
#define CORNER_THRESHOLD 0.2		// size of placement span compared to chip size when to consider all four corners
// if the placement span is (corner_threshold)% below the chip size we only consider 1 corner for placement

struct RZRect {
	int r;
	int z;
	struct rbp::Rect rect;

public:
	bool operator <(const struct RZRect& rhs) const {
		if (rect.x < rhs.rect.x) return true;
		else if (rect.x == rhs.rect.x) {
			if (rect.y < rhs.rect.y) return true;
			else if (rect.y == rhs.rect.y) {
				if (z < rhs.z) return true;
				else if (z == rhs.z) 
					return (r < rhs.r);
			}
		}
		return false;
	}
};

class LayoutSolver;
class LayoutBrancher;

class LayoutOptions
{
public:
	static const int STATIC_BCF=0;
	static const int DYNAMIC_MCF=1;
	int searchOrder;

	LayoutOptions() : searchOrder(STATIC_BCF)
	{
	}
};
extern LayoutOptions solverOptions;

extern Gecode::BrancherHandle lobranch(
	Gecode::Home home, 
	const Gecode::IntVarArgs& px, const Gecode::IntVarArgs& py,
	const Gecode::IntVarArgs& pz, const Gecode::IntVarArgs& pr,
	const Gecode::IntSharedArray& pw, const Gecode::IntSharedArray& ph,
	const Gecode::IntSharedArray& kow, const Gecode::IntSharedArray& koh,
	const Gecode::IntSharedArray& ebmx, const Gecode::IntSharedArray& ebmy,
	const Gecode::IntSharedArray& rc,
	const int bw, 
	const int bh, 
	const int nl, 
	const int cg,
	std::list<std::pair<double,int>>& area_index_list,
	std::list<RZRect>& ers); 

#endif //LAYOUT_DEFINES_H