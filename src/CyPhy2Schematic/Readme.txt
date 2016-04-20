The CyPhy2Schematic is a multi-pass traversal interpreter. The algorithm and location of code
is "approximately" described below. 

0. CyPhy2Schemati.cs:
	the boiler plate interpreter code - ignore most of it and jump to WorkInMainTransaction
	This function determines the mode of invocation
		(i.e. EDA mode, or SPICE mode, or SPICE Signal Integrity mode) and passes control
		to CodeGenerator.GenerateCode which is essentially the "Main"

1. Schematic/CodeGenerator.cpp :
	GenerateCode 
		does the common traversals which map the CyPhy model objects into a simple object network suitable for printing code
		as schematic OR as spice
		The traversal is done as visitor pattern with a set of visitors:
			BuildVisitor - creates a simple schematic object network
			ConnectVisitor - creates direct connections in the schematic object network after traversing domain models and connectors
			LayoutVisitor (1&2) - computes the placement of the objects in teh schematic diagram

		After the common traversals - then target specific traversals are done - i.e. Spice or EDA
			EdaVisitor 
			SpiceVisitor

		In the EDA mode, a (pcb) layout file is generated which is input to a constraint solver
			LayoutGenerator


2.