using System;
using System.Collections.Generic;
using UnityEngine;

public class RopeEndsCenter : EndsCenter<RopeEnd>
{
	/// <summary>
	/// The force applied on the rope when shooting it.
	/// </summary>
	public Single ShootingForce;

	/// <summary>
	/// The rate between the initial length of the rope (or its length when LinksCount is changed),
	/// to the desired length.
	/// </summary>
	[Range(0.01F, 1)]
	public Single DesiredLengthRate;

	/// <summary>
	/// The length of each rope links.
	/// This is used to break the rope into links based on its length.
	/// </summary>
	public Single RopeLinkLength;

	/// <summary>
	/// The total mass of the rope (excluding its head).
	/// </summary>
	public Single RopeTotalMass;

	/// <summary>
	/// The force of the springs of which the rope consists.
	/// </summary>
	public Single RopeSpringForce;

	/// <summary>
	/// The damping of the springs if which the rope consists.
	/// </summary>
	public Single RopeDampingRate;


	protected override void UpdateEnds(IEnumerable<RopeEnd> UpdateEnds)
	{
		foreach (var End in UpdateEnds)
		{
			End.ShootingForce = ShootingForce;
			End.DesiredLengthRate = DesiredLengthRate;
			End.RopeLinkLength = RopeLinkLength;
			End.RopeTotalMass = RopeTotalMass;
			End.RopeSpringForce = RopeSpringForce;
			End.RopeDampingRate = RopeDampingRate;
		}
	}
}