/*
This code was generated by a tool. DO NOT MODIFY this code manually, unless you really know what you are doing.
 */
using System;
				
namespace IFC4
{
	/// <summary>
	/// http://www.buildingsmart-tech.org/ifc/IFC4/final/html/link/ifcrationalbsplinesurfacewithknotsweightsdata.htm
	/// </summary>
	internal  partial class RationalBSplineSurfaceWithKnotsWeightsData : Object 
	{
		public Realwrapper[] IfcRealwrapper {get;set;}

		public String[] itemType {get;set;}

		public aggregateType[] cType {get;set;}

		public String[] arraySize {get;set;}

		public RationalBSplineSurfaceWithKnotsWeightsData(Realwrapper[] ifcRealwrapper,
				String[] itemType,
				aggregateType[] cType,
				String[] arraySize) : base()
		{
			this.IfcRealwrapper = ifcRealwrapper;
			this.itemType = itemType;
			this.cType = cType;
			this.arraySize = arraySize;
		}
	}
}