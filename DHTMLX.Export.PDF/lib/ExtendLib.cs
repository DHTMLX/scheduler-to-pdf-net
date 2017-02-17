#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing.Layout;
namespace DHTMLX.Export.PDF.Formatter
{
  /// <summary>
  /// Represents a very simple text formatter.
  /// If this class does not satisfy your needs on formatting paragraphs I recommend to take a look
  /// at MigraDoc Foundation. Alternatively you should copy this class in your own source code and modify it.
  /// </summary>
  public class DHXTextFormatter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XTextFormatter"/> class.
    /// </summary>
      public DHXTextFormatter(XGraphics gfx)
    {
      if (gfx == null)
        throw new ArgumentNullException("gfx");
      this.gfx = gfx;
    }
    XGraphics gfx;

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <value>The text.</value>
    public string Text
    {
      get { return this.text; }
      set { this.text = value; }
    }
    string text;

    /// <summary>
    /// Gets or sets the font.
    /// </summary>
    public XFont Font
    {
      get { return this.font; }
      set 
      {
        if (value == null)
          throw new ArgumentNullException("font");
        this.font = value;

        this.lineSpace = font.GetHeight(this.gfx);

        var family = this.Font.FontFamily;

        var cellSpace = family.GetLineSpacing(font.Style);
        var cellAscent = family.GetCellAscent(font.Style);
        var cellDescent = family.GetCellDescent(font.Style);

        this.cyAscent = lineSpace * cellAscent / cellSpace;
        this.cyDescent = lineSpace * cellDescent / cellSpace;

        // HACK in XTextFormatter
        this.spaceWidth = gfx.MeasureString("x x", value).Width;
        this.spaceWidth -= gfx.MeasureString("xx", value).Width;
      }
    }
    XFont font;
    double lineSpace;
    double cyAscent;
    double cyDescent;
    double spaceWidth;

    /// <summary>
    /// Gets or sets the bounding box of the layout.
    /// </summary>
    public XRect LayoutRectangle
    {
      get { return this.layoutRectangle; }
      set { this.layoutRectangle = value; }
    }
    XRect layoutRectangle;

    /// <summary>
    /// Gets or sets the alignment of the text.
    /// </summary>
    public XParagraphAlignment Alignment
    {
      get { return this.alignment; }
      set { this.alignment = value; }
    }
    XParagraphAlignment alignment = XParagraphAlignment.Left;

    /// <summary>
    /// Draws the text.
    /// </summary>
    /// <param name="text">The text to be drawn.</param>
    /// <param name="font">The font.</param>
    /// <param name="brush">The text brush.</param>
    /// <param name="layoutRectangle">The layout rectangle.</param>
    public void DrawString(string text, XFont font, XBrush brush, XRect layoutRectangle)
    {
      DrawString(text, font, brush, layoutRectangle, XStringFormats.TopLeft);
    }

    /// <summary>
    /// Draws the text.
    /// </summary>
    /// <param name="text">The text to be drawn.</param>
    /// <param name="font">The font.</param>
    /// <param name="brush">The text brush.</param>
    /// <param name="layoutRectangle">The layout rectangle.</param>
    /// <param name="format">The format. Must be <c>XStringFormat.TopLeft</c></param>
    public void DrawString(string text, XFont font, XBrush brush, XRect layoutRectangle, XStringFormat format)
    {
      if (text == null)
        throw new ArgumentNullException("text");
      if (font == null)
        throw new ArgumentNullException("font");
      if (brush == null)
        throw new ArgumentNullException("brush");
      if (format.Alignment != XStringAlignment.Near || format.LineAlignment!= XLineAlignment.Near)
        throw new ArgumentException("Only TopLeft alignment is currently implemented.");

      Text = text;
      Font = font;
      LayoutRectangle = layoutRectangle;

      if (text.Length == 0)
        return;

      CreateBlocks(layoutRectangle.Width);
      
      CreateLayout();

      double dx = layoutRectangle.Location.X;
      double mWidth = layoutRectangle.Size.Width;
      double dy = layoutRectangle.Location.Y + cyAscent;
      int count = this.blocks.Count;
      for (int idx = 0; idx < count; idx++)
      {
        Block block = (Block)this.blocks[idx];
        if (block.Stop)
          break;
        if (block.Type == BlockType.LineBreak)
          continue;

      
        if (idx +1 < count)
        {
            if (this.blocks[idx + 1].Stop)
            {
                if (block.Text.Length > 3)
                {
                    block.Text = block.Text.Substring(0, block.Text.Length - 3) + "...";
                }
                else
                {
                    int dots = 0;
                    while (dots < 3 && gfx.MeasureString(block.Text, font).Width < mWidth)
                    {
                        block.Text += ".";
                        dots++;
                    }
                }
            }
        }
        gfx.DrawString(block.Text, font, brush, dx + block.Location.X, dy + block.Location.Y);
      }
    }

    void CreateBlocks(double maxWidth)
    {
      this.blocks.Clear();
      int length = this.text.Length;
      bool inNonWhiteSpace = false;
      int startIndex = 0, blockLength = 0;
      for (int idx = 0; idx < length; idx++)
      {
        char ch = text[idx];

        // Treat CR and CRLF as LF
        if (ch == Chars.CR)
        {
          if (idx < length - 1 && text[idx + 1] == Chars.LF)
            idx++;
          ch = Chars.LF;
        }
        if (ch == Chars.LF)
        {
          if (blockLength != 0)
          {
            string token = text.Substring(startIndex, blockLength);
            this.blocks.Add(new Block(token, BlockType.Text,
              this.gfx.MeasureString(token, this.font).Width));
          }
          startIndex = idx + 1;
          blockLength = 0;
          this.blocks.Add(new Block(BlockType.LineBreak));
        }
        else if (Char.IsWhiteSpace(ch))
        {
          if (inNonWhiteSpace)
          {
            string token = text.Substring(startIndex, blockLength);
            this.blocks.Add(new Block(token, BlockType.Text,
              this.gfx.MeasureString(token, this.font).Width));
            startIndex = idx + 1;
            blockLength = 0;
          }
          else
          {
            blockLength++;
          }
        }
        else
        {
            inNonWhiteSpace = true;
            blockLength++;
            if (startIndex + blockLength < length)
            {
                if (this.gfx.MeasureString(text.Substring(startIndex, blockLength + 1), this.font).Width >= maxWidth)
                {
                    string token = text.Substring(startIndex, blockLength);
                    this.blocks.Add(new Block(token, BlockType.Text,
                      this.gfx.MeasureString(token, this.font).Width));
                    startIndex = startIndex + blockLength;
                    blockLength = 0;
                    inNonWhiteSpace = false;
                }
            }
            
        }
      }
      if (blockLength != 0)
      {
        string token = text.Substring(startIndex, blockLength);
        this.blocks.Add(new Block(token, BlockType.Text,
          this.gfx.MeasureString(token, this.font).Width));
      }
    }

    void CreateLayout()
    {
      double rectWidth = this.layoutRectangle.Width;
      double rectHeight = this.layoutRectangle.Height - this.cyAscent - this.cyDescent;
      int firstIndex = 0;
      double x = 0, y = 0;
      int count = this.blocks.Count;
      for (int idx = 0; idx < count; idx++)
      {
        Block block = (Block)this.blocks[idx];
        if (block.Type == BlockType.LineBreak)
        {
          if (Alignment == XParagraphAlignment.Justify)
            ((Block)this.blocks[firstIndex]).Alignment = XParagraphAlignment.Left;
          AlignLine(firstIndex, idx - 1, rectWidth);
          firstIndex = idx + 1;
          x = 0;
          y += this.lineSpace;
        }
        else
        {
          double width = block.Width; //!!!modTHHO 19.11.09 don't add this.spaceWidth here
          if ((x + width <= rectWidth || x == 0) && block.Type != BlockType.LineBreak)
          {
            block.Location = new XPoint(x, y);
            x += width + spaceWidth; //!!!modTHHO 19.11.09 add this.spaceWidth here
          }
          else
          {
            AlignLine(firstIndex, idx - 1, rectWidth);
            firstIndex = idx;
            y += lineSpace;
            if (y > rectHeight)
            {
              block.Stop = true;
              break;
            }
            block.Location = new XPoint(0, y);
            x = width + spaceWidth; //!!!modTHHO 19.11.09 add this.spaceWidth here
          }
        }
      }
      if (firstIndex < count && Alignment != XParagraphAlignment.Justify)
        AlignLine(firstIndex, count - 1, rectWidth);
    }

    /// <summary>
    /// Align center, right or justify.
    /// </summary>
    void AlignLine(int firstIndex, int lastIndex, double layoutWidth)
    {
      XParagraphAlignment blockAlignment = ((Block)(this.blocks[firstIndex])).Alignment;
      if (this.alignment == XParagraphAlignment.Left || blockAlignment == XParagraphAlignment.Left)
        return;

      int count = lastIndex - firstIndex + 1;
      if (count == 0)
        return;

      double totalWidth = -this.spaceWidth;
      for (int idx = firstIndex; idx <= lastIndex; idx++)
        totalWidth += ((Block)(this.blocks[idx])).Width + this.spaceWidth;

      double dx = Math.Max(layoutWidth - totalWidth, 0);
      //Debug.Assert(dx >= 0);
      if (this.alignment != XParagraphAlignment.Justify)
      {
        if (this.alignment == XParagraphAlignment.Center)
          dx /= 2;
        for (int idx = firstIndex; idx <= lastIndex; idx++)
        {
          Block block = (Block)this.blocks[idx];
          block.Location += new XSize(dx, 0);
        }
      }
      else if (count > 1) // case: justify
      {
        dx /= count - 1;
        for (int idx = firstIndex + 1, i = 1; idx <= lastIndex; idx++, i++)
        {
          Block block = (Block)this.blocks[idx];
          block.Location += new XSize(dx * i, 0);
        }
      }
    }

    readonly List<Block> blocks = new List<Block>();

    enum BlockType
    {
      Text, Space, Hyphen, LineBreak,
    }

    /// <summary>
    /// Represents a single word.
    /// </summary>
    class Block
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="Block"/> class.
      /// </summary>
      /// <param name="text">The text of the block.</param>
      /// <param name="type">The type of the block.</param>
      /// <param name="width">The width of the text.</param>
      public Block(string text, BlockType type, double width)
      {
        Text = text;
        Type = type;
        Width = width;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="Block"/> class.
      /// </summary>
      /// <param name="type">The type.</param>
      public Block(BlockType type)
      {
        Type = type;
      }

      /// <summary>
      /// The text represented by this block.
      /// </summary>
      public string Text;

      /// <summary>
      /// The type of the block.
      /// </summary>
      public BlockType Type;

      /// <summary>
      /// The width of the text.
      /// </summary>
      public double Width;

      /// <summary>
      /// The location relative to the upper left corner of the layout rectangle.
      /// </summary>
      public XPoint Location;

      /// <summary>
      /// The alignment of this line.
      /// </summary>
      public XParagraphAlignment Alignment;

      /// <summary>
      /// A flag indicating that this is the last bock that fits in the layout rectangle.
      /// </summary>
      public bool Stop;
    }
    // TODO:
    // - more XStringFormat variations
    // - calculate bounding box
    // - left and right indent
    // - first line indent
    // - margins and paddings
    // - background color
    // - text background color
    // - border style
    // - hyphens, soft hyphens, hyphenation
    // - kerning
    // - change font, size, text color etc.
    // - line spacing
    // - underine and strike-out variation
    // - super- and sub-script
    // - ...
  }



  /// <summary>
  /// Character table by name.
  /// </summary>
  internal sealed class Chars
  {
      // ReSharper disable InconsistentNaming
      public const char EOF = (char)65535; //unchecked((char)(-1));
      public const char NUL = '\0';   // EOF
      public const char CR = '\x0D'; // ignored by lexer
      public const char LF = '\x0A'; // Line feed
      public const char BEL = '\a';   // Bell
      public const char BS = '\b';   // Backspace
      public const char FF = '\f';   // Form feed
      public const char HT = '\t';   // Horizontal tab
      public const char VT = '\v';   // Vertical tab
      public const char NonBreakableSpace = (char)160;  // char(160)

      // The following names come from "PDF Reference Third Edition"
      // Appendix D.1, Latin Character Set and Encoding
      public const char SP = ' ';
      public const char QuoteDbl = '"';
      public const char QuoteSingle = '\'';
      public const char ParenLeft = '(';
      public const char ParenRight = ')';
      public const char BraceLeft = '{';
      public const char BraceRight = '}';
      public const char BracketLeft = '[';
      public const char BracketRight = ']';
      public const char Less = '<';
      public const char Greater = '>';
      public const char Equal = '=';
      public const char Period = '.';
      public const char Semicolon = ';';
      public const char Colon = ':';
      public const char Slash = '/';
      public const char Bar = '|';
      public const char BackSlash = '\\';
      public const char Percent = '%';
      public const char Dollar = '$';
      public const char At = '@';
      public const char NumberSign = '#';
      public const char Question = '?';
      public const char Hyphen = '-';  // char(45)
      public const char SoftHyphen = '­';  // char(173)
      public const char Currency = '¤';
      // ReSharper restore InconsistentNaming
  }
}
