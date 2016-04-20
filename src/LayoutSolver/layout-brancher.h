#ifndef LAYOUT_BRANCHER_H
#define LAYOUT_BRANCHER_H

#include "layout-defines.h"

class LayoutBrancher : public Gecode::Brancher
{
	// internal class to represent choices
	class Choice : public Gecode::Choice
	{
	public:
		int item;
		RZRect* possible;
		int n_possible;
		Choice(const Brancher& b, unsigned int a, int i, RZRect* p, int n_p);
		virtual size_t size(void) const {
			return sizeof(Choice) + sizeof(RZRect) * n_possible;
		}
		virtual void archive(Gecode::Archive& e) const {
			Gecode::Choice::archive(e);
			e << alternatives() << item << n_possible;
			for (int i=n_possible; i--;) 
				e << possible[i].r 
				  << possible[i].z 
				  << possible[i].rect.x << possible[i].rect.y
				  << possible[i].rect.width << possible[i].rect.height;
		}
		virtual ~Choice(void);
	};

protected:
	mutable rbp::MaxRectsBinPack binPacker[MAX_LAYERS];
	mutable std::vector<int> wasPlaced;  // list of what we placed

	// problem variables
	Gecode::ViewArray<Gecode::Int::IntView> px;
	Gecode::ViewArray<Gecode::Int::IntView> py;
	Gecode::ViewArray<Gecode::Int::IntView> pz;
	Gecode::ViewArray<Gecode::Int::IntView> pr;

	// chip sizes
	Gecode::IntSharedArray pw;
	Gecode::IntSharedArray ph;
	Gecode::IntSharedArray kow;
	Gecode::IntSharedArray koh;

	// edge of board margins
	Gecode::IntSharedArray ebmx;
	Gecode::IntSharedArray ebmy;
	
	// relative constraint dependency
	Gecode::IntSharedArray rc;

	// solver parameters
	int numLayers;
	int boardW;
	int boardH;
	int chipGap;

	mutable int item;	// next chip to place

public:
	LayoutBrancher(Gecode::Home home, 
		Gecode::ViewArray<Gecode::Int::IntView>& x, 
		Gecode::ViewArray<Gecode::Int::IntView>& y,
		Gecode::ViewArray<Gecode::Int::IntView>& z,
		Gecode::ViewArray<Gecode::Int::IntView>& r,
		Gecode::IntSharedArray& w,
		Gecode::IntSharedArray& h,
		Gecode::IntSharedArray& kw,
		Gecode::IntSharedArray& kh,
		Gecode::IntSharedArray& ebx,
		Gecode::IntSharedArray& eby,
		Gecode::IntSharedArray& rc,
		const std::list<RZRect>& exclusionRects,
		int bw,
		int bh,
		int nl,
		int cg);

	LayoutBrancher(Gecode::Space& home, bool share, LayoutBrancher& LayoutBrancher); 

	virtual Gecode::Actor* copy(Gecode::Space& home, bool share) {
		return new (home) LayoutBrancher(home, share, *this);
	}
	virtual size_t dispose(Gecode::Space& home);
	virtual bool status(const Gecode::Space&) const;
	virtual Gecode::Choice* choice(Gecode::Space& home);
	virtual const Gecode::Choice* choice(const Gecode::Space& home, Gecode::Archive& e);
	virtual Gecode::ExecStatus commit(Gecode::Space& home, const Gecode::Choice& _c, unsigned int a);
	virtual void print(const Gecode::Space&, const Gecode::Choice& _c, 
		unsigned int a,
		std::ostream& o) const;

private:
	bool staticSort(const Gecode::Space&) const;
	bool dynamicSort_MCF(const Gecode::Space&, std::vector<size_t>& indices) const;

	double constraintMetric(size_t a) const;
};

#endif // LAYOUT_BRANCHER_H