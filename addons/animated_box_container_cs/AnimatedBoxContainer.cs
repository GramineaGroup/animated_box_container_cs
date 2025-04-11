using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

struct MiniSizeCache
{
    public float MinSize;
    public bool WillStretch;
    public float FinalSize;
}

struct TweenMiniSizeCache
{
    public Tween Tween;
    public MiniSizeCache MiniSizeCache;
}

public enum AnimatedBoxContainerAlignment
{
    Begin,
    Center,
    End,
}

public enum AnimatedBoxContainerDirection
{
    Vertical,
    Horizontal
}

[Tool]
public partial class AnimatedBoxContainer : Container
{
    [Export]
    public AnimatedBoxContainerDirection ContainerDirection
    {
        get { return _direction; }
        set
        {
            _direction = value;
            UpdateMinimumSize();
            QueueSort();
        }
    }
    private AnimatedBoxContainerDirection _direction;
    [Export]
    private AnimatedBoxContainerAlignment ContainerAlignment
    {
        get { return _alignment; }
        set
        {
            _alignment = value;
            QueueSort();
        }
    }
    private AnimatedBoxContainerAlignment _alignment;
    [Export]
    public int Separation
    {
        get { return _separation; }
        set
        {
            _separation = value;
            QueueSort();
        }
    }
    private int _separation = 0;
    private double _delay = 0;
    private double _currentDuration = 0d;
    // default Linear
    private Tween.TransitionType _trans;
    private Tween.EaseType _ease;
    private bool _useTween = false;
    private Dictionary<Control, TweenMiniSizeCache> _tweenList = [];
    private Vector2 _size;

    public override void _EnterTree()
    {
        MouseFilter = MouseFilterEnum.Pass;
    }

    /// <summary>
    /// Adapted from godotengine/godot/blob/4.4/scene/gui/box_container.cpp
    /// </summary>
    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationSortChildren:
                Resort();
                break;
            case NotificationTranslationChanged:
            case NotificationLayoutDirectionChanged:
                QueueSort();
                break;
        }
        UpdateMinimumSize();
    }

    /// <summary>
    /// Adapted from godotengine/godot/blob/4.4/scene/gui/box_container.cpp
    /// </summary>
    private void Resort()
    {
        var newSize = new Vector2(_size.X, _size.Y);
        var rtl = IsLayoutRtl();
        var childrenCount = GetChildCount();
        var stretchMin = 0f;
        var stretchAvail = 0f;
        var stretchRatioTotal = 0f;
        Dictionary<Control, MiniSizeCache> minSizeCache = [];

        foreach (var child in GetChildren())
        {
            if (child is Control)
            {
                var c = child as Control;
                // tween is playing, skip this child
                _tweenList.TryGetValue(c, out TweenMiniSizeCache tweenMiniSizeCache);
                var msc = tweenMiniSizeCache.MiniSizeCache;

                if (tweenMiniSizeCache.Tween == null)
                {
                    var size = GetChildMinimumSize(c);
                    msc = new MiniSizeCache();

                    if (_direction == AnimatedBoxContainerDirection.Vertical)
                    {
                        stretchMin += size.Y;
                        msc.MinSize = size.Y;
                        msc.WillStretch = c.SizeFlagsVertical.HasFlag(SizeFlags.Expand);
                    }
                    else
                    {
                        stretchMin += size.X;
                        msc.MinSize += size.X;
                        msc.WillStretch = c.SizeFlagsHorizontal.HasFlag(SizeFlags.Expand);
                    }

                    if (msc.WillStretch)
                    {
                        stretchAvail += msc.MinSize;
                        stretchRatioTotal += c.SizeFlagsStretchRatio;
                    }

                    msc.FinalSize = msc.MinSize;
                }

                minSizeCache[c] = msc;
            }
        }

        if (childrenCount == 0)
        {
            return;
        }

        var stretchMax = (_direction == AnimatedBoxContainerDirection.Vertical ? newSize.Y : newSize.X) - (childrenCount - 1) * _separation;
        var stretchDiff = Math.Max(0, stretchMax - stretchMin);

        stretchAvail += stretchDiff;

        var hasStretched = false;
        while (stretchRatioTotal > 0)
        {
            hasStretched = true;
            var refitSuccessful = true;
            var error = 0f;

            foreach (var child in GetChildren())
            {
                if (child is Control)
                {
                    var c = child as Control;
                    var msc = minSizeCache[c];

                    if (msc.WillStretch)
                    {
                        var finalPixelSize = stretchAvail * c.SizeFlagsStretchRatio / stretchRatioTotal;
                        error += finalPixelSize - (int)finalPixelSize;
                        if (finalPixelSize < msc.MinSize)
                        {
                            msc.WillStretch = false;
                            stretchRatioTotal -= c.SizeFlagsStretchRatio;
                            refitSuccessful = false;
                            stretchAvail -= msc.MinSize;
                            msc.FinalSize = msc.MinSize;
                            break;
                        }
                        else
                        {
                            msc.FinalSize = finalPixelSize;
                            if (error >= 1)
                            {
                                msc.FinalSize += 1;
                                error -= 1;
                            }
                        }
                    }
                }
            }

            if (refitSuccessful)
            {
                break;
            }
        }

        float ofs = 0;
        if (!hasStretched)
        {
            if (_direction == AnimatedBoxContainerDirection.Vertical)
            {
                switch (_alignment)
                {
                    case AnimatedBoxContainerAlignment.Begin:
                        if (rtl)
                        {
                            ofs = stretchDiff;
                        }
                        break;
                    case AnimatedBoxContainerAlignment.Center:
                        ofs = stretchDiff / 2;
                        break;
                    case AnimatedBoxContainerAlignment.End:
                        if (!rtl)
                        {
                            ofs = stretchDiff;
                        }
                        break;
                }
            }
            else
            {
                switch (_alignment)
                {
                    case AnimatedBoxContainerAlignment.Begin:
                        break;
                    case AnimatedBoxContainerAlignment.Center:
                        ofs = stretchDiff / 2;
                        break;
                    case AnimatedBoxContainerAlignment.End:
                        ofs = stretchDiff;
                        break;
                }
            }
        }

        bool first = true;
        var idx = 0;
        var delta = 0;

        int start;
        int end;
        if (!rtl || _direction == AnimatedBoxContainerDirection.Vertical)
        {
            start = 0;
            end = GetChildCount();
            delta += 1;
        }
        else
        {
            start = GetChildCount() - 1;
            end = -1;
            delta = -1;
        }

        for (var i = start; i != end; i += delta)
        {
            var child = GetChild(i);
            if (child is Control)
            {
                var c = child as Control;
                var msc = minSizeCache[c];

                if (first)
                {
                    first = false;
                }
                else
                {
                    ofs += _separation;
                }

                var from = ofs;
                var to = ofs + msc.FinalSize;

                if (msc.WillStretch && idx == childrenCount - 1)
                {
                    to = _direction == AnimatedBoxContainerDirection.Vertical ? newSize.Y : newSize.X;
                }

                var size = to - from;
                Rect2 rect;

                if (_direction == AnimatedBoxContainerDirection.Vertical)
                {
                    rect = new Rect2(0, from, newSize.X, size);
                }
                else
                {
                    rect = new Rect2(from, 0, size, newSize.Y);
                }
                FitChildInRect(c, rect, msc);

                ofs = to;
                idx++;
            }
        }
    }

    /// <summary>
    /// Adapted from godotengine/godot/blob/4.4/scene/gui/container.cpp
    /// </summary>
    private void FitChildInRect(Control c, Rect2 rect, MiniSizeCache msc)
    {
        var rtl = IsLayoutRtl();
        var minsize = GetChildMinimumSize(c);
        var r = rect;

        if (!c.SizeFlagsHorizontal.HasFlag(SizeFlags.Fill))
        {
            r.Size = new Vector2(minsize.X, r.Size.Y);
            if (c.SizeFlagsHorizontal.HasFlag(SizeFlags.ShrinkEnd))
            {
                r.Position = new Vector2(r.Position.X + (rtl ? 0 : rect.Size.X - minsize.X), r.Position.Y);
            }
            else if (c.SizeFlagsHorizontal.HasFlag(SizeFlags.ShrinkCenter))
            {
                r.Position = new Vector2(r.Position.X + (float)Math.Floor((rect.Size.X - minsize.X) / 2), r.Position.Y);
            }
            else
            {
                r.Position = new Vector2(r.Position.X + (rtl ? (rect.Size.X - minsize.X) : 0), r.Position.Y);
            }
        }

        if (!c.SizeFlagsVertical.HasFlag(SizeFlags.Fill))
        {
            r.Size = new Vector2(r.Size.X, minsize.Y);
            if (c.SizeFlagsVertical.HasFlag(SizeFlags.ShrinkEnd))
            {
                r.Position = new Vector2(r.Position.X, r.Position.Y + (rect.Size.Y - minsize.Y));
            }
            else if (c.SizeFlagsVertical.HasFlag(SizeFlags.ShrinkCenter))
            {
                r.Position = new Vector2(r.Position.X, r.Position.Y + (float)Math.Floor((rect.Size.Y - minsize.Y) / 2));
            }
            else
            {
                r.Position = new Vector2(r.Position.X, r.Position.Y + 0);
            }
        }

        if (!_useTween)
        {
            c.Size = r.Size;
            c.Position = r.Position;
        }
        else
        {
            // tween is playing, skip this child
            _tweenList.TryGetValue(c, out TweenMiniSizeCache tweenMiniSizeCache);
            var tween = tweenMiniSizeCache.Tween;
            if (tween != null) return;
            tween = CreateTween().SetParallel(true).SetEase(_ease).SetTrans(_trans);
            tweenMiniSizeCache.Tween = tween;
            tweenMiniSizeCache.MiniSizeCache = msc;
            _tweenList.Add(c, tweenMiniSizeCache);
            tween.TweenProperty(c, "size", r.Size, _currentDuration).From(c.Size).SetDelay(_delay);
            tween.TweenProperty(c, "position", r.Position, _currentDuration).From(c.Position).SetDelay(_delay);
            tween.Finished += () =>
            {
                _tweenList.Remove(c);
                if (_tweenList.Count == 0)
                {
                    _useTween = false;
                }
            };
        }

    }

    /// <summary>
    /// Adapted from godotengine/godot/blob/4.4/scene/gui/box_container.cpp
    /// </summary>
    private new Vector2 GetMinimumSize()
    {
        var minimum = Vector2.Zero;
        var first = true;

        foreach (var child in GetChildren())
        {
            if (child is Control)
            {
                var c = child as Control;
                if (!c.Visible)
                {
                    continue;
                }
                var childSize = GetChildMinimumSize(c);

                if (_direction == AnimatedBoxContainerDirection.Vertical)
                {
                    if (childSize.X > minimum.X)
                    {
                        minimum.X = childSize.X;
                    }
                    minimum.Y += childSize.Y + (first ? 0 : _separation);
                }
                else
                {
                    if (childSize.Y > minimum.Y)
                    {
                        minimum.Y = childSize.Y;
                    }
                    minimum.X += childSize.X + (first ? 0 : _separation);
                }
                first = false;
            }
        }
        return minimum;
    }

    private Vector2 GetChildMinimumSize(Control c)
    {
        var v = c.Get(AnimatedPanelContainer.PropertyName._currentMinimumSize).AsVector2();
        return c.Get(AnimatedPanelContainer.PropertyName._classname).AsString() == nameof(AnimatedPanelContainer) && !v.Equals(Vector2.Zero) ? v : c.GetCombinedMinimumSize() * c.Scale;
    }

    private new void UpdateMinimumSize()
    {
        var minimum = GetMinimumSize();
        _size = minimum;
        if (_useTween)
        {
            // tween is playing, skip this child
            _tweenList.TryGetValue(this, out TweenMiniSizeCache tweenMiniSizeCache);
            var tween = tweenMiniSizeCache.Tween;
            if (tween != null) return;
            tween = CreateTween().SetParallel(true).SetEase(_ease).SetTrans(_trans);
            tweenMiniSizeCache.Tween = tween;
            _tweenList.Add(this, tweenMiniSizeCache);
            tween.TweenProperty(this, "custom_minimum_size", minimum, _currentDuration).From(CustomMinimumSize).SetDelay(_delay);
            tween.Finished += () =>
            {
                _tweenList.Remove(this);
                if (_tweenList.Count == 0)
                {
                    _useTween = false;
                }
            };
        }
        else
        {
            CustomMinimumSize = minimum;
        }

    }

    public AnimatedBoxContainer SetDelay(double delay)
    {
        _delay = delay;
        return this;
    }

    public AnimatedBoxContainer SetTrans(Tween.TransitionType trans)
    {
        _trans = trans;
        return this;
    }

    public AnimatedBoxContainer SetEase(Tween.EaseType ease)
    {
        _ease = ease;
        return this;
    }

    public void SetDirection(AnimatedBoxContainerDirection direction, double duration)
    {
        _useTween = true;
        _currentDuration = duration;
        ContainerDirection = direction;
    }
}
