using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class RedboxProductCarouselUserControl : BaseUserControl, IComponentConnector
{
	private class CarouselPositionList : List<CarouselPosition>
	{
		public int CenterCarouselPositionIndex => NumberOfPositionsLeftOfCenter;

		public CarouselPosition CenterCarouselPosition => base[NumberOfPositionsLeftOfCenter];

		private int NumberOfPositionsLeftOfCenter => base.Count / 2;

		public int NextCenterCarouselPositionIndex(CarouselDirection direction)
		{
			return NumberOfPositionsLeftOfCenter + ((direction == CarouselDirection.Left) ? 1 : (-1));
		}
	}

	private class CarouselPosition
	{
		public double Width { get; set; }

		public double Height { get; set; }

		public double HorizontalCenter { get; set; }

		public double VerticalCenter { get; set; }

		public double Left => HorizontalCenter - Width / 2.0;

		public double Top => VerticalCenter - Height / 2.0;
	}

	private enum CarouselDirection
	{
		Left,
		Right
	}

	public static readonly DependencyProperty IsRotatingProperty = DependencyProperty.Register("IsRotating", typeof(bool), typeof(RedboxProductCarouselUserControl), new FrameworkPropertyMetadata(false));

	private int _carouselPositionCount;

	private int _carouselImagePadding;

	private double _horizontalCenterOfCenterImage;

	private double _verticalCenterOfCenterImage;

	private CarouselDirection _carouselDirection = CarouselDirection.Right;

	private CarouselDirection _nextCarouselDirection = CarouselDirection.Right;

	private Storyboard _carouselRotationStoryboard;

	private Point _carouselImageSmallSize;

	private Point _carouselImageLargeSize;

	private CarouselPositionList _carouselPositions = new CarouselPositionList();

	private List<DisplayProductModel> _carouselProducts;

	private List<DisplayProductModel> _rotatingCarouselProducts;

	private List<DisplayProductCarouselUserControl> _displayProductUserControlList = new List<DisplayProductCarouselUserControl>();

	private bool _forceRotation;

	private DispatcherTimer _timer;

	private bool _timerStarted;

	private bool _isDarkMode;

	private Color _imageBorderColor = Colors.Gray;

	public bool IsRotating
	{
		get
		{
			return (bool)GetValue(IsRotatingProperty);
		}
		set
		{
			SetValue(IsRotatingProperty, value);
		}
	}

	private bool IsCarouselProperlyInitialized
	{
		get
		{
			if (_rotatingCarouselProducts != null && _rotatingCarouselProducts.Count > 0 && _carouselPositions != null && _carouselPositions.Count > 0 && _displayProductUserControlList != null)
			{
				return _displayProductUserControlList.Count == _carouselPositions.Count;
			}
			return false;
		}
	}

	private CarouselTestModel CarouselTestModel => base.DataContext as CarouselTestModel;

	public RedboxProductCarouselUserControl()
	{
		InitializeComponent();
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ConfigureCarousel();
	}

	private void ConfigureCarousel()
	{
		_displayProductUserControlList.Clear();
		CarouselContainer.Children.Clear();
		_rotatingCarouselProducts = new List<DisplayProductModel>();
		ConfigureTimer();
		InitializeCarouselPositions();
		if (CarouselTestModel == null)
		{
			return;
		}
		CarouselTestModel.OnIsAnimationOnChanged += _carouselModel_OnIsAnimationOnChanged;
		_carouselProducts = CarouselTestModel.CarouselDisplayProductModels;
		_rotatingCarouselProducts.AddRange(_carouselProducts);
		_nextCarouselDirection = CarouselDirection.Right;
		foreach (DisplayProductModel rotatingCarouselProduct in _rotatingCarouselProducts)
		{
			int num = _rotatingCarouselProducts.IndexOf(rotatingCarouselProduct);
			if (num < _carouselPositions.Count)
			{
				CarouselPosition carouselPosition = _carouselPositions[num];
				DisplayProductCarouselUserControl displayProductCarouselUserControl = new DisplayProductCarouselUserControl
				{
					DataContext = rotatingCarouselProduct,
					Margin = new Thickness(carouselPosition.Left, carouselPosition.Top, 0.0, 0.0),
					Width = carouselPosition.Width,
					Height = carouselPosition.Height,
					BorderThickness = new Thickness(0.0),
					ShaderOpacity = 0.5
				};
				if (num < _carouselPositions.CenterCarouselPositionIndex)
				{
					displayProductCarouselUserControl.Tag = CarouselDirection.Right;
					Panel.SetZIndex(displayProductCarouselUserControl, num);
				}
				else if (num > _carouselPositions.CenterCarouselPositionIndex)
				{
					displayProductCarouselUserControl.Tag = CarouselDirection.Left;
					Panel.SetZIndex(displayProductCarouselUserControl, _carouselPositions.Count - num);
				}
				else
				{
					Panel.SetZIndex(displayProductCarouselUserControl, _carouselPositions.Count);
					displayProductCarouselUserControl.ShaderOpacity = 0.0;
					displayProductCarouselUserControl.ShadowDepth = 10.0;
					displayProductCarouselUserControl.ShadowOpacity = 0.75;
					displayProductCarouselUserControl.ImageBorderThickness = new Thickness(3.0);
					displayProductCarouselUserControl.ImageBorderColor = _imageBorderColor;
				}
				_displayProductUserControlList.Add(displayProductCarouselUserControl);
				CarouselContainer.Children.Add(displayProductCarouselUserControl);
			}
		}
		CommandManager.InvalidateRequerySuggested();
		StartTimer();
	}

	private void _carouselModel_OnIsAnimationOnChanged(CarouselTestModel carouselModel)
	{
		if (carouselModel != null && carouselModel.IsAnimationOn)
		{
			StartTimer();
			return;
		}
		_carouselRotationStoryboard?.Stop();
		StopTimer();
		if (IsCarouselProperlyInitialized)
		{
			DisplayProductCarouselUserControl displayProductCarouselUserControl = _displayProductUserControlList[_carouselPositions.CenterCarouselPositionIndex];
			if (displayProductCarouselUserControl != null && displayProductCarouselUserControl.DataContext is DisplayProductModel displayProductModel)
			{
				displayProductModel.AddButtonVisibility = Visibility.Collapsed;
			}
		}
	}

	private void AnimateCarouselRotation()
	{
		_carouselDirection = _nextCarouselDirection;
		TimeSpan timeSpan = TimeSpan.FromSeconds(0.0);
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(_forceRotation ? 0 : 0);
		TimeSpan timeSpan3 = timeSpan + timeSpan2;
		TimeSpan timeSpan4 = TimeSpan.FromSeconds(0.0);
		TimeSpan timeSpan5 = timeSpan3 + timeSpan4;
		TimeSpan timeSpan6 = TimeSpan.FromSeconds(0.4);
		TimeSpan timeSpan7 = timeSpan5 + timeSpan6;
		TimeSpan timeSpan8 = timeSpan5 + TimeSpan.FromMilliseconds(timeSpan6.TotalMilliseconds / 2.0);
		TimeSpan timeSpan9 = timeSpan7;
		_forceRotation = false;
		_carouselRotationStoryboard?.Stop();
		_carouselRotationStoryboard = new Storyboard();
		_carouselRotationStoryboard.Duration = timeSpan9;
		_carouselRotationStoryboard.Completed += carousel_animation_storyBoard_Completed;
		if (IsCarouselProperlyInitialized)
		{
			for (int i = 0; i < _carouselPositionCount; i++)
			{
				if ((_carouselDirection == CarouselDirection.Left && i > 0) || (_carouselDirection == CarouselDirection.Right && i < _carouselPositionCount - 1))
				{
					CarouselPosition carouselPosition = _carouselPositions[i];
					int index = i + ((_carouselDirection != CarouselDirection.Left) ? 1 : (-1));
					CarouselPosition carouselPosition2 = _carouselPositions[index];
					DisplayProductCarouselUserControl displayProductCarouselUserControl = _displayProductUserControlList[i];
					TranslateTransform value = new TranslateTransform();
					ScaleTransform value2 = new ScaleTransform();
					displayProductCarouselUserControl.RenderTransform = new TransformGroup
					{
						Children = 
						{
							(Transform)value2,
							(Transform)value
						}
					};
					bool flag = carouselPosition2 == _carouselPositions.CenterCarouselPosition;
					bool flag2 = carouselPosition == _carouselPositions.CenterCarouselPosition;
					double width = carouselPosition.Width;
					double value3 = width / carouselPosition.Width;
					double value4 = carouselPosition2.Width / width;
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames
					{
						KeyFrames = { (DoubleKeyFrame)new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = value3
						} }
					};
					if (flag2)
					{
						doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan),
							Value = 1.0
						});
						doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan3),
							Value = 1.0
						});
					}
					doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan5),
						Value = value3
					});
					doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = value4
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames);
					double height = carouselPosition.Height;
					double value5 = height / carouselPosition.Height;
					double value6 = carouselPosition2.Height / height;
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames
					{
						KeyFrames = { (DoubleKeyFrame)new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = value5
						} }
					};
					if (flag2)
					{
						doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan),
							Value = 1.0
						});
						doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan3),
							Value = 1.0
						});
					}
					doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan5),
						Value = value5
					});
					doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = value6
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames2);
					double value7 = carouselPosition2.HorizontalCenter - carouselPosition.HorizontalCenter;
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames
					{
						KeyFrames = 
						{
							(DoubleKeyFrame)new EasingDoubleKeyFrame
							{
								KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
								Value = 0.0
							},
							(DoubleKeyFrame)new EasingDoubleKeyFrame
							{
								KeyTime = KeyTime.FromTimeSpan(timeSpan5),
								Value = 0.0
							},
							(DoubleKeyFrame)new EasingDoubleKeyFrame
							{
								KeyTime = KeyTime.FromTimeSpan(timeSpan7),
								Value = value7,
								EasingFunction = new CubicEase
								{
									EasingMode = EasingMode.EaseIn
								}
							}
						}
					};
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames3);
					double value8 = carouselPosition2.VerticalCenter - carouselPosition.VerticalCenter;
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames4 = new DoubleAnimationUsingKeyFrames();
					doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = 0.0
					});
					doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan5),
						Value = 0.0
					});
					doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = value8
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames4);
					if (flag2)
					{
						DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames5 = new DoubleAnimationUsingKeyFrames();
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = 0.0
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 0.0
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = 0.5
						});
						Storyboard.SetTarget(doubleAnimationUsingKeyFrames5, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames5, new PropertyPath("ShaderOpacity"));
						_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames5);
						DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames6 = new DoubleAnimationUsingKeyFrames();
						doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = 10.0
						});
						doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 10.0
						});
						doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = 0.0
						});
						Storyboard.SetTarget(doubleAnimationUsingKeyFrames6, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames6, new PropertyPath("ShadowDepth"));
						_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames6);
						DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames7 = new DoubleAnimationUsingKeyFrames();
						doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = 0.75
						});
						doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 0.75
						});
						doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = 0.0
						});
						Storyboard.SetTarget(doubleAnimationUsingKeyFrames7, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames7, new PropertyPath("ShadowOpacity"));
						_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames7);
						ColorAnimationUsingKeyFrames colorAnimationUsingKeyFrames = new ColorAnimationUsingKeyFrames();
						colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = _imageBorderColor
						});
						colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = _imageBorderColor
						});
						colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = _imageBorderColor
						});
						Storyboard.SetTarget(colorAnimationUsingKeyFrames, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(colorAnimationUsingKeyFrames, new PropertyPath("ImageBorderColor"));
						_carouselRotationStoryboard.Children.Add(colorAnimationUsingKeyFrames);
						ThicknessAnimationUsingKeyFrames thicknessAnimationUsingKeyFrames = new ThicknessAnimationUsingKeyFrames();
						thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = new Thickness(3.0)
						});
						thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = new Thickness(3.0)
						});
						thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = new Thickness(0.0)
						});
						Storyboard.SetTarget(thicknessAnimationUsingKeyFrames, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(thicknessAnimationUsingKeyFrames, new PropertyPath("ImageBorderThickness"));
						_carouselRotationStoryboard.Children.Add(thicknessAnimationUsingKeyFrames);
					}
					else if (flag)
					{
						DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames8 = new DoubleAnimationUsingKeyFrames();
						doubleAnimationUsingKeyFrames8.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = 0.5
						});
						doubleAnimationUsingKeyFrames8.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 0.5
						});
						doubleAnimationUsingKeyFrames8.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = 0.0
						});
						Storyboard.SetTarget(doubleAnimationUsingKeyFrames8, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames8, new PropertyPath("ShaderOpacity"));
						_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames8);
						DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames9 = new DoubleAnimationUsingKeyFrames();
						doubleAnimationUsingKeyFrames9.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = 0.0
						});
						doubleAnimationUsingKeyFrames9.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 0.0
						});
						doubleAnimationUsingKeyFrames9.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = 10.0
						});
						Storyboard.SetTarget(doubleAnimationUsingKeyFrames9, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames9, new PropertyPath("ShadowDepth"));
						_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames9);
						DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames10 = new DoubleAnimationUsingKeyFrames();
						doubleAnimationUsingKeyFrames10.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = 0.0
						});
						doubleAnimationUsingKeyFrames10.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 0.0
						});
						doubleAnimationUsingKeyFrames10.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = 0.75
						});
						Storyboard.SetTarget(doubleAnimationUsingKeyFrames10, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames10, new PropertyPath("ShadowOpacity"));
						_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames10);
						ColorAnimationUsingKeyFrames colorAnimationUsingKeyFrames2 = new ColorAnimationUsingKeyFrames();
						colorAnimationUsingKeyFrames2.KeyFrames.Add(new EasingColorKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = _imageBorderColor
						});
						colorAnimationUsingKeyFrames2.KeyFrames.Add(new EasingColorKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = _imageBorderColor
						});
						colorAnimationUsingKeyFrames2.KeyFrames.Add(new EasingColorKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = _imageBorderColor
						});
						Storyboard.SetTarget(colorAnimationUsingKeyFrames2, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(colorAnimationUsingKeyFrames2, new PropertyPath("ImageBorderColor"));
						_carouselRotationStoryboard.Children.Add(colorAnimationUsingKeyFrames2);
						ThicknessAnimationUsingKeyFrames thicknessAnimationUsingKeyFrames2 = new ThicknessAnimationUsingKeyFrames();
						thicknessAnimationUsingKeyFrames2.KeyFrames.Add(new EasingThicknessKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = new Thickness(0.0)
						});
						thicknessAnimationUsingKeyFrames2.KeyFrames.Add(new EasingThicknessKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = new Thickness(0.0)
						});
						thicknessAnimationUsingKeyFrames2.KeyFrames.Add(new EasingThicknessKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = new Thickness(3.0)
						});
						Storyboard.SetTarget(thicknessAnimationUsingKeyFrames2, displayProductCarouselUserControl);
						Storyboard.SetTargetProperty(thicknessAnimationUsingKeyFrames2, new PropertyPath("ImageBorderThickness"));
						_carouselRotationStoryboard.Children.Add(thicknessAnimationUsingKeyFrames2);
					}
					Int32AnimationUsingKeyFrames int32AnimationUsingKeyFrames = new Int32AnimationUsingKeyFrames();
					int zIndex = Panel.GetZIndex(displayProductCarouselUserControl);
					int num = ((_carouselDirection == CarouselDirection.Left) ? (i - 1) : (i + 1));
					int value9 = ((num < _carouselPositions.CenterCarouselPositionIndex) ? num : ((num <= _carouselPositions.CenterCarouselPositionIndex) ? _carouselPositions.Count : (_carouselPositions.Count - num)));
					int32AnimationUsingKeyFrames.KeyFrames.Add(new DiscreteInt32KeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan8),
						Value = value9
					});
					int32AnimationUsingKeyFrames.KeyFrames.Add(new DiscreteInt32KeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = zIndex
					});
					Storyboard.SetTarget(int32AnimationUsingKeyFrames, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(int32AnimationUsingKeyFrames, new PropertyPath("(Panel.ZIndex)"));
					_carouselRotationStoryboard.Children.Add(int32AnimationUsingKeyFrames);
					BooleanAnimationUsingKeyFrames booleanAnimationUsingKeyFrames = new BooleanAnimationUsingKeyFrames();
					booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = false
					});
					booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan3),
						Value = true
					});
					booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = true
					});
					Storyboard.SetTarget(booleanAnimationUsingKeyFrames, this);
					Storyboard.SetTargetProperty(booleanAnimationUsingKeyFrames, new PropertyPath("(local:RedboxProductCarouselUserControl.IsRotating)"));
					_carouselRotationStoryboard.Children.Add(booleanAnimationUsingKeyFrames);
				}
			}
		}
		_carouselRotationStoryboard.Begin();
	}

	private void carousel_animation_storyBoard_Completed(object sender, EventArgs e)
	{
		CarouselTestModel carouselTestModel = CarouselTestModel;
		if (carouselTestModel != null && carouselTestModel.IsAnimationOn)
		{
			_carouselRotationStoryboard?.Stop();
			_carouselRotationStoryboard = null;
			RotateCarouselPositions();
			IsRotating = false;
			StartTimer();
		}
	}

	private void StartTimer()
	{
		CarouselTestModel carouselTestModel = CarouselTestModel;
		if (carouselTestModel != null && carouselTestModel.IsAnimationOn && !_timerStarted)
		{
			_timerStarted = true;
			_timer.Start();
		}
	}

	private void StopTimer()
	{
		_timer.Stop();
		_timerStarted = false;
	}

	private void ConfigureTimer()
	{
		_timer = new DispatcherTimer(DispatcherPriority.Send);
		_timer.Tick += _timer_Tick;
		_timer.Interval = TimeSpan.FromSeconds(CarouselTestModel?.RotationDelay ?? 2.0);
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		StopTimer();
		AnimateCarouselRotation();
	}

	private void RotateCarouselPositions()
	{
		if (!IsCarouselProperlyInitialized)
		{
			return;
		}
		List<DisplayProductModel> list = new List<DisplayProductModel>();
		int count = ((_carouselDirection == CarouselDirection.Left) ? 1 : (_rotatingCarouselProducts.Count - 1));
		list.AddRange(_rotatingCarouselProducts.Skip(count));
		list.AddRange(_rotatingCarouselProducts.Take(count));
		_rotatingCarouselProducts = list;
		foreach (DisplayProductCarouselUserControl displayProductUserControl in _displayProductUserControlList)
		{
			int num = _displayProductUserControlList.IndexOf(displayProductUserControl);
			displayProductUserControl.RenderTransform = null;
			displayProductUserControl.DataContext = _rotatingCarouselProducts[num];
			displayProductUserControl.ShaderOpacity = 0.5;
			if (num < _carouselPositions.CenterCarouselPositionIndex)
			{
				Panel.SetZIndex(displayProductUserControl, num);
				continue;
			}
			if (num > _carouselPositions.CenterCarouselPositionIndex)
			{
				Panel.SetZIndex(displayProductUserControl, _carouselPositions.Count - num);
				continue;
			}
			Panel.SetZIndex(displayProductUserControl, _carouselPositions.Count);
			displayProductUserControl.ShaderOpacity = 0.0;
			displayProductUserControl.ShadowDepth = 10.0;
			displayProductUserControl.ShadowOpacity = 0.75;
			displayProductUserControl.ImageBorderThickness = new Thickness(3.0);
			displayProductUserControl.ImageBorderColor = _imageBorderColor;
		}
		CarouselTestModel?.ProcessCarouselRotation(_rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex], null);
	}

	private void InitializeCarouselPositions()
	{
		InitializeCarouselImageSizes();
		_carouselPositions.Clear();
		int num = _carouselPositionCount / 2;
		double num2 = _horizontalCenterOfCenterImage - (_carouselImageLargeSize.X / 2.0 + (double)num * ((double)_carouselImagePadding + _carouselImageSmallSize.X) - _carouselImageSmallSize.X / 2.0);
		_ = _verticalCenterOfCenterImage;
		for (int i = 0; i < _carouselPositionCount; i++)
		{
			bool flag = i == _carouselPositionCount / 2;
			CarouselPosition carouselPosition = new CarouselPosition();
			carouselPosition.Width = (flag ? _carouselImageLargeSize.X : _carouselImageSmallSize.X);
			carouselPosition.Height = (flag ? _carouselImageLargeSize.Y : _carouselImageSmallSize.Y);
			carouselPosition.VerticalCenter = _verticalCenterOfCenterImage;
			carouselPosition.HorizontalCenter = num2 + _carouselPositions.Sum((CarouselPosition x) => x.Width) + (double)(_carouselPositions.Count() * _carouselImagePadding) - _carouselImageSmallSize.X / 2.0 + carouselPosition.Width / 2.0;
			_carouselPositions.Add(carouselPosition);
		}
	}

	private void InitializeCarouselImageSizes()
	{
		_isDarkMode = CarouselTestModel?.IsDarkMode ?? false;
		_carouselImageSmallSize = new Point(244.0, 366.0);
		_carouselImageLargeSize = new Point(284.0, 426.0);
		_carouselPositionCount = 9;
		_carouselImagePadding = -58;
		_verticalCenterOfCenterImage = 181.0;
		_horizontalCenterOfCenterImage = 512.0;
		_imageBorderColor = (_isDarkMode ? Colors.White : Color.FromRgb(155, 153, 156));
	}

	private void DisplayProductSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (!IsRotating && IsCarouselProperlyInitialized)
		{
			DisplayProductCarouselUserControl displayProductUserControl = GetDisplayProductUserControl(e.OriginalSource);
			if (displayProductUserControl != null)
			{
				if (displayProductUserControl == _displayProductUserControlList[_carouselPositions.CenterCarouselPositionIndex])
				{
					DisplayProductModel displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
					CarouselTestModel?.ProcessDisplayProductModelSelected(displayProductModel, e.Parameter);
				}
				else if (displayProductUserControl.Tag != null && CarouselTestModel != null)
				{
					_nextCarouselDirection = (CarouselDirection)displayProductUserControl.Tag;
					_forceRotation = true;
					CarouselTestModel.IsAnimationOn = true;
					StopTimer();
					AnimateCarouselRotation();
				}
			}
		}
		HandleWPFHit();
	}

	private DisplayProductCarouselUserControl GetDisplayProductUserControl(object source)
	{
		FrameworkElement frameworkElement = source as FrameworkElement;
		while (frameworkElement != null && !(frameworkElement is DisplayProductCarouselUserControl))
		{
			frameworkElement = frameworkElement.Parent as FrameworkElement;
		}
		return frameworkElement as DisplayProductCarouselUserControl;
	}
}
