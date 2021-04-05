﻿using ShapeCrawler.Shapes;
using ShapeCrawler.SlideMaster;
using P = DocumentFormat.OpenXml.Presentation;

namespace ShapeCrawler.Tables
{
    /// <summary>
    ///     Represents a table on a Slide Layout.
    /// </summary>
    internal class LayoutTable : LayoutShape, IShape
    {
        internal LayoutTable(SCSlideLayout slideLayout, P.GraphicFrame pGraphicFrame)
            : base(slideLayout, pGraphicFrame)
        {
        }
    }
}