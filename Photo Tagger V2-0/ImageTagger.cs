using System;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Photo_Tagger_V2_0
{
    class ImageTagger
    { 
        public static void TagImage(FileInfo imageInfo, FileInfo destination, FileInfo logoInfo, string position)
        {
            Image image = Image.FromFile(imageInfo.FullName);
            Image logo = Image.FromFile(logoInfo.FullName);

            image = applyEXIFRotation(image);

            Image outputImage = new Bitmap(image);
            outputImage = applyEXIFData(image, outputImage);

            double scaleFactor = getLogoScale(logo, image, position);
            logo = resizeImage(logo, scaleFactor);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                switch (position)
                {
                    case "Watermark Large":
                        logo = setImageOpacity(logo, 0.3f);

                        graphics.DrawImage(logo, (image.Width / 2) - (logo.Width / 2), (image.Height / 2) - (logo.Height / 2));
                        break;

                    case "Watermark Tiled":
                        logo = setImageOpacity(logo, 0.3f);

                        int borderSpacing = (int)(image.Width * 0.05);
                        int spacingX, spacingY;
                        spacingX = (image.Width - (logo.Width * 3) - (borderSpacing * 2)) / 2;
                        spacingY = (int)(image.Height * 0.1);

                        int x = borderSpacing, y = borderSpacing;

                        while ((y + logo.Height) <= (image.Height - borderSpacing + (image.Width * 0.1)))
                        {
                            while ((x + logo.Width) <= (image.Width - borderSpacing))
                            {
                                graphics.DrawImage(logo, x, y);
                                x += logo.Width + spacingX;
                            }
                            x = borderSpacing;
                            y += logo.Height + spacingY;
                        }

                        break;
                    case "Top Left":
                        graphics.DrawImage(logo, 0, 0);
                        break;

                    case "Top Right":
                        graphics.DrawImage(logo, image.Width - logo.Width, 0);
                        break;

                    case "Top":
                        graphics.DrawImage(logo, (image.Width / 2) - (logo.Width / 2), 0);
                        break;

                    case "Bottom":
                        graphics.DrawImage(logo, (image.Width / 2) - (logo.Width / 2), image.Height - logo.Height);
                        break;

                    case "Left":
                        graphics.DrawImage(logo, 0, (image.Height / 2) - (logo.Height / 2));
                        break;

                    case "Right":
                        graphics.DrawImage(logo, image.Width - logo.Width, (image.Height / 2) - (logo.Height / 2));
                        break;

                    case "Bottom Right":
                        graphics.DrawImage(logo, image.Width - logo.Width, image.Height - logo.Height);
                        break;

                    case "Centre":
                        graphics.DrawImage(logo, (image.Width / 2) - (logo.Width / 2), (image.Height / 2) - (logo.Height / 2));
                        break;

                    default: // Bottom Left
                        graphics.DrawImage(logo, 0, image.Height - logo.Height);
                        break;
                }

            }
            logo.Dispose();
            image.Dispose();

            saveImage(outputImage, destination);
            outputImage.Dispose();
        }

        private static void saveImage(Image image, FileInfo fileInfo)
        {
            ImageFormat imgFormat = ImageFormat.Jpeg;

            if (fileInfo.Extension == ".png")
                imgFormat = ImageFormat.Png;


            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            image.Save(fileInfo.FullName, imgFormat);
        }

        private static Image applyEXIFRotation(Image image)
        {
            if (Array.IndexOf(image.PropertyIdList, 274) > 1)
            {
                var orientation = (int)image.GetPropertyItem(274).Value[0];

                switch (orientation)
                {
                    case 1:
                        // Not rotation required
                        break;
                    case 2:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        image.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        image.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                image.RemovePropertyItem(274);
            }
            return image;
        }

        private static Image applyEXIFData(Image inputImage, Image outputImage)
        {
            foreach (var id in inputImage.PropertyIdList)
            {
                outputImage.SetPropertyItem(inputImage.GetPropertyItem(id));
            }

            return outputImage;
        }

        private static double getLogoScale(Image logo, Image image, string position)
        {
            bool isImageLandscape, isLogoLandscape;

            isImageLandscape = image.Width >= image.Height;
            isLogoLandscape = logo.Width >= logo.Height;

            double scaleFactor = 1;
            double ratio = 0.2;

            if (position == "Watermark Large")
            {
                if (isLogoLandscape)
                {
                    scaleFactor = (image.Width * 0.9) / logo.Width;
                }
                else
                {
                    scaleFactor = (image.Height * 0.9) / logo.Height;
                }
            }
            else if (position == "Watermark Tiled")
            {
                scaleFactor = (image.Width * 0.25) / logo.Width;
            }
            else
            {
                if (isImageLandscape)
                {
                    if (isLogoLandscape)
                    {
                        ratio = 0.2;
                        int newLogoWidth = (int)(image.Width * ratio);
                        scaleFactor = (double)newLogoWidth / logo.Width;
                    }
                    else
                    {
                        ratio = 0.4;
                        int newLogoHeight = (int)(image.Height * ratio);
                        scaleFactor = (double)newLogoHeight / logo.Height;
                    }
                }
                else
                {
                    if (isLogoLandscape)
                    {
                        ratio = 0.4;
                        int newLogoWidth = (int)(image.Width * ratio);
                        scaleFactor = (double)newLogoWidth / logo.Width;
                    }
                    else
                    {
                        ratio = 0.2;
                        int newLogoHeight = (int)(image.Height * ratio);
                        scaleFactor = (double)newLogoHeight / logo.Height;
                    }
                }
            }
            

            return scaleFactor;
        }

        private static Image setImageOpacity(Image image, float opacity)
        {
            Bitmap outputImage = new Bitmap(image.Width, image.Height);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                ColorMatrix matrix = new ColorMatrix();

                matrix.Matrix33 = opacity;

                ImageAttributes attributes = new ImageAttributes();

                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                graphics.DrawImage(image, new Rectangle(0, 0, outputImage.Width, outputImage.Height), 0, 0, outputImage.Width, outputImage.Height, GraphicsUnit.Pixel, attributes);
            }
            image.Dispose();
            return outputImage;
        }

        private static Image rotateImage(Image image, float angle)
        {
            int width = (int)((image.Width * Math.Cos(angle)) + (image.Height * Math.Sin(angle)));
            int height = (int)((image.Width * Math.Sin(angle)) + (image.Height * Math.Cos(angle)));

            var destImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                graphics.TranslateTransform((float)width / 2, (float)height / 2);
                graphics.RotateTransform(angle);
                graphics.TranslateTransform(-(float)width / 2, -(float)height / 2);
                graphics.DrawImage(image, new Point(0,0));
            }
            image.Dispose();
            return destImage;
        }

        private static Image resizeImage(Image image, double factor)
        {
            int width = (int)(image.Width * factor);
            int height = (int)(image.Height * factor);
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            }
            image.Dispose();

            return destImage;
        }
    }
}
