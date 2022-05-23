using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HtmlTextBlock
{
	public class InlineImage
	{
		private double height = Double.NaN;

		/// <summary>
		/// Set BEFORE calling LoadImage()
		/// </summary>
		public double Height
		{
			get { return height; }
			set { height = value; }
		}

		private double width = Double.NaN;

		/// <summary>
		/// Set BEFORE calling LoadImage()
		/// </summary>
		public double Width
		{
			get { return width; }
			set { width = value; }
		}

		private bool isHyperlink = false;

		public bool IsHyperlink
		{
			get { return isHyperlink; }
			set { isHyperlink = value; }
		}
		
		private string URI;

		public string HyperlinkAddress
		{
			get { return URI; }
			set { URI = value; }
		}

		private string imageSource;

		public string ImageSource
		{
			get { return imageSource; }
			set { imageSource = value; }
		}

		private Image image;

		public Image Image
		{
			get { return image; }
			set { image = value; }
		}

		/// <summary>
		/// Loads the image from the specified URI.
		/// </summary>
		/// <param name="URI">The URI</param>
		public void LoadImage(string URI)
		{
		    Uri uri = null;

		    if (!Uri.TryCreate(URI, UriKind.RelativeOrAbsolute, out uri))
		        return;

			image = new Image();
			BitmapImage tmp = new BitmapImage();
			tmp.BeginInit();
		    tmp.UriSource = uri;
			tmp.EndInit();
			image.Source = tmp;
			image.Width = width;
			image.Height = height;


			RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
			image.UseLayoutRounding = true; //Prevents sub pixel positoning to remove jagged edges

		}

		/// <summary>
		/// Loads the image.
		/// </summary>
		public void LoadImage()
		{
			LoadImage(imageSource);
		}

		public Inline BuildInlineContext()
		{
			if (image == null)
				LoadImage();


			InlineUIContainer imageContainer = new InlineUIContainer(image);

			


			if (isHyperlink)
			{
				Hyperlink link = new Hyperlink(imageContainer);
				try
				{
					link.NavigateUri = new Uri(URI);
				}
				catch
				{
					link.NavigateUri = null;
				}
				return link;
			}
			else
				return imageContainer;
		}
	}
}
