AppendTo[$Echo, "stdout"]

(*
 myRead[pdfExpFile_] := Module[
    {expr, len},
    expr = Import[pdfExpFile];
    expr = StringReplace[expr, "List(" -> "{"];
    len = StringLength[expr];
    expr = StringReplacePart[expr, "}", {len, len}];
    ToExpression[expr]
    ];
 
 myDerPdfWrtArgOverPdf[pdfExpr_, i_] := Module[
   {num},
   num = Simplify[D[pdfExpr[[i]], u1]];
   Simplify[num / pdfExpr[[i]]]
   ];
 *);

myGetDer[cname_, m_] := Module[
  {Cdf, CdfDerWrtArg, CdfDerWrtPar, 
   Pdf, PdfDerWrtArgOverPdf, PdfDerWrtParOverPdf},
  Cdf = ToExpression[ReadList[cname <> "Copula.cdf.expr", "String"]];
  CdfDerWrtArg = Table[Simplify[D[Cdf[[i]], u1]], {i, 1, m}];
  CdfDerWrtPar = Table[Simplify[D[Cdf[[i]], alpha]], {i, 1, m}];
  Export[cname <> "Copula.cdfDerWrtArg.expr", FortranForm /@ CdfDerWrtArg, "Table"];
  Export[cname <> "Copula.cdfDerWrtPar.expr", FortranForm /@ CdfDerWrtPar, "Table"]; 
  Pdf = ToExpression[ReadList[cname <> "Copula.pdf.expr", "String"]];
  PdfDerWrtArgOverPdf = Table[Simplify[Simplify[D[Pdf[[i]], u1]] / Pdf[[i]]], {i, 1, m}];
  Export[cname <> "Copula.pdfDerWrtArgOverPdf.expr", FortranForm /@ PdfDerWrtArgOverPdf, "Table"];
  PdfDerWrtParOverPdf = Table[Simplify[Simplify[D[Pdf[[i]], alpha]] / Pdf[[i]]], {i, 1, m}];
  Export[cname <> "Copula.pdfDerWrtParOverPdf.expr", FortranForm /@ PdfDerWrtParOverPdf, "Table"];  
  True;
  ];

myGetDer["clayton", 4];
myGetDer["gumbel", 4];
myGetDer["frank", 4];



