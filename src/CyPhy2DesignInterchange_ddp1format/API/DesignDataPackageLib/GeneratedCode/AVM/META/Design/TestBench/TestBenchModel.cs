﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace AVM.META.Design.TestBench
{
	using AVM.META.Design;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class TestBenchModel
	{
		public virtual string Name
		{
			get;
			set;
		}

		public virtual List<PostProcessing> PostProcessing
		{
			get;
			set;
		}

		public virtual List<Metric> Metrics
		{
			get;
			set;
		}

		public virtual List<ComponentInstance> TestComponents
		{
			get;
			set;
		}

		public virtual List<PortConnector> PortConnectors
		{
			get;
			set;
		}

		public virtual ValueConnector ValueConnector
		{
			get;
			set;
		}

		public virtual TestBenchValuePort TestBenchValuePort
		{
			get;
			set;
		}

		public virtual ExecutionType ExecutionType
		{
			get;
			set;
		}

		public virtual TestBenchPort TestBenchPort
		{
			get;
			set;
		}

	}
}

