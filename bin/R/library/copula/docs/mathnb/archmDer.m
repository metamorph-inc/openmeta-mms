(*
To get both input and output lines use the following in the firstline:
AppendTo[$Echo, "stdout"]

To run the input file in background:
nohup time math < archmDer.m > archmDer.out &

*)

AppendTo[$Echo, "stdout"]

frankGenFun[x_] := -Log[(Exp[-alpha*x] - 1)/(Exp[-alpha] - 1)]
frankGenInv[s_] := -1/alpha*Log[1 + Exp[-s]*(Exp[-alpha] - 1)]

claytonGenFun[x_] := x^(-alpha) - 1
claytonGenInv[s_] := (1 + s)^(-1/alpha)

gumbelGenFun[x_] := ( - Log[x] )^alpha
gumbelGenInv[s_] := Exp[-s ^(1 / alpha)]

amhGenFun[x_] := Log[ (1 - alpha (1 - x) ) / x ]
amhGenInv[s_] := (1 - alpha) / (Exp[s] - alpha)

myD[f_, x_, n_] := Module[{df, i},
    df[0] = f; 
    For[i = 1, i <= n, i++,
      df[i] = Simplify[D[df[i - 1], x]];
      ];
    Table[df[i], {i, 1, n}]
    ]
uu = List[u1, u2, u3, u4, u5, u6, u7, u8, u9, u10]

mypdf[gfun_, ginv_, n_] := Module[
    {di, df, ss, part1, part2, val},
    di[s_] = Simplify[D[ginv[s], {s, n}]];
    df[u_] = Simplify[D[gfun[u], u]];
    ss = Sum[gfun[uu[[i]]], {i, 1, n}];
    part1 = Simplify[di[ss]];
    part2 = Simplify[Product[df[uu[[i]]], {i, 1, n}]];
    val = Simplify[part1 * part2];
    val
    ]
mycdf[gfun_, ginv_, n_] := Module[
    {ss},
    ss = Sum[gfun[uu[[i]]], {i, 1, n}];
    Simplify[ginv[ss]]
    ]

Export["frankCopula.pdf.expr", 
       FortranForm[Table[mypdf[frankGenFun, frankGenInv, i], {i, 1, 6}]], "Table"]
Export["frankCopula.genfun.expr", FortranForm[myD[frankGenFun[u], u, 2]], "Table"]

Export["claytonCopula.pdf.expr", 
       FortranForm[Table[mypdf[claytonGenFun, claytonGenInv, i], {i, 1, 10}]], "Table"]
Export["claytonCopula.genfun.expr", FortranForm[myD[claytonGenFun[u], u, 2]], "Table"]

Export["gumbelCopula.pdf.expr", 
       FortranForm[Table[mypdf[gumbelGenFun, gumbelGenInv, i], {i, 1, 10}]], "Table"]
Export["gumbelCopula.genfun.expr", FortranForm[myD[gumbelGenFun[u], u, 2]], "Table"]

Export["amhCopula.pdf.expr",
       FortranForm[Table[mypdf[amhGenFun, amhGenInv, i], {i, 1, 2}]], "Table"]
Export["amhCopula.genfun.expr", FortranForm[myD[amhGenFun[u], u, 2]], "Table"]


