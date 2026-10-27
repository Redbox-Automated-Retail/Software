using System;
using System.IO;
using System.Windows;
using System.Windows.Xps.Packaging;
using Redbox.Controls;
using Redbox.Core;
using Redbox.FMA.Models;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.Planogram;
using Redbox.Rental.Services;
using Redbox.Rental.UI;

namespace Redbox.FMA.Controllers;

public class ShowPlanogramViewController : BaseViewController
{
	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		ShowPlanogramViewModel viewModel = GetViewModel();
		Planogram selectedPlanogram = (ServiceLocator.Instance.GetService<IKioskClientService>()?.Planogram).GetSelectedPlanogram();
		if (viewModel == null && selectedPlanogram != null)
		{
			viewModel = new ShowPlanogramViewModel();
			viewModel.CloseCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				CloseButtonClickedAction();
			}, (Func<bool>)null);
			viewModel.SelectCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				SelectPlanogram();
			}, (Func<bool>)null);
			viewModel.Name = selectedPlanogram.Name;
			viewModel.StartDate = selectedPlanogram.StartDate;
			viewModel.FileType = selectedPlanogram.FileType;
			viewModel.FitHeightCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				SetDocumentScaling(viewModel, DocumentScaling.FitHeight);
			}, (Func<bool>)null);
			viewModel.FitWidthCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				SetDocumentScaling(viewModel, DocumentScaling.FitWidth);
			}, (Func<bool>)null);
			viewModel.DefaultScaleCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				SetDocumentScaling(viewModel, DocumentScaling.Normal);
			}, (Func<bool>)null);
			if (selectedPlanogram.FileType == FileType.Xps)
			{
				XpsDocument xpsDocument = new XpsDocument(Path.Combine(selectedPlanogram.Path, selectedPlanogram.Filename), FileAccess.Read);
				viewModel.XpsDocumentSource = xpsDocument.GetFixedDocumentSequence();
				xpsDocument.Close();
			}
			else
			{
				viewModel.rtfDocument = File.ReadAllBytes(Path.Combine(selectedPlanogram.Path, selectedPlanogram.Filename));
			}
		}
		else if (selectedPlanogram == null)
		{
			SelectPlanogram();
		}
		viewUserControl.DataContext = viewModel;
	}

	private static void SetDocumentScaling(ShowPlanogramViewModel viewModel, DocumentScaling scaleLevel)
	{
		viewModel.DocumentScaling = scaleLevel;
		BaseViewController.ProcessWPFHit();
	}

	private static void SelectPlanogram()
	{
		OrchestrationService.ViewShow("select_planogram_view");
	}

	private static void CloseButtonClickedAction()
	{
		OrchestrationService.UnwindToSafe("main_menu_view", discard: true, "show_planogram_view");
	}

	private static ShowPlanogramViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as ShowPlanogramViewModel;
		}
		return null;
	}
}
