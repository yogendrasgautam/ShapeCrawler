﻿using System.Collections.Generic;
using System.Linq;
using LogicNull.Utilities;
using SlideXML.Models.Settings;
using A = DocumentFormat.OpenXml.Drawing;

namespace SlideXML.Models.TextBody
{
    /// <summary>
    /// Represents a text paragraph.
    /// </summary>
    public class ParagraphSL
    {
        #region Fields

        private readonly A.Paragraph _aParagraph;
        private readonly ElementSettings _shapeSetting;

        private string _text;
        private int? _lvl; // paragraph's level
        private List<Portion> _portions;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Returns the paragraph's text string.
        /// </summary>
        public string Text {
            get
            {
                if (_text == null)
                {
                    InitText();
                }

                return _text;
            }
        } 

        /// <summary>
        /// Returns paragraph text portions.
        /// </summary>
        public IList<Portion> Portions {
            get
            {
                if (_portions == null)
                {
                    InitPortions();
                }

                return _portions;
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes an instance of the <see cref="ParagraphSL"/> class.
        /// </summary>
        /// <param name="elSetting"></param>
        /// <param name="aParagraph">A XML paragraph which contains a text.</param>
        public ParagraphSL(ElementSettings elSetting, A.Paragraph aParagraph)
        {
            Check.NotNull(aParagraph, nameof(aParagraph));
            Check.NotNull(elSetting, nameof(elSetting));
            _aParagraph = aParagraph;
            _shapeSetting = elSetting;
        }

        #endregion Constructors

        #region Private Methods

        private void InitText()
        {
            _text = Portions.Select(p => p.Text).Aggregate((t1, t2) => t1 + t2);
        }

        private void InitPortions()
        {
            var prLvl = ParseLevel();
            var runs = _aParagraph.Elements<A.Run>();
            _portions = new List<Portion>(runs.Count());
            var placeholderSL = _shapeSetting.Placeholder;

            foreach (var run in runs)
            {
                // First tries to get font height from run, then placeholder and only then from presentation settings.
                var fh = run.RunProperties?.FontSize?.Value ?? placeholderSL?.FontHeights[prLvl] ?? _shapeSetting.PreSettings.LlvFontHeights[prLvl];
                
                _portions.Add(new Portion(fh, run.Text.Text));
            }
        }

        private int ParseLevel()
        {
            if (_lvl == null)
            {
                // Gets default paragraph level font height for current paragraph's level
                _lvl = _aParagraph.ParagraphProperties?.Level?.Value;
                if (_lvl == null)
                {
                    _lvl = 1;
                }
                else
                {
                    // By unknown reason, slide and presentation's default settings have different numbering
                    _lvl++;
                }
            }

            return (int)_lvl;
        }

        #endregion Private Methods
    }
}
