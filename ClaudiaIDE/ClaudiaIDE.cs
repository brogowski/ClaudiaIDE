﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Options;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE
{
	/// <summary>
	/// Adornment class that draws a square box in the top right hand corner of the viewport
	/// </summary>
	public class ClaudiaIDE
	{
		private Setting _Config;
		private Image _Image;
		private BitmapImage _Bitmap;
		private IWpfTextView _View;
		private IAdornmentLayer _AdornmentLayer;

		private IServiceProvider _ServiceProvider;
		
		/// <summary>
		/// Creates a square image and attaches an event handler to the layout changed event that
		/// adds the the square in the upper right-hand corner of the TextView via the adornment layer
		/// </summary>
		/// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
		public ClaudiaIDE(IWpfTextView view, IServiceProvider sp)
		{
			try
			{
				_ServiceProvider = sp;
				_View = view;
				_Image = new Image();

				_Config = GetConfigFromVisualStudioSettings();

				_Image.Opacity = _Config.Opacity;

				_Bitmap = new BitmapImage(new Uri(_Config.BackgroundImageAbsolutePath, UriKind.Absolute));
				_Image.Source = _Bitmap;

				this._AdornmentLayer = view.GetAdornmentLayer("ClaudiaIDE");
				_View.ViewportHeightChanged += delegate { this.onSizeChange(); };
				_View.ViewportWidthChanged += delegate { this.onSizeChange(); };
			}
			catch (Exception)
			{
			}
		}

		private Setting GetConfigFromVisualStudioSettings()
		{
			try
			{
				var config = new Setting();

				var _DTE2 = (DTE2)_ServiceProvider.GetService(typeof(DTE));
				var props = _DTE2.Properties["ClaudiaIDE","General"];

				config.BackgroundImageAbsolutePath = Setting.ToFullPath(props.Item("BackgroundImageAbsolutePath").Value);
				config.Opacity = props.Item("Opacity").Value;
				config.PositionHorizon = (PositionH)props.Item("PositionHorizon").Value;
				config.PositionVertical = (PositionV)props.Item("PositionVertical").Value;
				return config;
			}
			catch (Exception)
			{
				return Setting.Desirialize();
			}
		}

		public void onSizeChange()
		{
			try
			{
				this._AdornmentLayer.RemoveAllAdornments();

				switch (_Config.PositionHorizon)
				{
					case PositionH.Left:
						Canvas.SetLeft(this._Image, 0);
						break;
					case PositionH.Right:
						Canvas.SetLeft(this._Image, this._View.ViewportRight - (double)_Bitmap.PixelWidth);
						break;
				}
				switch (_Config.PositionVertical)
				{
					case PositionV.Top:
						Canvas.SetTop(this._Image, 0);
						break;
					case PositionV.Bottom:
						Canvas.SetTop(this._Image, this._View.ViewportBottom - (double)_Bitmap.PixelHeight);
						break;
				}

				this._AdornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, 
					null,
					null,
					this._Image,
					null);
			}
			catch (Exception)
			{
			
			}
		}
	}
}