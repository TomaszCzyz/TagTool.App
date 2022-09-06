﻿using System.Text;
using Avalonia.Media;

namespace TagTool.App.Controls;

public sealed class StyledText
{
    private readonly StringBuilder _builder;
    private readonly List<FormattedTextStyleSpan> _spans;

    private StyledText()
    {
        _builder = new StringBuilder();
        _spans = new List<FormattedTextStyleSpan>();
    }

    public static StyledText Create() => new StyledText();

    public string Text => _builder.ToString();
    public IReadOnlyList<FormattedTextStyleSpan> Spans => _spans.AsReadOnly();

    public StyledText AppendLine()
    {
        _builder.AppendLine();

        return this;
    }

    public StyledText AppendLine(string text, IBrush? brush = null)
    {
        if (brush != null)
        {
            _spans.Add(new FormattedTextStyleSpan(_builder.Length, text.Length, brush));
        }

        _builder.AppendLine(text);

        return this;
    }

    public StyledText Append(string text, IBrush? brush = null)
    {
        if (brush != null)
        {
            _spans.Add(new FormattedTextStyleSpan(_builder.Length, text.Length, brush));
        }

        _builder.Append(text);

        return this;
    }

    public StyledText Clear()
    {
        _builder.Clear();
        _spans.Clear();
        return this;
    }
}
