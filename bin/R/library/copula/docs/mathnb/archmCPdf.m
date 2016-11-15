(*
 To get both input and output lines use the following in the firstline:
 AppendTo[$Echo, "stdout"]
 
 To run the input file in background:
 nohup time math < archmDer.m > archmDer.out &
 
*)
  
  AppendTo[$Echo, "stdout"];

frankGenFun[x_] := -Log[(Exp[-alpha*x] - 1)/(Exp[-alpha] - 1)];
frankGenInv[s_] := -1/alpha*Log[1 + Exp[-s]*(Exp[-alpha] - 1)];

claytonGenFun[x_] := x^(-alpha) - 1;
claytonGenInv[s_] := (1 + s)^(-1/alpha);

gumbelGenFun[x_] := ( - Log[x] )^alpha;
gumbelGenInv[s_] := Exp[-s ^(1 / alpha)];

amhGenFun[x_] := Log[ (1 - alpha (1 - x) ) / x ];
amhGenInv[s_] := (1 - alpha) / (Exp[s] - alpha);

myD[f_, x_, n_] := Module[
  {df, i},
  df[0] = f; 
  For[i = 1, i <= n, i++,
      df[i] = Simplify[D[df[i - 1], x]];
      ];
  Table[df[i], {i, 1, n}]
  ];

uu = List[u1, u2, u3, u4, u5, u6, u7, u8, u9, u10];

mypdf[gfun_, ginv_, n_] := Module[
  {di, df, ss, part1, part2, val},
  di[s_] = Simplify[D[ginv[s], {s, n}]];
  df[u_] = Simplify[D[gfun[u], u]];
  ss = Sum[gfun[uu[[i]]], {i, 1, n}];
  part1 = Simplify[di[ss]];
  part2 = Simplify[Product[df[uu[[i]]], {i, 1, n}]];
  val = Simplify[part1 * part2];
  val
  ];

mycdf[gfun_, ginv_, n_] := Module[
  {ss},
  ss = Sum[gfun[uu[[i]]], {i, 1, n}];
  Simplify[ginv[ss]]
  ];



myGenExpr[GenFun_, GenInv_, n_, m_, cname_] := Module[
(* 
 n: The maximum dimension for the pdf expression (n = 1 is nuissance)
 m: The maximum dimension for pdfDer (m = 1 is nuissance)
*)						       
  {Cdf, CdfDerWrtArg, CdfDerWrtPar, 
   Pdf, PdfDerWrtArg, PdfDerWrtPar},
  Cdf = Table[mycdf[GenFun, GenInv, i], {i, 1, n}];
  Export[cname <> "Copula.cdf.expr", FortranForm /@ Cdf, "Table"];

  CdfDerWrtArg = Table[Simplify[D[Cdf[[i]], u1]], {i, 1, m}];
  Export[cname <> "Copula.cdfDerWrtArg.expr", FortranForm /@ CdfDerWrtArg, "Table"];
  CdfDerWrtPar = Table[Simplify[D[Cdf[[i]], alpha]], {i, 1, m}];
  Export[cname <> "Copula.cdfDerWrtPar.expr", FortranForm /@ CdfDerWrtPar, "Table"]; 

  Pdf = Table[mypdf[GenFun, GenInv, i], {i, 1, n}];
  Export[cname <> "Copula.pdf.expr", FortranForm /@ Pdf, "Table"];
(* 
 PdfDerWrtArg = Table[Simplify[Simplify[D[Pdf[[i]], u1]] / Pdf[[i]]], {i, 1, m}];
 *)
  PdfDerWrtArg = Table[Simplify[D[Pdf[[i]], u1]], {i, 1, m}];
  Export[cname <> "Copula.pdfDerWrtArg.expr", FortranForm /@ PdfDerWrtArg, "Table"];
(*
  PdfDerWrtPar = Table[Simplify[Simplify[D[Pdf[[i]], alpha]] / Pdf[[i]]], {i, 1, m}];
 *)
  PdfDerWrtPar = Table[Simplify[D[Pdf[[i]], alpha]], {i, 1, m}];
  Export[cname <> "Copula.pdfDerWrtPar.expr", FortranForm /@ PdfDerWrtPar, "Table"];  

  Export[cname <> "Copula.genfunDer.expr", FortranForm /@ myD[GenFun[u], u, 2], "Table"];
  True
  ];


myGenExpr[claytonGenFun, claytonGenInv, 10, 5, "clayton"];
myGenExpr[gumbelGenFun, gumbelGenInv, 10, 5, "gumbel"];
myGenExpr[frankGenFun, frankGenInv, 6, 4, "frank"];
myGenExpr[amhGenFun, amhGenInv, 2, 2, "amh"];
