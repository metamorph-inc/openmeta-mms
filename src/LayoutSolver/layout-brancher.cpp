/*
*  Authors:
*    Sandeep Neema <neemask@metamorphsoftware.com>
*
* This file includes the custom Layout Brancher code
* The main search tree code is in the Choice function
*/


#include "layout-brancher.h"
#include <vector>
#include <numeric>
#include <set>
#include <tuple>

using namespace Gecode;

// Brancher inner class methods
LayoutBrancher::Choice::Choice(const Brancher& b, unsigned int a, int i, RZRect* p, int n_p)
	: Gecode::Choice(b,a), item(i), 
	possible(heap.alloc<RZRect>(n_p)), n_possible(n_p)
{
	for (int k=n_possible; k--; )
		possible[k] = p[k];
}

LayoutBrancher::Choice::~Choice(void)
{
	heap.free<RZRect>(possible, n_possible);
}


// Brancher class implementation

LayoutBrancher::LayoutBrancher(Home home, 
	ViewArray<Int::IntView>& x, 
	ViewArray<Int::IntView>& y,
	ViewArray<Int::IntView>& z,
	ViewArray<Int::IntView>& r,
	IntSharedArray& w,
	IntSharedArray& h,
	IntSharedArray& kw,
	IntSharedArray& kh,
	IntSharedArray& ebx,
	IntSharedArray& eby,
	IntSharedArray& rco,
	const std::list<RZRect>& exclusionRects,
	int bw,
	int bh,
	int nl,
	int cg ) 
	: Brancher(home), px(x), py(y), pz(z), pr(r), pw(w), ph(h), kow(kw), koh(kh), ebmx(ebx), ebmy(eby), rc(rco),
	boardW(bw), boardH(bh), numLayers(nl), chipGap(cg), item(0)
{
	for(int i=0; i<nl; i++)
		binPacker[i].Init(bw, bh);
	// reserve space for global exclusion rects
	for(std::list<RZRect>::const_iterator i=exclusionRects.begin(); i!=exclusionRects.end(); i++)
	{
		const rbp::Rect& rect = (*i).rect;
		binPacker[(*i).z].PlaceRect(rect);
		std::cout << "Placing exclusion rects[" << rect.x << "," << rect.y
			<< "," << rect.width << "," << rect.height << "] on layer [" << (*i).z << "]" << std::endl; 
	}
	wasPlaced.resize(x.size(), 0);
	home.notice(*this,AP_DISPOSE);
}

LayoutBrancher::LayoutBrancher(Space& home, bool share, LayoutBrancher& LayoutBrancher) 
	: Brancher(home, share, LayoutBrancher), item(LayoutBrancher.item)
{
	px.update(home, share, LayoutBrancher.px);
	py.update(home, share, LayoutBrancher.py);
	pz.update(home, share, LayoutBrancher.pz);
	pr.update(home, share, LayoutBrancher.pr);
	pw.update(home, share, LayoutBrancher.pw);
	ph.update(home, share, LayoutBrancher.ph);
	kow.update(home, share, LayoutBrancher.kow);
	koh.update(home, share, LayoutBrancher.koh);
	ebmx.update(home, share, LayoutBrancher.ebmx);
	ebmy.update(home, share, LayoutBrancher.ebmy);
	rc.update(home, share, LayoutBrancher.rc);
	boardW = LayoutBrancher.boardW;
	boardH = LayoutBrancher.boardH;
	numLayers = LayoutBrancher.numLayers;
	chipGap = LayoutBrancher.chipGap;
	for(int i=0; i<numLayers; i++)
		binPacker[i] = LayoutBrancher.binPacker[i];	// check OR implement the copy constructor of binPacker
	wasPlaced = LayoutBrancher.wasPlaced;
}

size_t LayoutBrancher::dispose(Gecode::Space& home)
{
	home.ignore(*this, Gecode::AP_DISPOSE);
	pw.~IntSharedArray();
	ph.~IntSharedArray();
	kow.~IntSharedArray();
	koh.~IntSharedArray();
	ebmx.~IntSharedArray();
	ebmy.~IntSharedArray();
	rc.~IntSharedArray();
	return sizeof(*this);
}

bool LayoutBrancher::status(const Space& s) const
{
	std::vector<size_t> indices(px.size()); // list of indicex not placed yet
	indices.clear();

	// place any parts that got assigned due to constraint propagation before selecting next chip
	for (int i=0; i < px.size(); i++)
	{
		if (wasPlaced[i] == 1)
			continue;

		bool assigned = px[i].assigned() && py[i].assigned() && pz[i].assigned() && pr[i].assigned();
		if (assigned) {		// we are assigning all px,py,pz,pr together - so picking one should be adequate
			struct rbp::Rect rect;
			rect.x = px[i].val(); rect.y = py[i].val();
			if (pr[i].val() == 0 || pr[i].val() == 2 ) { rect.width = pw[i]; rect.height = ph[i]; }
			else if (pr[i].val() == 1 || pr[i].val() == 3 ) { rect.width = ph[i]; rect.height = pw[i]; }
			binPacker[pz[i].val()].PlaceRect(rect);
			wasPlaced[i] = 1;
		}
		else
			indices.push_back(i);
	}

	// select next chip to place
	switch(solverOptions.searchOrder)
	{
	case LayoutOptions::STATIC_BCF:
		return staticSort(s);
	case LayoutOptions::DYNAMIC_MCF:
		return dynamicSort_MCF(s, indices);
	}
}

bool LayoutBrancher::staticSort(const Space&) const
{
	// order of selection variable index based - determined statically (by package sort in solver)
	for (int i = item; i < px.size(); i++)
	{
		bool assigned = px[i].assigned() && py[i].assigned() && pz[i].assigned() && pr[i].assigned();
		if (!assigned) {		
			item = i; 
			return true;
		} 
	}
	return false;
}

bool LayoutBrancher::dynamicSort_MCF(const Space&, std::vector<size_t>& indices) const
{
	if (indices.size() == 0) // all items assigned
		return false;

	std::sort(indices.begin(), indices.end(), [&](size_t a, size_t b) {
		return constraintMetric(a) > constraintMetric(b);
	});

	item = indices[0];
	return true;
}

double LayoutBrancher::constraintMetric(size_t a) const
{
	double area = pw[a]*(double)ph[a];
	double dpx = 1.0, dpy = 1.0;
	for(IntVarRanges pxr(px[a]); pxr(); ++pxr)
		dpx += (pxr.max() - pxr.min());
	for(IntVarRanges pyr(py[a]); pyr(); ++pyr)
		dpy += (pyr.max() - pyr.min());

	return (area*area) / (dpx*dpy);
}

Gecode::Choice* LayoutBrancher::choice(Space& home)
{
	int iw = pw[item];
	int ih = ph[item];

	typedef std::multimap<int, RZRect> AreaRects;
	AreaRects dsts; 

	// if the item being placed is relatively constrained - then offer its rotation/layer as choices, and 
	// let the propagator determine the location 
	if ((rc[item] >= 0) && 
		!(pr[item].assigned() && pz[item].assigned()))
	{
		for(int r=pr[item].min(); r<=pr[item].max(); r++)
			for(int z=pz[item].min(); z<=pz[item].max(); z++)
			{
				struct RZRect d = {r, z, {0, 0, 0, 0}};
				dsts.insert(std::pair<int, RZRect>(0, d));
			}
	}
	else // not a relatively constrained item with rotation/layer not assigned
	{
		for(int z=pz[item].min(); z<=pz[item].max(); z++)
		{
			rbp::MaxRectsBinPack& bpl = binPacker[z];

			for(int i=0; i<bpl.freeRectangles.size(); i++)
			{
				struct rbp::Rect& freeRect = bpl.freeRectangles[i];
				struct rbp::Rect dest;

				//  (1) CHECK free rect area > chip area
				int areaFit = freeRect.width * freeRect.height - iw * ih;
				if (areaFit < 0)
					continue;

				//  (2) freeRect above the allowable max of domain
				if ((px[item].max() < freeRect.x) || (py[item].max() < freeRect.y))
					continue;

				// bounds on origin of rects induced by domain constraints && free rect
				int lx = (freeRect.x <= ebmx[item]) ? ebmx[item] : freeRect.x + MAX(chipGap,kow[item]);
				int ly = (freeRect.y <= ebmy[item]) ? ebmy[item] : freeRect.y + MAX(chipGap,koh[item]);
				int xOrgLower = MAX(px[item].min(), lx);
				int yOrgLower = MAX(py[item].min(), ly);

				// top-right bounds on freeRect - we don't want it to go beyond edgeGap
				int fbx = ((freeRect.x+freeRect.width) > (boardW - ebmx[item])) ?
					(boardW - ebmx[item]) : ((freeRect.x+freeRect.width) - MAX(chipGap, kow[item]));
				int fby = ((freeRect.y+freeRect.height) > (boardH - ebmy[item])) ?
					(boardH - ebmy[item]) : ((freeRect.y+freeRect.height) - MAX(chipGap, koh[item]));

				bool tryDirect = (pr[item].in(0) || pr[item].in(2));
				bool tryRotate = (pr[item].in(1) || pr[item].in(3));

				// if the parts are square - then don't do both direct and rotate
				if ((ih == iw) && tryDirect && tryRotate)
					tryRotate = false;

				// direct placement
				if ((freeRect.width >= iw) && (freeRect.height >= ih) && tryDirect)
				{
					int xOrgUpper = MIN(px[item].max(), (fbx - iw));
					int yOrgUpper = MIN(py[item].max(), (fby - ih));

					int x[2] = {xOrgLower, xOrgUpper};
					int y[2] = {yOrgLower, yOrgUpper};

					int dw = xOrgUpper - xOrgLower;
					int dh = yOrgUpper - yOrgLower;

					// center
					if ((dw > CENTER_THRESHOLD * iw) || (dh > CENTER_THRESHOLD * ih))
					{
						dest.x = (xOrgUpper + xOrgLower)/2;
						dest.y = (yOrgUpper + yOrgLower)/2;

						if ((dest.x >= xOrgLower && dest.y >= yOrgLower) &&
							((dest.x + iw) <= fbx) &&
							((dest.y + ih) <= fby))
						{
							dest.width = iw;
							dest.height = ih;
							RZRect rzRect = {pr[item].assigned() ? pr[item].val() : 0, z, dest};

							dsts.insert(std::pair<int, RZRect>(areaFit, rzRect));
						}
					}

					// four corners
					for(int xi=0; xi<((dw < (CORNER_THRESHOLD * iw))? 1:2); xi++)
						for(int yi=0; yi<((dh < (CORNER_THRESHOLD * ih))?1:2); yi++)
						{
							dest.x = x[xi];
							dest.y = y[yi];
							if ((dest.x >= xOrgLower && dest.y >= yOrgLower) &&
								((dest.x + iw) <= fbx) &&
								((dest.y + ih) <= fby))
							{
								dest.width = iw;
								dest.height = ih;
								RZRect rzRect = {pr[item].assigned() ? pr[item].val() : 0, z, dest};

								dsts.insert(std::pair<int, RZRect>(areaFit, rzRect));
							}
						}

				} // if direct

				// rotated placement
				if ((freeRect.width >= ih) && (freeRect.height >= iw) && tryRotate)
				{
					int xOrgUpper = MIN(px[item].max(), (fbx-ih));
					int yOrgUpper = MIN(py[item].max(), (fby-iw));

					int x[2] = {xOrgLower, xOrgUpper};
					int y[2] = {yOrgLower, yOrgUpper};

					int dw = xOrgUpper - xOrgLower;
					int dh = yOrgUpper - yOrgLower;

					// center
					if ((dw > CENTER_THRESHOLD * ih) || (dh > CENTER_THRESHOLD * iw))
					{
						dest.x = (xOrgUpper + xOrgLower)/2;
						dest.y = (yOrgUpper + yOrgLower)/2;

						if ((dest.x >= xOrgLower && dest.y >= yOrgLower) &&
							((dest.x + ih) <= fbx) &&
							((dest.y + iw) <= fby))
						{
							dest.width = ih;
							dest.height = iw;
							RZRect rzRect = {pr[item].assigned() ? pr[item].val() : 1, z, dest};

							dsts.insert(std::pair<int, RZRect>(areaFit, rzRect));
						}
					}

					// four corners
					for(int xi=0; xi<((dw < (CORNER_THRESHOLD * ih))? 1:2); xi++)
						for(int yi=0; yi<((dh < (CORNER_THRESHOLD * iw))?1:2); yi++)
						{
							dest.x = x[xi];
							dest.y = y[yi];
							if ((dest.x >= xOrgLower && dest.y >= yOrgLower) &&
								((dest.x + ih) <= fbx) &&
								((dest.y + iw) <= fby))
							{
								dest.width = ih;
								dest.height = iw;
								RZRect rzRect = {pr[item].assigned() ? pr[item].val() : 1, z, dest};

								dsts.insert(std::pair<int, RZRect>(areaFit, rzRect));
							}
						}

				} // if rotated


			} // loop over freeRect's
		} // loop over layers
	} // else (relatively constrained with rotation/layer not asigned


	Region region(home);	// local space for allocation
	struct RZRect *possible = region.alloc<RZRect>(
		(dsts.size() > 0 ? dsts.size() : 1)
		* sizeof(RZRect));
	int n_possible = 0;

	// copy and filter duplicate rects
	std::map<RZRect, int> uniqDsts;
	for(AreaRects::const_iterator i=dsts.begin(); i!=dsts.end(); i++)
	{
		if (uniqDsts.find(i->second) == uniqDsts.end())	// is the choice unique
		{
			possible[n_possible++] = i->second;
			uniqDsts[i->second] = 1;
		}
	}
	dsts.clear();
	uniqDsts.clear();

	if (n_possible == 0)
	{
		RZRect invalid = {0, 0, {-1, -1, 10, 10}};
		possible[n_possible++] = invalid;
	}


	// all possible rectangle placements
	return new Choice(*this, n_possible, item, possible, n_possible);

}

const Gecode::Choice* LayoutBrancher::choice(const Space& home, Archive& e)
{
	int alt, item, n_same;
	e >> alt >> item >> n_same;
	Region re(home);
	struct RZRect* same = re.alloc<RZRect>(n_same);
	for (int i=n_same; i--;) e >> same[i].r >> same[i].z >> same[i].rect.x >> same[i].rect.y >> same[i].rect.width >> same[i].rect.height;
	return new Choice(*this, alt, item, same, n_same);
}

ExecStatus LayoutBrancher::commit(Space& home, const Gecode::Choice& _c, unsigned int a)
{
	const Choice& c = static_cast<const Choice&>(_c);

	// check the choice is not a partial choice
	if (c.possible[a].rect.width == 0 && c.possible[a].rect.height == 0 &&
		c.possible[a].rect.x == 0 && c.possible[a].rect.y == 0)
	{
		GECODE_ME_CHECK(pr[c.item].eq(home, c.possible[a].r));
		GECODE_ME_CHECK(pz[c.item].eq(home, c.possible[a].z));
	}
	else
	{
		// place the commited rect in binPacker
		rbp::MaxRectsBinPack& bpl = binPacker[c.possible[a].z];
		bpl.PlaceRect(c.possible[a].rect);
		wasPlaced[c.item] = 1;

		// assign the values of px, py, pr
		GECODE_ME_CHECK(pr[c.item].eq(home, c.possible[a].r));
		GECODE_ME_CHECK(pz[c.item].eq(home, c.possible[a].z));
		GECODE_ME_CHECK(px[c.item].eq(home, c.possible[a].rect.x));
		GECODE_ME_CHECK(py[c.item].eq(home, c.possible[a].rect.y));
	}

	return ES_OK;
}

void LayoutBrancher::print(const Space&, const Gecode::Choice& _c, 
	unsigned int a,
	std::ostream& o) const
{
	const Choice& c = static_cast<const Choice&>(_c);
	if (a == 0) {
		o << "px[" << c.item << "] = " << c.possible[0].rect.x;
		o << "py[" << c.item << "] = " << c.possible[0].rect.y;
	} else {
		o << "px[" << c.item << "] != " << c.possible[0].rect.x;
		o << "py[" << c.item << "] != " << c.possible[0].rect.y;
	}
}


/*
 * external function to instantiate LayoutBrancher
 */
BrancherHandle lobranch(
	Home home, 
	const IntVarArgs& px, const IntVarArgs& py,
	const IntVarArgs& pz, const IntVarArgs& pr,
	const IntSharedArray& pw, const IntSharedArray& ph,
	const IntSharedArray& kow, const IntSharedArray& koh,
	const IntSharedArray& ebmx, const IntSharedArray& ebmy,
	const IntSharedArray& rco,
	const int bw, 
	const int bh, 
	const int nl, 
	const int cg,
	std::list<std::pair<double,int>>& area_index_list,
	std::list<RZRect>& ers) 
{
	ViewArray<Int::IntView> pxv(home, px.size());
	ViewArray<Int::IntView> pyv(home, py.size());
	ViewArray<Int::IntView> pzv(home, pz.size());
	ViewArray<Int::IntView> prv(home, pr.size());
	IntSharedArray pws(px.size());
	IntSharedArray phs(py.size());
	IntSharedArray kows(px.size());
	IntSharedArray kohs(py.size());
	IntSharedArray ebmxs(px.size());
	IntSharedArray ebmys(py.size());
	IntSharedArray rc(px.size());

	int j=0;
	for(std::list<std::pair<double, int>>::const_iterator i=area_index_list.begin(); i!=area_index_list.end(); i++, j++)
	{
		pxv[j] = Int::IntView(px[ (*i).second ]);
		pyv[j] = Int::IntView(py[ (*i).second ]);
		pzv[j] = Int::IntView(pz[ (*i).second ]);
		prv[j] = Int::IntView(pr[ (*i).second ]);
		pws[j] = pw[ (*i).second ];
		phs[j] = ph[ (*i).second ];
		kows[j] = kow[ (*i).second ];
		kohs[j] = koh[ (*i).second ];
		ebmxs[j] = ebmx[ (*i).second ];
		ebmys[j] = ebmy[ (*i).second ];
		rc[j] = rco[ (*i).second ];
	}
	return *new (home) LayoutBrancher(home, pxv, pyv, pzv, prv, pws, phs, kows, kohs, ebmxs, ebmys, rc, ers, bw, bh, nl, cg );
}


