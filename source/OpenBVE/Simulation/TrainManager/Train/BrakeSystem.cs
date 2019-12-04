﻿using System;
using OpenBve.BrakeSystems;
using SoundManager;

namespace OpenBve
{
	public static partial class TrainManager
	{
		public partial class Train
		{
			/// <summary>Updates the brake system for the entire train</summary>
			/// <param name="TimeElapsed">The frame time elapsed</param>
			private void UpdateBrakeSystem(double TimeElapsed)
			{
				// individual brake systems
				for (int i = 0; i < Cars.Length; i++)
				{
					UpdateBrakeSystem(i, TimeElapsed);
				}
				// brake pipe pressure distribution dummy (just averages)
				double TotalPressure = 0.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					if (i > 0)
					{
						if (Cars[i - 1].Derailed | Cars[i].Derailed)
						{
							Cars[i].CarBrake.brakePipe.CurrentPressure -= Cars[i].CarBrake.brakePipe.LeakRate * TimeElapsed;
							if (Cars[i].CarBrake.brakePipe.CurrentPressure < 0.0) Cars[i].CarBrake.brakePipe.CurrentPressure = 0.0;
						}
					}
					if (i < Cars.Length - 1)
					{
						if (Cars[i].Derailed | Cars[i + 1].Derailed)
						{
							Cars[i].CarBrake.brakePipe.CurrentPressure -= Cars[i].CarBrake.brakePipe.LeakRate * TimeElapsed;
							if (Cars[i].CarBrake.brakePipe.CurrentPressure < 0.0) Cars[i].CarBrake.brakePipe.CurrentPressure = 0.0;
						}
					}
					TotalPressure += Cars[i].CarBrake.brakePipe.CurrentPressure;
				}
				double AveragePressure = TotalPressure / (double)Cars.Length;
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].CarBrake.brakePipe.CurrentPressure = AveragePressure;
				}
			}

			/// <summary>Updates the brake system for a car within this train</summary>
			/// <remarks>This must remain a property of the train, for easy access to various base properties</remarks>
			/// <param name="CarIndex">The induvidual car</param>
			/// <param name="TimeElapsed">The frame time elapsed</param>
			private void UpdateBrakeSystem(int CarIndex, double TimeElapsed)
			{
				Cars[CarIndex].DecelerationDueToBrake = 0.0;
				// air compressor
				if (Cars[CarIndex].CarBrake.brakeType == BrakeType.Main)
				{
					Cars[CarIndex].CarBrake.airCompressor.Update(TimeElapsed);
				}

				if (CarIndex == DriverCar && Handles.HasLocoBrake)
				{
					switch (Handles.LocoBrakeType)
					{
						case LocoBrakeType.Independant:
							//With an independant Loco brake, we always want to use this handle
							Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out Cars[CarIndex].DecelerationDueToBrake);
							break;
						case LocoBrakeType.Combined:
							if (Handles.LocoBrake is LocoBrakeHandle && Handles.Brake is NotchedHandle)
							{
								//Both handles are of the notched type
								if (Handles.Brake.MaximumNotch == Handles.LocoBrake.MaximumNotch)
								{
									//Identical number of notches, so return the handle with the higher setting
									if (Handles.LocoBrake.Actual >= Handles.Brake.Actual)
									{
										Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out Cars[CarIndex].DecelerationDueToBrake);
									}
									else
									{
										Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out Cars[CarIndex].DecelerationDueToBrake);
									}
								}
								else if (Handles.Brake.MaximumNotch > Handles.LocoBrake.MaximumNotch)
								{
									double nc = ((double) Handles.LocoBrake.Actual / Handles.LocoBrake.MaximumNotch) * Handles.Brake.MaximumNotch;
									if (nc > Handles.Brake.Actual)
									{
										Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out Cars[CarIndex].DecelerationDueToBrake);
									}
									else
									{
										Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out Cars[CarIndex].DecelerationDueToBrake);
									}
								}
								else
								{
									double nc = ((double) Handles.Brake.Actual / Handles.Brake.MaximumNotch) * Handles.LocoBrake.MaximumNotch;
									if (nc > Handles.LocoBrake.Actual)
									{
										Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out Cars[CarIndex].DecelerationDueToBrake);
									}
									else
									{
										Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out Cars[CarIndex].DecelerationDueToBrake);
									}
								}
							}
							else if (Handles.LocoBrake is LocoAirBrakeHandle && Handles.Brake is AirBrakeHandle)
							{
								if (Handles.LocoBrake.Actual < Handles.Brake.Actual)
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out Cars[CarIndex].DecelerationDueToBrake);
								}
								else
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out Cars[CarIndex].DecelerationDueToBrake);
								}
							}
							else
							{
								double p, tp;
								//Calculate the pressure differentials for the two handles
								if (Handles.LocoBrake is LocoAirBrakeHandle)
								{
									//Air brake handle
									p = Cars[CarIndex].CarBrake.brakeCylinder.CurrentPressure / Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure;
									tp = (Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure / Handles.Brake.MaximumNotch) * Handles.Brake.Actual;
								}
								else
								{
									//Notched handle
									p = Cars[CarIndex].CarBrake.brakeCylinder.CurrentPressure / Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure;
									tp = (Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure / Handles.LocoBrake.MaximumNotch) * Handles.LocoBrake.Actual;
								}

								if (p < tp)
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out Cars[CarIndex].DecelerationDueToBrake);
								}
								else
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out Cars[CarIndex].DecelerationDueToBrake);
								}
							}
							break;
						case LocoBrakeType.Blocking:
							if (Handles.LocoBrake.Actual != 0)
							{
								Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out Cars[CarIndex].DecelerationDueToBrake);
							}
							else
							{
								Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out Cars[CarIndex].DecelerationDueToBrake);
							}

							break;
					}

				}
				else
				{
					Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out Cars[CarIndex].DecelerationDueToBrake);
				}

				if(Cars[CarIndex].CarBrake.airSound != null)
				{
					SoundBuffer buffer = Cars[CarIndex].CarBrake.airSound.Buffer;
					if (buffer != null)
					{
						Program.Sounds.PlaySound(buffer, 1.0, 1.0, Cars[CarIndex].CarBrake.airSound.Position, Cars[CarIndex], false);
					}
				}

				// deceleration provided by motor
				if (Cars[CarIndex] is MotorCar)
				{
					MotorCar motorCar = (MotorCar) Cars[CarIndex];
					if (!(Cars[CarIndex].CarBrake is AutomaticAirBrake) && Math.Abs(Cars[CarIndex].CurrentSpeed) >= Cars[CarIndex].CarBrake.brakeControlSpeed & Handles.Reverser.Actual != 0 & !Handles.EmergencyBrake.Actual)
					{
						double f;
						if (Handles.LocoBrake.Actual != 0 && CarIndex == DriverCar)
						{
							f = (double) Handles.LocoBrake.Actual / (double) Handles.Brake.MaximumNotch;
						}
						else
						{
							f = (double) Handles.Brake.Actual / (double) Handles.Brake.MaximumNotch;
						}

						double a = Cars[CarIndex].Specs.MotorDeceleration;
						if (Cars[CarIndex] is MotorCar)
						{
							motorCar.DecelerationDueToMotor = f * a;
						}
					}
					else
					{
						motorCar.DecelerationDueToMotor = 0.0;
					}

					// hold brake
					Cars[CarIndex].Specs.HoldBrake.Update(ref motorCar.DecelerationDueToMotor, Handles.HoldBrake.Actual);
				}
				
				{
					// rub sound
					SoundBuffer buffer = Cars[CarIndex].CarBrake.Rub.Buffer;
					if (buffer != null)
					{
						double spd = Math.Abs(Cars[CarIndex].CurrentSpeed);
						double pitch = 1.0 / (spd + 1.0) + 1.0;
						double gain = Cars[CarIndex].Derailed ? 0.0 : Cars[CarIndex].CarBrake.brakeCylinder.CurrentPressure / Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure;
						if (spd < 1.38888888888889)
						{
							double t = spd * spd;
							gain *= 1.5552 * t - 0.746496 * spd * t;
						}
						else if (spd > 12.5)
						{
							double t = spd - 12.5;
							const double fadefactor = 0.1;
							gain *= 1.0 / (fadefactor * t * t + 1.0);
						}

						if (Program.Sounds.IsPlaying(Cars[CarIndex].CarBrake.Rub.Source))
						{
							if (pitch > 0.01 & gain > 0.001)
							{
								Cars[CarIndex].CarBrake.Rub.Source.Pitch = pitch;
								Cars[CarIndex].CarBrake.Rub.Source.Volume = gain;
							}
							else
							{
								Program.Sounds.StopSound(Cars[CarIndex].CarBrake.Rub);
							}
						}
						else if (pitch > 0.02 & gain > 0.01)
						{
							Cars[CarIndex].CarBrake.Rub.Source = Program.Sounds.PlaySound(buffer, pitch, gain, Cars[CarIndex].CarBrake.Rub.Position, Cars[CarIndex], true);
						}
					}
				}
			}
		}
	}
}
