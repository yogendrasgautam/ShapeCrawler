﻿using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Html.Dom;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using ShapeCrawler.Drawing;
using ShapeCrawler.Extensions;
using ShapeCrawler.Shapes;
using ShapeCrawler.Shared;
using ShapeCrawler.Texts;
using SkiaSharp;
using P = DocumentFormat.OpenXml.Presentation;

namespace ShapeCrawler.SlideShape;

internal sealed class SlideAutoShapeWithFill : IShape, IRemoveable
{
    private readonly P.Shape pShape;
    private readonly SlideAutoShape slideAutoShape;
    private readonly SlidePart sdkSlidePart;

    internal SlideAutoShapeWithFill(SlidePart sdkSlidePart, P.Shape pShape)
        : this(
            sdkSlidePart,
            pShape,
            new SlideAutoShape(sdkSlidePart, pShape),
            new SlideShapeFill(sdkSlidePart, pShape.GetFirstChild<P.ShapeProperties>() !, pShape.UseBackgroundFill)
        )
    {
    }

    private SlideAutoShapeWithFill(
        SlidePart sdkSlidePart,
        P.Shape pShape,
        SlideAutoShape slideAutoShape,
        IShapeFill fill)
    {
        this.sdkSlidePart = sdkSlidePart;
        this.pShape = pShape;
        this.slideAutoShape = slideAutoShape;
        this.Fill = fill;
    }

    public IShapeFill Fill { get; }
    
    public bool HasOutline => true;
    public IShapeOutline Outline => this.slideAutoShape.Outline;
    public bool HasFill => this.slideAutoShape.HasFill;
    public SCShapeType ShapeType => SCShapeType.AutoShape;
    public double Rotation => this.slideAutoShape.Rotation;
    public bool IsTextHolder => this.slideAutoShape.IsTextHolder;
    public ITextFrame TextFrame => this.slideAutoShape.TextFrame;
    public ITable AsTable() => this.slideAutoShape.AsTable();
    public IMediaShape AsMedia() => this.slideAutoShape.AsMedia();
    public bool IsPlaceholder => this.slideAutoShape.IsPlaceholder;
    public IPlaceholder Placeholder => this.slideAutoShape.Placeholder;

    internal void Draw(SKCanvas slideCanvas)
    {
        var skColorOutline = SKColor.Parse(this.Outline.HexColor);

        using var paint = new SKPaint
        {
            Color = skColorOutline,
            IsAntialias = true,
            StrokeWidth = UnitConverter.PointToPixel(this.Outline.Weight),
            Style = SKPaintStyle.Stroke
        };

        if (this.GeometryType == SCGeometry.Rectangle)
        {
            float left = this.X;
            float top = this.Y;
            float right = this.X + this.Width;
            float bottom = this.Y + this.Height;
            var rect = new SKRect(left, top, right, bottom);
            slideCanvas.DrawRect(rect, paint);
            var textFrame = (TextFrame)this.TextFrame!;
            textFrame.Draw(slideCanvas, left, this.Y);
        }
    }

    #region Shape

    public int Width
    {
        get => this.slideAutoShape.Width;
        set => this.slideAutoShape.Width = value;
    }

    public int Height
    {
        get => this.slideAutoShape.Height;
        set => this.slideAutoShape.Height = value;
    }

    public int Id => this.slideAutoShape.Id;
    public string Name => this.slideAutoShape.Name;
    public bool Hidden => this.slideAutoShape.Hidden;
    public SCGeometry GeometryType => this.slideAutoShape.GeometryType;

    public string? CustomData
    {
        get => this.slideAutoShape.CustomData;
        set => this.slideAutoShape.CustomData = value;
    }

    public int X
    {
        get => this.slideAutoShape.X;
        set => this.slideAutoShape.X = value;
    }

    public int Y
    {
        get => this.slideAutoShape.Y;
        set => this.slideAutoShape.Y = value;
    }

    #endregion Shape

    internal void CopyTo(
        int id,
        P.ShapeTree pShapeTree,
        IEnumerable<string> existingShapeNames,
        SlidePart targetSdkSlidePart)
    {
        var copy = this.pShape.CloneNode(true);
        copy.GetNonVisualDrawingProperties().Id = new UInt32Value((uint)id);
        pShapeTree.AppendChild(copy);
        var copyName = copy.GetNonVisualDrawingProperties().Name!.Value!;
        if (existingShapeNames.Any(existingShapeName => existingShapeName == copyName))
        {
            var currentShapeCollectionSuffixes = existingShapeNames
                .Where(c => c.StartsWith(copyName, StringComparison.InvariantCulture))
                .Select(c => c.Substring(copyName.Length))
                .ToArray();

            // We will try to check numeric suffixes only.
            var numericSuffixes = new List<int>();

            foreach (var currentSuffix in currentShapeCollectionSuffixes)
            {
                if (int.TryParse(currentSuffix, out var numericSuffix))
                {
                    numericSuffixes.Add(numericSuffix);
                }
            }

            numericSuffixes.Sort();
            var lastSuffix = numericSuffixes.LastOrDefault() + 1;
            copy.GetNonVisualDrawingProperties().Name = copyName + " " + lastSuffix;
        }
    }

    void IRemoveable.Remove()
    {
        this.pShape.Remove();
    }
}