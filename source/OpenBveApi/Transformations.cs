﻿using OpenBveApi.Math;

namespace OpenBveApi.World
{
	/// <summary>Describes a world transformation consisting of three vectors</summary>
	public struct Transformation
	{
		/// <summary>The X Vector</summary>
		public Vector3 X;
		/// <summary>The Y Vector</summary>
		public Vector3 Y;
		/// <summary>The Z Vector</summary>
		public Vector3 Z;

		/// <summary>Creates a new transformation, based upon yaw pitch and roll values</summary>
		/// <param name="Yaw">The yaw to apply</param>
		/// <param name="Pitch">The pitch to apply</param>
		/// <param name="Roll">The roll to apply</param>
		public Transformation(double Yaw, double Pitch, double Roll)
		{
			if (Yaw == 0.0 & Pitch == 0.0 & Roll == 0.0)
			{
				this.X = new Vector3(1.0, 0.0, 0.0);
				this.Y = new Vector3(0.0, 1.0, 0.0);
				this.Z = new Vector3(0.0, 0.0, 1.0);
			}
			else if (Pitch == 0.0 & Roll == 0.0)
			{
				double cosYaw = System.Math.Cos(Yaw);
				double sinYaw = System.Math.Sin(Yaw);
				this.X = new Vector3(cosYaw, 0.0, -sinYaw);
				this.Y = new Vector3(0.0, 1.0, 0.0);
				this.Z = new Vector3(sinYaw, 0.0, cosYaw);
			}
			else
			{
				X = new Vector3(1.0, 0.0, 0.0);
				Y = new Vector3(0.0, 1.0, 0.0);
				Z = new Vector3(0.0, 0.0, 1.0);
				double cosYaw = System.Math.Cos(Yaw);
				double sinYaw = System.Math.Sin(Yaw);
				double cosPitch = System.Math.Cos(-Pitch);
				double sinPitch = System.Math.Sin(-Pitch);
				double cosRoll = System.Math.Cos(-Roll);
				double sinRoll = System.Math.Sin(-Roll);
				X.Rotate(Y, cosYaw, sinYaw);
				Z.Rotate(Y, cosYaw, sinYaw);
				Y.Rotate(X, cosPitch, sinPitch);
				Z.Rotate(X, cosPitch, sinPitch);
				X.Rotate(Z, cosRoll, sinRoll);
				Y.Rotate(Z, cosRoll, sinRoll);
			}
		}

		/// <summary>Creates a new transformation, based upon an initial transformation, plus secondary yaw pitch and roll values</summary>
		/// <param name="Transformation">The initial transformation</param>
		/// <param name="Yaw">The yaw to apply</param>
		/// <param name="Pitch">The pitch to apply</param>
		/// <param name="Roll">The roll to apply</param>
		public Transformation(Transformation Transformation, double Yaw, double Pitch, double Roll)
		{
			X = new Vector3(Transformation.X.X, Transformation.X.Y, Transformation.X.Z);
			Y = new Vector3(Transformation.Y.X, Transformation.Y.Y, Transformation.Y.Z);
			Z = new Vector3(Transformation.Z.X, Transformation.Z.Y, Transformation.Z.Z);
			double cosYaw = System.Math.Cos(Yaw);
			double sinYaw = System.Math.Sin(Yaw);
			double cosPitch = System.Math.Cos(-Pitch);
			double sinPitch = System.Math.Sin(-Pitch);
			double cosRoll = System.Math.Cos(Roll);
			double sinRoll = System.Math.Sin(Roll);
			X.Rotate(Y, cosYaw, sinYaw);
			Z.Rotate(Y, cosYaw, sinYaw);
			Y.Rotate(X, cosPitch, sinPitch);
			Z.Rotate(X, cosPitch, sinPitch);
			X.Rotate(Z, cosRoll, sinRoll);
			Y.Rotate(Z, cosRoll, sinRoll);
		}

		/// <summary>Creates a new transformation, based upon a base transformation and an auxiliary transformation</summary>
		/// <param name="BaseTransformation">The base transformation</param>
		/// <param name="AuxTransformation">The auxiliary transformation</param>
		public Transformation(Transformation BaseTransformation, Transformation AuxTransformation)
		{
			X = new Vector3(BaseTransformation.X.X, BaseTransformation.X.Y, BaseTransformation.X.Z);
			Y = new Vector3(BaseTransformation.Y.X, BaseTransformation.Y.Y, BaseTransformation.Y.Z);
			Z = new Vector3(BaseTransformation.Z.X, BaseTransformation.Z.Y, BaseTransformation.Z.Z);
			X.Rotate(AuxTransformation.Z, AuxTransformation.Y, AuxTransformation.X);
			Y.Rotate(AuxTransformation.Z, AuxTransformation.Y, AuxTransformation.X);
			Z.Rotate(AuxTransformation.Z, AuxTransformation.Y, AuxTransformation.X);
		}

	}

}
