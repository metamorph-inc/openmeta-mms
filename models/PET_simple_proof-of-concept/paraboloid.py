from __future__ import print_function

from openmdao.api import IndepVarComp, Component, Problem, Group, AnalysisError

class Paraboloid(Component):
    """ Evaluates the equation f(x,y) = (x-3)^2 + xy + (y+4)^2 - 3 """

    def __init__(self):
        super(Paraboloid, self).__init__()

        self.add_param('x', val=0.0)
        self.add_param('y', val=0.0)

        self.add_output('f_xy', shape=1)

    def solve_nonlinear(self, params, unknowns, resids):
        """f(x,y) = (x-3)^2 + xy + (y+4)^2 - 3
        """

        x = params['x']
        y = params['y']

        if -7.5 <= y and y <= -6.5:
            raise AnalysisError("Parameter out of range")

        unknowns['f_xy'] = (x-3.0)**2 + x*y + (y+4.0)**2 - 3.0

if __name__ == "__main__":

    top = Problem()

    root = top.root = Group()

    root.add('p1', IndepVarComp('x', 3.0))
    root.add('p2', IndepVarComp('y', -4.0))
    root.add('p', Paraboloid())

    root.connect('p1.x', 'p.x')
    root.connect('p2.y', 'p.y')

    top.setup()
    top.run()

    print(top['p.f_xy'])
