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
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class RedboxClassicProductCarouselUserControl : UserControl, IComponentConnector
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

	public static readonly DependencyProperty IsRotatingProperty = DependencyProperty.Register("IsRotating", typeof(bool), typeof(RedboxClassicProductCarouselUserControl), new FrameworkPropertyMetadata(false));

	private double originalHorizontalCenterOfCenterImage = 562.0;

	private int _carouselPositionCount = 7;

	private int _carouselImagePadding = 30;

	private double _horizontalCenterOfCenterImage;

	private double _verticalCenterOfCenterImage = 384.0;

	private CarouselDirection _carouselDirection;

	private CarouselDirection _nextCarouselDirection;

	private Storyboard _carouselRotationStoryboard;

	private Point _carouselImageSmallSize;

	private Point _carouselImageLargeSize;

	private CarouselPositionList _carouselPositions = new CarouselPositionList();

	private CarouselModel _carouselModel;

	private List<DisplayProductModel> _carouselProducts;

	private List<DisplayProductModel> _rotatingCarouselProducts;

	private List<DisplayProductUserControl> _displayProductUserControlList = new List<DisplayProductUserControl>();

	private bool _forceRotation;

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

	public RedboxClassicProductCarouselUserControl()
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
		InitializeCarouselPositions();
		_carouselModel = base.DataContext as CarouselModel;
		if (_carouselModel == null)
		{
			return;
		}
		_carouselModel.OnIsAnimationOnChanged += _carouselModel_OnIsAnimationOnChanged;
		_carouselProducts = _carouselModel.CarouselDisplayProductModels;
		_rotatingCarouselProducts.AddRange(_carouselProducts);
		_nextCarouselDirection = CarouselDirection.Left;
		foreach (DisplayProductModel rotatingCarouselProduct in _rotatingCarouselProducts)
		{
			int index = _rotatingCarouselProducts.IndexOf(rotatingCarouselProduct);
			if (index < _carouselPositions.Count)
			{
				CarouselPosition carouselPosition = _carouselPositions[index];
				DisplayProductUserControl displayProductUserControl = new DisplayProductUserControl
				{
					DataContext = rotatingCarouselProduct,
					Margin = new Thickness(carouselPosition.Left, carouselPosition.Top, 0.0, 0.0),
					Width = carouselPosition.Width,
					Height = carouselPosition.Height
				};
				if (index < _carouselPositions.CenterCarouselPositionIndex)
				{
					displayProductUserControl.Tag = CarouselDirection.Right;
				}
				else if (index > _carouselPositions.CenterCarouselPositionIndex)
				{
					displayProductUserControl.Tag = CarouselDirection.Left;
				}
				_displayProductUserControlList.Add(displayProductUserControl);
				CarouselContainer.Children.Add(displayProductUserControl);
				displayProductUserControl.ApplyStyle();
			}
		}
		InitializeTitleRollupOverlay();
		CommandManager.InvalidateRequerySuggested();
		if (_carouselModel.IsAnimationOn)
		{
			AnimateCarouselRotation();
		}
	}

	private void SetDisplayProductModelsAddButtonVisibility()
	{
		if (!IsCarouselProperlyInitialized)
		{
			return;
		}
		foreach (DisplayProductModel rotatingCarouselProduct in _rotatingCarouselProducts)
		{
			if (rotatingCarouselProduct == _rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex])
			{
				CarouselModel carouselModel = _carouselModel;
				if (carouselModel != null && carouselModel.ShowAddButton)
				{
					rotatingCarouselProduct.AddButtonVisibility = Visibility.Visible;
					continue;
				}
			}
			rotatingCarouselProduct.AddButtonVisibility = Visibility.Collapsed;
		}
	}

	private void _carouselModel_OnIsAnimationOnChanged(CarouselModel carouselModel)
	{
		if (carouselModel != null && carouselModel.IsAnimationOn)
		{
			AnimateCarouselRotation();
			return;
		}
		_carouselRotationStoryboard.Stop();
		if (IsCarouselProperlyInitialized)
		{
			DisplayProductUserControl displayProductUserControl = _displayProductUserControlList[_carouselPositions.CenterCarouselPositionIndex];
			if (displayProductUserControl != null && displayProductUserControl.DataContext is DisplayProductModel displayProductModel)
			{
				displayProductModel.AddButtonVisibility = Visibility.Collapsed;
			}
		}
	}

	private void InitializeTitleRollupOverlay()
	{
		TitleRollupOverlay.ApplyStyle();
		if (IsCarouselProperlyInitialized)
		{
			TitleRollupOverlay.Width = _carouselPositions.CenterCarouselPosition.Width;
			TitleRollupOverlay.Height = _carouselPositions.CenterCarouselPosition.Height;
			TitleRollupOverlay.Margin = new Thickness(_carouselPositions.CenterCarouselPosition.Left, _carouselPositions.CenterCarouselPosition.Top, 0.0, 0.0);
			if (_carouselModel != null)
			{
				_carouselModel.TitleRollupOverlayModel.BrowseItemModel = _rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex];
			}
		}
	}

	private void AnimateCarouselRotation()
	{
		SetDisplayProductModelsAddButtonVisibility();
		_carouselDirection = _nextCarouselDirection;
		TimeSpan timeSpan = TimeSpan.FromSeconds(0.0);
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(_forceRotation ? 0.0 : 1.7);
		TimeSpan timeSpan3 = timeSpan + timeSpan2;
		TimeSpan timeSpan4 = TimeSpan.FromSeconds(0.0);
		TimeSpan timeSpan5 = timeSpan3 + timeSpan4;
		TimeSpan timeSpan6 = TimeSpan.FromSeconds(0.8);
		TimeSpan timeSpan7 = timeSpan5 + timeSpan6;
		TimeSpan timeSpan8 = timeSpan7;
		_forceRotation = false;
		_carouselRotationStoryboard?.Stop();
		_carouselRotationStoryboard = new Storyboard();
		_carouselRotationStoryboard.Duration = timeSpan8;
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
					DisplayProductUserControl displayProductUserControl = _displayProductUserControlList[i];
					DisplayProductModel displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
					TranslateTransform translateTransform = new TranslateTransform();
					ScaleTransform scaleTransform = new ScaleTransform();
					displayProductUserControl.RenderTransform = new TransformGroup
					{
						Children = 
						{
							(Transform)scaleTransform,
							(Transform)translateTransform
						}
					};
					Thickness startBorderThickness = new Thickness(3.0);
					Thickness centerBorderThickness = new Thickness(3.0);
					double borderThicknessDelta = centerBorderThickness.Bottom - startBorderThickness.Bottom;
					bool movingToCenter = carouselPosition2 == _carouselPositions.CenterCarouselPosition;
					bool movingFromCenter = carouselPosition == _carouselPositions.CenterCarouselPosition;
					double fromWidth = carouselPosition.Width - (movingFromCenter ? borderThicknessDelta : 0.0);
					double fromScaleX = fromWidth / carouselPosition.Width;
					double toScaleX = (carouselPosition2.Width - (movingToCenter ? borderThicknessDelta : 0.0)) / fromWidth;
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames
					{
						KeyFrames = { (DoubleKeyFrame)new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = fromScaleX
						} }
					};
					if (movingFromCenter)
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
						Value = fromScaleX
					});
					doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = toScaleX
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames, displayProductUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleX)"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames);
					double fromHeight = carouselPosition.Height - (movingFromCenter ? borderThicknessDelta : 0.0);
					double fromScaleY = fromHeight / carouselPosition.Height;
					double toScaleY = (carouselPosition2.Height - (movingToCenter ? borderThicknessDelta : 0.0)) / fromHeight;
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames
					{
						KeyFrames = { (DoubleKeyFrame)new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = fromScaleY
						} }
					};
					if (movingFromCenter)
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
						Value = fromScaleY
					});
					doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = toScaleY
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, displayProductUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames2);
					double deltaX = carouselPosition2.HorizontalCenter - carouselPosition.HorizontalCenter;
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
								Value = deltaX
							}
						}
					};
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, displayProductUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames3);
					double deltaY = carouselPosition2.VerticalCenter - carouselPosition.VerticalCenter;
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
						Value = deltaY
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, displayProductUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames4);
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames5 = new DoubleAnimationUsingKeyFrames();
					doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = 0.0
					});
					if (movingFromCenter)
					{
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan),
							Value = 1.0
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan3),
							Value = 1.0
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 0.0
						});
					}
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames5, displayProductModel);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames5, new PropertyPath(DisplayProductModel.AddButtonOpacityProperty));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames5);
					ColorAnimationUsingKeyFrames colorAnimationUsingKeyFrames = new ColorAnimationUsingKeyFrames();
					colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = Color.FromRgb(155, 153, 156)
					});
					if (movingFromCenter)
					{
						colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan),
							Value = Color.FromRgb(155, 153, 156)
						});
						colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan3),
							Value = Color.FromRgb(155, 153, 156)
						});
						colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = Color.FromRgb(155, 153, 156)
						});
					}
					Storyboard.SetTarget(colorAnimationUsingKeyFrames, displayProductModel);
					Storyboard.SetTargetProperty(colorAnimationUsingKeyFrames, new PropertyPath(DisplayProductModel.ImageBorderColorProperty));
					_carouselRotationStoryboard.Children.Add(colorAnimationUsingKeyFrames);
					ThicknessAnimationUsingKeyFrames thicknessAnimationUsingKeyFrames = new ThicknessAnimationUsingKeyFrames();
					thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = startBorderThickness
					});
					if (movingFromCenter)
					{
						thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan),
							Value = centerBorderThickness
						});
						thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan3),
							Value = centerBorderThickness
						});
						thicknessAnimationUsingKeyFrames.KeyFrames.Add(new EasingThicknessKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = startBorderThickness
						});
					}
					Storyboard.SetTarget(thicknessAnimationUsingKeyFrames, displayProductModel);
					Storyboard.SetTargetProperty(thicknessAnimationUsingKeyFrames, new PropertyPath(DisplayProductModel.ImageBorderThicknessProperty));
					_carouselRotationStoryboard.Children.Add(thicknessAnimationUsingKeyFrames);
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames6 = new DoubleAnimationUsingKeyFrames();
					doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = 0.0
					});
					if (movingFromCenter)
					{
						doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan),
							Value = 2.0
						});
						doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan3),
							Value = 2.0
						});
						doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 0.0
						});
					}
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames6, displayProductModel);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames6, new PropertyPath(DisplayProductModel.ImageBorderCornerRadiusProperty));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames6);
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
					Storyboard.SetTargetProperty(booleanAnimationUsingKeyFrames, new PropertyPath("(local:RedboxClassicProductCarouselUserControl.IsRotating)"));
					_carouselRotationStoryboard.Children.Add(booleanAnimationUsingKeyFrames);
				}
			}
		}
		_carouselRotationStoryboard.Begin();
	}

	private void carousel_animation_storyBoard_Completed(object sender, EventArgs e)
	{
		if (_carouselModel.IsAnimationOn)
		{
			RotateCarouselPositions();
			AnimateCarouselRotation();
		}
	}

	private void RotateCarouselPositions()
	{
		if (!IsCarouselProperlyInitialized)
		{
			return;
		}
		List<DisplayProductModel> displayProductModels = new List<DisplayProductModel>();
		int count = ((_carouselDirection == CarouselDirection.Left) ? 1 : (_rotatingCarouselProducts.Count - 1));
		displayProductModels.AddRange(_rotatingCarouselProducts.Skip(count));
		displayProductModels.AddRange(_rotatingCarouselProducts.Take(count));
		_rotatingCarouselProducts = displayProductModels;
		foreach (DisplayProductUserControl displayProductUserControl in _displayProductUserControlList)
		{
			int index = _displayProductUserControlList.IndexOf(displayProductUserControl);
			displayProductUserControl.RenderTransform = null;
			displayProductUserControl.DataContext = _rotatingCarouselProducts[index];
		}
		_carouselModel.TitleRollupOverlayModel.BrowseItemModel = _rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex];
		_carouselModel.ProcessCarouselRotation(_rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex], null);
	}

	private void InitializeCarouselPositions()
	{
		_carouselPositions.Clear();
		int sideImageCount = _carouselPositionCount / 2;
		InitializeCarouselImageSizes();
		_horizontalCenterOfCenterImage = originalHorizontalCenterOfCenterImage;
		double firstImageLeft = _horizontalCenterOfCenterImage - (_carouselImageLargeSize.X / 2.0 + (double)sideImageCount * ((double)_carouselImagePadding + _carouselImageSmallSize.X) - _carouselImageSmallSize.X / 2.0);
		_ = _verticalCenterOfCenterImage;
		for (int i = 0; i < _carouselPositionCount; i++)
		{
			bool isCenterPosition = i == _carouselPositionCount / 2;
			CarouselPosition carouselPosition = new CarouselPosition();
			carouselPosition.Width = (isCenterPosition ? _carouselImageLargeSize.X : _carouselImageSmallSize.X);
			carouselPosition.Height = (isCenterPosition ? _carouselImageLargeSize.Y : _carouselImageSmallSize.Y);
			carouselPosition.VerticalCenter = _verticalCenterOfCenterImage;
			carouselPosition.HorizontalCenter = firstImageLeft + _carouselPositions.Sum((CarouselPosition x) => x.Width) + (double)(_carouselPositions.Count() * _carouselImagePadding) - _carouselImageSmallSize.X / 2.0 + carouselPosition.Width / 2.0;
			_carouselPositions.Add(carouselPosition);
		}
	}

	private void InitializeCarouselImageSizes()
	{
		_carouselImageSmallSize = new Point(216.0, 324.0);
		_carouselImageLargeSize = new Point(280.0, 420.0);
	}

	private void DisplayProductSelectedCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void DisplayProductSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (IsRotating || !IsCarouselProperlyInitialized)
		{
			return;
		}
		DisplayProductUserControl displayProductUserControl = GetDisplayProductUserControl(e.OriginalSource);
		if (displayProductUserControl == null)
		{
			return;
		}
		if (displayProductUserControl == _displayProductUserControlList[_carouselPositions.CenterCarouselPositionIndex])
		{
			DisplayProductModel displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
			_carouselModel.ProcessDisplayProductModelSelected(displayProductModel, e.Parameter);
		}
		else if (displayProductUserControl.Tag != null)
		{
			if (_carouselModel != null && _carouselModel.TitleRollupOverlayModel != null)
			{
				_carouselModel.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
			}
			_nextCarouselDirection = (CarouselDirection)displayProductUserControl.Tag;
			_forceRotation = true;
			_carouselModel.IsAnimationOn = true;
			AnimateCarouselRotation();
		}
	}

	private void DisplayProductAddCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (!IsRotating && IsCarouselProperlyInitialized)
		{
			DisplayProductUserControl displayProductUserControl = GetDisplayProductUserControl(e.OriginalSource);
			if (displayProductUserControl != null)
			{
				DisplayProductModel displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
				_carouselModel.ProcessAddDisplayProductModel(displayProductModel, e.Parameter);
			}
		}
	}

	private DisplayProductUserControl GetDisplayProductUserControl(object source)
	{
		FrameworkElement frameworkElement = source as FrameworkElement;
		while (frameworkElement != null && !(frameworkElement is DisplayProductUserControl))
		{
			frameworkElement = frameworkElement.Parent as FrameworkElement;
		}
		return frameworkElement as DisplayProductUserControl;
	}
}
